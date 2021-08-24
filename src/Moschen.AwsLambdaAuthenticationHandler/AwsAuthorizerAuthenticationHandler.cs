using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Moschen.AwsLambdaAuthenticationHandler
{
    public class AwsAuthorizerAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions> 
        where TOptions : AuthenticationSchemeOptions, new()
    {
        public AwsAuthorizerAuthenticationHandler(
            IOptionsMonitor<TOptions> options, 
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
                if (Context.User.Claims?.Any() != true)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Couldn't find the user authenticated by API Gateway"));
                }
                    
                Logger.LogDebug("Found user already authenticated by API Gateway Authorizer");
                var claimsIdentity = new ClaimsIdentity(Context.User.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(claimsIdentity);
                return Task.FromResult(
                    AuthenticateResult.Success(
                        new AuthenticationTicket(principal, null, AwsAuthorizerDefaults.AuthenticationScheme)));
            }
            catch (Exception exception)
            {
                return Task.FromResult(AuthenticateResult.Fail(exception));
            }
        }
    }
}
