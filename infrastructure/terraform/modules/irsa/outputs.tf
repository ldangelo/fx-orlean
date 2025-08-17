output "oidc_provider_arn" {
  description = "The ARN of the OIDC Identity Provider"
  value       = aws_iam_openid_connect_provider.cluster.arn
}

output "oidc_provider_url" {
  description = "The URL of the OIDC Identity Provider"
  value       = aws_iam_openid_connect_provider.cluster.url
}

# AWS Load Balancer Controller outputs
output "aws_load_balancer_controller_role_arn" {
  description = "ARN of the AWS Load Balancer Controller IAM role"
  value       = aws_iam_role.aws_load_balancer_controller.arn
}

output "aws_load_balancer_controller_role_name" {
  description = "Name of the AWS Load Balancer Controller IAM role"
  value       = aws_iam_role.aws_load_balancer_controller.name
}

output "aws_load_balancer_controller_policy_arn" {
  description = "ARN of the AWS Load Balancer Controller IAM policy"
  value       = aws_iam_policy.aws_load_balancer_controller.arn
}

# External Secrets outputs
output "external_secrets_role_arn" {
  description = "ARN of the External Secrets IAM role"
  value       = aws_iam_role.external_secrets.arn
}

output "external_secrets_role_name" {
  description = "Name of the External Secrets IAM role"
  value       = aws_iam_role.external_secrets.name
}

output "external_secrets_policy_arn" {
  description = "ARN of the External Secrets IAM policy"
  value       = aws_iam_policy.external_secrets.arn
}

# Cluster Autoscaler outputs
output "cluster_autoscaler_role_arn" {
  description = "ARN of the Cluster Autoscaler IAM role"
  value       = aws_iam_role.cluster_autoscaler.arn
}

output "cluster_autoscaler_role_name" {
  description = "Name of the Cluster Autoscaler IAM role"
  value       = aws_iam_role.cluster_autoscaler.name
}

output "cluster_autoscaler_policy_arn" {
  description = "ARN of the Cluster Autoscaler IAM policy"
  value       = aws_iam_policy.cluster_autoscaler.arn
}

# Application outputs
output "application_role_arn" {
  description = "ARN of the application IAM role"
  value       = aws_iam_role.application.arn
}

output "application_role_name" {
  description = "Name of the application IAM role"
  value       = aws_iam_role.application.name
}

output "application_policy_arn" {
  description = "ARN of the application IAM policy"
  value       = aws_iam_policy.application.arn
}

# Service Account annotations for Helm charts
output "service_account_annotations" {
  description = "Annotations for Kubernetes service accounts to use IRSA"
  value = {
    aws_load_balancer_controller = {
      "eks.amazonaws.com/role-arn" = aws_iam_role.aws_load_balancer_controller.arn
    }
    external_secrets = {
      "eks.amazonaws.com/role-arn" = aws_iam_role.external_secrets.arn
    }
    cluster_autoscaler = {
      "eks.amazonaws.com/role-arn" = aws_iam_role.cluster_autoscaler.arn
    }
    application = {
      "eks.amazonaws.com/role-arn" = aws_iam_role.application.arn
    }
  }
}