using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Api.Test.Helper
{
    public class ExternalConfigResolver
    {
        private static ExternalConfigResolver instance = null;
        public Dictionary<string, object> CommonData = null;
        private ExternalConfigResolver()
        {
            string projectDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string filePath = Path.Combine(projectDir, $@"Api.Test\ExternalConfig.json");
            var serializedData = File.ReadAllText(filePath);
            CommonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedData);
        }

        public static ExternalConfigResolver GetInstance()
        {
            if (instance == null)
                instance = new ExternalConfigResolver();
            return instance;
        }
    }
}
