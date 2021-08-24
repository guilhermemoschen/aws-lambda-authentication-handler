using System;
using Microsoft.AspNetCore.Authentication;
using Moschen.AwsLambdaAuthenticationHandler;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class AwsAuthorizerExtensions
    {
        public static AuthenticationBuilder AddAwsAuthorizer(this AuthenticationBuilder builder)
        {
            return AddAwsAuthorizerInternal(builder, AwsAuthorizerDefaults.AuthenticationScheme);
        }

        public static AuthenticationBuilder AddAwsAuthorizer(this AuthenticationBuilder builder, string authenticationScheme)
        {
            return AddAwsAuthorizerInternal(builder, authenticationScheme);
        }

        public static AuthenticationBuilder AddAwsAuthorizer(
            this AuthenticationBuilder builder, 
            Action<AuthenticationSchemeOptions> configureOptions)
        {
            return AddAwsAuthorizerInternal(builder, AwsAuthorizerDefaults.AuthenticationScheme, configureOptions);
        }

        public static AuthenticationBuilder AddAwsAuthorizer(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            Action<AuthenticationSchemeOptions> configureOptions)
        {
            return AddAwsAuthorizerInternal(builder, authenticationScheme, configureOptions);
        }

        private static AuthenticationBuilder AddAwsAuthorizerInternal(
            this AuthenticationBuilder builder, 
            string? authenticationScheme = null,
            Action<AuthenticationSchemeOptions>? configureOptions = null)
        {
            return builder
                .AddScheme<AuthenticationSchemeOptions, AwsAuthorizerAuthenticationHandler<AuthenticationSchemeOptions>>(
                    authenticationScheme,
                    configureOptions);
        }
    }
}
