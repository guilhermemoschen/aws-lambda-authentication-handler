variable "aws_region" {
  description = "AWS Region"
  type    = string
}

variable "sample_container_repository" {
  description = "The ECR repository name for Sample"
  type    = string
  default = "aws-jwtauthorizer-sample"
}

variable "sample_image_tag" {
  description = "The container tag"
  type    = string
  default = "latest"
}

variable "sample_entrypoint" {
  description = "The WebApi Lambda Function Entry Point"
  type    = string
  default = "AwsJwtAuthorizerSample::AwsJwtAuthorizerSample.LambdaEntryPoint::FunctionHandlerAsync"
}

variable "google_oauth_client_id" {
  description = "The WebApi Lambda Function Entry Point"
  type    = string
}