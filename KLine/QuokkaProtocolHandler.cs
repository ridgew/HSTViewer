#region License

// Copyright 2004-2014 John Jeffery
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace Quokka.UI.WebBrowsers
{
	/// <summary>
	/// Summary description for EchoProtocol.
	/// </summary>
	[Guid(QuokkaProtocolHandler.Guid)]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	public class QuokkaProtocolHandler : ProtocolBase, IInternetProtocol, IInternetProtocolRoot, IInternetProtocolInfo
	{
		public const string Guid = "fa860b23-5511-4067-8091-a89932c1500c";

		public QuokkaProtocolHandler(EmbeddedResourceMap embeddedResourceMap)
		{
			_embeddedResourceMap = embeddedResourceMap;
		}

		private readonly EmbeddedResourceMap _embeddedResourceMap;

		#region IInternetProtocol Members

		public void Start(string szURL, IInternetProtocolSink Sink, IInternetBindInfo pOIBindInfo, uint grfPI, uint dwReserved)
		{
			Debug.WriteLine("Start:" + szURL, "Info");
			try
			{
				if (Sink is IServiceProvider)
				{
					Debug.WriteLine("ServiceProvider");
					IServiceProvider Provider = (IServiceProvider)Sink;
					object obj_Negotiate = new object();
					Provider.QueryService(ref Guids.IID_IHttpNegotiate, ref Guids.IID_IHttpNegotiate, out obj_Negotiate);
					IHttpNegotiate Negotiate = (IHttpNegotiate)obj_Negotiate;

					string strNewHeaders;
					Negotiate.BeginningTransaction(szURL, string.Empty, 0, out strNewHeaders);
					Debug.WriteLine(strNewHeaders);

					using (var resourceStream = _embeddedResourceMap.GetStream(szURL))
					{
						if (resourceStream == null)
						{
							StreamWriter Writer = new StreamWriter(Stream);
							Writer.Write("<html><body><p>Cannot find resource: {0}</p></body></html>", szURL);
							Writer.Flush();
							Stream.Position = 0;
						}
						else
						{
							resourceStream.CopyTo(Stream);
							Stream.Position = 0;
						}
					}

					string StrResponseHeaders = string.Format("HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nContent-Length:{0}\r\n\r\n", Stream.Length);
					string strNewResponseHeaders;
					Negotiate.OnResponse(200, StrResponseHeaders, strNewHeaders, out strNewResponseHeaders);
					Debug.WriteLine(strNewResponseHeaders);
				}

				Sink.ReportData(BSCF.BSCF_LASTDATANOTIFICATION, (uint)Stream.Length, (uint)Stream.Length);
				Sink.ReportResult(0, 200, null);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		#endregion

		[SecuritySafeCritical]
		public uint ParseUrl(string pwzUrl, PARSEACTION ParseAction, uint dwParseFlags, IntPtr pwzResult, uint cchResult, out uint pcchResult, uint dwReserved)
		{
			Debug.WriteLine("ParseUrl:" + pwzUrl);
			pcchResult = 0;
			return HRESULT.INET_E_DEFAULT_ACTION;

//			byte[] bytes = Encoding.Unicode.GetBytes(pwzUrl);
//			Marshal.Copy(bytes, 0, pwzResult, bytes.Length);
//			pcchResult = (uint)pwzUrl.Length;
//			return HRESULT.S_OK;
		}

		public uint CombineUrl(string pwzBaseUrl, string pwzRelativeUrl, uint dwCombineFlags, IntPtr pwzResult, uint cchResult, out uint pcchResult, uint dwReserved)
		{
			Debug.WriteLine("CombineUrl:" + pwzBaseUrl + "-" + pwzRelativeUrl);
			pcchResult = 0;
			return HRESULT.INET_E_DEFAULT_ACTION;
		}

		public uint CompareUrl(string pwzUrl1, string pwzUrl2, uint dwCompareFlags)
		{
			return HRESULT.INET_E_DEFAULT_ACTION;
		}

		public uint QueryInfo(string pwzUrl, QUERYOPTION OueryOption, uint dwQueryFlags, IntPtr pBuffer, uint cbBuffer, ref uint pcbBuf, uint dwReserved)
		{
			return HRESULT.INET_E_DEFAULT_ACTION;
		}
	}
}