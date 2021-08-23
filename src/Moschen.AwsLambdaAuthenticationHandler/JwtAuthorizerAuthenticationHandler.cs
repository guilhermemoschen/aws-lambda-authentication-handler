using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Moschen.AwsLambdaAuthenticationHandler
{
    public class JwtAuthorizerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string MissingAuthorizationHeaderMessage = "Invalid authorization header";
        private const string JwtBearerKey = "Bearer";

        public JwtAuthorizerAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                if (Context.User.Claims?.Any() == true)
                {
                    Logger.LogDebug("It's already authenticated by API Gateway");
                    return Task.FromResult(
                        AuthenticateResult.Success(
                            new AuthenticationTicket(Context.User, null, JwtAuthorizerDefaults.AuthenticationScheme)));
                }

                Logger.LogInformation("Extracting claims from token for local test");
                var token = GetToken();
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Task.FromResult(AuthenticateResult.Fail(MissingAuthorizationHeaderMessage));
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                var claimsIdentity = new ClaimsIdentity(jwtSecurityToken.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(claimsIdentity);

                Logger.LogInformation("Request is authenticated with '{user}'", principal.Identity?.Name);

                return Task.FromResult(
                    AuthenticateResult.Success(
                        new AuthenticationTicket(principal, null, JwtAuthorizerDefaults.AuthenticationScheme)));
            }
            catch (Exception exception)
            {
                return Task.FromResult(AuthenticateResult.Fail(exception));
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

            var bearer = authorizationHeader.FirstOrDefault(h => h.StartsWith(JwtBearerKey));
            if (bearer == null)
            {
                return null;
            }

            var token = bearer
                .Replace(JwtBearerKey, string.Empty)
                .Trim();

            return string.IsNullOrWhiteSpace(token) 
                ? null 
                : token;
        }
    }
}
