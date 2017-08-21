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

using HSTViewer.KLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Quokka.UI.WebBrowsers
{
    public class EmbeddedResourceMap
    {
        private readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();

#if NET40
        private Lazy<Dictionary<string, EmbeddedResource>> _lazy;
        private Dictionary<string, EmbeddedResource> GetDictionary()
        {
            return _lazy.Value;
        }
        private void ClearDict()
        {
            _lazy = new Lazy<Dictionary<string, EmbeddedResource>>(CreateDict, true);
        }
#endif

#if NET35
		private Dictionary<string, EmbeddedResource> _dict;
		private Dictionary<string, EmbeddedResource> GetDictionary() 
		{
			if (_dict == null) 
			{
				_dict = CreateDict();
			}
			return _dict;
		}
		private void ClearDict() 
		{
			_dict = null;
		}
#endif


        public EmbeddedResourceMap()
        {
            ClearDict();
        }

        public void AddAssembly(Assembly assembly)
        {
            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);
                ClearDict();
            }
        }

        public IUrlResourceStream OtherResourceStream { get; set; }

        public Stream GetStream(string name)
        {
            Uri uri = new Uri(name);

            var path = uri.AbsolutePath;
            var pieces = path.Split('/');
            var fileName = pieces[pieces.Length - 1];

            EmbeddedResource manifestInfo = null;
            if (GetDictionary().TryGetValue(fileName, out manifestInfo))
            {
                var rawStream = manifestInfo.Assembly.GetManifestResourceStream(manifestInfo.ResourceName);
                if (rawStream.Length > 2 && rawStream.ReadByte() == 0x50 && rawStream.ReadByte() == 0x4B) //PK
                {
                    return ZipStorer.DecompressStream(rawStream);
                }
                return rawStream;
            }
            else
            {
                if (OtherResourceStream != null)
                    return OtherResourceStream.GetByFullUrl(name);
            }
            return null;
        }

        private static readonly string[] Suffixes = new[] { ".html", ".htm", ".js", ".css", ".png", ".jpeg", ".gif", };

        private static bool IsValidResource(string name)
        {
            return Suffixes.Any(name.EndsWith);
        }

        private Dictionary<string, EmbeddedResource> CreateDict()
        {
            var dict = new Dictionary<string, EmbeddedResource>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var assembly in _assemblies)
            {
                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    if (IsValidResource(resourceName))
                    {
                        var embeddedResource = new EmbeddedResource(assembly, resourceName);

                        // split name into bits separated by periods
                        var nameParts = resourceName.Split('.');

                        var fileName = nameParts[nameParts.Length - 2] + "." + nameParts[nameParts.Length - 1];

                        dict[fileName] = embeddedResource;

                        // TODO: add alternatives that include the rest of the path
                    }
                }
            }
            return dict;
        }

        private class EmbeddedResource
        {
            public Assembly Assembly { get; private set; }

            public string ResourceName { get; private set; }

            public EmbeddedResource(Assembly assembly, string resourceName)
            {
                Assembly = assembly;
                ResourceName = resourceName;
            }
        }
    }

    public interface IUrlResourceStream
    {
        Stream GetByFullUrl(string url);
    }
}