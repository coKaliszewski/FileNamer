using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileNamer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_OnClick(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (folderDialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            txtLocation.Text = folderDialog.FileName;

            SearchForFiles();
        }

        private void SearchForFiles()
        {
            if(string.IsNullOrEmpty(txtLocation.Text)) return;

            string filepath = txtLocation.Text;
            DirectoryInfo files = new DirectoryInfo(filepath);

            foreach (var file in files.GetFiles())
            {

                lstFileList.Items.Add(file.Name);
                //Directory.Move(file.FullName, filepath + "\\TextFiles\\" + file.Name);
            }
        }
    }
}
