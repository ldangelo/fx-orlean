# DNS Configuration for FX-Orleans Public Services
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# Data source for existing hosted zone
data "aws_route53_zone" "main" {
  name         = var.domain_name
  private_zone = false
}

# CNAME record for Blazor Frontend (fx-expert-dev)
resource "aws_route53_record" "blazor_frontend" {
  zone_id = data.aws_route53_zone.main.zone_id
  name    = var.blazor_subdomain
  type    = "CNAME"
  ttl     = var.dns_ttl
  records = [var.blazor_alb_dns_name]

  depends_on = [data.aws_route53_zone.main]
}

# CNAME record for Keycloak (fx-expert-keycloak)
resource "aws_route53_record" "keycloak" {
  zone_id = data.aws_route53_zone.main.zone_id
  name    = var.keycloak_subdomain
  type    = "CNAME"
  ttl     = var.dns_ttl
  records = [var.keycloak_alb_dns_name]

  depends_on = [data.aws_route53_zone.main]
}

# Optional: Health checks for the services
resource "aws_route53_health_check" "blazor_frontend" {
  count                           = var.enable_health_checks ? 1 : 0
  fqdn                           = "${var.blazor_subdomain}.${var.domain_name}"
  port                           = 80
  type                           = "HTTP"
  resource_path                  = "/health"
  failure_threshold              = "3"
  request_interval               = "30"
  insufficient_data_health_status = "Healthy"

  tags = {
    Name        = "fx-orleans-blazor-health-check"
    Environment = var.environment
    Service     = "blazor-frontend"
  }
}

resource "aws_route53_health_check" "keycloak" {
  count                           = var.enable_health_checks ? 1 : 0
  fqdn                           = "${var.keycloak_subdomain}.${var.domain_name}"
  port                           = 80
  type                           = "HTTP"
  resource_path                  = "/health/ready"
  failure_threshold              = "3"
  request_interval               = "30"
  insufficient_data_health_status = "Healthy"

  tags = {
    Name        = "fx-orleans-keycloak-health-check"
    Environment = var.environment
    Service     = "keycloak"
  }
}