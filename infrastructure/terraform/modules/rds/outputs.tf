output "db_instance_id" {
  description = "RDS instance ID"
  value       = aws_db_instance.main.id
}

output "db_instance_arn" {
  description = "RDS instance ARN"
  value       = aws_db_instance.main.arn
}

output "db_endpoint" {
  description = "RDS instance endpoint"
  value       = aws_db_instance.main.endpoint
}

output "db_port" {
  description = "RDS instance port"
  value       = aws_db_instance.main.port
}

output "db_name" {
  description = "Database name"
  value       = aws_db_instance.main.db_name
}

output "db_username" {
  description = "Database username"
  value       = aws_db_instance.main.username
}

output "db_hosted_zone_id" {
  description = "RDS instance hosted zone ID"
  value       = aws_db_instance.main.hosted_zone_id
}

output "db_resource_id" {
  description = "RDS instance resource ID"
  value       = aws_db_instance.main.resource_id
}

output "db_status" {
  description = "RDS instance status"
  value       = aws_db_instance.main.status
}

output "db_availability_zone" {
  description = "RDS instance availability zone"
  value       = aws_db_instance.main.availability_zone
}

output "db_multi_az" {
  description = "RDS instance multi-AZ deployment status"
  value       = aws_db_instance.main.multi_az
}

# Subnet Group
output "db_subnet_group_id" {
  description = "DB subnet group ID"
  value       = aws_db_subnet_group.main.id
}

output "db_subnet_group_arn" {
  description = "DB subnet group ARN"
  value       = aws_db_subnet_group.main.arn
}

# Parameter Group
output "db_parameter_group_id" {
  description = "DB parameter group ID"
  value       = aws_db_parameter_group.main.id
}

output "db_parameter_group_arn" {
  description = "DB parameter group ARN"
  value       = aws_db_parameter_group.main.arn
}

# KMS
output "kms_key_id" {
  description = "KMS key ID for RDS encryption"
  value       = aws_kms_key.rds.key_id
}

output "kms_key_arn" {
  description = "KMS key ARN for RDS encryption"
  value       = aws_kms_key.rds.arn
}

output "kms_alias_name" {
  description = "KMS key alias name"
  value       = aws_kms_alias.rds.name
}

# Read Replica
output "read_replica_id" {
  description = "Read replica instance ID"
  value       = var.create_read_replica ? aws_db_instance.read_replica[0].id : null
}

output "read_replica_arn" {
  description = "Read replica instance ARN"
  value       = var.create_read_replica ? aws_db_instance.read_replica[0].arn : null
}

output "read_replica_endpoint" {
  description = "Read replica endpoint"
  value       = var.create_read_replica ? aws_db_instance.read_replica[0].endpoint : null
}

# Monitoring
output "enhanced_monitoring_role_arn" {
  description = "Enhanced monitoring IAM role ARN"
  value       = var.monitoring_interval > 0 ? aws_iam_role.rds_enhanced_monitoring[0].arn : null
}

# CloudWatch Log Groups
output "cloudwatch_log_group_arns" {
  description = "CloudWatch log group ARNs"
  value       = { for key, lg in aws_cloudwatch_log_group.postgresql : key => lg.arn }
}

output "cloudwatch_log_group_names" {
  description = "CloudWatch log group names"
  value       = { for key, lg in aws_cloudwatch_log_group.postgresql : key => lg.name }
}

# CloudWatch Alarms
output "cloudwatch_alarm_arns" {
  description = "CloudWatch alarm ARNs"
  value = {
    cpu_utilization      = aws_cloudwatch_metric_alarm.cpu_utilization.arn
    database_connections = aws_cloudwatch_metric_alarm.database_connections.arn
    free_storage_space   = aws_cloudwatch_metric_alarm.free_storage_space.arn
  }
}

# Connection Information for Applications
output "connection_info" {
  description = "Database connection information for applications"
  value = {
    host     = aws_db_instance.main.endpoint
    port     = aws_db_instance.main.port
    database = aws_db_instance.main.db_name
    username = aws_db_instance.main.username
    # Note: Password should be retrieved from AWS Secrets Manager
  }
  sensitive = false
}

# Database Connection String Template
output "connection_string_template" {
  description = "Database connection string template"
  value       = "postgresql://${aws_db_instance.main.username}:<PASSWORD>@${aws_db_instance.main.endpoint}:${aws_db_instance.main.port}/${aws_db_instance.main.db_name}"
  sensitive   = false
}