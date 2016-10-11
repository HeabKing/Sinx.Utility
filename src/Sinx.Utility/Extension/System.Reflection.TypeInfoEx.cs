using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    public static class TypeInfoEx
    {
	    /// <summary>
		/// 返回一个所有属性都是简单类型的复杂类型的被赋值的属性的集合
		/// </summary>
		/// <param name="model"></param>
		/// <returns>被赋值的属性</returns>
		public static IEnumerable<KeyValuePair<string, object>> GetAssignedProperties<T>(this T model)
		{
			// 断言有无参构造函数且实现了Equals(object{Type}.Equals能进行比较)的类型
			const string simpleTypesString = "Int16|Int32|Int64|DateTime|Boolean|String";
			// 寻找查询字段
			foreach (var p in model.GetType().GetTypeInfo().DeclaredProperties)
			{
				var isSimpleType = Regex.IsMatch(p.PropertyType.Name, simpleTypesString);   // 是否是简单类型
				if (!isSimpleType)
				{
					// 自定义类型可能没有重写Equals和实现无参ctor, 所以不支持
					throw new NotSupportedException($"不支持类型: {p.Name}, 支持持类型: {simpleTypesString}");
				}
				var value = p.GetValue(model, null);    // 引用类型: null/[INS], 值类型: 默认值, 被赋值
				var defaultValue = p.PropertyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(p.PropertyType) : null;

				if (value != null && !value.Equals(defaultValue))
				{
					yield return new KeyValuePair<string, object>(p.Name, value);
				}
			}
		}

		public static IEnumerable<KeyValuePair<string, object>> GetAssignedProperties<T>(this TypeInfo placeholder, T model)
		{
			return model.GetAssignedProperties();
		}

		// test invoke
		private static void A()
	    {
		    int a = 1;
		    var aa = a.GetAssignedProperties();
	    }
    }
}
