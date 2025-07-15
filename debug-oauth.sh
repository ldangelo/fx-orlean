#!/bin/bash

echo "=== FX-Orleans OAuth Troubleshooting ==="
echo ""

echo "1. Checking Docker services..."
docker-compose ps | grep -E "(keycloak|postgres)"
echo ""

echo "2. Checking Keycloak availability..."
echo "   Admin console: http://localhost:8085/admin (admin/itsasecret)"
echo "   Health check: $(curl -s http://localhost:8085/health 2>/dev/null || echo 'Not accessible')"
echo "   Realm endpoint: $(curl -s http://localhost:8085/realms/fx-expert/.well-known/openid_configuration 2>/dev/null | head -c 50 || echo 'Not accessible')"
echo ""

echo "3. Checking Google OAuth redirect URIs..."
echo "Current Keycloak redirectUris: https://localhost:8501/signin-oidc"
echo "Current Blazor CallbackPath: /signin-oidc"
echo ""

echo "4. Checking Google OAuth configuration..."
echo "Google Client ID in Keycloak: ${GOOGLE_CLIENT_ID}"
echo "Google Client ID in EventServer: ${GOOGLE_CLIENT_ID}"
echo ""

echo "5. Testing endpoints..."
echo "Keycloak realm: http://localhost:8085/realms/fx-expert"
echo "Expected Blazor app: https://localhost:8501"
echo ""

echo "6. Common issues to check:"
echo "   - Ensure Docker services are running: docker-compose up -d"
echo "   - Check if Blazor app is running on HTTPS port 8501"
echo "   - Verify Google OAuth app has correct redirect URI"
echo "   - Check Keycloak admin console: http://localhost:8085/admin"
echo ""

echo "=== Next Steps ==="
echo "1. Start services: docker-compose up -d"
echo "2. Run Blazor app: dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/"
echo "3. Test authentication at: https://localhost:8501"
echo "4. Check Keycloak admin: http://localhost:8085/admin (admin/itsasecret)"
