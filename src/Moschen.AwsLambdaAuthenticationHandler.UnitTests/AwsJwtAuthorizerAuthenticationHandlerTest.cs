using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moschen.AwsLambdaAuthenticationHandler.Jwt;
using Serilog.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Moschen.AwsLambdaAuthenticationHandler.UnitTests
{
    public class AwsJwtAuthorizerAuthenticationHandlerTest : UnitTest
    {
        private ClaimsIdentity SampleClaimsIdentity => new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "My name"),
                new Claim(ClaimTypes.Email, "email@any.com"),
                new Claim("custom-claim", "custom value"),
            },
            AwsJwtAuthorizerDefaults.AuthenticationScheme);

        public AwsJwtAuthorizerAuthenticationHandlerTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task AuthenticateAsync_ExistingClaims_Authenticate()
        {
            // Arrange
            var target = await CreateTargetAsync(
                new AwsJwtAuthorizerAuthenticationSchemeOptions() { RequireToken = false, ExtractClaimsFromToken = false },
                SampleClaimsIdentity);

            // Act
            var result = await target.AuthenticateAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Succeeded.Should().BeTrue();
                result.Ticket.AuthenticationScheme.Should().Be(AwsJwtAuthorizerDefaults.AuthenticationScheme);
                foreach (var expectedClaim in SampleClaimsIdentity.Claims)
                {
                    result.Principal.Claims.Should()
                        .Contain(actualClaim => actualClaim.Type == expectedClaim.Type && actualClaim.Value == expectedClaim.Value);
                }
            }
        }

        [Fact]
        public async Task AuthenticateAsync_MissingClaims_CreatePrincipalAndAuthenticate()
        {
            // Arrange
            var token = GenerateToken(SampleClaimsIdentity);
            var target = await CreateTargetAsync(
                new AwsJwtAuthorizerAuthenticationSchemeOptions() { RequireToken = true, ExtractClaimsFromToken = true },
                token: token);

            // Act
            var result = await target.AuthenticateAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Succeeded.Should().BeTrue();
                result.Ticket.AuthenticationScheme.Should().Be(AwsJwtAuthorizerDefaults.AuthenticationScheme);
                foreach (var expectedClaim in SampleClaimsIdentity.Claims)
                {
                    result.Principal.Claims.Should()
                        .Contain(actualClaim => actualClaim.Type == expectedClaim.Type && actualClaim.Value == expectedClaim.Value);
                }
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("Bearer")]
        [InlineData("Bearer ")]
        public async Task AuthenticateAsync_MissingClaimsAndToken_FailAuthentication(string authorizationHeader)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            if (authorizationHeader != null)
            {
                httpContext.Request.Headers.Add("Authorization", authorizationHeader);
            }

            var optionsMonitorMock = new Mock<IOptionsMonitor<AwsJwtAuthorizerAuthenticationSchemeOptions>>(MockBehavior.Strict);
            optionsMonitorMock
                .Setup(mock => mock.Get(It.IsAny<string>()))
                .Returns(new AwsJwtAuthorizerAuthenticationSchemeOptions());
            var target = new AwsJwtAuthorizerAuthenticationHandler(optionsMonitorMock.Object,
                new LoggerFactory(new[] { new SerilogLoggerProvider(Logger) }),
                new UrlTestEncoder(),
                new SystemClock());

            await target.InitializeAsync(
                new AuthenticationScheme(
                    AwsJwtAuthorizerDefaults.AuthenticationScheme,
                    null,
                    typeof(AwsJwtAuthorizerAuthenticationHandler)),
                httpContext);

            // Act
            var result = await target.AuthenticateAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Succeeded.Should().BeFalse();
                result.Ticket.Should().BeNull();
            }
        }

        private string GenerateToken(ClaimsIdentity claimsIdentity)
        {
            // Can be any issuer
            const string Issuer = "https://accounts.google.com";
            const string Audience = "your-client-id.apps.googleusercontent.com";

            var handler = new JwtSecurityTokenHandler()
            {
                // to avoid changing claims' types, consider checking the token at https://jwt.ms/
                OutboundClaimTypeMap = new Dictionary<string, string>(),
            };
            return handler.CreateEncodedJwt(
                Issuer,
                Audience,
                claimsIdentity,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow,
                new SigningCredentials(new RsaSecurityKey(RSA.Create(2048)), SecurityAlgorithms.RsaSsaPssSha256));
        }

        private async Task<AwsJwtAuthorizerAuthenticationHandler> CreateTargetAsync(
            AwsJwtAuthorizerAuthenticationSchemeOptions options = null,
            ClaimsIdentity claimsIdentity = null,
            string token = null)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<AwsJwtAuthorizerAuthenticationSchemeOptions>>(MockBehavior.Strict);
            optionsMonitorMock
                .Setup(mock => mock.Get(It.IsAny<string>()))
                .Returns(options ?? new AwsJwtAuthorizerAuthenticationSchemeOptions());
            var target = new AwsJwtAuthorizerAuthenticationHandler(optionsMonitorMock.Object,
                new LoggerFactory(new[] { new SerilogLoggerProvider(Logger) }),
                new UrlTestEncoder(),
                new SystemClock());

            var httpContext = new DefaultHttpContext();
            if (claimsIdentity != null)
            {
                httpContext.User = new ClaimsPrincipal(SampleClaimsIdentity);
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpContext.Request.Headers.Add("Authorization", $"Bearer {token}");
            }

            await target.InitializeAsync(
                new AuthenticationScheme(
                    AwsJwtAuthorizerDefaults.AuthenticationScheme,
                    null,
                    typeof(AwsJwtAuthorizerAuthenticationHandler)),
                httpContext);

            return target;
        }
    }
}
