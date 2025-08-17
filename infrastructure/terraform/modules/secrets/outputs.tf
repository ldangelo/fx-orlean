output "kms_key_id" {
  description = "KMS key ID for Secrets Manager encryption"
  value       = aws_kms_key.secrets_manager.key_id
}

output "kms_key_arn" {
  description = "KMS key ARN for Secrets Manager encryption"
  value       = aws_kms_key.secrets_manager.arn
}

output "kms_alias_name" {
  description = "KMS key alias name"
  value       = aws_kms_alias.secrets_manager.name
}

# Secret ARNs
output "secret_arns" {
  description = "Map of secret names to ARNs"
  value = {
    database_password         = aws_secretsmanager_secret.database_password.arn
    openai_api_key           = aws_secretsmanager_secret.openai_api_key.arn
    stripe_secret_key        = aws_secretsmanager_secret.stripe_secret_key.arn
    google_calendar_credentials = aws_secretsmanager_secret.google_calendar_credentials.arn
    keycloak_admin           = aws_secretsmanager_secret.keycloak_admin.arn
    jwt_signing_key          = aws_secretsmanager_secret.jwt_signing_key.arn
    app_config              = aws_secretsmanager_secret.app_config.arn
  }
}

# Secret Names (for External Secrets Operator)
output "secret_names" {
  description = "Map of secret names to their AWS Secrets Manager names"
  value = {
    database_password         = aws_secretsmanager_secret.database_password.name
    openai_api_key           = aws_secretsmanager_secret.openai_api_key.name
    stripe_secret_key        = aws_secretsmanager_secret.stripe_secret_key.name
    google_calendar_credentials = aws_secretsmanager_secret.google_calendar_credentials.name
    keycloak_admin           = aws_secretsmanager_secret.keycloak_admin.name
    jwt_signing_key          = aws_secretsmanager_secret.jwt_signing_key.name
    app_config              = aws_secretsmanager_secret.app_config.name
  }
}

# Database Secret Outputs
output "database_secret_arn" {
  description = "Database password secret ARN"
  value       = aws_secretsmanager_secret.database_password.arn
}

output "database_secret_name" {
  description = "Database password secret name"
  value       = aws_secretsmanager_secret.database_password.name
}

# OpenAI Secret Outputs
output "openai_secret_arn" {
  description = "OpenAI API key secret ARN"
  value       = aws_secretsmanager_secret.openai_api_key.arn
}

output "openai_secret_name" {
  description = "OpenAI API key secret name"
  value       = aws_secretsmanager_secret.openai_api_key.name
}

# Stripe Secret Outputs
output "stripe_secret_arn" {
  description = "Stripe secret key ARN"
  value       = aws_secretsmanager_secret.stripe_secret_key.arn
}

output "stripe_secret_name" {
  description = "Stripe secret key name"
  value       = aws_secretsmanager_secret.stripe_secret_key.name
}

# Google Calendar Secret Outputs
output "google_calendar_secret_arn" {
  description = "Google Calendar credentials secret ARN"
  value       = aws_secretsmanager_secret.google_calendar_credentials.arn
}

output "google_calendar_secret_name" {
  description = "Google Calendar credentials secret name"
  value       = aws_secretsmanager_secret.google_calendar_credentials.name
}

# Keycloak Secret Outputs
output "keycloak_secret_arn" {
  description = "Keycloak admin credentials secret ARN"
  value       = aws_secretsmanager_secret.keycloak_admin.arn
}

output "keycloak_secret_name" {
  description = "Keycloak admin credentials secret name"
  value       = aws_secretsmanager_secret.keycloak_admin.name
}

# JWT Secret Outputs
output "jwt_secret_arn" {
  description = "JWT signing key secret ARN"
  value       = aws_secretsmanager_secret.jwt_signing_key.arn
}

output "jwt_secret_name" {
  description = "JWT signing key secret name"
  value       = aws_secretsmanager_secret.jwt_signing_key.name
}

# Application Config Secret Outputs
output "app_config_secret_arn" {
  description = "Application configuration secret ARN"
  value       = aws_secretsmanager_secret.app_config.arn
}

output "app_config_secret_name" {
  description = "Application configuration secret name"
  value       = aws_secretsmanager_secret.app_config.name
}

