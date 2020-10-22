using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Model
{
    public class Entry
    {
        public string Path { get; set; }
        public string Extension { get; set; }
        public bool IsAFolder { get; set; }
        public int ItemsInFolder { get; set; }
        public List<Entry> Items { get; set; }
        public bool IsWriteable { get; set; }
        public bool IsReadable { get; set; }
    }
}
