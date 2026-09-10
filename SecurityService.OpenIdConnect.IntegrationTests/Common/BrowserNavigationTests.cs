using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;

namespace SecurityService.IntergrationTests.Common;

[TestFixture]
public class BrowserNavigationTests
{
    [Test]
    public async Task NavigateWithDiagnosticsAsync_DoesNotRetryAfterNavigationFailure()
    {
        Int32 attempts = 0;
        String logMessage = String.Empty;

        Assert.ThrowsAsync<WebDriverException>(async () => await BrowserNavigation.NavigateWithDiagnosticsAsync(
            async () =>
            {
                attempts++;
                throw new WebDriverException("navigation failure");
            },
            message => logMessage = message));

        Assert.That(attempts, Is.EqualTo(1));
        Assert.That(logMessage, Does.Contain("navigation failure"));
    }
}
