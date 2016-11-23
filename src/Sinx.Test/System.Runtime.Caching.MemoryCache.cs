using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Sinx.Test
{
    public class MemoryCacheTest
    {
	    private readonly IMemoryCache _memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
	    private const string CacheKey = "123465";
	    private string _result;
		[Fact]
	    public void SetAndGetTest()
	    {
			var cts = new CancellationTokenSource();
			var pause = new ManualResetEvent(false);

			using (var cacheEntry = _memoryCache.CreateEntry(CacheKey))
			{
				_memoryCache.Set("master key", "some value",
					new MemoryCacheEntryOptions()
					.AddExpirationToken(new CancellationChangeToken(cts.Token)));

				cacheEntry.SetValue(123456/*_cacheItem*/)
					.RegisterPostEvictionCallback(
						(key, value, reason, substate) =>
						{
							_result = $"'{key}':'{value}' was evicted because: {reason}";
							pause.Set();
						}
					);
			}

			// trigger the token to expire the master item
			cts.Cancel();

			Assert.True(pause.WaitOne(500));

			Assert.Equal("'key':'value' was evicted because: TokenExpired", _result);
		}
    }
}
