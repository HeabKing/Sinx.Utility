using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sinx.Test
{
    public class DynamicTest
    {
		[Fact]
	    public void Test()
	    {
			// http://stackoverflow.com/a/15819760
			dynamic d = new ExpandoObject();
		    d.Id = 1;
		    Assert.True(d.Id == 1);
		    //d["Age"] = 2;
		    //Assert.True(d["Age"] == 2);
			// maybe dynamic is based on IEnumerable<KeyValuePair<string, object>>
	    }
    }
}
