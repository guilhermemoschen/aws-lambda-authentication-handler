# aws-lambda-authentication-handler
A simple library to enable ASP.NET Core [authentication and authorization](https://docs.microsoft.com/en-us/aspnet/core/security) to [AspNetCoreServer](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.AspNetCoreServer).
This library is necessary to let ASP.NET Core runtime understand that the user is already authenticated by AWS API Gateway, and all claims are already created. With that, it's possible to use regular authorization features from ASP.NET Core like `[Authorize]` and `AuthorizationPolicy`.

## Installation and Configuration
First, install the Moschen.AwsLambdaAuthenticationHandler.Jwt NuGet package into your app.
```
dotnet add package Moschen.AwsLambdaAuthenticationHandlerNuGet
```
After, configure your ASP.Net Core project. At **Startup**, configure the Authentication Handler and enable authentication and authorization.
```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddAuthentication(AwsJwtAuthorizerDefaults.AuthenticationScheme)
      .AddJwtAuthorizer(options =>
      {
          // In the case of local run, this option enables the extraction of claims from the Id Token
          options.ExtractClaimsFromToken = true;
          
          // Validates the presence of the token.
          options.RequireToken = true;
      });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseAuthentication();
    app.UseAuthorization();
}
```

## Build
Consider checking the [GitHub Actions workflows](https://github.com/guilhermemoschen/aws-lambda-authentication-handler/blob/main/.github/workflows).

## Samples
Consider checking the samples [here](https://github.com/guilhermemoschen/aws-lambda-authentication-handler/tree/main/samples).

## Running Using AWS
The best way to run is using AWS platform free tier. Yes, you can try to use [localstack](https://github.com/localstack/localstack), however, since the AspNetCoreServer requires a docker image and the free tier doesn't support ECR, maybe you will have to need the pro version.

The example provided uses Google OAuth Client as Cognito Identity Pool and uses JWT authorizer.

### Requirements
- Linux
- Docker
- .Net CLI
- AWS CLI
- Terraform
- Google OAuth Client

### Deploy
From the local repository root.
```bash
./deployment/deploy.bash <aws account id> <aws region> <google oauth client id>
```
The results should be something like:
![image](https://user-images.githubusercontent.com/509459/130861941-8d564419-c3ab-4b17-be5c-7230cacd85b9.png)

### Run
Access the swagger https://<api gateway id>.execute-api.region.amazonaws.com/prod/swagger and configure the authentication:
![image](https://user-images.githubusercontent.com/509459/130862642-54779847-2cd9-4dab-8cd6-1b3dee4148e7.png)

Test the API:

![image](https://user-images.githubusercontent.com/509459/130863249-08b4efbf-5d78-41f6-837a-077b93d5d00e.png)


