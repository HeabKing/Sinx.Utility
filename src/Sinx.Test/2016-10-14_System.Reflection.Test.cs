using System;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Xunit;

namespace Sinx.Test
{
	public class ReflectionTest
	{
		#region Assembly
		[Fact]
		public void AssemblyTest()
		{
			#region 根据程序集成员获取程序集名字, 路径等相关信息

			var factAttribute = typeof(FactAttribute);
			var assembly = factAttribute.GetTypeInfo().Assembly;	// 获取程序集
			// Name 程序集名字
			Assert.True(Regex.IsMatch(assembly.FullName, "xunit.core, Version=[^,]+, Culture=[^,]+, PublicKeyToken=.+"));
			Assert.True(assembly.GetName().Name == "xunit.core"); 
			// 路径
			Assert.True(assembly.Location.Contains(@"\.nuget\packages\xunit.extensibility.core\2.2.0-beta2-build3300\lib\netstandard1.0\xunit.core.dll"));
			// 某个类
			var factType = assembly.DefinedTypes.FirstOrDefault(c => c.Name == nameof(FactAttribute));	// 类型集合
			Assert.True(factType != null);

			#endregion
		}

		#endregion

		#region Attribute

		[Fact]
		public void AttributeTest()
		{
			// GetAttribute
			var ins = new NoDbAttributeTestClass { Id = 1, Password = "123", CheckPassword = "123" };
			var typeinfo = ins.GetType().GetTypeInfo();
			var properties = typeinfo.GetProperties();
			var hasNoDbAttribute = false;
			foreach (var property in properties)
			{
				var attributes = property.GetCustomAttributes();
				var attributeFirst = attributes.FirstOrDefault(a => a.GetType().Name == nameof(NoDbAttribute));
				if (attributeFirst != null)
				{
					hasNoDbAttribute = true;
				}
			}
			Assert.True(hasNoDbAttribute);
		}

		#endregion
	}

	class NoDbAttribute : Attribute
	{
		public string Description => "I am not a filed in db in this entity";
	}

	class NoDbAttributeTestClass
	{
		public int Id { get; set; }
		public string Password { get; set; }
		[NoDb]
		[Display]
		public string CheckPassword { get; set; }
	}
}