# External Secrets Operator Configuration
output "external_secrets_store_config" {
  description = "Configuration for External Secrets Operator SecretStore"
  value = {
    region = data.aws_region.current.name
    auth = {
      jwt = {
        serviceAccountRef = {
          name = "external-secrets"
        }
      }
    }
  }
}

# Kubernetes Secret Templates for External Secrets Operator
output "kubernetes_secret_templates" {
  description = "Templates for Kubernetes secrets created by External Secrets Operator"
  value = {
    database = {
      secretName = "fx-orleans-database"
      secretStore = "aws-secrets-manager"
      target = {
        type = "Opaque"
        template = {
          data = {
            username = "{{ .username }}"
            password = "{{ .password }}"
            host     = "{{ .host }}"
            port     = "{{ .port }}"
            dbname   = "{{ .dbname }}"
            connection_string = "postgresql://{{ .username }}:{{ .password }}@{{ .host }}:{{ .port }}/{{ .dbname }}"
          }
        }
      }
      dataFrom = [
        {
          extract = {
            key = aws_secretsmanager_secret.database_password.name
          }
        }
      ]
    }
    
    openai = {
      secretName = "fx-orleans-openai"
      secretStore = "aws-secrets-manager"
      target = {
        type = "Opaque"
        template = {
          data = {
            api_key = "{{ .api_key }}"
          }
        }
      }
      dataFrom = [
        {
          extract = {
            key = aws_secretsmanager_secret.openai_api_key.name
          }
        }
      ]
    }
    
    stripe = {
      secretName = "fx-orleans-stripe"
      secretStore = "aws-secrets-manager"
      target = {
        type = "Opaque"
        template = {
          data = {
            secret_key      = "{{ .secret_key }}"
            publishable_key = "{{ .publishable_key }}"
            webhook_secret  = "{{ .webhook_secret }}"
          }
        }
      }
      dataFrom = [
        {
          extract = {
            key = aws_secretsmanager_secret.stripe_secret_key.name
          }
        }
      ]
    }
    
    google_calendar = {
      secretName = "fx-orleans-google-calendar"
      secretStore = "aws-secrets-manager"
      target = {
        type = "Opaque"
        template = {
          data = {
            client_id     = "{{ .client_id }}"
            client_secret = "{{ .client_secret }}"
            project_id    = "{{ .project_id }}"
          }
        }
      }
      dataFrom = [
        {
          extract = {
            key = aws_secretsmanager_secret.google_calendar_credentials.name
          }
        }
      ]
    }
    
    keycloak = {
      secretName = "fx-orleans-keycloak"
      secretStore = "aws-secrets-manager"
      target = {
        type = "Opaque"
        template = {
          data = {
            admin_username = "{{ .admin_username }}"
            admin_password = "{{ .admin_password }}"
            client_secret  = "{{ .client_secret }}"
          }
        }
      }
      dataFrom = [
        {
          extract = {
            key = aws_secretsmanager_secret.keycloak_admin.name
          }
        }
      ]
    }
    
    jwt = {
      secretName = "fx-orleans-jwt"
      secretStore = "aws-secrets-manager"
      target = {
        type = "Opaque"
        template = {
          data = {
            signing_key = "{{ .signing_key }}"
            issuer      = "{{ .issuer }}"
            audience    = "{{ .audience }}"
          }
        }
      }
      dataFrom = [
        {
          extract = {
            key = aws_secretsmanager_secret.jwt_signing_key.name
          }
        }
      ]
    }
    
    app_config = {
      secretName = "fx-orleans-app-config"
      secretStore = "aws-secrets-manager"
      target = {
        type = "Opaque"
        template = {
          data = {
            encryption_key    = "{{ .encryption_key }}"
            session_secret    = "{{ .session_secret }}"
            cors_origins      = "{{ .cors_origins | join \",\" }}"
            rate_limit_secret = "{{ .rate_limit_secret }}"
          }
        }
      }
      dataFrom = [
        {
          extract = {
            key = aws_secretsmanager_secret.app_config.name
          }
        }
      ]
    }
  }
}

# Current AWS region data source
data "aws_region" "current" {}