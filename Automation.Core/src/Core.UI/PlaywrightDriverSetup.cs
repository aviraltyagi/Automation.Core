using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace Core.UI
{
    public class PlaywrightDriverSetup
    {
        private PlaywrightDriver _playwrightDriver;

        public async Task<PlaywrightDriver> Init(string browserType, bool isHeadless = false)
        {
            try
            {
                IPlaywright playwright = await Playwright.CreateAsync();
                var browserTypeLaunchOptions = new BrowserTypeLaunchOptions()
                {
                    Headless = isHeadless,
                    Args = new[]
                    {
                        "--start-maximized"
                    }
                };
                IBrowser browser = null;
                switch (browserType)
                {
                    case BrowserType.Chromium:
                        browser = await playwright.Chromium.LaunchAsync(browserTypeLaunchOptions);
                        break;
                    case BrowserType.Firefox:
                        browser = await playwright.Firefox.LaunchAsync(browserTypeLaunchOptions);
                        break;
                    case BrowserType.Webkit:
                        browser = await playwright.Webkit.LaunchAsync(browserTypeLaunchOptions);
                        break;
                }

                IBrowserContext browserContext = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    IgnoreHTTPSErrors = true,
                    ViewportSize = ViewportSize.NoViewport
                });

                IPage page = await browserContext.NewPageAsync();

                _playwrightDriver = new PlaywrightDriver()
                {
                    Playwright = playwright,
                    Browser = browser,
                    BrowserContext = browserContext,
                    Page = page
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return _playwrightDriver;
        }
    }
}
