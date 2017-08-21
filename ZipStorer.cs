// ZipStorer, by Jaime Olivares
// Website: zipstorer.codeplex.com
// Version: 2.35 (March 14, 2010)

/*
 Ridge Wong,  vbyte@163.com
 Ridge, 添加zip文件读取时，如果存在注释则读取注释, 2010-4-10。
 Ridge, 添加单一文件对内存流的转换静态方法，2017-8-21。
 */

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace System.IO
{
    /// <summary>
    /// Unique class for compression/decompression file. Represents a Zip file.
    /// </summary>
    public class ZipStorer : IDisposable
    {
        /// <summary>
        /// Compression method enumeration
        /// <remarks>
        /// http://en.wikipedia.org/wiki/ZIP_(file_format)
        /// http://zh.wikipedia.org/zh-cn/ZIP_(%E6%96%87%E4%BB%B6%E6%A0%BC%E5%BC%8F)
        /// </remarks>
        /// </summary>
        public enum Compression : ushort
        {
            /// <summary>Uncompressed storage</summary> 
            Store = 0,
            /// <summary>Deflate compression method, 32K windows zie.</summary>
            Deflate = 8,

            ///// <summary>
            ///// Enhanced Deflate compression method, 64K windows zie.
            ///// </summary>
            //EnhanceDeflate = 9,
            ///// <summary>
            ///// Reserved by PKWARE
            ///// </summary>
            //Reserved = 11,
            ///// <summary>
            ///// http://en.wikipedia.org/wiki/Bzip2
            ///// </summary>
            //Bzip2 = 12

        }

        /// <summary>
        /// Represents an entry in Zip file directory
        /// </summary>
        public struct ZipFileEntry
        {
            /// <summary>Compression method</summary>
            public Compression Method;
            /// <summary>Full path and filename as stored in Zip</summary>
            public string FilenameInZip;
            /// <summary>Original file size</summary>
            public uint FileSize;
            /// <summary>Compressed file size</summary>
            public uint CompressedSize;
            /// <summary>Offset of header information inside Zip storage</summary>
            public uint HeaderOffset;
            /// <summary>Offset of file inside Zip storage</summary>
            public uint FileOffset;
            /// <summary>Size of header information</summary>
            public uint HeaderSize;
            /// <summary>32-bit checksum of entire file</summary>
            public uint Crc32;
            /// <summary>Last modification time of file</summary>
            public DateTime ModifyTime;
            /// <summary>User comment for file</summary>
            public string Comment;
            /// <summary>True if UTF8 encoding for filename and comments, false if default (CP 437)</summary>
            public bool EncodeUTF8;

            /// <summary>Overriden method</summary>
            /// <returns>Filename in Zip</returns>
            public override string ToString()
            {
                return this.FilenameInZip;
            }
        }

        #region Public fields
        /// <summary>True if UTF8 encoding for filename and comments, false if default (CP 437)</summary>
        public bool EncodeUTF8 = false;
        /// <summary>Force deflate algotithm even if it inflates the stored file. Off by default.</summary>
        public bool ForceDeflating = false;
        /// <summary>
        /// General comment
        /// </summary>
        public string Comment = "";
        #endregion

        #region Private fields
        // List of files to store
        private List<ZipFileEntry> Files = new List<ZipFileEntry>();
        // Filename of storage file
        private string FileName;
        // Stream object of storage file
        private Stream ZipFileStream;
        // Central dir image
        private byte[] FileDataImage = null;
        // Existing files in zip
        private ushort ExistingFileNumber = 0;
        // File access for Open method
        private FileAccess Access;
        // Static CRC32 Table
        private static UInt32[] CrcTable = null;

        // United States (DOS) 437
        // Default filename encoder
        internal static Encoding DefaultEncoding = Encoding.Default; //Encoding.GetEncoding(437);
        #endregion

        #region Public methods
        // Static constructor. Just invoked once in order to create the CRC32 lookup table.
        static ZipStorer()
        {
            // Generate CRC32 table
            CrcTable = new UInt32[256];
            for (int i = 0; i < CrcTable.Length; i++)
            {
                UInt32 c = (UInt32)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((c & 1) != 0)
                        c = 3988292384 ^ (c >> 1);
                    else
                        c >>= 1;
                }
                CrcTable[i] = c;
            }
        }

        /// <summary>
        /// Method to create a new storage file
        /// </summary>
        /// <param name="_filename">Full path of Zip file to create</param>
        /// <param name="_comment">General comment for Zip file</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Create(string _filename, string _comment)
        {
            Stream stream = new FileStream(_filename, FileMode.Create, FileAccess.ReadWrite);

            ZipStorer zip = Create(stream, _comment);
            zip.Comment = _comment;
            zip.FileName = _filename;

            return zip;
        }

        /// <summary>
        /// Method to create a new zip storage in a stream
        /// </summary>
        /// <param name="_stream"></param>
        /// <param name="_comment"></param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Create(Stream _stream, string _comment)
        {
            ZipStorer zip = new ZipStorer();
            zip.Comment = _comment;
            zip.ZipFileStream = _stream;
            zip.Access = FileAccess.Write;

            return zip;
        }

        /// <summary>
        /// 只包含一个文件的zip storage
        /// </summary>
        /// <param name="filefullPath">文件路径</param>
        /// <returns></returns>
        public static MemoryStream SingleFileStream(string filefullPath)
        {
            MemoryStream zip = new MemoryStream();
            ZipStorer.DefaultEncoding = Encoding.UTF8;
            using (var zs = ZipStorer.Create(zip, null))
            {
                FileInfo fi = new FileInfo(filefullPath);
                zs.AddFile(ZipStorer.Compression.Deflate, fi.FullName, fi.Name, null);
            }
            return zip;
        }

        /// <summary>
        /// 解压一个文件为MemoryStream
        /// </summary>
        /// <returns></returns>
        public static MemoryStream DecompressStream(Stream zip)
        {
            MemoryStream raw = new MemoryStream();
            ZipStorer.DefaultEncoding = Encoding.UTF8;
            using (var zs = ZipStorer.Open(zip, FileAccess.Read))
            {
                if (zs.Files != null && zs.Files.Count > 0)
                {
                    zs.ExtractFile(zs.Files[0], raw);
                }
                raw.Flush();
                raw.Position = 0;
            }
            return raw;
        }

        public static void CreateZipFromDir(string zipFilePath, string zipDir, Predicate<string> ignoreItemHandler)
        {
            using (ZipStorer zip = Create(zipFilePath, ""))
            {
                string[] allFiles = Directory.GetFiles(zipDir, "*.*", SearchOption.AllDirectories);
                int trimLen = zipDir.Length;
                foreach (string fileItem in allFiles)
                {
                    if ((ignoreItemHandler == null) || !ignoreItemHandler(fileItem))
                    {
                        zip.AddFile(Compression.Deflate, fileItem, fileItem.Substring(trimLen), "");
                    }
                }
            }
        }

        public static void ExtractZipToDir(string zipFilePath, string extraDir)
        {
            using (ZipStorer zip = Open(zipFilePath, FileAccess.Read))
            {
                if (!Directory.Exists(extraDir)) Directory.CreateDirectory(extraDir);
                zip.ReadCentralDirAction(delegate (ZipFileEntry f)
                {
                    zip.ExtractFile(f, Path.Combine(extraDir, f.FilenameInZip));
                });
            }
        }


        /// <summary>
        /// 释放包内文件到当前目录下并保留覆盖文件的文件备份
        /// </summary>
        /// <param name="zipFilePath">要释放的zip包文件</param>
        /// <param name="extraDir">释放目录</param>
        /// <param name="overrideFpkgPath">覆盖文件备份包名称,为空或不传值则自动命名</param>
        public static void ExtractZipToDirWithOverrideBak(string zipFilePath, string extraDir, string overrideFpkgPath)
        {
            if (string.IsNullOrEmpty(overrideFpkgPath))
            {
                int idx = zipFilePath.IndexOf('.');
                overrideFpkgPath = zipFilePath.Substring(0, idx) + "_overridebak.zip";
            }

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            int successCount = 0, failedBakCount = 0, overrideCount = 0;
            using (ZipStorer zipBak = ZipStorer.Create(overrideFpkgPath, ""))
            {
                using (ZipStorer zip = Open(zipFilePath, FileAccess.Read))
                {
                    if (!Directory.Exists(extraDir)) Directory.CreateDirectory(extraDir);

                    zip.ReadCentralDirAction(delegate (ZipFileEntry f)
                    {
                        string targetFilePath = Path.Combine(extraDir, f.FilenameInZip);
                        bool backupSuccess = true;
                        if (File.Exists(targetFilePath))
                        {
                            sw.Write(string.Format("#备份文件{0}", f.FilenameInZip));
                            try
                            {
                                zipBak.AddFile(Compression.Deflate, targetFilePath, f.FilenameInZip, "");
                                sw.WriteLine(" OK!");
                                successCount++;
                            }
                            catch (Exception bakEx)
                            {
                                backupSuccess = false;
                                sw.WriteLine();
                                sw.WriteLine(string.Format("\n#{0} 备份失败 \n#{1}", f.FilenameInZip, bakEx.Message.Replace("\n", "\n#")));
                                failedBakCount++;
                            }

                        }

                        if (!backupSuccess)
                        {
                            sw.WriteLine(string.Format("## {0} 因备份失败未更新", f.FilenameInZip));
                        }
                        else
                        {
                            try
                            {
                                zip.ExtractFile(f, targetFilePath);
                                overrideCount++;
                            }
                            catch (Exception ioEx)
                            {
                                sw.WriteLine(string.Format("\n##*{0} 覆盖失败 \n#{1}", f.FilenameInZip, ioEx.Message.Replace("\n", "\n#")));
                            }
                        }
                    });
                }

            }

            sw.WriteLine(string.Format("# -> 共备份成功{0}个文件，{1}个失败，覆盖{2}个文件，{3}个文件未被更新！", successCount, failedBakCount, overrideCount, successCount - overrideCount));

            sw.Flush();
            sw.Close();

            File.WriteAllBytes(overrideFpkgPath.Replace(".zip", ".log"), ms.ToArray());

            ms.Close();
            ms.Dispose();
        }


        /// <summary>
        /// Method to open an existing storage file
        /// </summary>
        /// <param name="_filename">Full path of Zip file to open</param>
        /// <param name="_access">File access mode as used in FileStream constructor</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Open(string _filename, FileAccess _access)
        {
            Stream stream = (Stream)new FileStream(_filename, FileMode.Open,
                _access == FileAccess.Read ? FileAccess.Read : FileAccess.ReadWrite);

            ZipStorer zip = Open(stream, _access);
            zip.FileName = _filename;

            return zip;
        }


        /// <summary>
        /// Method to open an existing storage from stream
        /// </summary>
        /// <param name="_stream">Already opened stream with zip contents</param>
        /// <param name="_access">File access mode for stream operations</param>
        /// <returns>A valid ZipStorer object</returns>
        public static ZipStorer Open(Stream _stream, FileAccess _access)
        {
            if (!_stream.CanSeek && _access != FileAccess.Read)
                throw new InvalidOperationException("Stream cannot seek");

            ZipStorer zip = new ZipStorer();
            //zip.FileName = _filename;
            zip.ZipFileStream = _stream;
            zip.Access = _access;

            if (zip.ReadFileInfo())
            {
                zip.ReadCentralDirAction(zip.Files.Add);
                return zip;
            }

            throw new System.IO.InvalidDataException();
        }


        /// <summary>
        /// Add full contents of a file into the Zip storage
        /// </summary>
        /// <param name="_method">Compression method</param>
        /// <param name="_pathname">Full path of file to add to Zip storage</param>
        /// <param name="_filenameInZip">Filename and path as desired in Zip directory</param>
        /// <param name="_comment">Comment for stored file</param>        
        public void AddFile(Compression _method, string _pathname, string _filenameInZip, string _comment)
        {
            if (Access == FileAccess.Read)
                throw new InvalidOperationException("Writing is not allowed");

            FileStream stream = new FileStream(_pathname, FileMode.Open, FileAccess.Read);
            AddStream(_method, _filenameInZip, stream, File.GetLastWriteTime(_pathname), _comment);
            stream.Close();
        }

        /// <summary>
        /// Add full contents of a stream into the Zip storage
        /// </summary>
        /// <param name="_method">Compression method</param>
        /// <param name="_filenameInZip">Filename and path as desired in Zip directory</param>
        /// <param name="_source">Stream object containing the data to store in Zip</param>
        /// <param name="_modTime">Modification time of the data to store</param>
        /// <param name="_comment">Comment for stored file</param>
        public void AddStream(Compression _method, string _filenameInZip, Stream _source, DateTime _modTime, string _comment)
        {
            if (Access == FileAccess.Read)
                throw new InvalidOperationException("Writing is not allowed");

            long offset;
            if (this.Files.Count == 0)
                offset = 0;
            else
            {
                ZipFileEntry last = this.Files[this.Files.Count - 1];
                offset = last.HeaderOffset + last.HeaderSize;
            }

            // Prepare the fileinfo
            ZipFileEntry zfe = new ZipFileEntry();
            zfe.Method = _method;
            zfe.EncodeUTF8 = this.EncodeUTF8;
            zfe.FilenameInZip = NormalizedFilename(_filenameInZip);
            zfe.Comment = (_comment == null ? "" : _comment);

            // Even though we write the header now, it will have to be rewritten, since we don't know compressed size or crc.
            zfe.Crc32 = 0;  // to be updated later
            zfe.HeaderOffset = (uint)this.ZipFileStream.Position;  // offset within file of the start of this local record
            zfe.ModifyTime = _modTime;

            // Write local header
            WriteLocalHeader(ref zfe);
            zfe.FileOffset = (uint)this.ZipFileStream.Position;

            // Write file to zip (store)
            Store(ref zfe, _source);
            _source.Close();

            this.UpdateCrcAndSizes(ref zfe);

            Files.Add(zfe);
        }

        /// <summary>
        /// Updates central directory (if pertinent) and close the Zip storage
        /// </summary>
        /// <remarks>This is a required step, unless automatic dispose is used</remarks>
        public void Close()
        {
            if (this.Access != FileAccess.Read && this.ZipFileStream != null)
            {
                uint centralOffset = (uint)this.ZipFileStream.Position;
                uint centralSize = 0;

                if (this.FileDataImage != null)
                    this.ZipFileStream.Write(FileDataImage, 0, FileDataImage.Length);

                for (int i = 0; i < Files.Count; i++)
                {
                    long pos = this.ZipFileStream.Position;
                    this.WriteCentralDirRecord(Files[i]);
                    centralSize += (uint)(this.ZipFileStream.Position - pos);
                }

                if (this.FileDataImage != null)
                    this.WriteEndRecord(centralSize + (uint)FileDataImage.Length, centralOffset);
                else
                    this.WriteEndRecord(centralSize, centralOffset);
            }

            if (this.ZipFileStream != null)
            {
                this.ZipFileStream.Flush();
                this.ZipFileStream.Dispose();
                this.ZipFileStream = null;
            }
        }

        /// <summary>
        /// Read all the file records in the central directory 
        /// </summary>
        /// <returns>List of all entries in directory</returns>
        public List<ZipFileEntry> ReadCentralDir()
        {
            List<ZipFileEntry> result = new List<ZipFileEntry>();
            this.ReadCentralDirAction(delegate (ZipFileEntry f)
            {
                result.Add(f);
            });
            return result;
        }

        public void ReadCentralDirAction(Action<ZipFileEntry> zfeAct)
        {
            ushort filenameSize;
            ushort extraSize;
            ushort commentSize;
            if (this.FileDataImage == null)
            {
                throw new InvalidOperationException("Central directory currently does not exist");
            }
            for (int pointer = 0; pointer < this.FileDataImage.Length; pointer += ((46 + filenameSize) + extraSize) + commentSize)
            {
                if (BitConverter.ToUInt32(this.FileDataImage, pointer) != 33639248)
                {
                    return;
                }
                bool encodeUTF8 = (BitConverter.ToUInt16(this.FileDataImage, pointer + 8) & 2048) != 0;
                ushort method = BitConverter.ToUInt16(this.FileDataImage, pointer + 10);
                uint modifyTime = BitConverter.ToUInt32(this.FileDataImage, pointer + 12);
                uint crc32 = BitConverter.ToUInt32(this.FileDataImage, pointer + 16);
                uint comprSize = BitConverter.ToUInt32(this.FileDataImage, pointer + 20);
                uint fileSize = BitConverter.ToUInt32(this.FileDataImage, pointer + 24);
                filenameSize = BitConverter.ToUInt16(this.FileDataImage, pointer + 28);
                extraSize = BitConverter.ToUInt16(this.FileDataImage, pointer + 30);
                commentSize = BitConverter.ToUInt16(this.FileDataImage, pointer + 32);
                uint headerOffset = BitConverter.ToUInt32(this.FileDataImage, pointer + 42);
                uint headerSize = (uint)(((46 + filenameSize) + extraSize) + commentSize);
                Encoding encoder = encodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
                ZipFileEntry zfe = new ZipFileEntry
                {
                    Method = (Compression)method,
                    FilenameInZip = encoder.GetString(this.FileDataImage, pointer + 46, filenameSize),
                    FileOffset = this.GetFileOffset(headerOffset),
                    FileSize = fileSize,
                    CompressedSize = comprSize,
                    HeaderOffset = headerOffset,
                    HeaderSize = headerSize,
                    Crc32 = crc32,
                    ModifyTime = DosTimeToDateTime(modifyTime)
                };
                if (commentSize > 0)
                {
                    zfe.Comment = encoder.GetString(this.FileDataImage, ((pointer + 46) + filenameSize) + extraSize, commentSize);
                }
                zfeAct(zfe);
            }
        }

        /// <summary>
        /// Copy the contents of a stored file into a physical file
        /// </summary>
        /// <param name="_zfe">Entry information of file to extract</param>
        /// <param name="_filename">Name of file to store uncompressed data</param>
        /// <returns>True if success, false if not.</returns>
        /// <remarks>Unique compression methods are Store and Deflate</remarks>
        public bool ExtractFile(ZipFileEntry _zfe, string _filename)
        {
            // Make sure the parent directory exist
            string path = System.IO.Path.GetDirectoryName(_filename);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            // Check it is directory. If so, do nothing
            if (Directory.Exists(_filename))
                return true;

            if (File.Exists(_filename)) File.SetAttributes(_filename, FileAttributes.Normal);
            Stream output = new FileStream(_filename, FileMode.Create, FileAccess.Write);
            bool result = ExtractFile(_zfe, output);
            if (result)
                output.Close();

            File.SetCreationTime(_filename, _zfe.ModifyTime);
            File.SetLastWriteTime(_filename, _zfe.ModifyTime);

            return result;
        }

        /// <summary>
        /// Copy the contents of a stored file into an opened stream
        /// </summary>
        /// <param name="_zfe">Entry information of file to extract</param>
        /// <param name="_stream">Stream to store the uncompressed data</param>
        /// <returns>True if success, false if not.</returns>
        /// <remarks>Unique compression methods are Store and Deflate</remarks>
        public bool ExtractFile(ZipFileEntry _zfe, Stream _stream)
        {
            if (!_stream.CanWrite)
                throw new InvalidOperationException("Stream cannot be written");

            // check signature
            byte[] signature = new byte[4];
            this.ZipFileStream.Seek(_zfe.HeaderOffset, SeekOrigin.Begin);
            this.ZipFileStream.Read(signature, 0, 4);
            if (BitConverter.ToUInt32(signature, 0) != 0x04034b50)
                return false;

            // Select input stream for inflating or just reading
            Stream inStream;
            if (_zfe.Method == Compression.Store)
                inStream = this.ZipFileStream;
            else if (_zfe.Method == Compression.Deflate)
                inStream = new DeflateStream(this.ZipFileStream, CompressionMode.Decompress, true);
            else
                return false;

            // Buffered copy
            byte[] buffer = new byte[16384];
            this.ZipFileStream.Seek(_zfe.FileOffset, SeekOrigin.Begin);
            uint bytesPending = _zfe.FileSize;
            while (bytesPending > 0)
            {
                int bytesRead = inStream.Read(buffer, 0, (int)Math.Min(bytesPending, buffer.Length));
                _stream.Write(buffer, 0, bytesRead);
                bytesPending -= (uint)bytesRead;
            }
            _stream.Flush();

            if (_zfe.Method == Compression.Deflate)
                inStream.Dispose();
            return true;
        }

        /// <summary>
        /// Removes one of many files in storage. It creates a new Zip file.
        /// </summary>
        /// <param name="_zip">Reference to the current Zip object</param>
        /// <param name="_zfes">List of Entries to remove from storage</param>
        /// <returns>True if success, false if not</returns>
        /// <remarks>This method only works for storage of type FileStream</remarks>
        public static bool RemoveEntries(ref ZipStorer _zip, List<ZipFileEntry> _zfes)
        {
            if (!(_zip.ZipFileStream is FileStream))
                throw new InvalidOperationException("RemoveEntries is allowed just over streams of type FileStream");

            //Get full list of entries
            List<ZipFileEntry> fullList = _zip.ReadCentralDir();
            //In order to delete we need to create a copy of the zip file excluding the selected items
            string tempZipName = Path.GetTempFileName();
            string tempEntryFile = Path.GetTempFileName();

            try
            {
                ZipStorer tempTargetZip = ZipStorer.Create(tempZipName, _zip.Comment);
                foreach (ZipFileEntry zfe in fullList)
                {
                    if (!_zfes.Contains(zfe))
                    {
                        if (_zip.ExtractFile(zfe, tempEntryFile))
                        {
                            tempTargetZip.AddFile(zfe.Method, tempEntryFile, zfe.FilenameInZip, zfe.Comment);
                        }
                    }
                }
                _zip.Close();
                tempTargetZip.Close();

                File.Delete(_zip.FileName);
                File.Move(tempZipName, _zip.FileName);
                _zip = ZipStorer.Open(_zip.FileName, _zip.Access);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (File.Exists(tempZipName)) File.Delete(tempZipName);
                if (File.Exists(tempEntryFile)) File.Delete(tempEntryFile);
            }
            return true;
        }
        #endregion

        #region Private methods
        // Calculate the file offset by reading the corresponding local header
        private uint GetFileOffset(uint _headerOffset)
        {
            byte[] buffer = new byte[2];
            this.ZipFileStream.Seek(_headerOffset + 26, SeekOrigin.Begin);
            this.ZipFileStream.Read(buffer, 0, 2);
            ushort filenameSize = BitConverter.ToUInt16(buffer, 0);
            this.ZipFileStream.Read(buffer, 0, 2);
            ushort extraSize = BitConverter.ToUInt16(buffer, 0);
            return (uint)(30 + filenameSize + extraSize + _headerOffset);
        }

        /* Local file header: 文件头开始标志
            local file header signature     4 bytes  (0x04034b50)
            version needed to extract       2 bytes
            general purpose bit flag        2 bytes
            compression method              2 bytes
            last mod file time              2 bytes
            last mod file date              2 bytes
            crc-32                          4 bytes
            compressed size                 4 bytes
            uncompressed size               4 bytes
            filename length                 2 bytes
            extra field length              2 bytes

            filename (variable size)
            extra field (variable size)
        */
        private void WriteLocalHeader(ref ZipFileEntry _zfe)
        {
            long pos = this.ZipFileStream.Position;
            Encoding encoder = _zfe.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            byte[] encodedFilename = encoder.GetBytes(_zfe.FilenameInZip);

            this.ZipFileStream.Write(new byte[] { 80, 75, 3, 4, 20, 0 }, 0, 6); // No extra header
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)(_zfe.EncodeUTF8 ? 0x0800 : 0)), 0, 2); // filename and comment encoding 
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)_zfe.Method), 0, 2);  // zipping method
            this.ZipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(_zfe.ModifyTime)), 0, 4); // zipping date and time
            this.ZipFileStream.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 12); // unused CRC, un/compressed size, updated later
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)encodedFilename.Length), 0, 2); // filename length
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // extra length

            this.ZipFileStream.Write(encodedFilename, 0, encodedFilename.Length);
            _zfe.HeaderSize = (uint)(this.ZipFileStream.Position - pos);
        }

        /* Central directory's File header: //目录开始标志
            central file header signature   4 bytes  (0x02014b50)
            version made by                 2 bytes
            version needed to extract       2 bytes
            general purpose bit flag        2 bytes
            compression method              2 bytes
            last mod file time              2 bytes
            last mod file date              2 bytes
            crc-32                          4 bytes
            compressed size                 4 bytes
            uncompressed size               4 bytes
            filename length                 2 bytes
            extra field length              2 bytes
            file comment length             2 bytes
            disk number start               2 bytes
            internal file attributes        2 bytes
            external file attributes        4 bytes
            relative offset of local header 4 bytes

            filename (variable size)
            extra field (variable size)
            file comment (variable size)
        */
        private void WriteCentralDirRecord(ZipFileEntry _zfe)
        {
            Encoding encoder = _zfe.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            byte[] encodedFilename = encoder.GetBytes(_zfe.FilenameInZip);
            byte[] encodedComment = encoder.GetBytes(_zfe.Comment);

            this.ZipFileStream.Write(new byte[] { 80, 75, 1, 2, 23, 0xB, 20, 0 }, 0, 8);
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)(_zfe.EncodeUTF8 ? 0x0800 : 0)), 0, 2); // filename and comment encoding 
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)_zfe.Method), 0, 2);  // zipping method
            this.ZipFileStream.Write(BitConverter.GetBytes(DateTimeToDosTime(_zfe.ModifyTime)), 0, 4);  // zipping date and time
            this.ZipFileStream.Write(BitConverter.GetBytes(_zfe.Crc32), 0, 4); // file CRC
            this.ZipFileStream.Write(BitConverter.GetBytes(_zfe.CompressedSize), 0, 4); // compressed file size
            this.ZipFileStream.Write(BitConverter.GetBytes(_zfe.FileSize), 0, 4); // uncompressed file size
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)encodedFilename.Length), 0, 2); // Filename in zip
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // extra length
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)encodedComment.Length), 0, 2);

            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // disk=0
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // file type: binary
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0), 0, 2); // Internal file attributes
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)0x8100), 0, 2); // External file attributes (normal/readable)
            this.ZipFileStream.Write(BitConverter.GetBytes(_zfe.HeaderOffset), 0, 4);  // Offset of header

            this.ZipFileStream.Write(encodedFilename, 0, encodedFilename.Length);
            this.ZipFileStream.Write(encodedComment, 0, encodedComment.Length);
        }

        /* End of central dir record: //目录结束标识
            end of central dir signature                                                            4 bytes  (0x06054b50)
            number of this disk                                                                     2 bytes
            number of the disk with the start of the central directory                              2 bytes
            total number of entries in the central dir on this disk                                 2 bytes


            total number of entries in the central dir                                              2 bytes
            size of the central directory                                                           4 bytes
            offset of start of central directory with respect to the starting disk number           4 bytes
            zipfile comment length                                                                  2 bytes
            zipfile comment (variable size)
        */
        private void WriteEndRecord(uint _size, uint _offset)
        {
            Encoding encoder = this.EncodeUTF8 ? Encoding.UTF8 : DefaultEncoding;
            byte[] encodedComment = encoder.GetBytes(this.Comment ?? string.Empty);

            this.ZipFileStream.Write(new byte[] { 80, 75, 5, 6, 0, 0, 0, 0 }, 0, 8);
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count + ExistingFileNumber), 0, 2);
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)Files.Count + ExistingFileNumber), 0, 2);

            //
            this.ZipFileStream.Write(BitConverter.GetBytes(_size), 0, 4);
            this.ZipFileStream.Write(BitConverter.GetBytes(_offset), 0, 4);

            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)encodedComment.Length), 0, 2);
            this.ZipFileStream.Write(encodedComment, 0, encodedComment.Length);
        }

        // Reads the end-of-central-directory record
        private bool ReadFileInfo()
        {
            if (this.ZipFileStream.Length < 22)
                return false;

            try
            {
                this.ZipFileStream.Seek(-17, SeekOrigin.End);
                BinaryReader br = new BinaryReader(this.ZipFileStream);
                do
                {
                    this.ZipFileStream.Seek(-5, SeekOrigin.Current);
                    UInt32 sig = br.ReadUInt32();
                    if (sig == 0x06054b50)
                    {
                        this.ZipFileStream.Seek(6, SeekOrigin.Current);

                        UInt16 entries = br.ReadUInt16();           //2
                        Int32 fileTotalSize = br.ReadInt32();       //4
                        UInt32 fileDataOffset = br.ReadUInt32();    //4
                        UInt16 commentSize = br.ReadUInt16();       //2

                        // check if comment field is the very last data in file
                        if (this.ZipFileStream.Position + commentSize != this.ZipFileStream.Length)
                            return false;

                        // [TODO]读取Zip文件的注释
                        if (commentSize > 0)
                        {
                            byte[] fileCmtBytes = new byte[commentSize];
                            ZipFileStream.Read(fileCmtBytes, 0, fileCmtBytes.Length);
                            this.Comment = DefaultEncoding.GetString(fileCmtBytes);
                        }

                        // Copy entire central directory to a memory buffer
                        this.ExistingFileNumber = entries;
                        this.FileDataImage = new byte[fileTotalSize];
                        this.ZipFileStream.Seek(fileDataOffset, SeekOrigin.Begin);
                        this.ZipFileStream.Read(this.FileDataImage, 0, fileTotalSize);

                        // Leave the pointer at the begining of central dir, to append new files
                        this.ZipFileStream.Seek(fileDataOffset, SeekOrigin.Begin);
                        return true;
                    }
                } while (this.ZipFileStream.Position > 0);
            }
            catch { }

            return false;
        }

        // Copies all source file into storage file
        private void Store(ref ZipFileEntry _zfe, Stream _source)
        {
            byte[] buffer = new byte[16384];
            int bytesRead;
            uint totalRead = 0;
            Stream outStream;

            long posStart = this.ZipFileStream.Position;
            long sourceStart = _source.Position;

            if (_zfe.Method == Compression.Store)
                outStream = this.ZipFileStream;
            else
                outStream = new DeflateStream(this.ZipFileStream, CompressionMode.Compress, true);

            _zfe.Crc32 = 0 ^ 0xffffffff;

            do
            {
                bytesRead = _source.Read(buffer, 0, buffer.Length);
                totalRead += (uint)bytesRead;
                if (bytesRead > 0)
                {
                    outStream.Write(buffer, 0, bytesRead);

                    for (uint i = 0; i < bytesRead; i++)
                    {
                        _zfe.Crc32 = ZipStorer.CrcTable[(_zfe.Crc32 ^ buffer[i]) & 0xFF] ^ (_zfe.Crc32 >> 8);
                    }
                }
            } while (bytesRead == buffer.Length);
            outStream.Flush();

            if (_zfe.Method == Compression.Deflate)
                outStream.Dispose();

            _zfe.Crc32 ^= 0xffffffff;
            _zfe.FileSize = totalRead;
            _zfe.CompressedSize = (uint)(this.ZipFileStream.Position - posStart);

            // Verify for real compression 确保压缩后数据长度减小
            if (_zfe.Method == Compression.Deflate && !this.ForceDeflating
                && _source.CanSeek && _zfe.CompressedSize > _zfe.FileSize)
            {
                // Start operation again with Store algorithm
                _zfe.Method = Compression.Store;
                this.ZipFileStream.Position = posStart;
                this.ZipFileStream.SetLength(posStart);
                _source.Position = sourceStart;

                this.Store(ref _zfe, _source);
            }

        }

        /* DOS Date and time:
            MS-DOS date. The date is a packed value with the following format. Bits Description 
                0-4 Day of the month (1?1) 
                5-8 Month (1 = January, 2 = February, and so on) 
                9-15 Year offset from 1980 (add 1980 to get actual year) 
            MS-DOS time. The time is a packed value with the following format. Bits Description 
                0-4 Second divided by 2 
                5-10 Minute (0?9) 
                11-15 Hour (0?3 on a 24-hour clock) 
        */
        private uint DateTimeToDosTime(DateTime _dt)
        {
            return (uint)(
                (_dt.Second / 2) | (_dt.Minute << 5) | (_dt.Hour << 11) |
                (_dt.Day << 16) | (_dt.Month << 21) | ((_dt.Year - 1980) << 25));
        }

        private DateTime DosTimeToDateTime(uint _dt)
        {
            return new DateTime(
                (int)(_dt >> 25) + 1980,
                (int)(_dt >> 21) & 15,
                (int)(_dt >> 16) & 31,

                (int)(_dt >> 11) & 31,
                (int)(_dt >> 5) & 63,
                (int)(_dt & 31) * 2);
        }

        /* CRC32 algorithm
          The 'magic number' for the CRC is 0xdebb20e3.  
          The proper CRC pre and post conditioning
          is used, meaning that the CRC register is
          pre-conditioned with all ones (a starting value
          of 0xffffffff) and the value is post-conditioned by
          taking the one's complement of the CRC residual.
          If bit 3 of the general purpose flag is set, this
          field is set to zero in the local header and the correct
          value is put in the data descriptor and in the central
          directory.
        */
        private void UpdateCrcAndSizes(ref ZipFileEntry _zfe)
        {
            long lastPos = this.ZipFileStream.Position;                                     // remember position

            this.ZipFileStream.Position = _zfe.HeaderOffset + 8;
            this.ZipFileStream.Write(BitConverter.GetBytes((ushort)_zfe.Method), 0, 2);    // zipping method

            this.ZipFileStream.Position = _zfe.HeaderOffset + 14;
            this.ZipFileStream.Write(BitConverter.GetBytes(_zfe.Crc32), 0, 4);              // Update CRC
            this.ZipFileStream.Write(BitConverter.GetBytes(_zfe.CompressedSize), 0, 4);     // Compressed size
            this.ZipFileStream.Write(BitConverter.GetBytes(_zfe.FileSize), 0, 4);           // Uncompressed size

            this.ZipFileStream.Position = lastPos;  // restore position
        }

        // Replaces backslashes with slashes to store in zip header
        private string NormalizedFilename(string _filename)
        {
            string filename = _filename.Replace('\\', '/');
            int pos = filename.IndexOf(':');
            if (pos >= 0)
                filename = filename.Remove(0, pos) + "_" + filename.Substring(pos + 1);
            return filename.Trim('/');
        }

        #endregion

        #region IDisposable Members
        /// <summary>
        /// Closes the Zip file stream
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }
        #endregion
    }
}
