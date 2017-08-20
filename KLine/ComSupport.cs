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
using System.Text;

// ReSharper disable InconsistentNaming
namespace Quokka.UI.WebBrowsers
{
    public struct _LARGE_INTEGER
    {
        public Int64 QuadPart;
    }

    public struct _ULARGE_INTEGER
    {
        public UInt64 QuadPart;
    }

    public struct _tagPROTOCOLDATA
    {
        public uint grfFlags;
        public uint dwState;
        public IntPtr pData;
        public uint cbData;
    }

    public struct BINDINFO
    {
        public UInt32 cbSize;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szExtraInfo;
        public STGMEDIUM stgmedData;
        public UInt32 grfBindInfoF;
        [MarshalAs(UnmanagedType.U4)]
        public BINDVERB dwBindVerb;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szCustomVerb;
        public UInt32 cbStgmedData;
        public UInt32 dwOptions;
        public UInt32 dwOptionsFlags;
        public UInt32 dwCodePage;
        public SECURITY_ATTRIBUTES securityAttributes;
        public Guid iid;
        [MarshalAs(UnmanagedType.IUnknown)]
        public object pUnk;
        public UInt32 dwReserved;
    }

    public struct STGMEDIUM
    {
        [MarshalAs(UnmanagedType.U4)]
        public TYMED enumType;
        public IntPtr u;
        [MarshalAs(UnmanagedType.IUnknown)]
        public object pUnkForRelease;
    }

    public struct SECURITY_ATTRIBUTES
    {
        public UInt32 nLength;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
    }

    public enum TYMED : uint //Should be UInt32 but you can only use C# keywords here.
    {
        TYMED_HGLOBAL = 1,
        TYMED_FILE = 2,
        TYMED_ISTREAM = 4,
        TYMED_ISTORAGE = 8,
        TYMED_GDI = 16,
        TYMED_MFPICT = 32,
        TYMED_ENHMF = 64,
        TYMED_NULL = 0
    }

    public enum BINDVERB : uint
    {
        BINDVERB_GET = 0,
        BINDVERB_POST = 1,
        BINDVERB_PUT = 2,
        BINDVERB_CUSTOM = 3,
    }


    public enum PARSEACTION
    {
        PARSE_CANONICALIZE = 1,
        PARSE_FRIENDLY = PARSE_CANONICALIZE + 1,
        PARSE_SECURITY_URL = PARSE_FRIENDLY + 1,
        PARSE_ROOTDOCUMENT = PARSE_SECURITY_URL + 1,
        PARSE_DOCUMENT = PARSE_ROOTDOCUMENT + 1,
        PARSE_ANCHOR = PARSE_DOCUMENT + 1,
        PARSE_ENCODE = PARSE_ANCHOR + 1,
        PARSE_DECODE = PARSE_ENCODE + 1,
        PARSE_PATH_FROM_URL = PARSE_DECODE + 1,
        PARSE_URL_FROM_PATH = PARSE_PATH_FROM_URL + 1,
        PARSE_MIME = PARSE_URL_FROM_PATH + 1,
        PARSE_SERVER = PARSE_MIME + 1,
        PARSE_SCHEMA = PARSE_SERVER + 1,
        PARSE_SITE = PARSE_SCHEMA + 1,
        PARSE_DOMAIN = PARSE_SITE + 1,
        PARSE_LOCATION = PARSE_DOMAIN + 1,
        PARSE_SECURITY_DOMAIN = PARSE_LOCATION + 1,
        PARSE_ESCAPE = PARSE_SECURITY_DOMAIN + 1,
        PARSE_UNESCAPE = PARSE_ESCAPE + 1,
    }

