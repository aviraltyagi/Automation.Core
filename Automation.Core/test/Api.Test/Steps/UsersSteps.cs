using System.Collections.Generic;
using Core.Api;
using Api.Test.Api.Helper;
using Api.Test.Helper;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Api.Test.Steps
{
    [Binding]
    public class UsersSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly Context _context;
        private Dictionary<string, object> CommonData = null;
        public UsersSteps(ScenarioContext scenarioContext, Context context)
        {
            _scenarioContext = scenarioContext;
            _context = context;
            ExternalConfigResolver instance = ExternalConfigResolver.GetInstance();
            CommonData = instance.CommonData;
            _context.SetBaseUrl(CommonData["BaseUrl"].ToString());
        }

        [Given(@"the user details as follows:")]
        public void GivenTheUserDetailsAsFollows(Contract.Request.Users user)
        {
            UserHelper userHelper = new UserHelper(_scenarioContext, _context);
            userHelper.CreateUser(user.name, user.job);
            var response = _context.LastResult;
        }

        [StepArgumentTransformation]
        public Contract.Request.Users ConvertTableToUser(Table table)
        {
            return table.CreateInstance<Contract.Request.Users>();
        }
    }
}
