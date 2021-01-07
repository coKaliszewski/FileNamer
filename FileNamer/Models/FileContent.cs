using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileNamer.Controllers;

namespace FileNamer.Models
{
    public class FileContent : ViewModelBase
    {
        private bool _isEnabled;
        private string _originalFileName;
        private string _newFileName;
        private string _previewFileName;
        

        public bool IsEnabled 
        {
            get => _isEnabled; 
            set => SetProperty(ref _isEnabled, value);
        }

        public string OriginalFileName
        {
            get => _originalFileName;
            set => SetProperty(ref _originalFileName, value); 

        }

        public string NewFileName
        {
            get => _newFileName;
            set => SetProperty(ref _newFileName, value);
        }

        public string PreviewFileName
        {
            get => _previewFileName;
            set => SetProperty(ref _previewFileName, value);
        }

        public FileInfo FileInfo { get; set; }
        public int FileID { get; set; }
    }
}
