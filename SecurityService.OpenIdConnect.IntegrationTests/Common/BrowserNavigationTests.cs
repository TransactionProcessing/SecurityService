using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;

namespace SecurityService.IntergrationTests.Common;

[TestFixture]
public class BrowserNavigationTests
{
    [Test]
    public async Task NavigateWithRetryAsync_RetriesAfterTransientNavigationFailure()
    {
        Int32 attempts = 0;

        await BrowserNavigation.NavigateWithRetryAsync(
            () =>
            {
                attempts++;
                if (attempts == 1)
                {
                    throw new WebDriverException("transient navigation failure");
                }

                return Task.CompletedTask;
            });

        Assert.That(attempts, Is.EqualTo(2));
    }
}
