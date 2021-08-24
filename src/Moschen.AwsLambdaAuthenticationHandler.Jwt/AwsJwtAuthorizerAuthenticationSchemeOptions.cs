using Microsoft.AspNetCore.Authentication;

namespace Moschen.AwsLambdaAuthenticationHandler.Jwt
{
    public class AwsJwtAuthorizerAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// When authenticating the request, require the presence of the Bearer token.
        /// </summary>
        public bool RequireToken { get; set; } = true;

        /// <summary>
        /// For local/dev run (without AWS context), when the claims are not already populated by lambda entry point, extract the claims from JWT.
        /// </summary>
        public bool ExtractClaimsFromToken { get; set; }
    }
}
