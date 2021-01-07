using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileNamer.Models
{
    public class ErrorResponse
    {
        public FileInfo File { get; set; }
        public string ErrorMessage { get; set; }
    }
}
