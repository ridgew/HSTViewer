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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Quokka.UI.WebBrowsers
{
	[ComVisible(true)]
	[Guid("79eac9e7-baf9-11ce-8c82-00aa004ba90b")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IInternetSession
	{
		[PreserveSig]
		int RegisterNameSpace(
			[In] IClassFactory classFactory,
			[In] ref Guid rclsid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pwzProtocol,
			[In]
            int cPatterns,
			[In, MarshalAs(UnmanagedType.LPWStr)]
            string ppwzPatterns,
			[In] int dwReserved);

		[PreserveSig]
		int UnregisterNameSpace(
			[In] IClassFactory classFactory,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszProtocol);

		int Bogus1();

		int Bogus2();

		int Bogus3();

		int Bogus4();

		int Bogus5();
	}
}
