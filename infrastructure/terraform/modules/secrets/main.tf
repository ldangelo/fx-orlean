terraform {
  required_version = ">= 1.6.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# KMS Key for Secrets Manager encryption
resource "aws_kms_key" "secrets_manager" {
  description             = "KMS key for AWS Secrets Manager - ${var.cluster_name}"
  deletion_window_in_days = 7

  tags = {
    Name        = "${var.cluster_name}-secrets-manager-key"
    Environment = var.environment
  }
}

resource "aws_kms_alias" "secrets_manager" {
  name          = "alias/secrets-manager-${var.cluster_name}"
  target_key_id = aws_kms_key.secrets_manager.key_id
}

# Database Password Secret
resource "aws_secretsmanager_secret" "database_password" {
  name                    = "${var.cluster_name}/${var.environment}/database/password"
  description             = "Database password for FX-Orleans ${var.environment} environment"
  kms_key_id             = aws_kms_key.secrets_manager.arn
  recovery_window_in_days = var.recovery_window_in_days

  tags = {
    Name        = "${var.cluster_name}-database-password"
    Environment = var.environment
    Type        = "database"
  }
}

resource "aws_secretsmanager_secret_version" "database_password" {
  secret_id     = aws_secretsmanager_secret.database_password.id
  secret_string = jsonencode({
    username = var.database_username
    password = var.database_password
    engine   = "postgres"
    host     = var.database_host
    port     = var.database_port
    dbname   = var.database_name
  })
}

# OpenAI API Key Secret
resource "aws_secretsmanager_secret" "openai_api_key" {
  name                    = "${var.cluster_name}/${var.environment}/openai/api-key"
  description             = "OpenAI API key for AI matching service"
  kms_key_id             = aws_kms_key.secrets_manager.arn
  recovery_window_in_days = var.recovery_window_in_days

  tags = {
    Name        = "${var.cluster_name}-openai-api-key"
    Environment = var.environment
    Type        = "api-key"
  }
}

resource "aws_secretsmanager_secret_version" "openai_api_key" {
  secret_id     = aws_secretsmanager_secret.openai_api_key.id
  secret_string = jsonencode({
    api_key = var.openai_api_key
  })
}

# Stripe Secret Key
resource "aws_secretsmanager_secret" "stripe_secret_key" {
  name                    = "${var.cluster_name}/${var.environment}/stripe/secret-key"
  description             = "Stripe secret key for payment processing"
  kms_key_id             = aws_kms_key.secrets_manager.arn
  recovery_window_in_days = var.recovery_window_in_days

  tags = {
    Name        = "${var.cluster_name}-stripe-secret-key"
    Environment = var.environment
    Type        = "payment"
  }
}

resource "aws_secretsmanager_secret_version" "stripe_secret_key" {
  secret_id     = aws_secretsmanager_secret.stripe_secret_key.id
  secret_string = jsonencode({
    secret_key      = var.stripe_secret_key
    publishable_key = var.stripe_publishable_key
    webhook_secret  = var.stripe_webhook_secret
  })
}

# Google Calendar API Credentials
resource "aws_secretsmanager_secret" "google_calendar_credentials" {
  name                    = "${var.cluster_name}/${var.environment}/google/calendar-credentials"
  description             = "Google Calendar API credentials for meeting scheduling"
  kms_key_id             = aws_kms_key.secrets_manager.arn
  recovery_window_in_days = var.recovery_window_in_days

  tags = {
    Name        = "${var.cluster_name}-google-calendar-credentials"
    Environment = var.environment
    Type        = "google-api"
  }
}

resource "aws_secretsmanager_secret_version" "google_calendar_credentials" {
  secret_id     = aws_secretsmanager_secret.google_calendar_credentials.id
  secret_string = jsonencode({
    client_id     = var.google_client_id
    client_secret = var.google_client_secret
    project_id    = var.google_project_id
  })
}

# Keycloak Admin Credentials
resource "aws_secretsmanager_secret" "keycloak_admin" {
  name                    = "${var.cluster_name}/${var.environment}/keycloak/admin-credentials"
  description             = "Keycloak admin credentials for authentication service"
  kms_key_id             = aws_kms_key.secrets_manager.arn
  recovery_window_in_days = var.recovery_window_in_days

  tags = {
    Name        = "${var.cluster_name}-keycloak-admin"
    Environment = var.environment
    Type        = "authentication"
  }
}

resource "aws_secretsmanager_secret_version" "keycloak_admin" {
  secret_id     = aws_secretsmanager_secret.keycloak_admin.id
  secret_string = jsonencode({
    admin_username = var.keycloak_admin_username
    admin_password = var.keycloak_admin_password
    client_secret  = var.keycloak_client_secret
  })
}

# JWT Signing Key
resource "aws_secretsmanager_secret" "jwt_signing_key" {
  name                    = "${var.cluster_name}/${var.environment}/jwt/signing-key"
  description             = "JWT signing key for token authentication"
  kms_key_id             = aws_kms_key.secrets_manager.arn
  recovery_window_in_days = var.recovery_window_in_days

  tags = {
    Name        = "${var.cluster_name}-jwt-signing-key"
    Environment = var.environment
    Type        = "jwt"
  }
}

resource "aws_secretsmanager_secret_version" "jwt_signing_key" {
  secret_id     = aws_secretsmanager_secret.jwt_signing_key.id
  secret_string = jsonencode({
    signing_key = var.jwt_signing_key
    issuer      = var.jwt_issuer
    audience    = var.jwt_audience
  })
}

# Application Configuration Secret (for sensitive app settings)
resource "aws_secretsmanager_secret" "app_config" {
  name                    = "${var.cluster_name}/${var.environment}/app/configuration"
  description             = "Application configuration with sensitive settings"
  kms_key_id             = aws_kms_key.secrets_manager.arn
  recovery_window_in_days = var.recovery_window_in_days

  tags = {
    Name        = "${var.cluster_name}-app-configuration"
    Environment = var.environment
    Type        = "application"
  }
}

resource "aws_secretsmanager_secret_version" "app_config" {
  secret_id     = aws_secretsmanager_secret.app_config.id
  secret_string = jsonencode({
    encryption_key    = var.app_encryption_key
    session_secret    = var.session_secret
    cors_origins      = var.cors_origins
    rate_limit_secret = var.rate_limit_secret
  })
}

# Resource-based policy for External Secrets Operator access
data "aws_iam_policy_document" "secrets_policy" {
  statement {
    sid    = "AllowExternalSecretsOperatorAccess"
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = var.external_secrets_role_arns
    }

    actions = [
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret"
    ]

    resources = ["*"]

    condition {
      test     = "StringEquals"
      variable = "secretsmanager:ResourceTag/Environment"
      values   = [var.environment]
    }
  }

  statement {
    sid    = "AllowApplicationRoleAccess"
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = var.application_role_arns
    }

    actions = [
      "secretsmanager:GetSecretValue",
      "secretsmanager:DescribeSecret"
    ]

    resources = ["*"]

    condition {
      test     = "StringEquals"
      variable = "secretsmanager:ResourceTag/Environment"
      values   = [var.environment]
    }
  }
}

