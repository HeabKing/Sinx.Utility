using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Sinx.Test
{
	public class DynamicTest
	{
		[Fact]
		public void SummaryTest()
		{
			ExpandoObject eo = new ExpandoObject();
			//eo.Id = 2;
			//eo["Id"] = 2;
			//eo.Add("Id", 2);

			// http://stackoverflow.com/a/15819760
			dynamic d = new ExpandoObject { };
			d.Id = 1;
			Assert.True(d.Id == 1);
			Assert.True(d is IDictionary<string, object>);
			Assert.True((d as IDictionary<string, object>)["Id"].Equals(1));
			//Assert.True((d as IDictionary<string, object>)["Don't have key"].Equals(1));	// Error: KeyNotFoundException
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
			var dynamicType = (dynamic)1 + 1;
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

		[Fact]
		public void JsonConvertTest()
		{
			// 匿名类型转换
			var json1 = JsonConvert.SerializeObject(new { Id = 1, Name = "Sinx" });
			// 动态类型转换
			dynamic d = new ExpandoObject();
			d.Id = 1;
			d.Name = "Sinx";
			var json2 = JsonConvert.SerializeObject(d);
			Assert.True(json1 == json2);
			var dc = new ExpandoObject() as IDictionary<string, object>;
			dc.Add("Id", 1);
			dc.Add("Name", "Sinx");
			var json3 = JsonConvert.SerializeObject(dc);
			var json4 = JsonConvert.SerializeObject((dynamic)dc);
			Assert.True(json3 == json2);
			Assert.True(json3 == json4);
			var dc2 = new Dictionary<string, object>
			{
				{"Id", 1},
				{"Name", "Sinx"}
			};
			var json5 = JsonConvert.SerializeObject(dc2);
			Assert.True(json4 == json5);    // 这里居然是true, 但是Dictionary是不具备动态分发功能的, 这里可能是json.net的特殊处理
			var json6 = JsonConvert.SerializeObject(dc2.AsEnumerable().ToList());	// List, 数组等不是, IEnumerable是接口, 实际行为取决是实例是什么类型
			Assert.False(json5 == json6);
			dynamic d2 = new Dictionary<string, object>();
			//d2.Id = 1;	// expcetion 不允许动态绑定
			//d2.Name = "Sinx";
		}
	}
}
