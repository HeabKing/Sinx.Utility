using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sinx.Utility.Tools.AliYun;
using Xunit;

namespace Sinx.Utility.Test.Tools.AliYun
{
	public class ProducterAndConsumer
	{
		[Fact]
		public void SummaryTest()
		{
			using (var helper = new MqHelper("SinxTest", "wDuuwhy2ibIX4l9l", "roBHfibj5L2om2dsx0OhpASof3RUA1"))
			{
				// ---------------- 发送 ---------------
				var r = helper.SendAsync("tag", "key", "body").Result;
				Assert.True(r);
				// ---------------- 接收 ---------------
				var mqMessageList = helper.GetAsync().Result;
				Assert.True(mqMessageList.Any(m => m.ReconsumeTimes == 0)); // 队列中可定有一个我新添加的, 第一次消费的
				Debug.WriteLine(mqMessageList.Count());
				// ---------------- 删除 ---------------
				var tasks = mqMessageList.Select(m => helper.DeleteAsync(m.MsgHandle));
				var results = Task.WhenAll(tasks).Result;
				Assert.True(results.All(b => b));   // 删除了所有查出来的消息
			}
		}
	};
}

