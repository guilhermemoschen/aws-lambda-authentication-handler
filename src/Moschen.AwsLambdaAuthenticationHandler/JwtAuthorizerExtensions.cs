using System;
using Microsoft.AspNetCore.Authentication;
using Moschen.AwsLambdaAuthenticationHandler;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class JwtAuthorizerExtensions
    {
        public static AuthenticationBuilder AddJwtAuthorizer(this AuthenticationBuilder builder)
        {
            return AddJwtAuthorizerInternal(builder, JwtAuthorizerDefaults.AuthenticationScheme);
        }

        public static AuthenticationBuilder AddJwtAuthorizer(this AuthenticationBuilder builder, string authenticationScheme)
        {
            return AddJwtAuthorizerInternal(builder, authenticationScheme);
        }

        public static AuthenticationBuilder AddJwtAuthorizer(
            this AuthenticationBuilder builder, 
            Action<AuthenticationSchemeOptions> configureOptions)
        {
            return AddJwtAuthorizerInternal(builder, JwtAuthorizerDefaults.AuthenticationScheme, configureOptions);
        }

        public static AuthenticationBuilder AddJwtAuthorizer(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            Action<AuthenticationSchemeOptions> configureOptions)
        {
            return AddJwtAuthorizerInternal(builder, authenticationScheme, configureOptions);
        }

        private static AuthenticationBuilder AddJwtAuthorizerInternal(
            this AuthenticationBuilder builder, 
            string? authenticationScheme = null,
            Action<AuthenticationSchemeOptions>? configureOptions = null)
        {
            return builder
                .AddScheme<AuthenticationSchemeOptions, JwtAuthorizerAuthenticationHandler>(
                    authenticationScheme,
                    configureOptions);
        }
    }
}
