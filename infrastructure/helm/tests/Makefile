.PHONY: test test-eventserver test-blazor test-keycloak test-external-secrets test-lint clean deps help

# Default test target
test: test-eventserver test-blazor test-keycloak test-external-secrets

# Individual chart tests
test-eventserver:
	@echo "Testing EventServer Helm chart..."
	go test -v -timeout 30m -run TestEventServerHelmChart

test-blazor:
	@echo "Testing Blazor Frontend Helm chart..."
	go test -v -timeout 30m -run TestBlazorFrontendHelmChart

test-keycloak:
	@echo "Testing Keycloak Helm chart..."
	go test -v -timeout 30m -run TestKeycloakHelmChart

test-external-secrets:
	@echo "Testing External Secrets Operator Helm chart..."
	go test -v -timeout 30m -run TestExternalSecretsOperatorHelmChart

# Helm chart linting and validation
test-lint:
	@echo "Running Helm chart linting tests..."
	go test -v -timeout 15m -run TestHelmChartLinting

# Values and configuration tests
test-values:
	@echo "Testing Helm chart values..."
	go test -v -timeout 15m -run TestHelmChartValues

# Service account and IRSA tests
test-irsa:
	@echo "Testing IRSA service account annotations..."
	go test -v -timeout 15m -run TestServiceAccountAnnotations

# Resource configuration tests
test-resources:
	@echo "Testing resource limits and requests..."
	go test -v -timeout 15m -run TestResourceLimitsAndRequests

# Namespace consistency tests
test-namespace:
	@echo "Testing namespace consistency..."
	go test -v -timeout 15m -run TestNamespaceConsistency

# Dependency tests
test-deps:
	@echo "Testing Helm chart dependencies..."
	go test -v -timeout 15m -run TestHelmChartDependencies

# Run all tests including validation
test-all: test test-lint test-values test-irsa test-resources test-namespace test-deps

# Format Go code
fmt:
	go fmt ./...

# Clean up test artifacts
clean:
	go clean -testcache
	rm -rf test-output/

# Install dependencies
deps:
	go mod download
	go mod tidy

# Lint Helm charts (requires helm binary)
helm-lint:
	@echo "Linting Helm charts..."
	@for chart in ../charts/*/; do \
		if [ -f "$$chart/Chart.yaml" ]; then \
			echo "Linting $$chart"; \
			helm lint "$$chart" || exit 1; \
		fi \
	done

# Template Helm charts (requires helm binary)
helm-template:
	@echo "Templating Helm charts..."
	@for chart in ../charts/*/; do \
		if [ -f "$$chart/Chart.yaml" ]; then \
			chart_name=$$(basename "$$chart"); \
			echo "Templating $$chart"; \
			helm template "$$chart_name" "$$chart" --dry-run > /dev/null || exit 1; \
		fi \
	done

# Validate Helm charts (requires helm binary)
helm-validate: helm-lint helm-template

# Package Helm charts
helm-package:
	@echo "Packaging Helm charts..."
	@mkdir -p ../packages
	@for chart in ../charts/*/; do \
		if [ -f "$$chart/Chart.yaml" ]; then \
			echo "Packaging $$chart"; \
			helm package "$$chart" --destination ../packages/; \
		fi \
	done

# Help target
help:
	@echo "Available targets:"
	@echo "  test              - Run all chart tests"
	@echo "  test-eventserver  - Test EventServer chart only"
	@echo "  test-blazor       - Test Blazor Frontend chart only"
	@echo "  test-keycloak     - Test Keycloak chart only"
	@echo "  test-external-secrets - Test External Secrets chart only"
	@echo "  test-lint         - Run chart linting tests"
	@echo "  test-values       - Test chart values"
	@echo "  test-irsa         - Test IRSA annotations"
	@echo "  test-resources    - Test resource limits"
	@echo "  test-namespace    - Test namespace consistency"
	@echo "  test-deps         - Test chart dependencies"
	@echo "  test-all          - Run all tests including validation"
	@echo "  helm-lint         - Lint charts with helm binary"
	@echo "  helm-template     - Template charts with helm binary"
	@echo "  helm-validate     - Validate charts with helm binary"
	@echo "  helm-package      - Package charts"
	@echo "  fmt               - Format Go code"
	@echo "  clean             - Clean up test artifacts"
	@echo "  deps              - Install dependencies"
	@echo "  help              - Show this help message"