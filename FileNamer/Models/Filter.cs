using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileNamer.Models
{
    public class Filter
    {
        public string FilterType { get; set; }
        public int FilterID { get; set; }
        public List<string> FileTypes { get; set; }
    }
}
