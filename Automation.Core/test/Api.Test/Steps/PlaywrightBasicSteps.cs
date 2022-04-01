using Core.UI;
using System.Threading.Tasks;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace Api.Test.Steps
{
    [Binding]
    public class PlaywrightBasicSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private PlaywrightDriverSetup _playwrightDriverSetup;
        private PlaywrightDriver _playwrightDriver;

        public PlaywrightBasicSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"User open '(.*)' browser")]
        public async Task GivenUserOpenBrowser(string browserType)
        {
            _playwrightDriverSetup = new PlaywrightDriverSetup();
            _playwrightDriver = await _playwrightDriverSetup.Init(browserType);
            _scenarioContext.Add("PlaywrightDriver", _playwrightDriver);
        }

        [When(@"I navigate to '(.*)'")]
        public async Task WhenINavigateTo(string url)
        {
            await _playwrightDriver.Page.GotoAsync(url);
        }

        [Then(@"'(.*)' opens")]
        public async Task ThenOpens(string url)
        {
            string actualUrl = _playwrightDriver.Page.Url;
            Assert.That(actualUrl.Equals(url), $"Expected Url : {url} but was {actualUrl}");
        }

    }
}
