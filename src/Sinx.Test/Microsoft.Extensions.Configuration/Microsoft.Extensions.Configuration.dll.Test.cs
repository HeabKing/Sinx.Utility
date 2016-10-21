using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Sinx.Test
{
	/// <summary>
	/// 配置
	/// </summary>
	/// <remarks>何士雄 2016-10-21</remarks>
	public class ConfigurationTest
	{
		[Fact]  // 配置 Conciguration
		public void SummaryTest()
		{
			// 构建
			IConfiguration config = new ConfigurationBuilder()  // Microsoft.Extensions.Configuration.dll
				.SetBasePath(Directory.GetCurrentDirectory())   // Microsoft.Extensions.Configuration.FileExtensions.dll
				.AddJsonFile("appsettings.json")                // Microsoft.Extensions.Configuration.Json.dll
				.Build();

			// 使用
			string connStr0 = config.GetSection("ConnectionStrings").GetSection("Ali").Value;
			string connStr1 = config["ConnectionStrings:Ali"];
			string connStr2 = config.GetConnectionString("Ali");
			IEnumerable<IConfigurationSection> connStrs = config.GetSection("ConnectionStrings").GetChildren();

			// 断言
			Assert.True(connStr0 == connStr1);
			Assert.True(connStr1 == connStr2 && connStr2 != null);
			Assert.True(connStrs.FirstOrDefault()?.Value == connStr0);
		}
	}
}