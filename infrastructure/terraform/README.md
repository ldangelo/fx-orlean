# AWS EKS Infrastructure for FX-Orleans

This directory contains Terraform modules and configurations for deploying FX-Orleans on AWS EKS with complete infrastructure including VPC, EKS cluster, RDS PostgreSQL, and AWS Secrets Manager.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        AWS VPC                              │
│  ┌─────────────────┐                 ┌─────────────────┐   │
│  │  Public Subnets │                 │ Private Subnets │   │
│  │                 │                 │                 │   │
│  │  ┌─────────────┐│                 │┌─────────────┐  │   │
│  │  │     ALB     ││                 ││   EKS Nodes │  │   │
│  │  └─────────────┘│                 │└─────────────┘  │   │
│  │  ┌─────────────┐│                 │┌─────────────┐  │   │
│  │  │  NAT GW     ││                 ││     RDS     │  │   │
│  │  └─────────────┘│                 │└─────────────┘  │   │
│  └─────────────────┘                 └─────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Components

### Core Infrastructure Modules

- **VPC Module** (`modules/vpc/`): VPC with public/private subnets across 3 AZs
- **EKS Module** (`modules/eks/`): EKS cluster with managed node groups
- **Security Groups** (`modules/security-groups/`): Security groups and NACLs
- **IRSA Module** (`modules/irsa/`): IAM Roles for Service Accounts
- **RDS Module** (`modules/rds/`): PostgreSQL with backups and monitoring
- **Secrets Module** (`modules/secrets/`): AWS Secrets Manager integration

### Environment Configurations

- **Development** (`environments/dev/`): Development environment configuration
- **Staging** (`environments/staging/`): Staging environment (template)
- **Production** (`environments/prod/`): Production environment (template)

### Testing Framework

- **Terratest** (`tests/`): Go-based infrastructure testing with Terratest

## Prerequisites

### Required Tools

```bash
# Terraform (>= 1.6.0)
terraform --version

# AWS CLI (>= 2.0)
aws --version

# kubectl (>= 1.28)
kubectl version --client

# Helm (>= 3.12)
helm version

# Go (>= 1.21) - for testing
go version
```

### AWS Configuration

```bash
# Configure AWS credentials
aws configure

# Or use environment variables
export AWS_ACCESS_KEY_ID=your-access-key
export AWS_SECRET_ACCESS_KEY=your-secret-key
export AWS_DEFAULT_REGION=us-west-2
```

## Quick Start

### 1. Configure Environment

```bash
cd environments/dev
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
```

### 2. Initialize and Deploy

```bash
# Initialize Terraform
terraform init

# Plan deployment
terraform plan

# Apply infrastructure
terraform apply
```

### 3. Configure kubectl

```bash
# Get cluster credentials
aws eks update-kubeconfig --region us-west-2 --name fx-orleans-dev

# Verify connection
kubectl get nodes
```

## Module Documentation

### VPC Module

Creates AWS VPC with:
- Public/private subnets across 3 AZs
- Internet Gateway and NAT Gateways
- Route tables and associations
- VPC Flow Logs
- Proper tagging for EKS integration

**Usage:**
```hcl
module "vpc" {
  source = "../modules/vpc"
  
  cluster_name = "fx-orleans-dev"
  environment  = "dev"
  vpc_cidr     = "10.0.0.0/16"
  azs          = ["us-west-2a", "us-west-2b", "us-west-2c"]
}
```

### EKS Module

Creates EKS cluster with:
- Managed node groups
- EKS add-ons (VPC CNI, CoreDNS, kube-proxy, EBS CSI)
- CloudWatch logging
- KMS encryption
- IRSA support

**Usage:**
```hcl
module "eks" {
  source = "../modules/eks"
  
  cluster_name       = "fx-orleans-dev"
  environment       = "dev"
  vpc_id            = module.vpc.vpc_id
  private_subnet_ids = module.vpc.private_subnet_ids
}
```

### RDS Module

Creates PostgreSQL RDS with:
- Multi-AZ deployment (configurable)
- Automated backups
- Performance Insights
- CloudWatch monitoring and alarms
- Read replica (optional)
- KMS encryption

**Usage:**
```hcl
module "rds" {
  source = "../modules/rds"
  
  cluster_name       = "fx-orleans-dev"
  environment       = "dev"
  private_subnet_ids = module.vpc.private_subnet_ids
  security_group_ids = [module.security_groups.rds_security_group_id]
  db_password       = var.db_password
}
```

