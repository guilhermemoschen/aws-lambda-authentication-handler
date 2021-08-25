output "sample_image_uri" {
  description = "Sample Image URI of the container"
  value = aws_lambda_function.sample.image_uri
}

output "sample_function_name" {
  description = "Sample lambda function name"
  value = aws_lambda_function.sample.function_name
}

output "sample_apigateway_url" {
  description = "Sample API Gateway stage base URL"
  value = aws_apigatewayv2_stage.sample.invoke_url
}

output "sample_swagger_url" {
  description = "Swagger Url"
  value = "${aws_apigatewayv2_stage.sample.invoke_url}/swagger"
}