    public enum QUERYOPTION
    {
        QUERY_EXPIRATION_DATE = 1,
        QUERY_TIME_OF_LAST_CHANGE = QUERY_EXPIRATION_DATE + 1,
        QUERY_CONTENT_ENCODING = QUERY_TIME_OF_LAST_CHANGE + 1,
        QUERY_CONTENT_TYPE = QUERY_CONTENT_ENCODING + 1,
        QUERY_REFRESH = QUERY_CONTENT_TYPE + 1,
        QUERY_RECOMBINE = QUERY_REFRESH + 1,
        QUERY_CAN_NAVIGATE = QUERY_RECOMBINE + 1,
        QUERY_USES_NETWORK = QUERY_CAN_NAVIGATE + 1,
        QUERY_IS_CACHED = QUERY_USES_NETWORK + 1,
        QUERY_IS_INSTALLEDENTRY = QUERY_IS_CACHED + 1,
        QUERY_IS_CACHED_OR_MAPPED = QUERY_IS_INSTALLEDENTRY + 1,
        QUERY_USES_CACHE = QUERY_IS_CACHED_OR_MAPPED + 1,
        QUERY_IS_SECURE = QUERY_USES_CACHE + 1,
        QUERY_IS_SAFE = QUERY_IS_SECURE + 1,
    }

    public enum BSCF : uint
    {
        BSCF_FIRSTDATANOTIFICATION = 0,
        BSCF_INTERMEDIATEDATANOTIFICATION = 1,
        BSCF_LASTDATANOTIFICATION = 2,
        BSCF_DATAFULLYAVAILABLE = 3,
        BSCF_AVAILABLEDATASIZEUNKNOWN = 4,
    }


    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9E1-BAF9-11CE-8C82-00AA004BA90B")]
    [ComVisible(true)]
    public interface IInternetBindInfo
    {
        void GetBindInfo(out UInt32 grfBINDF, [In, Out] ref BINDINFO pbindinfo);

        void GetBindString(UInt32 ulStringType, [MarshalAs(UnmanagedType.LPWStr)] ref string ppwzStr, UInt32 cEl, ref UInt32 pcElFetched);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9E5-BAF9-11CE-8C82-00AA004BA90B")]
    [ComVisible(true)]
    public interface IInternetProtocolSink
    {
        void Switch(ref _tagPROTOCOLDATA pProtocolData);
        void ReportProgress(UInt32 ulStatusCode, [MarshalAs(UnmanagedType.LPWStr)] string szStatusText);
        void ReportData(BSCF grfBSCF, UInt32 ulProgress, UInt32 ulProgressMax);
        void ReportResult(Int32 hrResult, UInt32 dwError, [MarshalAs(UnmanagedType.LPWStr)] string szResult);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9E4-BAF9-11CE-8C82-00AA004BA90B")]
    [ComVisible(true)]
    public interface IInternetProtocol
    {
        //IInternetProtcolRoot
        void Start(
            [MarshalAs(UnmanagedType.LPWStr)] string szURL,
           IInternetProtocolSink Sink,
           IInternetBindInfo pOIBindInfo,
           UInt32 grfPI,
           UInt32 dwReserved);

        void Continue(ref _tagPROTOCOLDATA pProtocolData);

        void Abort(Int32 hrReason, UInt32 dwOptions);

        void Terminate(UInt32 dwOptions);

        void Suspend();

        void Resume();

        //IInternetProtocol
        [PreserveSig()]
        UInt32 Read(IntPtr pv, UInt32 cb, out UInt32 pcbRead);

        void Seek(_LARGE_INTEGER dlibMove, UInt32 dwOrigin, out _ULARGE_INTEGER plibNewPosition);

        void LockRequest(UInt32 dwOptions);

        void UnlockRequest();
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79EAC9E3-BAF9-11CE-8C82-00AA004BA90B")]
    [ComVisible(true)]
    public interface IInternetProtocolRoot
    {
        void Start(
            [MarshalAs(UnmanagedType.LPWStr)]string szURL,
           IInternetProtocolSink Sink,
           IInternetBindInfo pOIBindInfo,
           UInt32 grfPI,
           UInt32 dwReserved);

        void Continue(ref _tagPROTOCOLDATA pProtocolData);

        void Abort(Int32 hrReason, UInt32 dwOptions);

