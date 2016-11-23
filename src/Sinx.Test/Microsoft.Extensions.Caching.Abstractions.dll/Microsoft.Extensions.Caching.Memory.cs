using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Xunit;

namespace Sinx.Test.Microsoft.Extensions.Caching.Abstractions.dll
{
	public class MemoryCacheTest
	{
		[Fact]
		public void StartTest()
		{
			IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions
			{
				ExpirationScanFrequency = TimeSpan.FromSeconds(30),	// 过期检测频率 默认为一分钟
				Clock = new SystemClock(),	// 系统当前UTC时间
				CompactOnMemoryPressure = true	// 默认为true
			}));
			const int obj = 123;
			cache.Set("key", obj);  // 采用默认添加实体动作, 同key替换
			var obj0 = cache.Get("key");
			Assert.Equal(obj0, obj);

			cache.Set("key", obj + obj);    // 同key替换
			var obj1 = cache.Get("key");
			Assert.NotEqual(obj1, obj);
			Assert.Equal(obj1, obj + obj);
		}
	}
}
