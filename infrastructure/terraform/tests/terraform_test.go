package test

import (
	"fmt"
	"testing"

	"github.com/gruntwork-io/terratest/modules/aws"
	"github.com/gruntwork-io/terratest/modules/terraform"
	"github.com/stretchr/testify/assert"
)

func TestVPCModule(t *testing.T) {
	t.Parallel()

	// Random AWS region for testing
	awsRegion := aws.GetRandomStableRegion(t, nil, nil)

	terraformOptions := terraform.WithDefaultRetryableErrors(t, &terraform.Options{
		TerraformDir: "../modules/vpc",
		Vars: map[string]interface{}{
			"aws_region":     awsRegion,
			"cluster_name":   "fx-orleans-test",
			"environment":    "test",
			"vpc_cidr":       "10.0.0.0/16",
			"azs":            []string{awsRegion + "a", awsRegion + "b", awsRegion + "c"},
		},
	})

	defer terraform.Destroy(t, terraformOptions)
	terraform.InitAndApply(t, terraformOptions)

	// Validate VPC creation
	vpcId := terraform.Output(t, terraformOptions, "vpc_id")
	assert.NotEmpty(t, vpcId)

	// Validate subnets
	privateSubnets := terraform.OutputList(t, terraformOptions, "private_subnet_ids")
	publicSubnets := terraform.OutputList(t, terraformOptions, "public_subnet_ids")
	assert.Len(t, privateSubnets, 3)
	assert.Len(t, publicSubnets, 3)
}

func TestEKSModule(t *testing.T) {
	t.Parallel()

	awsRegion := aws.GetRandomStableRegion(t, nil, nil)

	terraformOptions := terraform.WithDefaultRetryableErrors(t, &terraform.Options{
		TerraformDir: "../modules/eks",
		Vars: map[string]interface{}{
			"aws_region":           awsRegion,
			"cluster_name":         "fx-orleans-test",
			"environment":          "test",
			"vpc_id":              "vpc-12345678", // Mock VPC ID for testing
			"private_subnet_ids":   []string{"subnet-1", "subnet-2", "subnet-3"},
			"node_group_min_size":  1,
			"node_group_max_size":  3,
			"node_group_desired_size": 2,
			"instance_types":       []string{"t3.medium"},
		},
	})

	defer terraform.Destroy(t, terraformOptions)
	terraform.InitAndApply(t, terraformOptions)

	// Validate EKS cluster creation
	clusterEndpoint := terraform.Output(t, terraformOptions, "cluster_endpoint")
	clusterArn := terraform.Output(t, terraformOptions, "cluster_arn")
	assert.NotEmpty(t, clusterEndpoint)
	assert.NotEmpty(t, clusterArn)
}

func TestRDSModule(t *testing.T) {
	t.Parallel()

	awsRegion := aws.GetRandomStableRegion(t, nil, nil)

	terraformOptions := terraform.WithDefaultRetryableErrors(t, &terraform.Options{
		TerraformDir: "../modules/rds",
		Vars: map[string]interface{}{
			"aws_region":         awsRegion,
			"environment":        "test",
			"vpc_id":            "vpc-12345678",
			"private_subnet_ids": []string{"subnet-1", "subnet-2"},
			"db_name":           "fxorleans",
			"db_username":       "fxadmin",
			"db_password":       "testPassword123!",
			"allocated_storage":  20,
			"instance_class":     "db.t3.micro",
		},
	})

	defer terraform.Destroy(t, terraformOptions)
	terraform.InitAndApply(t, terraformOptions)

	// Validate RDS instance creation
	dbEndpoint := terraform.Output(t, terraformOptions, "db_endpoint")
	dbPort := terraform.Output(t, terraformOptions, "db_port")
	assert.NotEmpty(t, dbEndpoint)
	assert.Equal(t, "5432", dbPort)
}

func TestSecretsManagerModule(t *testing.T) {
	t.Parallel()

	awsRegion := aws.GetRandomStableRegion(t, nil, nil)

	terraformOptions := terraform.WithDefaultRetryableErrors(t, &terraform.Options{
		TerraformDir: "../modules/secrets",
		Vars: map[string]interface{}{
			"aws_region":  awsRegion,
			"environment": "test",
			"cluster_name": "fx-orleans-test",
			"secrets": map[string]string{
				"database_password": "testPassword123!",
				"stripe_secret":     "sk_test_1234567890",
				"openai_api_key":    "sk-test-abcdef",
			},
		},
	})

	defer terraform.Destroy(t, terraformOptions)
	terraform.InitAndApply(t, terraformOptions)

	// Validate secrets creation
	secretsArns := terraform.OutputMap(t, terraformOptions, "secret_arns")
	assert.Contains(t, secretsArns, "database_password")
	assert.Contains(t, secretsArns, "stripe_secret")
	assert.Contains(t, secretsArns, "openai_api_key")
}

func TestCompleteInfrastructure(t *testing.T) {
	t.Parallel()

	awsRegion := aws.GetRandomStableRegion(t, nil, nil)

	terraformOptions := terraform.WithDefaultRetryableErrors(t, &terraform.Options{
		TerraformDir: "../environments/test",
		Vars: map[string]interface{}{
			"aws_region":   awsRegion,
			"environment":  "test",
			"cluster_name": "fx-orleans-test",
		},
	})

	defer terraform.Destroy(t, terraformOptions)
	terraform.InitAndApply(t, terraformOptions)

	// Validate complete infrastructure
	vpcId := terraform.Output(t, terraformOptions, "vpc_id")
	clusterEndpoint := terraform.Output(t, terraformOptions, "cluster_endpoint")
	dbEndpoint := terraform.Output(t, terraformOptions, "db_endpoint")

	assert.NotEmpty(t, vpcId)
	assert.NotEmpty(t, clusterEndpoint)
	assert.NotEmpty(t, dbEndpoint)

	fmt.Printf("VPC ID: %s\n", vpcId)
	fmt.Printf("EKS Cluster Endpoint: %s\n", clusterEndpoint)
	fmt.Printf("RDS Endpoint: %s\n", dbEndpoint)
}