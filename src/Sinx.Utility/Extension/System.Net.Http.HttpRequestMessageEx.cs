using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinx.Utility.Extension
{
    public static class HttpRequestMessageEx
    {
        /// <summary>
		/// 根据原始的Http请求字符生成HttpRequestMessage
		/// </summary>
		/// <param name="request">用于表示拓展方法的this</param>
		/// <param name="reqRaw">原始Http请求字符</param>
		/// <returns></returns>
		public static HttpRequestMessage CreateFromRaw(this HttpRequestMessage request, string reqRaw)
        {
            // 解析reqRaw
            var splitLine = Regex.Split(reqRaw.Trim(), Environment.NewLine).Select(m => m.Trim()).ToList();
            #region 1. 解析请求行
            // 1. 解析请求行
            var requestLine = Regex.Split(splitLine.FirstOrDefault() ?? "", "\\s");
            if (requestLine.Count() != 3 ||
                !RegexEx.IsUrl(requestLine[1]) ||
                !Regex.IsMatch(requestLine[2].ToLower(), @"http/\d+\.\d+"))
            {
                throw new ArgumentException("请求行解析出错");
            }
            var httpMethod = requestLine[0].Trim();
            var httpUrl = requestLine[1].Trim();
            var httpVersion = requestLine[2].Trim();    // 这里先使用默认的, 不用reqRaw中的
            request.Method = new HttpMethod(httpMethod);
            request.RequestUri = new Uri(httpUrl);
            splitLine.Remove(splitLine.First());
            #endregion
            #region 2. 解析请求体
            // 2. 解析请求体
            if (httpMethod.ToLower() != "get")
            {
                var contentFlag = splitLine.FirstOrDefault(string.IsNullOrWhiteSpace);
                var indexFlag = splitLine.IndexOf(contentFlag);
                string content = "";
                if (contentFlag != null)
                {
                    for (int i = indexFlag; i < splitLine.Count; i++)
                    {
                        content += splitLine[i];
                    }
                    splitLine.RemoveRange(indexFlag, splitLine.Count - indexFlag);
                }
                request.Content = new StringContent(content);
                // 我截取的Content内容可能不跟reqRaw中的完全长度一样, 所以还是依赖框架自己计算Content-Length吧
                splitLine = splitLine.Where(m => !Regex.IsMatch(m, "Content-Length", RegexOptions.IgnoreCase)).ToList();
            }
            #endregion
            // 3. 解析请求头
            foreach (var keyvalue in splitLine)
            {
                var keyValue = keyvalue.Split(":".ToCharArray(), 2);
                var key = keyValue.FirstOrDefault()?.Trim();
                var value = keyValue.LastOrDefault()?.Trim();
                if (key == null)
                {
                    throw new ArgumentException("请求头解析出错");
                }
                // 先尝试添加到HttpRequestHeaders中, 如果不行尝试添加到HttpContentHeaders中
                var removeOk = false;
                try
                {
                    request.Headers.Remove(key);
                    removeOk = true;
                }
                catch (Exception)
                {
                    // ignored
                }
                // 如果可以添加到HttpRequestHeaders中
                if (removeOk && request.Headers.TryAddWithoutValidation(key, value)) continue;
                // 尝试添加到HttpContentHeaders中
                removeOk = false;
                try
                {
                    request.Content.Headers.Remove(key);
                    removeOk = true;
                }
                catch (Exception)
                {
                    // ignored
                }
                if (!removeOk || !request.Content.Headers.TryAddWithoutValidation(key, value))
                {
                    throw new ArgumentException(nameof(key));
                }
            }
            return request;
        }
        public static HttpRequestMessage CreateFromRaw(string reqRaw)
        {
            var reuqest = new HttpRequestMessage();
            return CreateFromRaw(reuqest, reqRaw);
        }
    }
}
