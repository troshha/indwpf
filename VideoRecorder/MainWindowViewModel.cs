using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Video.FFMPEG;
using AForge.Video;
using AForge.Video.DirectShow;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;

namespace VideoRecorder
{
    internal class MainWindowViewModel : ObservableObject, IDisposable
    {
        #region Private fields

        private FilterInfo _currentDevice;

        private BitmapImage _image;

        private bool _isDesktopSource;
        

        private IVideoSource _videoSource;
        private VideoFileWriter _writer;
        private bool _recording;
        private DateTime? _firstFrameTime;

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            VideoDevices = new ObservableCollection<FilterInfo>();
            GetVideoDevices();
            IsDesktopSource = true;
            StartSourceCommand = new RelayCommand(StartCamera);
            StopSourceCommand = new RelayCommand(StopCamera);
            StartRecordingCommand = new RelayCommand(StartRecording);
            StopRecordingCommand = new RelayCommand(StopRecording);
        }

        #endregion

        #region Properties

        public ObservableCollection<FilterInfo> VideoDevices { get; set; }

        public BitmapImage Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }

        public bool IsDesktopSource
        {
            get { return _isDesktopSource; }
            set { Set(ref _isDesktopSource, value); }
        }

        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { Set(ref _currentDevice, value); }
        }

        public ICommand StartRecordingCommand { get; private set; }

        public ICommand StopRecordingCommand { get; private set; }

        public ICommand StartSourceCommand { get; private set; }

        public ICommand StopSourceCommand { get; private set; }

        public ICommand SaveSnapshotCommand { get; private set; }

        #endregion

        private void GetVideoDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in devices)
            {
                VideoDevices.Add(device);
            }
            if (VideoDevices.Any())
            {
                CurrentDevice = VideoDevices[0];
            }
           
        }

        private void StartCamera()
        {
            if (IsDesktopSource)
            {
                var rectangle = new Rectangle();
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    rectangle = Rectangle.Union(rectangle, screen.Bounds);
                }
                _videoSource = new ScreenCaptureStream(rectangle);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
            
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_recording)
                {
                    using (var bitmap = (Bitmap) eventArgs.Frame.Clone())
                    {
                        if (_firstFrameTime != null)
                        {
                            _writer.WriteVideoFrame(bitmap, DateTime.Now - _firstFrameTime.Value);
                        }
                        else
                        {
                            _writer.WriteVideoFrame(bitmap);
                            _firstFrameTime = DateTime.Now;
                        }
                    }
                }
                using (var bitmap = (Bitmap) eventArgs.Frame.Clone())
                {
                    var bi = bitmap.ToBitmapImage();
                    bi.Freeze();
                    Dispatcher.CurrentDispatcher.Invoke(() => Image = bi);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error on _videoSource_NewFrame:\n" + exc.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
                _videoSource.NewFrame -= video_NewFrame;
            }
            Image = null;
        }

        private void StopRecording()
        {
            _recording = false;
            _writer.Close();
            _writer.Dispose();
        }

        private void StartRecording()
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "Video1";
            dialog.DefaultExt = ".avi";
            dialog.AddExtension = true;
            var dialogresult = dialog.ShowDialog();
            if (dialogresult != true)
            {
                return;
            }
            _firstFrameTime = null;
            _writer = new VideoFileWriter();
            _writer.Open(dialog.FileName, (int)Math.Round(Image.Width, 0), (int)Math.Round(Image.Height, 0));
            _recording = true;
        }

       

        public void Dispose()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.Stop();
            }
            _writer?.Dispose();
        }
    }
}