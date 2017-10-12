using System;
using System.Collections.Generic;
using System.Text;

namespace WorldCollector.Yande
{
    public class YandeCollectorOptions
    {
        public string Site { get; set; }

        public string DbConnectionString { get; set; }

        public string DownloadPath { get; set; } = "./Collection/";

        public int MinInterval { get; set; }
        public int MaxThreads { get; set; }
        public string ListUrlTemplate { get; set; }
        public int ListInterval { get; set; }
        public int ListThreads { get; set; }
        public int DownloadInterval { get; set; }
        public int DownloadThreads { get; set; }
    }
}
