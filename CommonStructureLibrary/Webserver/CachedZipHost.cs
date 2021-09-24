using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace CSL.Webserver
{
    public class CachedZipHost
    {
        private readonly Dictionary<string, byte[]> cachedContent;
        private readonly Func<Stream> streamFunction;
        private readonly bool caseSensitive;
        public CachedZipHost(string path, bool caseSensitive = false):this(()=>new FileStream(path,FileMode.Open),caseSensitive)
        {
            if(path == null) {throw new ArgumentNullException();}
        }
        public CachedZipHost(Func<Stream> streamFunction, bool caseSensitive = false)
        {
            this.streamFunction = streamFunction;
            this.caseSensitive = caseSensitive;
            cachedContent = new Dictionary<string, byte[]>();
            Refresh();
        }
        public void Refresh()
        {
            using (Stream s = streamFunction())
            using (ZipArchive za = new ZipArchive(s, ZipArchiveMode.Read))
            {
                cachedContent.Clear();
                foreach (ZipArchiveEntry zae in za.Entries)
                {
                    int exceptioncount = 0;
                    while (exceptioncount < 3)
                    {
                        try
                        {
                            string name = caseSensitive ? zae.FullName : zae.FullName.ToLower();
                            if (zae.Length > 0)
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    zae.Open().CopyTo(ms);
                                    cachedContent.Add(name, ms.ToArray());
                                }
                            }
                            else
                            {
                                cachedContent.Add(name, new byte[0]);
                            }
                            break;
                        }
                        catch (Exception)
                        {
                            exceptioncount++;
                        }
                    }
                }
            }
        }
        public bool ContainsContent(string key) => cachedContent.ContainsKey(caseSensitive ? key : key.ToLower());
        public MemoryStream GetContent(string key) => new MemoryStream(cachedContent[caseSensitive ? key : key.ToLower()], false);
    }
}
