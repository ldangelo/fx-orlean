# VPC Outputs
output "vpc_id" {
  description = "ID of the VPC"
  value       = module.vpc.vpc_id
}

output "private_subnet_ids" {
  description = "IDs of the private subnets"
  value       = module.vpc.private_subnet_ids
}

output "public_subnet_ids" {
  description = "IDs of the public subnets"
  value       = module.vpc.public_subnet_ids
}

# EKS Outputs
output "cluster_arn" {
  description = "EKS cluster ARN"
  value       = module.eks.cluster_arn
}

output "cluster_endpoint" {
  description = "EKS cluster endpoint"
  value       = module.eks.cluster_endpoint
}

output "cluster_name" {
  description = "EKS cluster name"
  value       = module.eks.cluster_name
}

output "cluster_certificate_authority_data" {
  description = "Base64 encoded certificate data"
  value       = module.eks.cluster_certificate_authority_data
  sensitive   = true
}

output "cluster_oidc_issuer_url" {
  description = "OIDC issuer URL"
  value       = module.eks.cluster_oidc_issuer_url
}

# RDS Outputs
output "db_endpoint" {
  description = "RDS instance endpoint"
  value       = module.rds.db_endpoint
}

output "db_port" {
  description = "RDS instance port"
  value       = module.rds.db_port
}

output "db_name" {
  description = "Database name"
  value       = module.rds.db_name
}

# Security Groups
output "security_group_ids" {
  description = "Security group IDs"
  value       = module.security_groups.security_group_ids
}

# IRSA Outputs
output "irsa_role_arns" {
  description = "IRSA role ARNs"
  value = {
    aws_load_balancer_controller = module.irsa.aws_load_balancer_controller_role_arn
    external_secrets            = module.irsa.external_secrets_role_arn
    cluster_autoscaler          = module.irsa.cluster_autoscaler_role_arn
    application                 = module.irsa.application_role_arn
  }
}

# Secrets Manager Outputs
output "secret_arns" {
  description = "AWS Secrets Manager secret ARNs"
  value       = module.secrets.secret_arns
  sensitive   = true
}

output "secret_names" {
  description = "AWS Secrets Manager secret names"
  value       = module.secrets.secret_names
}

# Kubernetes Configuration
output "kubectl_config" {
  description = "kubectl configuration command"
  value       = "aws eks update-kubeconfig --region ${var.aws_region} --name ${module.eks.cluster_name}"
}

# Helm Configuration
output "helm_values" {
  description = "Important values for Helm deployments"
  value = {
    cluster_name     = module.eks.cluster_name
    aws_region      = var.aws_region
    vpc_id          = module.vpc.vpc_id
    private_subnets = module.vpc.private_subnet_ids
    public_subnets  = module.vpc.public_subnet_ids
    
    # IRSA role ARNs for service accounts
    service_account_roles = {
      aws_load_balancer_controller = module.irsa.aws_load_balancer_controller_role_arn
      external_secrets            = module.irsa.external_secrets_role_arn
      cluster_autoscaler          = module.irsa.cluster_autoscaler_role_arn
      application                 = module.irsa.application_role_arn
    }
    
    # Database connection info
    database = {
      endpoint = module.rds.db_endpoint
      port     = module.rds.db_port
      name     = module.rds.db_name
      secret_name = module.secrets.database_secret_name
    }
    
    # External Secrets configuration
    external_secrets = {
      secret_store_config = module.secrets.external_secrets_store_config
      secret_templates   = module.secrets.kubernetes_secret_templates
    }
  }
  sensitive = false
}

# Next Steps Information
output "next_steps" {
  description = "Next steps for deployment"
  value = {
    configure_kubectl = "aws eks update-kubeconfig --region ${var.aws_region} --name ${module.eks.cluster_name}"
    install_helm_charts = "Navigate to ../../helm/ directory and follow deployment instructions"
    verify_deployment = "kubectl get nodes && kubectl get pods -A"
    access_applications = "Check ALB DNS name after deploying ingress controller"
  }
}