using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Sinx.Utility.Extension;
using Xunit;

namespace Sinx.Utility.Test
{
    public class HttpRequestMessageExTest
    {
		[Fact]
        public void CreateFromRawTest()
		{
		    string requestRaw = $@"
				GET https://www.baidu.com/ HTTP/1.1
				User-Agent: Fiddler
				Host: www.baidu.com
				";
		    var request = HttpRequestMessageEx.CreateFromRaw(requestRaw);
		    var client = new HttpClient();
		    var response = client.SendAsync(request).Result;
		    var responseString = response.Content.ReadAsStringAsync().Result;
		    Assert.True(responseString.Any());
		}
    }
}
