using System.Collections.Generic;

namespace WebApi.Model
{
    public class FolderResponse
    {
        public List<Entry> Entries { get; set; }
        public string PreviousFolder { get; set; }
    }
}
