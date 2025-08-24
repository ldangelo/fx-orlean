# DNS Module Outputs

output "hosted_zone_id" {
  description = "Route 53 hosted zone ID"
  value       = data.aws_route53_zone.main.zone_id
}

output "hosted_zone_name" {
  description = "Route 53 hosted zone name"
  value       = data.aws_route53_zone.main.name
}

output "blazor_fqdn" {
  description = "Fully qualified domain name for Blazor frontend"
  value       = "${var.blazor_subdomain}.${var.domain_name}"
}

output "keycloak_fqdn" {
  description = "Fully qualified domain name for Keycloak"
  value       = "${var.keycloak_subdomain}.${var.domain_name}"
}

output "blazor_dns_record_name" {
  description = "DNS record name for Blazor frontend"
  value       = aws_route53_record.blazor_frontend.name
}

output "keycloak_dns_record_name" {
  description = "DNS record name for Keycloak"
  value       = aws_route53_record.keycloak.name
}

output "public_urls" {
  description = "Public URLs for the services"
  value = {
    blazor_frontend = "http://${var.blazor_subdomain}.${var.domain_name}"
    keycloak_admin  = "http://${var.keycloak_subdomain}.${var.domain_name}"
  }
}

output "health_check_ids" {
  description = "Route 53 health check IDs"
  value = {
    blazor_frontend = var.enable_health_checks ? aws_route53_health_check.blazor_frontend[0].id : null
    keycloak        = var.enable_health_checks ? aws_route53_health_check.keycloak[0].id : null
  }
}