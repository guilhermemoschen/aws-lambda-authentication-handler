#!/bin/bash

if [ $# -ne 3 ] 
then
    echo "Invalid number of parameters."
	echo $help_message
	exit
fi

if [ -d "sample_output" ]
then
	echo "Deleting webapi_output directory."
	rm -rf webapi_output
fi

AWS_ACCOUNT_ID=$1
AWS_REGION=$2
GOOGLE_OAUTH_CLIENT_ID=$3

export TF_VAR_aws_region=$AWS_REGION
export TF_VAR_google_oauth_client_id=$GOOGLE_OAUTH_CLIENT_ID

echo "Bulding JWT Authorizer Sample"
dotnet publish samples/AwsJwtAuthorizerSample -c Release -o sample_output
docker build -t aws-jwtauthorizer-sample:latest -f samples/AwsJwtAuthorizerSample/Dockerfile .
docker tag aws-jwtauthorizer-sample:latest "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/aws-jwtauthorizer-sample:latest"

echo "Pushing JWT Authorizer Sample to AWS ECR"
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com
docker push $AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/aws-jwtauthorizer-sample:latest

echo "Publishing JWT Authorizer Sample"
export TF_VAR_webapi_image_tag=latest
terraform -chdir=deployment init
terraform -chdir=deployment plan -out="plan.tfplan"
terraform -chdir=deployment apply "plan.tfplan"

echo "Cleaning"
rm -rf "deployment/plan.tfplan"
rm -rf sample_output