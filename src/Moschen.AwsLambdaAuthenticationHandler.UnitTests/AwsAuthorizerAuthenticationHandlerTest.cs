using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Serilog.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Moschen.AwsLambdaAuthenticationHandler.UnitTests
{
    public class AwsAuthorizerAuthenticationHandlerTest : UnitTest
    {
        private readonly AwsAuthorizerAuthenticationHandler<AuthenticationSchemeOptions> _target;
        private ClaimsIdentity SampleClaimsIdentity => new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "My name"),
                new Claim(ClaimTypes.Email, "email@any.com"),
                new Claim("custom-claim", "custom value"),
            },
            AwsAuthorizerDefaults.AuthenticationScheme);

        public AwsAuthorizerAuthenticationHandlerTest(ITestOutputHelper output) : base(output)
        {
            var optionsMonitorMock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>(MockBehavior.Strict);
            optionsMonitorMock
                .Setup(mock => mock.Get(It.IsAny<string>()))
                .Returns(new AuthenticationSchemeOptions());
            _target = new AwsAuthorizerAuthenticationHandler<AuthenticationSchemeOptions>(optionsMonitorMock.Object,
                new LoggerFactory(new[] { new SerilogLoggerProvider(Logger) }),
                new UrlTestEncoder(),
                new SystemClock());
        }

        [Fact]
        public async Task AuthenticateAsync_ExistingClaims_Authenticate()
        {
            // Arrange
            var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(SampleClaimsIdentity) };
            await _target.InitializeAsync(
                new AuthenticationScheme(
                    AwsAuthorizerDefaults.AuthenticationScheme,
                    null,
                    typeof(AwsAuthorizerAuthenticationHandler<AuthenticationSchemeOptions>)),
                httpContext);

            // Act
            var result = await _target.AuthenticateAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Succeeded.Should().BeTrue();
                result.Ticket.AuthenticationScheme.Should().Be(AwsAuthorizerDefaults.AuthenticationScheme);
                foreach (var expectedClaim in SampleClaimsIdentity.Claims)
                {
                    result.Principal.Claims.Should()
                        .Contain(actualClaim => actualClaim.Type == expectedClaim.Type && actualClaim.Value == expectedClaim.Value);
                }
            }
        }

        [Fact]
        public async Task AuthenticateAsync_MissingClaims_FailAuthentication()
        {
            // Arrange
            await _target.InitializeAsync(
                new AuthenticationScheme(
                    AwsAuthorizerDefaults.AuthenticationScheme,
                    null,
                    typeof(AwsAuthorizerAuthenticationHandler<AuthenticationSchemeOptions>)),
                new DefaultHttpContext());

            // Act
            var result = await _target.AuthenticateAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Succeeded.Should().BeFalse();
                result.Ticket.Should().BeNull();
            }
        }
    }
}
