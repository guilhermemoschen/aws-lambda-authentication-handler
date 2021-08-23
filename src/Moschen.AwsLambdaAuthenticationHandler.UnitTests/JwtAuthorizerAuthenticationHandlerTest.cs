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
using Serilog.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Moschen.AwsLambdaAuthenticationHandler.UnitTests
{
    public class JwtAuthorizerAuthenticationHandlerTest : UnitTest
    {
        private readonly JwtAuthorizerAuthenticationHandler _target;
        private ClaimsIdentity SampleClaimsIdentity => new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "My name"),
                new Claim(ClaimTypes.Email, "email@any.com"),
                new Claim("custom-claim", "custom value"),
            },
            JwtAuthorizerDefaults.AuthenticationScheme);

        public JwtAuthorizerAuthenticationHandlerTest(ITestOutputHelper output) : base(output)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>(MockBehavior.Strict);
            optionsMonitorMock
                .Setup(mock => mock.Get(It.IsAny<string>()))
                .Returns(new AuthenticationSchemeOptions());
            _target = new JwtAuthorizerAuthenticationHandler(optionsMonitorMock.Object,
                new LoggerFactory(new[] { new SerilogLoggerProvider(Logger) }),
                new UrlTestEncoder(),
                new SystemClock());
        }

        [Fact]
        public async Task AuthenticateAsync_ExistingClaims_Authenticate()
        {
            // Arrange
            await SetupMocks(SampleClaimsIdentity);

            // Act
            var result = await _target.AuthenticateAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Succeeded.Should().BeTrue();
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
            await SetupMocks(token);

            // Act
            var result = await _target.AuthenticateAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Succeeded.Should().BeTrue();
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
            
            await _target.InitializeAsync(
                new AuthenticationScheme(
                    JwtAuthorizerDefaults.AuthenticationScheme,
                    null,
                    typeof(JwtAuthorizerAuthenticationHandler)),
                httpContext);

            // Act
            var result = await _target.AuthenticateAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Succeeded.Should().BeFalse();
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

        private async Task SetupMocks(string token)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", $"Bearer {token}");

            await _target.InitializeAsync(
                new AuthenticationScheme(
                    JwtAuthorizerDefaults.AuthenticationScheme,
                    null,
                    typeof(JwtAuthorizerAuthenticationHandler)),
                httpContext);
        }

        private async Task SetupMocks(ClaimsIdentity claimsIdentity)
        {
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(claimsIdentity) };
            await _target.InitializeAsync(
                new AuthenticationScheme(
                    JwtAuthorizerDefaults.AuthenticationScheme,
                    null,
                    typeof(JwtAuthorizerAuthenticationHandler)),
                httpContext);
        }
    }
}
