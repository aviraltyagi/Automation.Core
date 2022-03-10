using Core.Api;
using Core.Api.Utils;
using TechTalk.SpecFlow;

namespace Api.Test.Api.Helper
{
    public class UserHelper
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly Context _context;

        public UserHelper(ScenarioContext scenarioContext, Context context)
        {
            _scenarioContext = scenarioContext;
            _context = context;
        }

        public void CreateUser(string name, string job)
        {
            Contract.Request.Users request = new Contract.Request.Users()
            {
                name = name,
                job = job
            };
            _context.ExecutePostRequest<Contract.Request.Users, Contract.Response.Users, ErrorDetails>("api/users", request);
        }
    }
}
