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
			ExpandoObject eo = new ExpandoObject();
			//eo.Id = 2;
			//eo["Id"] = 2;
			//eo.Add("Id", 2);

			// http://stackoverflow.com/a/15819760
			dynamic d = new ExpandoObject{};
		    d.Id = 1;
		    Assert.True(d.Id == 1);
			//d["Age"] = 2;
			//Assert.True(d["Age"] == 2);
			// maybe dynamic is based on IEnumerable<KeyValuePair<string, object>>
			// 对动态类型进行反射
		    var typeinfo = d.GetType();
		    Assert.True(typeinfo.Name == nameof(ExpandoObject));
		    //var dtype = typeof(dynamic);	// typeof 运算符不能用于动态类型上

		    dynamic d1 = 1;
		    Assert.True(d1.GetType().Name == "Int32");

			// 动态操作的返回类型是动态的
			var dynamicType = (dynamic) 1 + 1;
			Assert.True(dynamicType.GetType().Name == "Int32");

			// dynamic 更像是对类型的包装
			dynamic d2 = 1;
			Assert.True(d2.GetType().Name == "Int32");
			//d2.Id = 3;  // RuntimeBonderException: 其他信息: 'int' does not contain a definition for 'Id'

			// dynamic 跟 object 很像
			Assert.True(1 is dynamic);
			Assert.True(1 is object);
			Assert.False(null is dynamic);
			Assert.False(null is object);
			var t = typeof(List<dynamic>);
			Assert.True(t.FullName == "System.Collections.Generic.List`1[[System.Object, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]");
		}
    }
}