        void Terminate(UInt32 dwOptions);

        void Suspend();

        void Resume();
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9d2-baf9-11ce-8c82-00aa004ba90b")]
    [ComVisible(true)]
    public interface IHttpNegotiate
    {
        void BeginningTransaction(
            [MarshalAs(UnmanagedType.LPWStr)]string szURL,
           [MarshalAs(UnmanagedType.LPWStr)]string szHeaders,
          UInt32 dwReserved,
          [MarshalAs(UnmanagedType.LPWStr)] out string szAdditionalHeaders);
        void OnResponse(
            UInt32 dwResponseCode,
            [MarshalAs(UnmanagedType.LPWStr)]string szResponseHeaders,
           [MarshalAs(UnmanagedType.LPWStr)]string szRequestHeaders,
          [MarshalAs(UnmanagedType.LPWStr)]out string szAdditionalRequestHeaders);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
    [ComVisible(true)]
    public interface IServiceProvider
    {
        void QueryService(
            [In] ref System.Guid guidService,
            [In] ref System.Guid guidType,
            [Out, MarshalAs(UnmanagedType.Interface)] out object Object);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("79eac9ec-baf9-11ce-8c82-00aa004ba90b")]
    [ComVisible(true)]
    public interface IInternetProtocolInfo
    {
        [PreserveSig()]
        UInt32 ParseUrl(
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            /* [in] */ PARSEACTION ParseAction,
               UInt32 dwParseFlags,
               IntPtr pwzResult,
               UInt32 cchResult,
               out UInt32 pcchResult,
               UInt32 dwReserved);

        [PreserveSig()]
        UInt32 CombineUrl(
            [MarshalAs(UnmanagedType.LPWStr)] string pwzBaseUrl,
           [MarshalAs(UnmanagedType.LPWStr)] string pwzRelativeUrl,
          UInt32 dwCombineFlags,
          IntPtr pwzResult,
          UInt32 cchResult,
          out UInt32 pcchResult,
          UInt32 dwReserved);

        [PreserveSig()]
        UInt32 CompareUrl(
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl1,
           [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl2,
          UInt32 dwCompareFlags);

        [PreserveSig()]
        UInt32 QueryInfo(
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
           QUERYOPTION OueryOption,
           UInt32 dwQueryFlags,
           IntPtr pBuffer,
           UInt32 cbBuffer,
           ref UInt32 pcbBuf,
           UInt32 dwReserved);
    };

    public class HRESULT
    {
        public static UInt32 S_OK = 0;
        public static UInt32 S_FALSE = 1;
        public static UInt32 INET_E_DEFAULT_ACTION = 0x800C0011;
        public static UInt32 E_NOINTERFACE = 0x80004002;
    }


    [System.Runtime.InteropServices.ComVisible(false)]
    public class Guids
    {
        public static Guid IID_IUnknown = new Guid("00000000-0000-0000-c000-000000000046");
        public static Guid IID_IHttpNegotiate = new Guid("79eac9d2-baf9-11ce-8c82-00aa004ba90b");

        public static Guid IID_IInternetProtocolInfo = new Guid("79eac9ec-baf9-11ce-8c82-00aa004ba90b");
        public static Guid IID_IInternetProtocolRoot = new Guid("79eac9e3-baf9-11ce-8c82-00aa004ba90b");
        public static Guid IID_IInternetProtocol = new Guid("79EAC9E4-BAF9-11CE-8C82-00AA004BA90B");
    }

    /* Make these constants */
    public enum MIIM : uint
    {
        STATE = 0x00000001,
        ID = 0x00000002,
        SUBMENU = 0x00000004,
        CHECKMARKS = 0x00000008,
        TYPE = 0x00000010,
        DATA = 0x00000020,
        STRING = 0x00000040,
        BITMAP = 0x00000080,
        FTYPE = 0x00000100
    }

