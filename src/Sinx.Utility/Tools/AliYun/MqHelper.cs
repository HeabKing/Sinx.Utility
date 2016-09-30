using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sinx.Utility.Tools.AliYun
{
	public class MqHelper : IDisposable
	{
		/// <summary>
		/// 发送消息队列/接收请求的客户端
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="accessKeyId"></param>
		/// <param name="accessKeySecret"></param>
		/// <param name="producerId">生产者Id, 默认为PID_{topic}, 若仅作为消费者可以不设置</param>
		/// <param name="consumerId">消费者Id, 默认为CID_{topic}, 若仅作为生产者可以不设置</param>
		public MqHelper(string topic, string accessKeyId, string accessKeySecret, string producerId = "default", string consumerId = "default")
		{
			_topic = topic;
			_accessKeyId = accessKeyId;
			_accessKeySecret = accessKeySecret;
			_producerId = producerId == "default" ? "PID_" + _topic : producerId;
			_consumerId = consumerId == "default" ? "CID_" + _topic : consumerId;
			_httpClient.DefaultRequestHeaders.Add("AccessKey", accessKeyId);
		}

		/// <summary>
		/// 发送消息队列
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="key"></param>
		/// <param name="body"></param>
		/// <returns></returns>
		public async Task<bool> SendAsync(string tag, string key, string body)
		{
			var time = GetTime();
			var url = $"/message/?topic={_topic}&time={time}&tag={tag}&key={key}";
			var request = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = new StringContent(body)
			};
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
			request.Headers.Add("Signature", GetSignature(_accessKeySecret, $"{_topic}\n{_producerId}\n{Md5(body)}\n{time}"));
			request.Headers.Add("ProducerID", _producerId);
			var res = await _httpClient.SendAsync(request);
			
			return res.StatusCode == System.Net.HttpStatusCode.Created;
		}

		/// <summary>
		/// 获取消息
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public async Task<IList<MqMessage>> GetAsync(string tag = "*")    // TODO tag
		{
			if (tag != "*")
			{
				throw new NotImplementedException("获取特定tag的MQ功能还没实现呢");
			}
			var url = $"/message/?topic={_topic}&time={GetTime()}&num=32";  // TODO 32?
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Add("Signature", GetSignature(_accessKeySecret, $"{_topic}\n{_consumerId}\n{GetTime()}"));
			request.Headers.Add("ConsumerID", _consumerId);
			var response = await _httpClient.SendAsync(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				return JsonConvert.DeserializeObject<MqMessage[]>(await response.Content.ReadAsStringAsync());
			}
			return new List<MqMessage>();
		}

		/// <summary>
		/// 从消息队列中删除一条消息
		/// </summary>
		/// <param name="msgHandle"></param>
		/// <returns></returns>
		public async Task<bool> DeleteAsync(string msgHandle)
		{
			var time = GetTime();
			var url = $"/message/?topic={_topic}&time={time}&msgHandle={msgHandle}";
			var request = new HttpRequestMessage(HttpMethod.Delete, url);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
			request.Headers.Add("Signature", GetSignature(_accessKeySecret, $"{_topic}\n{_consumerId}\n{msgHandle}\n{time}"));
			request.Headers.Add("ConsumerID", _consumerId);
			var response = await _httpClient.SendAsync(request);
			return response.StatusCode == System.Net.HttpStatusCode.NoContent;
		}

		public class MqMessage
		{
			public string MsgId { get; set; }
			public string Tag { get; set; }
			public string Body { get; set; }
			public string BornTime { get; set; }
			public string MsgHandle { get; set; }
			public int ReconsumeTimes { get; set; }
		}

		private readonly string _topic;
		/// <summary>
		/// 阿里云身份验证码
		/// </summary>
		private string _accessKeyId;
		/// <summary>
		/// 阿里云身份验证密钥
		/// </summary>
		private readonly string _accessKeySecret;
		/// <summary>
		/// 生产者Id
		/// </summary>
		private readonly string _producerId;
		/// <summary>
		/// 消费者Id
		/// </summary>
		private readonly string _consumerId;

		/// <summary>
		/// Http协议下的发送请求/接收响应客户端
		/// </summary>
		private readonly HttpClient _httpClient = new HttpClient
		{
			BaseAddress = new Uri("http://publictest-rest.ons.aliyun.com")
		};

		/// <summary>
		/// Md5, 32位小写
		/// </summary>
		private static readonly Func<string, string> Md5 = input => System.Security.Cryptography.MD5.Create()
			.ComputeHash(Encoding.UTF8.GetBytes(input))
			.Aggregate(new StringBuilder(), (sb, index) => sb.Append(index.ToString("x2")))
			.ToString();

		/// <summary>
		/// 获取指定格式的时间
		/// </summary>
		private static readonly Func<long> GetTime = () => (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

		/// <summary>
		/// 获取签名
		/// </summary>
		/// <param name="accessKeySecret"></param>
		/// <param name="signatureValue"></param>
		/// <param name="isRaw"></param>
		/// <returns></returns>
		private static string GetSignature(string accessKeySecret, string signatureValue, bool isRaw = true)
		{
			var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(accessKeySecret));
			hmac.Initialize();
			var buffer = Encoding.UTF8.GetBytes(signatureValue);
			if (isRaw)
			{
				var ret = hmac.ComputeHash(buffer);
				return Convert.ToBase64String(ret);
			}
			else
			{
				var res = BitConverter.ToString(hmac.ComputeHash(buffer)).Replace("-", "").ToLower();
				return Convert.ToBase64String(Encoding.UTF8.GetBytes(res));
			}
		}

		public void Dispose()
		{
			_httpClient.Dispose();
		}
	}
}