### Secrets Manager Module

Creates AWS Secrets Manager secrets for:
- Database credentials
- OpenAI API keys
- Stripe payment keys
- Google Calendar API credentials
- Keycloak authentication
- JWT signing keys
- Application configuration

**Usage:**
```hcl
module "secrets" {
  source = "../modules/secrets"
  
  cluster_name           = "fx-orleans-dev"
  environment           = "dev"
  external_secrets_role_arns = [module.irsa.external_secrets_role_arn]
  database_password     = var.db_password
  openai_api_key       = var.openai_api_key
  # ... other secrets
}
```

### IRSA Module

Creates IAM roles for:
- AWS Load Balancer Controller
- External Secrets Operator
- Cluster Autoscaler
- Application pods
- OIDC identity provider

**Usage:**
```hcl
module "irsa" {
  source = "../modules/irsa"
  
  cluster_name           = "fx-orleans-dev"
  environment           = "dev"
  cluster_oidc_issuer_url = module.eks.cluster_oidc_issuer_url
  secrets_manager_arns   = values(module.secrets.secret_arns)
}
```

## Environment Configuration

### Development Environment

Optimized for development with:
- Single-AZ RDS (cost optimization)
- Smaller instance types
- SSH access enabled
- Direct RDS admin access
- Reduced backup retention

### Production Environment

Configured for production with:
- Multi-AZ RDS
- Larger instance types
- Enhanced monitoring
- Deletion protection
- Extended backup retention
- Strict security groups

## Testing

### Running Tests

```bash
cd tests

# Install dependencies
go mod download

# Run all tests
make test

# Run specific module tests
make test-vpc
make test-eks
make test-rds

# Run integration tests
make test-integration
```

### Test Coverage

- **VPC Module**: Network configuration and connectivity
- **EKS Module**: Cluster creation and node group functionality
- **RDS Module**: Database deployment and backup configuration
- **Secrets Module**: Secret creation and access policies
- **Integration**: Complete infrastructure deployment

## Security Considerations

### Network Security

- Private subnets for EKS nodes and RDS
- Security groups with least privilege
- Network ACLs for additional protection
- VPC Flow Logs enabled

### Access Control

- IRSA for pod-level AWS permissions
- AWS Secrets Manager for sensitive data
- KMS encryption for all encrypted resources
- Resource-based policies for fine-grained access

### Monitoring and Logging

- CloudWatch logs for EKS and RDS
- Performance Insights for database monitoring
- CloudWatch alarms for critical metrics
- VPC Flow Logs for network monitoring

## Troubleshooting

### Common Issues

1. **EKS Cluster Access**: Ensure your AWS user/role has proper EKS permissions
2. **Node Group Creation**: Check security groups and subnet configurations
3. **RDS Connectivity**: Verify security groups allow PostgreSQL traffic
4. **IRSA Issues**: Confirm OIDC provider and role trust policies

### Debugging Commands

```bash
# Check EKS cluster status
aws eks describe-cluster --name fx-orleans-dev

# View node group details
aws eks describe-nodegroup --cluster-name fx-orleans-dev --nodegroup-name fx-orleans-dev-node-group

# Check RDS instance
aws rds describe-db-instances --db-instance-identifier fx-orleans-dev-postgres

# List secrets
aws secretsmanager list-secrets --region us-west-2
```

## Cost Optimization

### Development Environment

- Use `t3.micro` for RDS
- Single-AZ deployment
- `t3.medium` worker nodes
- Spot instances (optional)

### Production Environment

- Right-size instances based on usage
- Use Reserved Instances for predictable workloads
- Enable Auto Scaling
- Monitor costs with AWS Cost Explorer

## Next Steps

After infrastructure deployment:

1. **Deploy Helm Charts**: Navigate to `../../helm/` directory
2. **Install External Secrets Operator**: For secret management
3. **Deploy AWS Load Balancer Controller**: For ingress management
4. **Deploy FX-Orleans Applications**: EventServer and Blazor frontend
5. **Configure Domain and TLS**: Set up custom domain with ACM

## Support

For infrastructure issues:
- Check Terraform logs and error messages
- Review AWS CloudFormation events (if using)
- Consult AWS documentation for service-specific issues
- Use terratest for infrastructure validation

## Contributing

When adding new modules or features:
1. Follow existing module structure
2. Add comprehensive tests
3. Update documentation
4. Test in development environment first
5. Follow Terraform best practices