    public enum MF : uint
    {
        INSERT = 0x00000000,
        CHANGE = 0x00000080,
        APPEND = 0x00000100,
        DELETE = 0x00000200,
        REMOVE = 0x00001000,
        BYCOMMAND = 0x00000000,
        BYPOSITION = 0x00000400,
        SEPARATOR = 0x00000800,
        ENABLED = 0x00000000,
        GRAYED = 0x00000001,
        DISABLED = 0x00000002,
        UNCHECKED = 0x00000000,
        CHECKED = 0x00000008,
        USECHECKBITMAPS = 0x00000200,
        STRING = 0x00000000,
        BITMAP = 0x00000004,
        OWNERDRAW = 0x00000100,
        POPUP = 0x00000010,
        MENUBARBREAK = 0x00000020,
        MENUBREAK = 0x00000040,
        UNHILITE = 0x00000000,
        HILITE = 0x00000080,
        DEFAULT = 0x00001000,
        SYSMENU = 0x00002000,
        HELP = 0x00004000,
        RIGHTJUSTIFY = 0x00004000,
        MOUSESELECT = 0x00008000
    }

    public enum MFS : uint
    {
        GRAYED = 0x00000003,
        DISABLED = MFS.GRAYED,
        CHECKED = MF.CHECKED,
        HILITE = MF.HILITE,
        ENABLED = MF.ENABLED,
        UNCHECKED = MF.UNCHECKED,
        UNHILITE = MF.UNHILITE,
        DEFAULT = MF.DEFAULT,
        MASK = 0x0000108B,
        HOTTRACKDRAWN = 0x10000000,
        CACHEDBMP = 0x20000000,
        BOTTOMGAPDROP = 0x40000000,
        TOPGAPDROP = 0x80000000,
        GAPDROP = 0xC0000000
    }

    public enum CLIPFORMAT : uint
    {
        CF_TEXT = 1,
        CF_BITMAP = 2,
        CF_METAFILEPICT = 3,
        CF_SYLK = 4,
        CF_DIF = 5,
        CF_TIFF = 6,
        CF_OEMTEXT = 7,
        CF_DIB = 8,
        CF_PALETTE = 9,
        CF_PENDATA = 10,
        CF_RIFF = 11,
        CF_WAVE = 12,
        CF_UNICODETEXT = 13,
        CF_ENHMETAFILE = 14,
        CF_HDROP = 15,
        CF_LOCALE = 16,
        CF_MAX = 17,

        CF_OWNERDISPLAY = 0x0080,
        CF_DSPTEXT = 0x0081,
        CF_DSPBITMAP = 0x0082,
        CF_DSPMETAFILEPICT = 0x0083,
        CF_DSPENHMETAFILE = 0x008E,

        CF_PRIVATEFIRST = 0x0200,
        CF_PRIVATELAST = 0x02FF,

        CF_GDIOBJFIRST = 0x0300,
        CF_GDIOBJLAST = 0x03FF
    }


    public enum DVASPECT : uint
    {
        DVASPECT_CONTENT = 1,
        DVASPECT_THUMBNAIL = 2,
        DVASPECT_ICON = 4,
        DVASPECT_DOCPRINT = 8
    }

    public enum CMF : uint
    {
        CMF_NORMAL = 0x00000000,
        CMF_DEFAULTONLY = 0x00000001,
        CMF_VERBSONLY = 0x00000002,
        CMF_EXPLORE = 0x00000004,
        CMF_NOVERBS = 0x00000008,
        CMF_CANRENAME = 0x00000010,
        CMF_NODEFAULT = 0x00000020,
        CMF_INCLUDESTATIC = 0x00000040,
        CMF_RESERVED = 0xffff0000      // View specific
    }

    // GetCommandString uFlags
    public enum GCS : uint
    {
        VERBA = 0x00000000,     // canonical verb
        HELPTEXTA = 0x00000001,     // help text (for status bar)
        VALIDATEA = 0x00000002,     // validate command exists
        VERBW = 0x00000004,     // canonical verb (unicode)
        HELPTEXTW = 0x00000005,     // help text (unicode version)
        VALIDATEW = 0x00000006,     // validate command exists (unicode)
        UNICODE = 0x00000004,     // for bit testing - Unicode string
        VERB = GCS.VERBA,
        HELPTEXT = GCS.HELPTEXTA,
        VALIDATE = GCS.VALIDATEA
    }

