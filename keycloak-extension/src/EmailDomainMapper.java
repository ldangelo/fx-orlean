import org.keycloak.models.*;
import org.keycloak.protocol.oidc.mappers.*;
import org.keycloak.provider.ProviderConfigProperty;
import org.keycloak.representations.IDToken;

import java.util.*;

public class EmailDomainRoleMapper extends AbstractOIDCProtocolMapper implements OIDCAccessTokenMapper {

    public static final String PROVIDER_ID = "oidc-email-domain-role-mapper";

    private static final List<ProviderConfigProperty> configProperties = new ArrayList<>();

    static {
        OIDCAttributeMapperHelper.addIncludeInTokensConfig(configProperties, EmailDomainRoleMapper.class);
    }

    @Override
    public String getId() {
        return PROVIDER_ID;
    }

    @Override
    public String getDisplayType() {
        return "Email Domain Role Mapper";
    }

    @Override
    public String getDisplayCategory() {
        return TOKEN_MAPPER_CATEGORY;
    }

    @Override
    public String getHelpText() {
        return "Assigns the 'PARTNER' role to users with an email domain of 'fortiumpartners.com'.";
    }

    @Override
    public List<ProviderConfigProperty> getConfigProperties() {
        return configProperties;
    }

    @Override
    protected void setClaim(IDToken token, ProtocolMapperModel mappingModel, UserSessionModel userSession) {
        // Get user model
        UserModel user = userSession.getUser();

        // Check if user email ends with 'fortiumpartners.com'
        if (user.getEmail() != null && user.getEmail().endsWith("@fortiumpartners.com")) {
            // Get 'PARTNER' role
            RoleModel partnerRole = userSession.getRealm().getRole("PARTNER");

            // If 'PARTNER' role exists, add to user roles
            if (partnerRole != null) {
                AccessToken.Access realmAccess = token.getRealmAccess();
                if (realmAccess == null) {
                    realmAccess = new AccessToken.Access();
                    token.setRealmAccess(realmAccess);
                }
                realmAccess.addRole(partnerRole.getName());
            }
        }
    }
}
