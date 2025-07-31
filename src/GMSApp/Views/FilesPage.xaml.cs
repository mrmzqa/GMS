using GMSApp.Models;
using GMSApp.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GMSApp.Views
{
    public partial class FilesPage : UserControl
    {
        private readonly FileViewModel _viewModel;

        public FilesPage(FileViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            DataContext = _viewModel;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileViewModel.SelectedFile))
            {
                DisplayFile(_viewModel.SelectedFile);
            }
        }

        private void DisplayFile(FileItem? file)
        {
            FileViewer.Content = null;

            if (file == null) return;

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
            var stream = new MemoryStream(file.Data);

            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                case ".gif":
                    var image = new Image
                    {
                        Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad),
                        Stretch = System.Windows.Media.Stretch.Uniform
                    };
                    FileViewer.Content = image;
                    break;

                case ".mp4":
                case ".avi":
                case ".mov":
                case ".wmv":
                    string videoPath = Path.Combine(Path.GetTempPath(), file.FileName);
                    File.WriteAllBytes(videoPath, file.Data);
                    var media = new MediaElement
                    {
                        Source = new Uri(videoPath),
                        LoadedBehavior = MediaState.Manual,
                        UnloadedBehavior = MediaState.Close
                    };
                    media.Play();
                    FileViewer.Content = media;
                    break;

                case ".txt":
                    using (var reader = new StreamReader(stream))
                    {
                        string text = reader.ReadToEnd();
                        FileViewer.Content = new TextBlock
                        {
                            Text = text,
                            TextWrapping = TextWrapping.Wrap
                        };
                    }
                    break;

                case ".pdf":
                case ".docx":
                case ".xlsx":
                case ".pptx":
                case ".zip":
                case ".rar":
                    string docPath = Path.Combine(Path.GetTempPath(), file.FileName);
                    File.WriteAllBytes(docPath, file.Data);
                    Process.Start(new ProcessStartInfo(docPath) { UseShellExecute = true });
                    FileViewer.Content = new TextBlock
                    {
                        Text = $"Opened {file.FileName} with default application.",
                        Foreground = System.Windows.Media.Brushes.Green
                    };
                    break;

                default:
                    FileViewer.Content = new TextBlock
                    {
                        Text = $"Preview not available for this file type: {ext}",
                        Foreground = System.Windows.Media.Brushes.Red
                    };
                    break;
            }
        }
    }
}