    [StructLayout(LayoutKind.Sequential)]
    public class StartupInfo
    {
        public int cb;
        public String lpReserved;
        public String lpDesktop;
        public String lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public UInt16 wShowWindow;
        public UInt16 cbReserved2;
        public Byte lpReserved2;
        public int hStdInput;
        public int hStdOutput;
        public int hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ProcessInformation
    {
        public int hProcess;
        public int hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENUITEMINFO
    {
        public uint cbSize;
        public uint fMask;
        public uint fType;
        public uint fState;
        public int wID;
        public int  /*HMENU*/      hSubMenu;
        public int  /*HBITMAP*/   hbmpChecked;
        public int  /*HBITMAP*/      hbmpUnchecked;
        public int  /*ULONG_PTR*/ dwItemData;
        public String dwTypeData;
        public uint cch;
        public int /*HBITMAP*/ hbmpItem;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FORMATETC
    {
        public CLIPFORMAT cfFormat;
        public uint ptd;
        public DVASPECT dwAspect;
        public int lindex;
        public TYMED tymed;
    }
    /*
		[StructLayout(LayoutKind.Sequential)]
		public struct STGMEDIUM
		{
			public uint tymed;
			public uint hGlobal;
			public uint pUnkForRelease;
		}
	*/
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct INVOKECOMMANDINFO
    {
        //NOTE: When SEE_MASK_HMONITOR is set, hIcon is treated as hMonitor
        public uint cbSize;                     // sizeof(CMINVOKECOMMANDINFO)
        public uint fMask;                      // any combination of CMIC_MASK_*
        public uint wnd;                        // might be NULL (indicating no owner window)
        public int verb;
        [MarshalAs(UnmanagedType.LPStr)]
        public String parameters;               // might be NULL (indicating no parameter)
        [MarshalAs(UnmanagedType.LPStr)]
        public String directory;                // might be NULL (indicating no specific directory)
        public int Show;                        // one of SW_ values for ShowWindow() API
        public uint HotKey;
        public uint hIcon;
    }

    [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010e-0000-0000-C000-000000000046")]
    [ComVisible(true)]
    public interface IDataObject
    {
        [PreserveSig()]
        int GetData(ref FORMATETC a, ref STGMEDIUM b);
        [PreserveSig()]
        void GetDataHere(int a, ref STGMEDIUM b);
        [PreserveSig()]
        int QueryGetData(int a);
        [PreserveSig()]
        int GetCanonicalFormatEtc(int a, ref int b);
        [PreserveSig()]
        int SetData(int a, int b, int c);
        [PreserveSig()]
        int EnumFormatEtc(uint a, ref Object b);
        [PreserveSig()]
        int DAdvise(int a, uint b, Object c, ref uint d);
        [PreserveSig()]
        int DUnadvise(uint a);
        [PreserveSig()]
        int EnumDAdvise(ref Object a);
    }

    [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214e8-0000-0000-c000-000000000046")]
    [ComVisible(true)]
    public interface IShellExtInit
    {
        [PreserveSig()]
        int Initialize(IntPtr pidlFolder, IntPtr lpdobj, uint /*HKEY*/ hKeyProgID);
    }


    [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214e4-0000-0000-c000-000000000046")]
    [ComVisible(true)]
    public interface IContextMenu
    {
        // IContextMenu methods
        [PreserveSig()]
        int QueryContextMenu(uint hmenu, uint iMenu, int idCmdFirst, int idCmdLast, uint uFlags);
        [PreserveSig()]
        void InvokeCommand(IntPtr pici);
        [PreserveSig()]
        void GetCommandString(int idcmd, uint uflags, int reserved, StringBuilder commandstring, int cch);
    }
}