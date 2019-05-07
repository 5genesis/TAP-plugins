// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using Newtonsoft.Json.Linq;

namespace Tap.Plugins._5Genesis.Monroe.Instruments
{
    public class MonroeReply
    {
        public string Message { get; set; }

        public HttpStatusCode Status { get; set; }

        public string StatusDescription { get; set; }

        public string FilePath { get; set; }

        public List<string> RunningExperiments { get; set; }

        public List<string> ScheduledExperiments { get; set; }

        public void RemoveTempFile()
        {
            if (FilePath != null && File.Exists(FilePath))
            {
                try { File.Delete(FilePath); } catch { } // Silently ignore
            }
        }

        public bool Success
        {
            get { return ((int)Status >= 200) && ((int)Status <= 299); }
        }

        public IEnumerable<Dictionary<string, string>> Results
        {
            get
            {
                if (FilePath != null && File.Exists(FilePath))
                {
                    ZipArchive zip = ZipFile.Open(FilePath, ZipArchiveMode.Read);
                    foreach (ZipArchiveEntry entry in zip.Entries.OrderBy(e => e.Name))
                    {
                        if (entry.Name.EndsWith(".json"))
                        {
                            foreach (var result in parseEntry(entry)) { yield return result; }
                        }
                    }
                    zip.Dispose();
                }
            }
        }

        private IEnumerable<Dictionary<string, string>> parseEntry(ZipArchiveEntry entry)
        {
            StreamReader reader = new StreamReader(entry.Open());
            string line = string.Empty;
            while ((line = reader.ReadLine()) != null)
            {
                yield return parseLine(line);
            }
        }

        private Dictionary<string, string> parseLine(string line)
        {
            JObject json = JObject.Parse(line);
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var item in json)
            {
                result[item.Key.ToString()] = item.Value.ToString();
            }
            return result;
        }
    }
}
