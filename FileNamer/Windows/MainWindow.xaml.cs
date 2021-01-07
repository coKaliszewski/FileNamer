using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using FileNamer.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

// ReSharper disable once IdentifierTypo
namespace FileNamer.Windows
{
    public partial class MainWindow : Window
    {
        private string _fileDirectory;

        private string FileDirectory
        {
            get => _fileDirectory;
            set
            {
                _fileDirectory = value; 
                SearchForFiles();
            }
        }

        public List<Filter> Filters { get; set; }

        public ObservableCollection<FileContent> FileList { get; set; }


        public MainWindow()
        {
            FileList = new ObservableCollection<FileContent>();
            Filters = new List<Filter>
            {
                new Filter {FilterType = "Any", FileTypes = new List<string>{".*"}, FilterID = 0},
                new Filter
                {
                    FilterType = "Video Files",
                    FileTypes = Properties.Settings.Default.VideoFiles.Split(',').ToList(),
                    FilterID = 1
                },
                new Filter
                {
                    FilterType = "Images",
                    FileTypes = Properties.Settings.Default.Images.Split(',').ToList(),
                    FilterID = 2
                },
                new Filter
                {
                    FilterType = "Documents",
                    FileTypes = Properties.Settings.Default.Documents.Split(',').ToList(),
                    FilterID = 3
                }
            };

            InitializeComponent();


            txtRemoveEndingCharacters.TextChanged += UpdatePreview_OnTextChanged;
            txtRemoveBeginningCharacters.TextChanged += UpdatePreview_OnTextChanged;
            txtReplaceSearch.TextChanged += UpdatePreview_OnTextChanged;
            txtReplaceWith.TextChanged += UpdatePreview_OnTextChanged;
            txtPrefix.TextChanged += UpdatePreview_OnTextChanged;
            txtPrefixIncrementNumber.TextChanged += UpdatePreview_OnTextChanged;
            txtPrefixNumberFormat.TextChanged += UpdatePreview_OnTextChanged;
            txtPrefixStartingNumber.TextChanged += UpdatePreview_OnTextChanged;

            foreach (var filter in Filters)
            {
                ComboboxFilter.Items.Add(filter);
            }

            ComboboxFilter.SelectedValue = 0;


        }

        private void Browse_OnClick(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (folderDialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            txtLocation.Text = folderDialog.FileName;
            FileDirectory = folderDialog.FileName;
        }

        private void SearchForFiles()
        {
            if (!Directory.Exists(FileDirectory))
            {
                txtLocation.BorderBrush = Brushes.Red;
                txtFileLocationError.Visibility = Visibility.Visible;
                return;
            }

            txtLocation.BorderBrush = Brushes.Gray;
            txtFileLocationError.Visibility = Visibility.Collapsed;
            DirectoryInfo files = new DirectoryInfo(FileDirectory);
            FileList.Clear();

            foreach (var file in files.GetFiles())
            {
                FileList.Add(new FileContent
                {
                    FileInfo = file,
                    NewFileName = Path.GetFileNameWithoutExtension(file.Name),
                    PreviewFileName = Path.GetFileNameWithoutExtension(file.Name),
                    OriginalFileName = file.Name,
                    IsEnabled = true,
                    FileID = FileList.Count
                });
            }

            UpdatePreview_OnTextChanged(null,null);
        }

        private void FileName_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ((FileContent) e.Row.Item).NewFileName = ((FileContent) e.Row.Item).PreviewFileName;
        }

        private void UpdatePreview_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            int prefixNumber = int.TryParse(txtPrefixStartingNumber.Text, out prefixNumber) ? prefixNumber : 0;
            int prefixIncrementNumber = int.TryParse(txtPrefixIncrementNumber.Text, out prefixIncrementNumber) ? prefixIncrementNumber : 0;
            int beginningCharacters = !string.IsNullOrEmpty(txtRemoveBeginningCharacters.Text) && int.TryParse(txtRemoveBeginningCharacters.Text, out beginningCharacters) ? beginningCharacters : 0;
            int endingCharacters = !string.IsNullOrEmpty(txtRemoveEndingCharacters.Text) && int.TryParse(txtRemoveEndingCharacters.Text, out endingCharacters) ? endingCharacters : 0;

