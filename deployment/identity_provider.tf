resource "aws_cognito_identity_pool" "main" {
  identity_pool_name               = "Sample Identity Pool with Google"
  allow_unauthenticated_identities = false
  allow_classic_flow               = false

  supported_login_providers = {
    "accounts.google.com" = "${var.google_oauth_client_id}.apps.googleusercontent.com"
  }
}

resource "aws_cognito_identity_pool_roles_attachment" "main" {
      identity_pool_id = "${aws_cognito_identity_pool.main.id}"

      roles = {
           authenticated   = "${aws_iam_role.authenticated.arn}"
      }
 }

resource "aws_iam_role" "authenticated" {
  name = "sample_cognito_authenticated"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Federated": "cognito-identity.amazonaws.com"
      },
      "Action": "sts:AssumeRoleWithWebIdentity",
      "Condition": {
        "StringEquals": {
          "cognito-identity.amazonaws.com:aud": "${aws_cognito_identity_pool.main.id}"
        },
        "ForAnyValue:StringLike": {
          "cognito-identity.amazonaws.com:amr": "authenticated"
        }
      }
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "authenticated" {
  name = "authenticated_policy"
  role = aws_iam_role.authenticated.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "mobileanalytics:PutEvents",
        "cognito-sync:*",
        "cognito-identity:*"
      ],
      "Resource": [
        "*"
      ]
    }
  ]
}
EOF
}