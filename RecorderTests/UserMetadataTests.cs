using System;
using System.Diagnostics;

using Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Recorder.Models;


namespace RecorderTests
{
    public class UserMetadataTests
    {
        UserMetadata data;

        public UserMetadataTests()
        {
            data = new UserMetadata();
        }

        [Fact]
        public void TestJsonGenerated()
        {
            JObject obj = data.ToJsonObject();
            string result = obj.ToString();
            Assert.NotEqual("", result);
        }
    }
}