            foreach (var file in FileList)
            {
                if (!file.IsEnabled) continue;

                string tempFileName = file.NewFileName;

                // Deprecate characters
                if (!string.IsNullOrEmpty(txtRemoveBeginningCharacters.Text) || !string.IsNullOrEmpty(txtRemoveEndingCharacters.Text))
                {
                    tempFileName = tempFileName.Substring(beginningCharacters > tempFileName.Length ? tempFileName.Length : beginningCharacters);
                    tempFileName = tempFileName.Substring(0, (tempFileName.Length - endingCharacters) < 0 ? 0 : tempFileName.Length - endingCharacters);
                }

                // Add the prefix
                if (txtPrefix.Text.Contains("*"))
                {
                    tempFileName = txtPrefix.Text.Replace("*", prefixNumber.ToString(txtPrefixNumberFormat.Text)) + tempFileName;
                    prefixNumber += prefixIncrementNumber;
                }
                else
                {
                    tempFileName = txtPrefix.Text + tempFileName;
                }

                // Replace values
                if (!string.IsNullOrEmpty(txtReplaceSearch.Text))
                {
                    tempFileName = tempFileName.Replace(txtReplaceSearch.Text, txtReplaceWith.Text);
                }

                file.PreviewFileName = tempFileName.Trim();
            }
        }

        private void LocationChanged_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FileDirectory = txtLocation.Text;
            }
        }

        private void PreviewFileName_OnLostFocus(object sender, RoutedEventArgs e)
        {
            FileContent file = ((FileContent) ((TextBox) sender).DataContext);

            List<string> invalidCharacters = new List<string> { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" };
            foreach (var invalidCharacter in invalidCharacters)
            {
                file.PreviewFileName = file.PreviewFileName.Replace(invalidCharacter, string.Empty).Trim();
            }

            file.NewFileName = file.PreviewFileName;
            UpdatePreview_OnTextChanged(null,null);
        }


        private void PreviewFileName_OnGotFocus(object sender, RoutedEventArgs e)
        {
            FileContent file = ((FileContent)((TextBox)sender).DataContext);
            file.PreviewFileName = file.NewFileName;
        }



        private async void Rename_OnClick(object sender, RoutedEventArgs e)
        {
            txtFilesInfo.Foreground = Brushes.Black;
            txtFilesInfo.Text = "Processing...";

            List<ErrorResponse> errors = new List<ErrorResponse>();
            foreach (var file in FileList)
            {
                if (file.PreviewFileName.Length <= 0 || Path.GetFileNameWithoutExtension(file.FileInfo.Name) == file.PreviewFileName /*|| !File.Exists(file.FileInfo.FullName)*/) continue;

                try
                {
                    File.Move(file.FileInfo.FullName, file.FileInfo.DirectoryName + "\\" + file.PreviewFileName + file.FileInfo.Extension);
                }
                catch (Exception ex)
                {
                    errors.Add(new ErrorResponse
                    {
                        File = file.FileInfo,
                        ErrorMessage = ex.Message
                    });
                    Console.WriteLine(ex);
                }
            }

            FileDirectory = FileDirectory;

            if (!errors.Any())
            {
                txtFilesInfo.Foreground = Brushes.LimeGreen;
                txtFilesInfo.Text = "All files were successfully renamed";

                await Task.Delay(15000);
                txtFilesInfo.Text = string.Empty;

                return;
            }

            txtFilesInfo.Foreground = Brushes.Red;
            txtFilesInfo.Text = "Some files were not renamed";

            ErrorWindow errorWindow = new ErrorWindow(errors)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            errorWindow.ShowDialog();

            await Task.Delay(15000);
            txtFilesInfo.Text = string.Empty;
        }

        private void GridViewFiles_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Space || FocusManager.GetFocusedElement(this) is TextBox) return;

            foreach (var item in DataGrid_Files.SelectedItems)
            {
               var test = FocusManager.GetFocusedElement(this);
                
                ((FileContent) item).IsEnabled = !((FileContent) item).IsEnabled;
            }
        }


        private void Exit_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void File_Filter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(DataGrid_Files.ItemsSource).Refresh();
        }

        private void FilesView_OnFilter(object sender, FilterEventArgs e)
        {
            if(!(e.Item is FileContent file)) return;

            Filter filter = Filters.FirstOrDefault(f => f.FilterID == (int)ComboboxFilter.SelectedValue);
            if(filter == null) return;


            e.Accepted = (filter.FilterID == 0 || filter.FileTypes.Any(f => f == file.FileInfo.Extension));
        }
    }
}
