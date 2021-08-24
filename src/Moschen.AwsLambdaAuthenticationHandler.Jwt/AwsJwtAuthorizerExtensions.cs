using System;
using Microsoft.AspNetCore.Authentication;
using Moschen.AwsLambdaAuthenticationHandler.Jwt;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class AwsJwtAuthorizerExtensions
    {
        public static AuthenticationBuilder AddJwtAuthorizer(this AuthenticationBuilder builder)
        {
            return AddJwtAuthorizerInternal(builder, AwsJwtAuthorizerDefaults.AuthenticationScheme);
        }

        public static AuthenticationBuilder AddJwtAuthorizer(this AuthenticationBuilder builder, string authenticationScheme)
        {
            return AddJwtAuthorizerInternal(builder, authenticationScheme);
        }

        public static AuthenticationBuilder AddJwtAuthorizer(
            this AuthenticationBuilder builder, 
            Action<AwsJwtAuthorizerAuthenticationSchemeOptions> configureOptions)
        {
            return AddJwtAuthorizerInternal(builder, AwsJwtAuthorizerDefaults.AuthenticationScheme, configureOptions);
        }

        public static AuthenticationBuilder AddJwtAuthorizer(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            Action<AwsJwtAuthorizerAuthenticationSchemeOptions> configureOptions)
        {
            return AddJwtAuthorizerInternal(builder, authenticationScheme, configureOptions);
        }

        private static AuthenticationBuilder AddJwtAuthorizerInternal(
            this AuthenticationBuilder builder, 
            string? authenticationScheme = null,
            Action<AwsJwtAuthorizerAuthenticationSchemeOptions>? configureOptions = null)
        {
            return builder
                .AddScheme<AwsJwtAuthorizerAuthenticationSchemeOptions, AwsJwtAuthorizerAuthenticationHandler>(
                    authenticationScheme,
                    configureOptions);
        }
    }
}
