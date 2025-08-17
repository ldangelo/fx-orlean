terraform {
  required_version = ">= 1.6.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

# Configure AWS Provider
provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Environment = var.environment
      Project     = "fx-orleans"
      ManagedBy   = "terraform"
      CreatedBy   = "fx-orleans-infrastructure"
    }
  }
}

# Data sources
data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_caller_identity" "current" {}

locals {
  azs = slice(data.aws_availability_zones.available.names, 0, 3)
}

# VPC Module
module "vpc" {
  source = "../../modules/vpc"

  aws_region   = var.aws_region
  cluster_name = var.cluster_name
  environment  = var.environment
  vpc_cidr     = var.vpc_cidr
  azs          = local.azs
}

# Security Groups Module
module "security_groups" {
  source = "../../modules/security-groups"

  cluster_name              = var.cluster_name
  environment              = var.environment
  vpc_id                   = module.vpc.vpc_id
  private_subnet_ids       = module.vpc.private_subnet_ids
  public_subnet_ids        = module.vpc.public_subnet_ids
  private_subnet_cidrs     = module.vpc.private_subnet_cidrs
  public_subnet_cidrs      = module.vpc.public_subnet_cidrs
  cluster_security_group_id = module.eks.cluster_security_group_id
  
  # Development specific settings
  enable_ssh_access        = var.enable_ssh_access
  ssh_allowed_cidr_blocks  = var.ssh_allowed_cidr_blocks
  enable_rds_admin_access  = var.enable_rds_admin_access
  rds_admin_cidr_blocks    = var.rds_admin_cidr_blocks
}

# EKS Module
module "eks" {
  source = "../../modules/eks"

  cluster_name                        = var.cluster_name
  environment                        = var.environment
  vpc_id                             = module.vpc.vpc_id
  private_subnet_ids                 = module.vpc.private_subnet_ids
  public_subnet_ids                  = module.vpc.public_subnet_ids
  kubernetes_version                 = var.kubernetes_version
  
  # Node group configuration
  node_group_desired_size            = var.node_group_desired_size
  node_group_max_size               = var.node_group_max_size
  node_group_min_size               = var.node_group_min_size
  instance_types                     = var.instance_types
  capacity_type                      = var.capacity_type
  disk_size                          = var.disk_size
  
  # Development specific settings
  cluster_endpoint_public_access_cidrs = var.cluster_endpoint_public_access_cidrs
  ec2_ssh_key                         = var.ec2_ssh_key
}

# IRSA Module
module "irsa" {
  source = "../../modules/irsa"

  cluster_name           = var.cluster_name
  environment           = var.environment
  aws_region            = var.aws_region
  account_id            = data.aws_caller_identity.current.account_id
  cluster_oidc_issuer_url = module.eks.cluster_oidc_issuer_url
  secrets_manager_arns   = values(module.secrets.secret_arns)
  rds_cluster_arn       = module.rds.db_instance_arn
}

# RDS Module
module "rds" {
  source = "../../modules/rds"

  cluster_name        = var.cluster_name
  environment        = var.environment
  private_subnet_ids = module.vpc.private_subnet_ids
  security_group_ids = [module.security_groups.rds_security_group_id]
  
  # Database configuration
  db_name             = var.db_name
  db_username         = var.db_username
  db_password         = var.db_password
  instance_class      = var.rds_instance_class
  allocated_storage   = var.rds_allocated_storage
  max_allocated_storage = var.rds_max_allocated_storage
  
  # Development specific settings
  multi_az                    = var.rds_multi_az
  backup_retention_period     = var.rds_backup_retention_period
  deletion_protection         = var.rds_deletion_protection
  skip_final_snapshot        = var.rds_skip_final_snapshot
  create_read_replica        = var.rds_create_read_replica
  monitoring_interval        = var.rds_monitoring_interval
  performance_insights_enabled = var.rds_performance_insights_enabled
}

# Secrets Manager Module
module "secrets" {
  source = "../../modules/secrets"

  cluster_name = var.cluster_name
  environment  = var.environment
  
  # IAM roles that need access to secrets
  external_secrets_role_arns = [module.irsa.external_secrets_role_arn]
  application_role_arns      = [module.irsa.application_role_arn]
  
  # Database secrets
  database_username = var.db_username
  database_password = var.db_password
  database_host     = module.rds.db_endpoint
  database_port     = module.rds.db_port
  database_name     = var.db_name
  
  # Application secrets
  openai_api_key           = var.openai_api_key
  stripe_secret_key        = var.stripe_secret_key
  stripe_publishable_key   = var.stripe_publishable_key
  stripe_webhook_secret    = var.stripe_webhook_secret
  google_client_id         = var.google_client_id
  google_client_secret     = var.google_client_secret
  google_project_id        = var.google_project_id
  keycloak_admin_username  = var.keycloak_admin_username
  keycloak_admin_password  = var.keycloak_admin_password
  keycloak_client_secret   = var.keycloak_client_secret
  jwt_signing_key          = var.jwt_signing_key
  jwt_issuer              = var.jwt_issuer
  jwt_audience            = var.jwt_audience
  app_encryption_key      = var.app_encryption_key
  session_secret          = var.session_secret
  cors_origins            = var.cors_origins
  rate_limit_secret       = var.rate_limit_secret
}