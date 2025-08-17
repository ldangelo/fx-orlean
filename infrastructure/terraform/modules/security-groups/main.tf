terraform {
  required_version = ">= 1.6.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# Security Group for Application Load Balancer
resource "aws_security_group" "alb" {
  name        = "${var.cluster_name}-alb-sg"
  description = "Security group for Application Load Balancer"
  vpc_id      = var.vpc_id

  # HTTP access
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = var.allowed_cidr_blocks
    description = "HTTP access"
  }

  # HTTPS access
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = var.allowed_cidr_blocks
    description = "HTTPS access"
  }

  # Outbound traffic
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "All outbound traffic"
  }

  tags = {
    Name        = "${var.cluster_name}-alb-sg"
    Environment = var.environment
    Type        = "application-load-balancer"
  }
}

# Security Group for EKS Worker Nodes
resource "aws_security_group" "worker_nodes" {
  name        = "${var.cluster_name}-worker-nodes-sg"
  description = "Security group for EKS worker nodes"
  vpc_id      = var.vpc_id

  # Allow inbound traffic from ALB
  ingress {
    from_port       = 30000
    to_port         = 32767
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
    description     = "ALB to worker nodes"
  }

  # Allow communication between worker nodes
  ingress {
    from_port = 0
    to_port   = 65535
    protocol  = "tcp"
    self      = true
    description = "Worker node to worker node communication"
  }

  # Allow ICMP between worker nodes
  ingress {
    from_port = -1
    to_port   = -1
    protocol  = "icmp"
    self      = true
    description = "ICMP between worker nodes"
  }

  # SSH access for troubleshooting (if SSH key is provided)
  dynamic "ingress" {
    for_each = var.enable_ssh_access ? [1] : []
    content {
      from_port   = 22
      to_port     = 22
      protocol    = "tcp"
      cidr_blocks = var.ssh_allowed_cidr_blocks
      description = "SSH access for troubleshooting"
    }
  }

  # Outbound traffic
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "All outbound traffic"
  }

  tags = {
    Name        = "${var.cluster_name}-worker-nodes-sg"
    Environment = var.environment
    Type        = "worker-nodes"
  }
}

# Security Group for RDS Database
resource "aws_security_group" "rds" {
  name        = "${var.cluster_name}-rds-sg"
  description = "Security group for RDS PostgreSQL database"
  vpc_id      = var.vpc_id

  # PostgreSQL access from worker nodes
  ingress {
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.worker_nodes.id]
    description     = "PostgreSQL access from worker nodes"
  }

  # PostgreSQL access from EKS control plane (for migrations)
  ingress {
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [var.cluster_security_group_id]
    description     = "PostgreSQL access from EKS control plane"
  }

  # Optional: Direct access for database administration
  dynamic "ingress" {
    for_each = var.enable_rds_admin_access ? [1] : []
    content {
      from_port   = 5432
      to_port     = 5432
      protocol    = "tcp"
      cidr_blocks = var.rds_admin_cidr_blocks
      description = "PostgreSQL access for database administration"
    }
  }

  # Outbound traffic (for maintenance and updates)
  egress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "HTTPS for maintenance and updates"
  }

  tags = {
    Name        = "${var.cluster_name}-rds-sg"
    Environment = var.environment
    Type        = "database"
  }
}

# Security Group for Redis (ElastiCache) - Optional
resource "aws_security_group" "redis" {
  count       = var.enable_redis ? 1 : 0
  name        = "${var.cluster_name}-redis-sg"
  description = "Security group for Redis ElastiCache"
  vpc_id      = var.vpc_id

  # Redis access from worker nodes
  ingress {
    from_port       = 6379
    to_port         = 6379
    protocol        = "tcp"
    security_groups = [aws_security_group.worker_nodes.id]
    description     = "Redis access from worker nodes"
  }

  tags = {
    Name        = "${var.cluster_name}-redis-sg"
    Environment = var.environment
    Type        = "cache"
  }
}

