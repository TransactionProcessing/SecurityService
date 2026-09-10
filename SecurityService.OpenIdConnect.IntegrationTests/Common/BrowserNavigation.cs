using System;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace SecurityService.IntergrationTests.Common;

public static class BrowserNavigation
{
    public static async Task NavigateWithRetryAsync(
        Func<Task> navigate,
        Action<string> log = null,
        Int32 maxAttempts = 2,
        TimeSpan retryDelay = default)
    {
        if (maxAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        }

        Exception lastException = null;
        TimeSpan delay = retryDelay == default ? TimeSpan.FromSeconds(1) : retryDelay;

        for (Int32 attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await navigate();
                return;
            }
            catch (WebDriverException exception)
            {
                lastException = exception;
                log?.Invoke($"Browser navigation attempt {attempt} of {maxAttempts} failed: {exception.Message}");

                if (attempt < maxAttempts)
                {
                    await Task.Delay(delay);
                }
            }
        }

        throw new WebDriverException($"Browser navigation failed after {maxAttempts} attempts.", lastException);
    }
}
