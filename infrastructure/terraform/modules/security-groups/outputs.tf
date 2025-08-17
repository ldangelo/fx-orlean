output "alb_security_group_id" {
  description = "ID of the Application Load Balancer security group"
  value       = aws_security_group.alb.id
}

output "alb_security_group_arn" {
  description = "ARN of the Application Load Balancer security group"
  value       = aws_security_group.alb.arn
}

output "worker_nodes_security_group_id" {
  description = "ID of the worker nodes security group"
  value       = aws_security_group.worker_nodes.id
}

output "worker_nodes_security_group_arn" {
  description = "ARN of the worker nodes security group"
  value       = aws_security_group.worker_nodes.arn
}

output "rds_security_group_id" {
  description = "ID of the RDS security group"
  value       = aws_security_group.rds.id
}

output "rds_security_group_arn" {
  description = "ARN of the RDS security group"
  value       = aws_security_group.rds.arn
}

output "redis_security_group_id" {
  description = "ID of the Redis security group"
  value       = var.enable_redis ? aws_security_group.redis[0].id : null
}

output "redis_security_group_arn" {
  description = "ARN of the Redis security group"
  value       = var.enable_redis ? aws_security_group.redis[0].arn : null
}

output "vpc_endpoints_security_group_id" {
  description = "ID of the VPC endpoints security group"
  value       = aws_security_group.vpc_endpoints.id
}

output "vpc_endpoints_security_group_arn" {
  description = "ARN of the VPC endpoints security group"
  value       = aws_security_group.vpc_endpoints.arn
}

output "private_network_acl_id" {
  description = "ID of the private subnet network ACL"
  value       = aws_network_acl.private.id
}

output "public_network_acl_id" {
  description = "ID of the public subnet network ACL"
  value       = aws_network_acl.public.id
}

# Security group mappings for easy reference
output "security_group_ids" {
  description = "Map of security group names to IDs"
  value = {
    alb           = aws_security_group.alb.id
    worker_nodes  = aws_security_group.worker_nodes.id
    rds          = aws_security_group.rds.id
    redis        = var.enable_redis ? aws_security_group.redis[0].id : null
    vpc_endpoints = aws_security_group.vpc_endpoints.id
  }
}

output "security_group_arns" {
  description = "Map of security group names to ARNs"
  value = {
    alb           = aws_security_group.alb.arn
    worker_nodes  = aws_security_group.worker_nodes.arn
    rds          = aws_security_group.rds.arn
    redis        = var.enable_redis ? aws_security_group.redis[0].arn : null
    vpc_endpoints = aws_security_group.vpc_endpoints.arn
  }
}

# Network ACL mappings
output "network_acl_ids" {
  description = "Map of network ACL names to IDs"
  value = {
    private = aws_network_acl.private.id
    public  = aws_network_acl.public.id
  }
}