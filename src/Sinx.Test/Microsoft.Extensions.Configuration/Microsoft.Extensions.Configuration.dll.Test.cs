using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
			IConfigurationBuilder configurationBuilder = new ConfigurationBuilder() // Microsoft.Extensions.Configuration.dll
				.SetBasePath(Directory.GetCurrentDirectory())                       // Microsoft.Extensions.Configuration.FileExtensions.dll
				.AddJsonFile("appsettings.json");                                   // Microsoft.Extensions.Configuration.Json.dll
			IConfiguration config = configurationBuilder.Build();

			// 使用
			string connStr0 = config.GetSection("ConnectionStrings").GetSection("Ali").Value;   // IConfiguration.Value 也是取的叶节点, 如果不是叶节点, 返回null, 如果不存在IConfiguration, 返回null
			string connStr1 = config["ConnectionStrings:Ali"];  // this[key] 寻找的是叶节点 # 1
			string connStr2 = config.GetConnectionString("Ali");
			IEnumerable<IConfigurationSection> connStrs = config.GetSection("ConnectionStrings").GetChildren();

			// 断言
			Assert.True(connStr0 == connStr1);
			Assert.True(connStr1 == connStr2 && connStr2 != null);
			Assert.True(connStrs.FirstOrDefault()?.Value == connStr0);
			Assert.True(config["ConnectionStrings"] == null);   // this[key] 寻找的是叶节点 # 1 ConnectionStrings 不是叶节点
			Assert.True(config.GetSection("ConnectionStrings").Value == null);  // 不是叶节点, 返回null

			// 使用 - 不存在的节点
			Assert.True(config["NonSection"] == null);
			Assert.True(config.GetSection("NonSection").Value == null); // IConfigurationSection.Value == null
			Assert.True(config.GetSection("NonSection").GetChildren().Any() == false);

			// 使用 - 数组
			Assert.Null(config["Es:Table:SearchedFileds"]); // get null string but want array
			Assert.Null(config.GetSection("Es").GetSection("Table").GetSection("SearchedFileds").Value);
			Assert.True(config.GetSection("Es").GetSection("Table").GetSection("SearchedFileds").GetChildren().First().Value != null);

			// 获取指定节点下的 raw json
			Func<IConfigurationBuilder, string, string> getSectionJson = (cb, key) =>
			{
				var path = ((cb as ConfigurationBuilder)
					?.Sources.First() as Microsoft.Extensions.Configuration.Json.JsonConfigurationSource)
				?.Path;
				if (path == null)
				{
					throw new ArgumentException(nameof(cb) + " 无法获取配置文件路径");
				}
				var configText = File.ReadAllText(path);
				// 去除注释
				configText = Regex.Replace(configText, @"//[^""]+?" + Environment.NewLine, Environment.NewLine);
				var keys = key.Split(':');
				string jsonResult = null;

				try
				{
					var children = (JsonConvert.DeserializeObject(configText) as IEnumerable<dynamic>)?.ToList() ?? new List<dynamic>();
					for (var i = 0; i < keys.Length; i++)
					{
						var i1 = i;
						var section = children?.FirstOrDefault(m => m.Name.ToLower() == keys[i1].ToLower());
						children = (section?.Value as IEnumerable<dynamic>)?.ToList();
						if (i == keys.Length - 1)
						{
							jsonResult = section?.Value?.ToString();
						}
					}
				}
				catch (Exception e)
				{
					throw new ArgumentException($"未能找到指定路径: {key} {Environment.NewLine} {e}");
				}
				if (jsonResult == null)
				{
					throw new ArgumentException($"未能找到指定路径: {key} {Environment.NewLine}");
				}
				return jsonResult;
			};
			var rawJson = getSectionJson(configurationBuilder, "Es:DbToEsMapSettings:settings:similarity");
			Assert.True(Regex.IsMatch(Regex.Replace(rawJson, @"\s", ""), @"{""_bm25"":{""type"":""BM25"",""b"":0}}", RegexOptions.IgnorePatternWhitespace));
		}
	}
}