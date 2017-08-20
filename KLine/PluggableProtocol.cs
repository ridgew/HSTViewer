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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace Quokka.UI.WebBrowsers
{
    public static class PluggableProtocol
    {
        private static bool _isRegistered;
        private static IClassFactory _classFactory;
        private static readonly EmbeddedResourceMap EmbeddedResourceMap = new EmbeddedResourceMap();
        internal const string SchemeName = "hstv";

        public static void Register(IUrlResourceStream otherRes, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                EmbeddedResourceMap.AddAssembly(assembly);
            }

            if (!_isRegistered)
            {
                EmbeddedResourceMap.OtherResourceStream = otherRes;
                var internetSession = GetInternetSession();
                var factory = new QuokkaProtocolHandlerFactory(EmbeddedResourceMap);
                var guid = new Guid(QuokkaProtocolHandler.Guid);
                var hr = internetSession.RegisterNameSpace(factory, ref guid, SchemeName, 0, null, 0);
                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
                _isRegistered = true;
                _classFactory = factory;
            }
        }

        public static void Unregister()
        {
            if (_isRegistered)
            {
                var internetSession = GetInternetSession();
                internetSession.UnregisterNameSpace(_classFactory, SchemeName);
            }
        }

        [SecuritySafeCritical]
        private static IInternetSession GetInternetSession()
        {
            IInternetSession internetSession = null;
            var hr = CoInternetGetSession(0, ref internetSession, 0);
            if (hr != 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
            return internetSession;
        }


        [DllImport("urlmon.dll")]
        private static extern int CoInternetGetSession(UInt32 dwSessionMode /* = 0 */,
                                                       ref IInternetSession ppIInternetSession,
                                                       UInt32 dwReserved /* = 0 */);

    }
}
