using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Xunit;

namespace Sinx.Test
{
	public class ReflectionTest
	{
		#region 2016-10-17 System.Reflection.Emit
		// TODO http://stackoverflow.com/a/3862241

			[Fact]
		public void CreateRuntimeClassInstanceTest()
		{
			var dic = new Dictionary<string, Type> {{"Id", typeof(int)}};
			var ins = MyTypeBuilder.CreateNewObject(dic);
			Assert.True(ins.GetType().GetTypeInfo().GetProperty("Id") != null);
		}

		private static class MyTypeBuilder
		{
			public static object CreateNewObject(IEnumerable<KeyValuePair<string, Type>> filedList)
			{
				var myType = CompileResultType(filedList);
				return Activator.CreateInstance(myType.BaseType);
			}
			public static TypeInfo CompileResultType(IEnumerable<KeyValuePair<string, Type>> filedList)
			{
				TypeBuilder tb = GetTypeBuilder();
				ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

				// NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
				foreach (var field in filedList)
					CreateProperty(tb, field.Key, field.Value);

				TypeInfo objectType = tb.CreateTypeInfo();
				return objectType;
			}

			private static TypeBuilder GetTypeBuilder()
			{
				var typeSignature = "MyDynamicType";
				var an = new AssemblyName(typeSignature);
				AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
				ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
				TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
						TypeAttributes.Public |
						TypeAttributes.Class |
						TypeAttributes.AutoClass |
						TypeAttributes.AnsiClass |
						TypeAttributes.BeforeFieldInit |
						TypeAttributes.AutoLayout,
						null);
				return tb;
			}

			private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
			{
				FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

				PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
				MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
				ILGenerator getIl = getPropMthdBldr.GetILGenerator();

				getIl.Emit(OpCodes.Ldarg_0);
				getIl.Emit(OpCodes.Ldfld, fieldBuilder);
				getIl.Emit(OpCodes.Ret);

				MethodBuilder setPropMthdBldr =
					tb.DefineMethod("set_" + propertyName,
					  MethodAttributes.Public |
					  MethodAttributes.SpecialName |
					  MethodAttributes.HideBySig,
					  null, new[] { propertyType });

				ILGenerator setIl = setPropMthdBldr.GetILGenerator();
				Label modifyProperty = setIl.DefineLabel();
				Label exitSet = setIl.DefineLabel();

				setIl.MarkLabel(modifyProperty);
				setIl.Emit(OpCodes.Ldarg_0);
				setIl.Emit(OpCodes.Ldarg_1);
				setIl.Emit(OpCodes.Stfld, fieldBuilder);

				setIl.Emit(OpCodes.Nop);
				setIl.MarkLabel(exitSet);
				setIl.Emit(OpCodes.Ret);

				propertyBuilder.SetGetMethod(getPropMthdBldr);
				propertyBuilder.SetSetMethod(setPropMthdBldr);
			}
		}
		#endregion

		#region 2016-10-15 Assembly
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

		#region 2016-10-15 Attribute

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