# Apply resource policy to all secrets
resource "aws_secretsmanager_secret_policy" "database_password" {
  secret_arn = aws_secretsmanager_secret.database_password.arn
  policy     = data.aws_iam_policy_document.secrets_policy.json
}

resource "aws_secretsmanager_secret_policy" "openai_api_key" {
  secret_arn = aws_secretsmanager_secret.openai_api_key.arn
  policy     = data.aws_iam_policy_document.secrets_policy.json
}

resource "aws_secretsmanager_secret_policy" "stripe_secret_key" {
  secret_arn = aws_secretsmanager_secret.stripe_secret_key.arn
  policy     = data.aws_iam_policy_document.secrets_policy.json
}

resource "aws_secretsmanager_secret_policy" "google_calendar_credentials" {
  secret_arn = aws_secretsmanager_secret.google_calendar_credentials.arn
  policy     = data.aws_iam_policy_document.secrets_policy.json
}

resource "aws_secretsmanager_secret_policy" "keycloak_admin" {
  secret_arn = aws_secretsmanager_secret.keycloak_admin.arn
  policy     = data.aws_iam_policy_document.secrets_policy.json
}

resource "aws_secretsmanager_secret_policy" "jwt_signing_key" {
  secret_arn = aws_secretsmanager_secret.jwt_signing_key.arn
  policy     = data.aws_iam_policy_document.secrets_policy.json
}

resource "aws_secretsmanager_secret_policy" "app_config" {
  secret_arn = aws_secretsmanager_secret.app_config.arn
  policy     = data.aws_iam_policy_document.secrets_policy.json
}