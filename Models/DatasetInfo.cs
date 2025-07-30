using System;
using System.Collections.Generic;

namespace CustomerSegmentationML.Models
{
    public class DatasetInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int RowCount { get; set; }
        public List<string> Columns { get; set; }
        public string Description { get; set; }
        public DateTime LoadedTime { get; set; }
    }
}
