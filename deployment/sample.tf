data "aws_ecr_repository" "sample" {
  name = var.sample_container_repository
}

data "aws_ecr_image" "sample" {
 repository_name = data.aws_ecr_repository.sample.name
 image_tag       = var.sample_image_tag
}

resource "aws_lambda_function" "sample" {
  function_name = "webapi-sample"
  image_config {
    command = [var.sample_entrypoint]
  }
  image_uri = "${data.aws_ecr_repository.sample.repository_url}@${data.aws_ecr_image.sample.id}"
  package_type = "Image"
  role = aws_iam_role.sample_function.arn
  
  timeout = 30
}

resource "aws_cloudwatch_log_group" "sample_function" {
  name = "/aws/lambda/${aws_lambda_function.sample.function_name}"

  retention_in_days = 30
}

resource "aws_iam_role" "sample_function" {
  name = "sample_function_role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Sid    = ""
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "sample_function" {
  role       = aws_iam_role.sample_function.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_apigatewayv2_api" "sample" {
  name          = "sample_apigateway"
  protocol_type = "HTTP"

  cors_configuration {
    allow_credentials = false
    allow_headers     = ["*"]
    allow_methods     = ["*"]
    allow_origins     = ["*"]
    expose_headers    = ["*"]
    max_age           = 3600
  }
}

resource "aws_apigatewayv2_stage" "sample" {
  api_id = aws_apigatewayv2_api.sample.id

  name        = "prod"
  auto_deploy = true

  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.sample_apigateway.arn

    format = jsonencode({
      requestId               = "$context.requestId"
      sourceIp                = "$context.identity.sourceIp"
      requestTime             = "$context.requestTime"
      protocol                = "$context.protocol"
      httpMethod              = "$context.httpMethod"
      resourcePath            = "$context.resourcePath"
      routeKey                = "$context.routeKey"
      status                  = "$context.status"
      responseLength          = "$context.responseLength"
      integrationErrorMessage = "$context.integrationErrorMessage"
      }
    )
  }
}

resource "aws_apigatewayv2_integration" "sample" {
  api_id = aws_apigatewayv2_api.sample.id

  integration_uri    = aws_lambda_function.sample.invoke_arn
  integration_type   = "AWS_PROXY"
  integration_method = "POST"
  payload_format_version = "2.0"
}

resource "aws_apigatewayv2_route" "sample_default_route" {
  api_id             = aws_apigatewayv2_api.sample.id
  route_key          = "$default"
  target             = "integrations/${aws_apigatewayv2_integration.sample.id}"
  authorizer_id      = aws_apigatewayv2_authorizer.sample.id
  authorization_type = "JWT"
}

resource "aws_apigatewayv2_route" "sample_swaggerplus_route" {
  api_id             = aws_apigatewayv2_api.sample.id
  route_key          = "GET /swagger/{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.sample.id}"
}

resource "aws_apigatewayv2_route" "sample_swagger_route" {
  api_id             = aws_apigatewayv2_api.sample.id
  route_key          = "GET /swagger"
  target             = "integrations/${aws_apigatewayv2_integration.sample.id}"
}

resource "aws_cloudwatch_log_group" "sample_apigateway" {
  name = "/aws/apigateway/${aws_apigatewayv2_api.sample.name}"

  retention_in_days = 30
}

resource "aws_lambda_permission" "design_deploymentarea" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.sample.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.sample.execution_arn}/*/*"
}

resource "aws_apigatewayv2_authorizer" "sample" {
  api_id           = aws_apigatewayv2_api.sample.id
  authorizer_type  = "JWT"
  identity_sources = ["$request.header.Authorization"]
  name             = "cognito_authorizer"

  jwt_configuration {
    audience = ["${var.google_oauth_client_id}.apps.googleusercontent.com"]
    issuer   = "https://accounts.google.com"
  }
}