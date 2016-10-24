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
		private readonly IDbConnection _db;
		private readonly HttpClient _client;
		private readonly string _tableName;
		private readonly string _indexName;
		// 表信息
		private readonly IEnumerable<dynamic> _tblNameValues;
		public IndexMaintainTest()
		{
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json");
			_configuration = configBuilder.Build();
			_db = new SqlConnection(_configuration["ConnectionStrings:Ali"]);
			_client = new HttpClient { BaseAddress = new Uri(_configuration["Es:Host"]) };
			_tableName = _configuration["Es:TableName"];
			_indexName = _configuration["Es:IndexName"];
			_tblNameValues = _db.QueryAsync($@"
				SELECT COLUMN_NAME AS Name, DATA_TYPE AS Type
				FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = '{_tableName}'").Result;
		}
		/// <summary>
		/// 首次索引维护 - 创建索引表
		/// </summary>
		[Fact]
		public void CreateIndex()
		{
			// 创建ES索引连接
			var request = new HttpRequestMessage(HttpMethod.Post, $"/{_indexName}");
			// 添加映射报文体
			string mappingStr = "{'settings':{'similarity':{'_bm25':{'type':'BM25','b':0}},'mappings':{'@IndexName':{'properties':@Mappings}}}}";
			var mappingsFromConfig = _configuration.GetSection("Es").GetSection("DbToEsTypeMappings").GetChildren();

			string body = mappingStr.Replace("@IndexName", _indexName)
							.Replace("@Mappings", GetEsSettingsMappingsPropertyes(mappingsFromConfig))
							.Replace("'", "\"");
			request.Content = new StringContent(body);
			// 进行连接
			var response = _client.SendAsync(request).Result;
			var content = response.Content.ReadAsStringAsync().Result;
			Assert.True(Regex.IsMatch(content, "true"));
		}

		/// <summary>
		/// ES属性类型映射的配置Json
		/// </summary>
		/// <param name="sections"></param>
		/// <returns></returns>
		private string GetEsSettingsMappingsPropertyes(IEnumerable<IConfigurationSection> sections)
		{
			var mappings = from mfc in sections
						   let nv = _tblNameValues.FirstOrDefault(m => m.Type == mfc.Key)
						   where nv != null
						   select $"\"{nv.Name}\":{{\"type\":\"{mfc.Value}\"}}";
			return $"{string.Join(",", mappings)}";
		}

		/// <summary>
		/// 从配置中寻找ES节点并寻找节点下的相关配置
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		private dynamic GetEsSettings(IConfiguration config)
		{
			IConfigurationSection es = config.GetSection("Es");
			IConfigurationSection esSettings = es.GetSection("settings");
			IConfigurationSection esSettingsMappings = es.GetSection("mappings");
			return null;
		}

		[Fact]
		public void IndexMaintain()
		{
			dynamic zConment = _db.QueryAsync($"SELECT * FROM {_tableName}").Result.First();
			var requestEntity = new ExpandoObject() as IDictionary<string, object>;
			foreach (var property in zConment)
			{
				if (_tblNameValues.Any(nv => nv.Name == property.Key))
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
			Assert.True(Regex.IsMatch(resContent, ""));
			Assert.True(response.StatusCode == HttpStatusCode.Created);
		}
	}
}
