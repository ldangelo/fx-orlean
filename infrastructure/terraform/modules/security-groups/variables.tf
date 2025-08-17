variable "cluster_name" {
  description = "Name of the EKS cluster"
  type        = string
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "vpc_id" {
  description = "VPC ID where security groups will be created"
  type        = string
}

variable "private_subnet_ids" {
  description = "List of private subnet IDs"
  type        = list(string)
}

variable "public_subnet_ids" {
  description = "List of public subnet IDs"
  type        = list(string)
}

variable "private_subnet_cidrs" {
  description = "List of private subnet CIDR blocks"
  type        = list(string)
}

variable "public_subnet_cidrs" {
  description = "List of public subnet CIDR blocks"
  type        = list(string)
}

variable "cluster_security_group_id" {
  description = "Security group ID of the EKS cluster"
  type        = string
}

variable "allowed_cidr_blocks" {
  description = "List of CIDR blocks allowed to access the ALB"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}

variable "enable_ssh_access" {
  description = "Enable SSH access to worker nodes"
  type        = bool
  default     = false
}

variable "ssh_allowed_cidr_blocks" {
  description = "List of CIDR blocks allowed for SSH access"
  type        = list(string)
  default     = []
}

variable "enable_rds_admin_access" {
  description = "Enable direct RDS access for database administration"
  type        = bool
  default     = false
}

variable "rds_admin_cidr_blocks" {
  description = "List of CIDR blocks allowed for RDS administration"
  type        = list(string)
  default     = []
}

variable "enable_redis" {
  description = "Enable Redis ElastiCache security group"
  type        = bool
  default     = false
}

variable "enable_strict_nacls" {
  description = "Enable strict Network ACLs for additional security"
  type        = bool
  default     = true
}

variable "custom_security_group_rules" {
  description = "Custom security group rules to add"
  type = list(object({
    type                     = string
    from_port               = number
    to_port                 = number
    protocol                = string
    cidr_blocks             = optional(list(string))
    source_security_group_id = optional(string)
    description             = string
    target_sg               = string # Which security group to add the rule to
  }))
  default = []
}