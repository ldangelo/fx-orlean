# General Configuration
variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-west-2"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "cluster_name" {
  description = "Name of the EKS cluster"
  type        = string
  default     = "fx-orleans-dev"
}

# VPC Configuration
variable "vpc_cidr" {
  description = "CIDR block for VPC"
  type        = string
  default     = "10.0.0.0/16"
}

# EKS Configuration
variable "kubernetes_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.28"
}

variable "node_group_desired_size" {
  description = "Desired number of nodes"
  type        = number
  default     = 2
}

variable "node_group_max_size" {
  description = "Maximum number of nodes"
  type        = number
  default     = 4
}

variable "node_group_min_size" {
  description = "Minimum number of nodes"
  type        = number
  default     = 1
}

variable "instance_types" {
  description = "Instance types for worker nodes"
  type        = list(string)
  default     = ["t3.medium"]
}

variable "capacity_type" {
  description = "Capacity type (ON_DEMAND or SPOT)"
  type        = string
  default     = "ON_DEMAND"
}

variable "disk_size" {
  description = "Disk size for worker nodes"
  type        = number
  default     = 50
}

variable "cluster_endpoint_public_access_cidrs" {
  description = "CIDR blocks for EKS API access"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}

variable "ec2_ssh_key" {
  description = "EC2 SSH key name"
  type        = string
  default     = null
}

# Security Configuration
variable "enable_ssh_access" {
  description = "Enable SSH access to worker nodes"
  type        = bool
  default     = true
}

variable "ssh_allowed_cidr_blocks" {
  description = "CIDR blocks allowed for SSH"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "enable_rds_admin_access" {
  description = "Enable direct RDS admin access"
  type        = bool
  default     = true
}

variable "rds_admin_cidr_blocks" {
  description = "CIDR blocks for RDS admin access"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

# Database Configuration
variable "db_name" {
  description = "Database name"
  type        = string
  default     = "fxorleans"
}

variable "db_username" {
  description = "Database username"
  type        = string
  default     = "fxadmin"
}

variable "db_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}

variable "rds_instance_class" {
  description = "RDS instance class"
  type        = string
  default     = "db.t3.micro"
}

variable "rds_allocated_storage" {
  description = "RDS allocated storage"
  type        = number
  default     = 20
}

variable "rds_max_allocated_storage" {
  description = "RDS max allocated storage"
  type        = number
  default     = 100
}

variable "rds_multi_az" {
  description = "Enable RDS Multi-AZ"
  type        = bool
  default     = false
}

variable "rds_backup_retention_period" {
  description = "RDS backup retention period"
  type        = number
  default     = 7
}

variable "rds_deletion_protection" {
  description = "Enable RDS deletion protection"
  type        = bool
  default     = false
}

variable "rds_skip_final_snapshot" {
  description = "Skip final snapshot"
  type        = bool
  default     = true
}

variable "rds_create_read_replica" {
  description = "Create RDS read replica"
  type        = bool
  default     = false
}

variable "rds_monitoring_interval" {
  description = "RDS monitoring interval"
  type        = number
  default     = 60
}

variable "rds_performance_insights_enabled" {
  description = "Enable Performance Insights"
  type        = bool
  default     = true
}

# Application Secrets
variable "openai_api_key" {
  description = "OpenAI API key"
  type        = string
  sensitive   = true
}

variable "stripe_secret_key" {
  description = "Stripe secret key"
  type        = string
  sensitive   = true
}

variable "stripe_publishable_key" {
  description = "Stripe publishable key"
  type        = string
  default     = ""
}

variable "stripe_webhook_secret" {
  description = "Stripe webhook secret"
  type        = string
  sensitive   = true
  default     = ""
}

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
  description = "Google project ID"
  type        = string
  default     = ""
}

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
  description = "Keycloak client secret"
  type        = string
  sensitive   = true
}

variable "jwt_signing_key" {
  description = "JWT signing key"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT issuer"
  type        = string
  default     = "fx-orleans-dev"
}

variable "jwt_audience" {
  description = "JWT audience"
  type        = string
  default     = "fx-orleans-api"
}

variable "app_encryption_key" {
  description = "Application encryption key"
  type        = string
  sensitive   = true
}

variable "session_secret" {
  description = "Session secret"
  type        = string
  sensitive   = true
}

variable "cors_origins" {
  description = "CORS origins"
  type        = list(string)
  default     = ["http://localhost:3000", "https://dev.fx-orleans.com"]
}

variable "rate_limit_secret" {
  description = "Rate limit secret"
  type        = string
  sensitive   = true
  default     = "dev-rate-limit-secret"
}

# Public Ingress Configuration
variable "blazor_public_hostname" {
  description = "Public hostname for Blazor frontend"
  type        = string
  default     = "fx-expert-dev.fortiumpartners.com"
}

variable "keycloak_public_hostname" {
  description = "Public hostname for Keycloak"
  type        = string
  default     = "fx-expert-keycloak.fortiumpartners.com"
}

variable "blazor_service_name" {
  description = "Name of the Blazor frontend Kubernetes service"
  type        = string
  default     = "fx-orleans-blazor-blazor-frontend"
}

variable "blazor_service_port" {
  description = "Port of the Blazor frontend Kubernetes service"
  type        = number
  default     = 80
}

variable "keycloak_service_name" {
  description = "Name of the Keycloak Kubernetes service"
  type        = string
  default     = "fx-orleans-keycloak"
}

variable "keycloak_service_port" {
  description = "Port of the Keycloak Kubernetes service"
  type        = number
  default     = 80
}

# SSL Configuration
variable "enable_ssl" {
  description = "Enable SSL/TLS termination at the load balancer"
  type        = bool
  default     = false
}

variable "ssl_certificate_arn" {
  description = "ARN of the SSL certificate in AWS Certificate Manager"
  type        = string
  default     = ""
}

# DNS Configuration
variable "enable_dns_health_checks" {
  description = "Enable Route 53 health checks for the services"
  type        = bool
  default     = false
}