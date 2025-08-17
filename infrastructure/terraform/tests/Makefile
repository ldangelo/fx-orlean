.PHONY: test test-vpc test-eks test-rds test-secrets test-integration clean

# Default test target
test: test-vpc test-eks test-rds test-secrets

# Individual module tests
test-vpc:
	@echo "Testing VPC module..."
	go test -v -timeout 30m -run TestVPCModule

test-eks:
	@echo "Testing EKS module..."
	go test -v -timeout 45m -run TestEKSModule

test-rds:
	@echo "Testing RDS module..."
	go test -v -timeout 30m -run TestRDSModule

test-secrets:
	@echo "Testing Secrets Manager module..."
	go test -v -timeout 15m -run TestSecretsManagerModule

# Integration test (tests complete infrastructure)
test-integration:
	@echo "Running integration tests..."
	go test -v -timeout 60m -run TestCompleteInfrastructure

# Run all tests including integration
test-all: test test-integration

# Format Go code
fmt:
	go fmt ./...

# Clean up test artifacts
clean:
	go clean -testcache
	rm -rf .terraform*
	rm -f terraform.tfstate*

# Install dependencies
deps:
	go mod download
	go mod tidy

# Validate Terraform syntax (requires terraform binary)
validate:
	@echo "Validating Terraform syntax..."
	@find ../modules -name "*.tf" -type f | xargs -I {} dirname {} | sort -u | xargs -I {} sh -c 'cd {} && terraform fmt -check && terraform validate'
	@find ../environments -name "*.tf" -type f | xargs -I {} dirname {} | sort -u | xargs -I {} sh -c 'cd {} && terraform fmt -check && terraform validate'

# Help target
help:
	@echo "Available targets:"
	@echo "  test          - Run all module tests"
	@echo "  test-vpc      - Test VPC module only"
	@echo "  test-eks      - Test EKS module only"
	@echo "  test-rds      - Test RDS module only"
	@echo "  test-secrets  - Test Secrets Manager module only"
	@echo "  test-integration - Run integration tests"
	@echo "  test-all      - Run all tests including integration"
	@echo "  validate      - Validate Terraform syntax"
	@echo "  fmt           - Format Go code"
	@echo "  clean         - Clean up test artifacts"
	@echo "  deps          - Install dependencies"
	@echo "  help          - Show this help message"