# Security Group for VPC Endpoints
resource "aws_security_group" "vpc_endpoints" {
  name        = "${var.cluster_name}-vpc-endpoints-sg"
  description = "Security group for VPC endpoints"
  vpc_id      = var.vpc_id

  # HTTPS access from worker nodes
  ingress {
    from_port       = 443
    to_port         = 443
    protocol        = "tcp"
    security_groups = [aws_security_group.worker_nodes.id]
    description     = "HTTPS access from worker nodes"
  }

  # HTTPS access from EKS control plane
  ingress {
    from_port       = 443
    to_port         = 443
    protocol        = "tcp"
    security_groups = [var.cluster_security_group_id]
    description     = "HTTPS access from EKS control plane"
  }

  tags = {
    Name        = "${var.cluster_name}-vpc-endpoints-sg"
    Environment = var.environment
    Type        = "vpc-endpoints"
  }
}

# Network ACL for Private Subnets (Additional Security Layer)
resource "aws_network_acl" "private" {
  vpc_id     = var.vpc_id
  subnet_ids = var.private_subnet_ids

  # Allow inbound HTTP/HTTPS from public subnets
  dynamic "ingress" {
    for_each = var.public_subnet_cidrs
    content {
      rule_number = 100 + index(var.public_subnet_cidrs, ingress.value)
      protocol    = "tcp"
      from_port   = 80
      to_port     = 80
      cidr_block  = ingress.value
      action      = "allow"
    }
  }

  dynamic "ingress" {
    for_each = var.public_subnet_cidrs
    content {
      rule_number = 200 + index(var.public_subnet_cidrs, ingress.value)
      protocol    = "tcp"
      from_port   = 443
      to_port     = 443
      cidr_block  = ingress.value
      action      = "allow"
    }
  }

  # Allow inbound ephemeral ports for responses
  ingress {
    rule_number = 300
    protocol    = "tcp"
    from_port   = 1024
    to_port     = 65535
    cidr_block  = "0.0.0.0/0"
    action      = "allow"
  }

  # Allow inbound traffic between private subnets
  dynamic "ingress" {
    for_each = var.private_subnet_cidrs
    content {
      rule_number = 400 + index(var.private_subnet_cidrs, ingress.value)
      protocol    = "-1"
      from_port   = 0
      to_port     = 0
      cidr_block  = ingress.value
      action      = "allow"
    }
  }

  # Allow all outbound traffic
  egress {
    rule_number = 100
    protocol    = "-1"
    from_port   = 0
    to_port     = 0
    cidr_block  = "0.0.0.0/0"
    action      = "allow"
  }

  tags = {
    Name        = "${var.cluster_name}-private-nacl"
    Environment = var.environment
    Type        = "private-subnets"
  }
}

# Network ACL for Public Subnets
resource "aws_network_acl" "public" {
  vpc_id     = var.vpc_id
  subnet_ids = var.public_subnet_ids

  # Allow inbound HTTP from anywhere
  ingress {
    rule_number = 100
    protocol    = "tcp"
    from_port   = 80
    to_port     = 80
    cidr_block  = "0.0.0.0/0"
    action      = "allow"
  }

  # Allow inbound HTTPS from anywhere
  ingress {
    rule_number = 110
    protocol    = "tcp"
    from_port   = 443
    to_port     = 443
    cidr_block  = "0.0.0.0/0"
    action      = "allow"
  }

  # Allow inbound ephemeral ports for responses
  ingress {
    rule_number = 200
    protocol    = "tcp"
    from_port   = 1024
    to_port     = 65535
    cidr_block  = "0.0.0.0/0"
    action      = "allow"
  }

  # Allow inbound SSH if enabled
  dynamic "ingress" {
    for_each = var.enable_ssh_access ? [1] : []
    content {
      rule_number = 300
      protocol    = "tcp"
      from_port   = 22
      to_port     = 22
      cidr_block  = "0.0.0.0/0"
      action      = "allow"
    }
  }

  # Allow all outbound traffic
  egress {
    rule_number = 100
    protocol    = "-1"
    from_port   = 0
    to_port     = 0
    cidr_block  = "0.0.0.0/0"
    action      = "allow"
  }

  tags = {
    Name        = "${var.cluster_name}-public-nacl"
    Environment = var.environment
    Type        = "public-subnets"
  }
}