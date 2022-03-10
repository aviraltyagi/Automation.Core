using System;

namespace Api.Test.Contract.Response
{
    public class Users
    {
        public string name { get; set; }
        public string job { get; set; }
        public int id { get; set; }
        public TimeSpan createdAt { get; set; }
    }
}
