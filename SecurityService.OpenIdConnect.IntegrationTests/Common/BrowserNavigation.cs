using System;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace SecurityService.IntergrationTests.Common;

public static class BrowserNavigation
{
    public static async Task NavigateWithDiagnosticsAsync(
        Func<Task> navigate,
        Action<string> log = null)
    {
        try
        {
            await navigate();
        }
        catch (WebDriverException exception)
        {
            log?.Invoke($"Browser navigation failed without retry because the URL may be state-changing: {exception.Message}");
            throw;
        }
    }
}
