# DNS Module Variables

variable "domain_name" {
  description = "The domain name for the hosted zone"
  type        = string
  default     = "fortiumsoftware.com"
}

variable "blazor_subdomain" {
  description = "Subdomain for the Blazor frontend"
  type        = string
  default     = "fx-expert-dev"
}

variable "keycloak_subdomain" {
  description = "Subdomain for Keycloak"
  type        = string
  default     = "fx-expert-keycloak"
}

variable "blazor_alb_dns_name" {
  description = "DNS name of the Blazor frontend ALB"
  type        = string
}

variable "keycloak_alb_dns_name" {
  description = "DNS name of the Keycloak ALB"
  type        = string
}

variable "dns_ttl" {
  description = "TTL for DNS records in seconds"
  type        = number
  default     = 300
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "aws_region" {
  description = "AWS region for health checks"
  type        = string
  default     = "us-east-2"
}

variable "enable_health_checks" {
  description = "Enable Route 53 health checks for the services"
  type        = bool
  default     = false
}

variable "cluster_name" {
  description = "Name of the EKS cluster"
  type        = string
}