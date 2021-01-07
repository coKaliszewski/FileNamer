using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using FileNamer.Models;

namespace FileNamer.Windows
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public List<ErrorResponse> Errors { get; set; }

        public ErrorWindow(List<ErrorResponse> errors)
        {
            Errors = errors;
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
