using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dapper;
using Xunit;
using System.Reflection;
using Newtonsoft.Json;

namespace Sinx.Test.Sinx.Search
{
	/// <summary>
	/// 索引维护
	/// </summary>
	public class IndexMaintainTest
	{
		// IConfigurationRoot 多一个Reload()方法
		private readonly IConfiguration _configuration;
		private readonly IConfigurationBuilder _configurationBuilder;
		private readonly IDbConnection _db;
		private readonly HttpClient _client;
		private readonly string _tableName;
		private readonly string _indexName;
		private readonly IEnumerable<dynamic> _tblNameTypes;
		public IndexMaintainTest()
		{
			_configurationBuilder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json");
			_configuration = _configurationBuilder.Build();
			var connectionString = _configuration["ConnectionStrings:Zxxk"];
			_db = new SqlConnection(connectionString);
			_client = new HttpClient { BaseAddress = new Uri(_configuration["Es:Host"]) };
			_tableName = _configuration["Es:Table:Name"];
			_indexName = _configuration["Es:IndexName"];
			_tblNameTypes = _db.QueryAsync($@"
				SELECT COLUMN_NAME AS Name, DATA_TYPE AS Type
				FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = '{_tableName}'").Result;
			if (_tableName.Count() == 0)
			{
				throw new Exception($"无法从指定的连接字符串{connectionString.Substring(0, 21)}... 下获取 {_tableName} 下的数据");
			}
		}
		/// <summary>
		/// 创建索引表
		/// </summary>
		[Fact]
		public void CreateIndex()
		{
			// 创建ES索引连接
			var request = new HttpRequestMessage(HttpMethod.Post, $"/{_indexName}");
			// 添加映射报文体
			// 获取类型映射
			var properties = GetEsSettingsMappingsPropertyes(_configurationBuilder);
			var dbToEsMapSettings = _configurationBuilder.GetJson("Es:DbToEsMapSettings");
			var body = dbToEsMapSettings.TrimEnd().TrimEnd('}').TrimEnd().TrimEnd('}') + ",\"properties\":"+ properties + "}}";
			request.Content = new StringContent(body);
			// 进行连接
			var response = _client.SendAsync(request).Result;
			var content = response.Content.ReadAsStringAsync().Result;
			Assert.True(response.StatusCode == HttpStatusCode.OK);
			Assert.True(Regex.IsMatch(content, "true"));
		}

		/// <summary>
		/// ES属性类型映射的配置Json
		/// </summary>
		/// <returns></returns>
		private string GetEsSettingsMappingsPropertyes(IConfigurationBuilder cb)
		{

			var dbToEsTypeMappingsChildren = cb.Build().GetSection("Es").GetSection("DbToEsTypeMappings").GetChildren();

			var temp = from mfc in dbToEsTypeMappingsChildren
					from nt in _tblNameTypes
					where mfc.Key == nt.Type && mfc.Key != null
					select $"\"{nt.Name}\":{cb.GetJson(mfc.Path)}";
			return $"{{{string.Join(",", temp)}}}";
		}

		/// <summary>
		/// 索引表数据写入
		/// </summary>
		[Fact]
		public void IndexMaintain()
		{
			dynamic zConment = _db.QueryAsync($"SELECT TOP 100 *  FROM {_tableName} WHERE SoftID = 3160659").Result.First();
			var requestEntity = new ExpandoObject() as IDictionary<string, object>;
			foreach (var property in zConment)
			{
				if (_tblNameTypes.Any(nv => nv.Name == property.Key))
				{
					requestEntity.Add(property.Key, property.Value);
				}
			}
			var jsonBody = JsonConvert.SerializeObject(requestEntity);
			var request = new HttpRequestMessage(HttpMethod.Post, $"/{_indexName}/{_indexName}")
			{
				Content = new StringContent(jsonBody)
			};
			var response = _client.SendAsync(request).Result;
			var resContent = response.Content.ReadAsStringAsync().Result;
			Assert.True(Regex.IsMatch(resContent, "\"created\":true"));
			Assert.True(response.StatusCode == HttpStatusCode.Created);
		}
	}

	/// <summary>
	/// 拓展类
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// 配置文件构造器的拓展 - 获取指定节点下[eg:"Es:Host"]原始json文本
		/// </summary>
		/// <param name="cb"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetJson(this IConfigurationBuilder cb, string key)
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
		}
	}
}
