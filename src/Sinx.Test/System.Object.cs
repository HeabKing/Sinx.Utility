using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sinx.Test
{
	/// <summary>
	/// class 类 - 对事物的抽象
	/// </summary>
    public class Person
    {
		/// <summary>
		/// Property 属性
		/// </summary>
	    public string Name { get; set; }

		/// <summary>
		/// Action 动作
		/// </summary>
		/// <returns></returns>
	    public string ToSring()
	    {
			return Name;
	    }

		[Fact]
	    public void PersonTest()
		{
			// Object 物体, 对象 - 类的实体
			var person = new Person();
		}
    }
}
