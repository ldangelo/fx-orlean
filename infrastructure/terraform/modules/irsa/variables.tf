variable "cluster_name" {
  description = "Name of the EKS cluster"
  type        = string
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "aws_region" {
  description = "AWS region"
  type        = string
}

variable "account_id" {
  description = "AWS account ID"
  type        = string
}

variable "cluster_oidc_issuer_url" {
  description = "The URL on the EKS cluster OIDC Issuer"
  type        = string
}

variable "secrets_manager_arns" {
  description = "List of AWS Secrets Manager ARNs that the application needs access to"
  type        = list(string)
  default     = []
}

variable "rds_cluster_arn" {
  description = "ARN of the RDS cluster"
  type        = string
  default     = ""
}

variable "enable_load_balancer_controller" {
  description = "Enable AWS Load Balancer Controller IRSA role"
  type        = bool
  default     = true
}

variable "enable_external_secrets" {
  description = "Enable External Secrets Operator IRSA role"
  type        = bool
  default     = true
}

variable "enable_cluster_autoscaler" {
  description = "Enable Cluster Autoscaler IRSA role"
  type        = bool
  default     = true
}

variable "enable_application_role" {
  description = "Enable application-specific IRSA role"
  type        = bool
  default     = true
}