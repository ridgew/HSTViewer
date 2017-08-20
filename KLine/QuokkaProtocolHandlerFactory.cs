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
using System.Runtime.InteropServices;
using System.Security;

namespace Quokka.UI.WebBrowsers
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	public class QuokkaProtocolHandlerFactory : IClassFactory
	{
		private readonly EmbeddedResourceMap _embeddedResourceMap;

		public QuokkaProtocolHandlerFactory(EmbeddedResourceMap embeddedResourceMap)
		{
			_embeddedResourceMap = embeddedResourceMap;
		}

		[SecuritySafeCritical]
		public uint CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
		{
			if (riid == Guids.IID_IUnknown || riid == Guids.IID_IInternetProtocolInfo)
			{
				IInternetProtocolInfo p = new QuokkaProtocolHandler(_embeddedResourceMap);
				ppvObject = Marshal.GetComInterfaceForObject(p, typeof (IInternetProtocolInfo));
				return HRESULT.S_OK;
			}

			if (riid == Guids.IID_IInternetProtocolRoot)
			{
				IInternetProtocolRoot p = new QuokkaProtocolHandler(_embeddedResourceMap);
				ppvObject = Marshal.GetComInterfaceForObject(p, typeof (IInternetProtocolRoot));
				return HRESULT.S_OK;
			}

			if (riid == Guids.IID_IInternetProtocol)
			{
				IInternetProtocol p = new QuokkaProtocolHandler(_embeddedResourceMap);
				ppvObject = Marshal.GetComInterfaceForObject(p, typeof (IInternetProtocol));
				return HRESULT.S_OK;
			}

			ppvObject = IntPtr.Zero;
			return HRESULT.E_NOINTERFACE;
		}

		public uint LockServer(bool fLock)
		{
			return HRESULT.S_OK;
		}
	}
}