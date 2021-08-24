using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Moschen.AwsLambdaAuthenticationHandler.Jwt
{
    public class AwsJwtAuthorizerAuthenticationHandler : AwsAuthorizerAuthenticationHandler<AwsJwtAuthorizerAuthenticationSchemeOptions>
    {
        public static string InvalidAuthenticationRequestMessage = "Invalid authentication request";

        public AwsJwtAuthorizerAuthenticationHandler(
            IOptionsMonitor<AwsJwtAuthorizerAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                string? token = null;

                if (Options.RequireToken)
                {
                    Logger.LogDebug("Validating Bearer token presence");
                    token = GetToken();
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return AuthenticateResult.Fail(InvalidAuthenticationRequestMessage);
                    }
                }

                var result = await base.HandleAuthenticateAsync();
                if (result.Succeeded)
                {
                    return AuthenticateResult.Success(new AuthenticationTicket(Context.User, null, AwsJwtAuthorizerDefaults.AuthenticationScheme));
                }

                if (string.IsNullOrWhiteSpace(token) || !Options.ExtractClaimsFromToken)
                {
                    return AuthenticateResult.Fail(InvalidAuthenticationRequestMessage);
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                var claimsIdentity = new ClaimsIdentity(jwtSecurityToken.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(claimsIdentity);

                return AuthenticateResult.Success(new AuthenticationTicket(principal, null, AwsJwtAuthorizerDefaults.AuthenticationScheme));
            }
            catch (Exception exception)
            {
                return AuthenticateResult.Fail(exception);
            }
        }

        private string? GetToken()
        {
            var authorizationHeader = Context.Request
                .Headers["Authorization"]
                .ToArray();
            if (authorizationHeader?.Any() != true)
            {
                return null;
            }

            const string AuthorizationBearerKey = "Bearer";

            var bearer = authorizationHeader.FirstOrDefault(h => h.StartsWith(AuthorizationBearerKey));
            if (bearer == null)
            {
                return null;
            }

            var token = bearer
                .Replace(AuthorizationBearerKey, string.Empty)
                .Trim();

            return string.IsNullOrWhiteSpace(token)
                ? null
                : token;
        }
    }
}
