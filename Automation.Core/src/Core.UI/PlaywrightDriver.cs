using Microsoft.Playwright;

namespace Core.UI
{
    public class PlaywrightDriver
    {
        public IPlaywright Playwright { get; set; }
        public IBrowser Browser { get; set; }
        public IBrowserContext BrowserContext { get; set; }
        public IPage Page { get; set; }
    }
}
