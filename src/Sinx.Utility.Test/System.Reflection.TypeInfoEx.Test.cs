using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;

namespace Sinx.Utility.Test
{
	public class TypeInfoExTest
	{
		[Fact]
		public void GetAssignedPropertiesTest()
		{
			var student = new Student {Id = 1, Name = "HelloWord"};
			// ReSharper disable once InvokeAsExtensionMethod
			var s0 = TypeInfoEx.GetAssignedProperties(student).ToList();
			Assert.True(s0.Any(m => m.Key == nameof(Student.Id)));
			Assert.True(s0.Any(m => m.Key == nameof(Student.Name)));
			Assert.False(s0.Any(m => m.Key == nameof(Student.Age)));

			// recommend usage
			var s1 = student.GetAssignedProperties().ToList();
			Assert.True(s1.Any(m => m.Key == nameof(Student.Id)));
			Assert.True(s1.Any(m => m.Key == nameof(Student.Name)));
			Assert.False(s1.Any(m => m.Key == nameof(Student.Age)));

			var s2 = default(TypeInfo).GetAssignedProperties(student).ToList();
			Assert.True(s2.Any(m => m.Key == nameof(Student.Id)));
			Assert.True(s2.Any(m => m.Key == nameof(Student.Name)));
			Assert.False(s2.Any(m => m.Key == nameof(Student.Age)));
		}

		class Student
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
		}
	}
}
