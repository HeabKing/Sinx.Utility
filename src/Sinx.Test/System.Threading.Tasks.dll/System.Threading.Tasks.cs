using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Sinx.Test.System.Threading.Tasks.dll
{
	/// <summary>
	/// 用于Task类的相关测试
	/// </summary>
	/// <remarks>何士雄 2016-11-07</remarks>
	public class TaskTest
	{
		/// <summary>
		/// Instance A Task
		/// http://stackoverflow.com/questions/29693362/regarding-usage-of-task-start-task-run-and-task-factory-startnew
		/// </summary>
		[Fact]
		public void InstanceTest()
		{
			// 总结:
			//		1. 通常使用 Task.Run(Action) -> net45
			//		2. 如果需要 TaskCreationOptions.LongRunning(暗示TPL此任务比平常的需要更长的时间), 使用 Task.Factory.StartNew(Action, TaskCreationOptions) -> net40
			//		3. 除非特别需要明确指定 Task.Start 开始, 才需要使用 new Task(Action)

			Action printWords0 = () => Debug.WriteLine("Hello World! From: Task.Run(Action)");
			var task0 = Task.Run(printWords0);
			//task0.Start();  // System.InvalidOperationException: Start may not be called on a task that was already started.
			task0.Wait();

			Action printWords1 = () => Debug.WriteLine("Hello World! From: new Task(Action)");
			var task1 = new Task(printWords1);  // Action or Action<object>		// 自己创建实例的话, 需要考虑同步和并行的问题, 太麻烦了
			task1.Start();
			task1.Wait();
			Assert.False(task1.IsFaulted);

			Action printWords2 = () => Debug.WriteLine("Hello World! From: Task.Factory.StartNew(Action)");
			var task2 = Task.Factory.StartNew(printWords2);
			//task2.Start();  // System.InvalidOperationException: Start may not be called on a task that was already started.
			task2.Wait();
		}
	}
}
