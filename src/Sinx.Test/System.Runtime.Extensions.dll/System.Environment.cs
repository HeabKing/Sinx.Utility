using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sinx.Test.System.Runtime.Extensions.dll
{
	/// <summary>
	/// 提供当前[环境]和[平台]的信息以及操作他们的方法, 此类不能被继承
	/// </summary>
	public class EnvironmentTest
	{
		/// <summary>
		/// 当前托管线程的唯一标识符(int)
		/// </summary>
		[Fact]
		public void CurrentManagedThreadIdTest()
		{
			var threadId1 = Environment.CurrentManagedThreadId;
			Assert.IsType<int>(threadId1);
			Assert.True(threadId1 > 0);

			var threadId2 = Thread.CurrentThread.ManagedThreadId;
			Assert.True(threadId2 == threadId1);    // 跟通过Thread中获取的是一个

			var taskId1 = Task.CurrentId;            // 当前任务的Id, 不是任务的线程的Id
			Assert.NotEqual(threadId2, taskId1);     // 任务Id跟线程Id不是一个, 任务的线程Id跟任务Id是一个
			Assert.NotEqual(threadId1, taskId1);

			var taskThreadId2 = Task.Run(() => Environment.CurrentManagedThreadId).Result;
			Assert.NotEqual(taskThreadId2, threadId1);  // 执行任务的线程跟当前代码块中的线程不同, 即使都是使用Environment检测的
			Assert.NotEqual(taskThreadId2, Environment.CurrentManagedThreadId);
			Assert.Equal(threadId1, Environment.CurrentManagedThreadId);
		}

		/// <summary>
		/// 
		/// </summary>
		[Fact]
		public void EnvironmentVariableTest()
		{
			Environment.SetEnvironmentVariable("GlobalVariable", "GlobalVariableValue");
			Func<string> getGlobalVariable = () => Task.Run(() => Environment.GetEnvironmentVariable("GlobalVariable")).Result;
			Assert.Equal("GlobalVariableValue", getGlobalVariable());
		}
	}
}
