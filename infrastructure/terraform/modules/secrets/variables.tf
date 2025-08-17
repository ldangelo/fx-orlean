variable "cluster_name" {
  description = "Name of the EKS cluster"
  type        = string
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "recovery_window_in_days" {
  description = "Recovery window for deleted secrets in days (0-30)"
  type        = number
  default     = 7
}

# External Secrets Operator and Application Role ARNs
variable "external_secrets_role_arns" {
  description = "List of External Secrets Operator IAM role ARNs that need access to secrets"
  type        = list(string)
  default     = []
}

variable "application_role_arns" {
  description = "List of application IAM role ARNs that need access to secrets"
  type        = list(string)
  default     = []
}

# Database Secrets
variable "database_username" {
  description = "Database username"
  type        = string
  default     = "fxadmin"
}

variable "database_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}

variable "database_host" {
  description = "Database host/endpoint"
  type        = string
  default     = ""
}

variable "database_port" {
  description = "Database port"
  type        = number
  default     = 5432
}

variable "database_name" {
  description = "Database name"
  type        = string
  default     = "fxorleans"
}

# OpenAI API Secrets
variable "openai_api_key" {
  description = "OpenAI API key for AI matching service"
  type        = string
  sensitive   = true
}

# Stripe Payment Secrets
variable "stripe_secret_key" {
  description = "Stripe secret key for payment processing"
  type        = string
  sensitive   = true
}

variable "stripe_publishable_key" {
  description = "Stripe publishable key"
  type        = string
  default     = ""
}

variable "stripe_webhook_secret" {
  description = "Stripe webhook secret for webhook verification"
  type        = string
  sensitive   = true
  default     = ""
}

# Google Calendar API Secrets
variable "google_client_id" {
  description = "Google OAuth client ID"
  type        = string
  default     = ""
}

variable "google_client_secret" {
  description = "Google OAuth client secret"
  type        = string
  sensitive   = true
  default     = ""
}

variable "google_project_id" {
  description = "Google Cloud project ID"
  type        = string
  default     = ""
}

# Keycloak Secrets
variable "keycloak_admin_username" {
  description = "Keycloak admin username"
  type        = string
  default     = "admin"
}

variable "keycloak_admin_password" {
  description = "Keycloak admin password"
  type        = string
  sensitive   = true
}

variable "keycloak_client_secret" {
  description = "Keycloak OAuth client secret"
  type        = string
  sensitive   = true
}

# JWT Secrets
variable "jwt_signing_key" {
  description = "JWT signing key for token authentication"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT token issuer"
  type        = string
  default     = "fx-orleans"
}

variable "jwt_audience" {
  description = "JWT token audience"
  type        = string
  default     = "fx-orleans-api"
}

# Application Configuration Secrets
variable "app_encryption_key" {
  description = "Application encryption key for sensitive data"
  type        = string
  sensitive   = true
}

variable "session_secret" {
  description = "Session secret for session management"
  type        = string
  sensitive   = true
}

variable "cors_origins" {
  description = "Allowed CORS origins"
  type        = list(string)
  default     = []
}

variable "rate_limit_secret" {
  description = "Secret for rate limiting"
  type        = string
  sensitive   = true
  default     = ""
}