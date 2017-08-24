using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HK24kTickData
{
    /// <summary>
    /// 支持 Cookie 的 WebClient。
    /// </summary>
    /// <remarks>
    /// URL:http://www.cnblogs.com/anjou/archive/2008/05/25/1206832.html
    /// </remarks>
    public class HttpClient : WebClient
    {
        // Cookie 容器
        private CookieContainer cookieContainer;

        /// <summary>
        /// 创建一个新的 WebClient 实例。
        /// </summary>
        public HttpClient()
        {
            cookieContainer = new CookieContainer();
        }

        /// <summary>
        /// 基于现有的Cookie容器，创建一个新的 WebClient 实例。
        /// </summary>
        /// <param name="cookies">Cookie 容器</param>
        public HttpClient(CookieContainer cookies)
        {
            this.cookieContainer = cookies;
        }

        /// <summary>
        /// Cookie 容器
        /// </summary>
        public CookieContainer Cookies
        {
            get { return this.cookieContainer; }
            set { this.cookieContainer = value; }
        }

        /// <summary>
        /// 默认超时时间为20秒
        /// </summary>
        int _httpTimeoutDefault = 20 * 1000;
        /// <summary>
        /// 获取或设置HTTP请求的超时时间，单位为毫秒。（默认为20秒）
        /// </summary>
        public int HttpTimeout
        {
            get { return _httpTimeoutDefault; }
            set { _httpTimeoutDefault = value; }
        }

        /// <summary>
        /// 返回带有 Cookie 的 HttpWebRequest。
        /// </summary>
        /// <param name="address">一个 <see cref="T:System.Uri"/>，用于标识要请求的资源。</param>
        /// <returns>
        /// 一个新的 <see cref="T:System.Net.WebRequest"/> 对象，用于指定的资源。
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                HttpWebRequest httpRequest = request as HttpWebRequest;
                httpRequest.CookieContainer = cookieContainer;

                //以毫秒为单位
                httpRequest.Timeout = _httpTimeoutDefault;
                //                System.Net.ServicePointManager.Expect100Continue = false;
                //                httpRequest.KeepAlive = false;
                //                httpRequest.ProtocolVersion = System.Net.HttpVersion.Version10;
            }
            return request;
        }

        /// <summary>
        /// 返回对指定 <see cref="T:System.Net.WebRequest"/> 的 <see cref="T:System.Net.WebResponse"/>。
        /// </summary>
        /// <param name="request">用于获取响应的 <see cref="T:System.Net.WebRequest"/>。</param>
        /// <returns>
        /// 	<see cref="T:System.Net.WebResponse"/> 包含对指定 <see cref="T:System.Net.WebRequest"/> 的响应。
        /// </returns>
        /// <remarks>TOTest</remarks>
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            if (response is HttpWebResponse)
            {
                HttpWebResponse httpResponse = response as HttpWebResponse;
                #region 可能更新Cookie
                if (httpResponse.Cookies.Count > 0)
                {
                    cookieContainer.Add(httpResponse.Cookies);
                }
                #endregion
            }
            return response;
        }

    }
}
