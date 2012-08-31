using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Diagnostics;
using Microsoft.Kinect;
using Microsoft.Samples.Kinect.WpfViewers;
using GestureControls;
using GestureControls.Controls;
using System.Windows.Forms.Integration;
using Viewers.UserControls;
using System.Windows.Threading;

namespace Viewers
{
    public partial class MainWindow : Window
    {
        #region MemberVariables
        // Here are some important variables; detailed information of each in README file
        //private 
        private Skeleton[] _FrameSkeles;
        private double _percentWidth;
        private double _imageOffset;
        private DispatcherTimer _FastForward = new DispatcherTimer();
        private DispatcherTimer _Rewind = new DispatcherTimer();
        private string[] _FilesPics;
        private string[] _FilesPDF;
        private string[] _FilesMP4;
        private string[] _ThumbNails;
        private AdobeReader LoadedPDF;
        private int _currentFile = 0;


        // Indexes for which elements go on top. Lower values send element further back
        private const int ZVisuals = 2;
        private const int ZButtons = 5;

        // Define here the variables for Swipe Detection
        private const double _SwpLngth = 100;      // length of Swipe along X-Axis (MUST BE POSITIVE)
        private const double _SwpDeviate = 20;    // Y-Axis boundaries
        private const int _SwpTime = 1000;        // In MilliSeconds
        private const double _XOff = 0;


        // List all viewers here, for easier access by the KinectCursorManager functions
        private enum Theviewer
        {
            Image = 0,
            Video = 1,
            PDF = 2
        }
        #endregion MemberVariables


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            _KinectSensor1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(_KinectSensor1_KinectSensorChanged);

            _Rewind.Interval = TimeSpan.FromMilliseconds(1);
            _FastForward.Interval = TimeSpan.FromMilliseconds(1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // For more precise dimensions and positions, we set them while loading the window

            // Main Canvas' Dimensions and percentages compared to image dimensions
            ImLayout.Width = Application.Current.MainWindow.Width - 14;
            ImLayout.Height = Application.Current.MainWindow.Height - 12;

            _percentWidth = (Application.Current.MainWindow.Width - 12) / 1024;
            _imageOffset = (ImLayout.Height - 768 * _percentWidth) / 2;

            // If the MainWindow's height is less than the image height, the diff. is negative
            if (_imageOffset < 0)
                _imageOffset *= -1;

            // Background Image
            ImBackGround.Width = ImLayout.Width;
            ImBackGround.Height = ImLayout.Height;

            // Kinect's Tilt Angle Buttons
            Bttn_DownTilt.Height = 30 * _percentWidth;
            Bttn_DownTilt.Width = 30 * _percentWidth;
            Canvas.SetTop(Bttn_DownTilt, ImLayout.Height / 2 + Bttn_DownTilt.Height * 2);
            Canvas.SetLeft(Bttn_DownTilt, 10);

            Bttn_UpTilt.Height = 30 * _percentWidth;
            Bttn_UpTilt.Width = 30 * _percentWidth;
            Canvas.SetTop(Bttn_UpTilt, ImLayout.Height / 2);
            Canvas.SetLeft(Bttn_UpTilt, 10);

            // Text Blocks
            TxtTilt.Width = Bttn_DownTilt.Width;
            TxtTilt.Height = Bttn_DownTilt.Height;
            Canvas.SetTop(TxtTilt, ImLayout.Height / 2 + Bttn_UpTilt.Height);
            Canvas.SetLeft(TxtTilt, 10);

            TxtInformation.Width = Bttn_DownTilt.Width * 3.5;
            TxtInformation.Height = TxtInformation.Width;
            TxtInformation.Text = "|Tilt\n";
            Canvas.SetTop(TxtInformation, ImLayout.Height / 2 + Bttn_DownTilt.Height);
            Canvas.SetLeft(TxtInformation, 10 + TxtTilt.Width);

            // Skeleton Viewer Box
            _SkeleViewer.Height = ImLayout.Height / 4;
            _SkeleViewer.Width = ImLayout.Width / 4;
            Canvas.SetTop(_SkeleViewer, ImLayout.Height / 4);
            Canvas.SetLeft(_SkeleViewer, 10);

            // Define the Label's content (Folder to search) and button to change folder
            _Folder.Width = ImLayout.Width;
            _Folder.FontSize = 12;
            _Folder.Foreground = Brushes.DarkSlateGray;
            _Folder.Content = @"C:\Users\jkarlo\Desktop\Content For Viewers";
            Canvas.SetBottom(_Folder, _imageOffset);
            Canvas.SetLeft(_Folder, ImLayout.Width / 4);

            Bttn_Browser.FontSize = 12;
            Bttn_Browser.Width = 70 * _percentWidth;
            Bttn_Browser.Foreground = Brushes.DarkSlateGray;
            Bttn_Browser.Background = Brushes.Transparent;
            Canvas.SetBottom(Bttn_Browser, _imageOffset);
            Canvas.SetLeft(Bttn_Browser, ImLayout.Width / 4 - Bttn_Browser.Width);

            // Set the video player's properties
            //_VideoPlayer.Width = ImLayout.Width * 0.2;
            //_VideoPlayer.Height = ImLayout.Height * 0.2;
            _VideoPlayer.Width = ImContent.Width * 0.8;
            _VideoPlayer.Height = ImContent.Height * 0.8;
            _VideoPlayer.LoadedBehavior = MediaState.Manual;
            _VideoPlayer.ScrubbingEnabled = true;
            _VideoPlayer.Source = null;

            // Finally, use only the _percentWidth to adjust the Content Canvas
            // This is because the BackGround image is not stretched.  For a stretched image, use both
            ImContent.Height = ImLayout.Height;
            ImContent.Width = ImLayout.Width;

            // We initialize the Arrays to be used by the viewers
            _FilesPics = Directory.GetFiles(_Folder.Content.ToString() + @"\Pictures", "*.*");
            _FilesMP4 = Directory.GetFiles(_Folder.Content.ToString(), "*.wmv");
            _ThumbNails = Directory.GetFiles(_Folder.Content.ToString() + @"\Thumbs", "*.png");
            _FilesPDF = Directory.GetFiles(_Folder.Content.ToString(), "*.pdf");

            LoadMainMenu();
        }

        private void _KinectSensor1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor _oldSensor = (KinectSensor)e.OldValue;
            Uninitsensor(_oldSensor);

            KinectSensor _newSensor = (KinectSensor)e.NewValue;
            Initsensor(_newSensor);

            TxtTilt.Text = _KinectSensor1.Kinect.ElevationAngle.ToString();
        }
        #endregion Constructor


        #region KinectCursorManager
        private void StartTracking(Theviewer sender)
        {
            var _KinectCursor = KinectCursorManager.Instance;
            _KinectCursor.GesturePointTrackingInitialize(_SwpLngth, _SwpDeviate, _SwpTime, _XOff);
            _KinectCursor.GesturePointTrackingStart();
            _KinectCursor.SwipeOutOfBoundsDetected += OutOfBounds;

            switch (sender)
            {
                case Theviewer.Image:
                    _KinectCursor.SwipeDetected += SwitchPicture;
                    break;

                case Theviewer.Video:
                    _KinectCursor.SwipeDetected += PauseVid;
                    break;

                case Theviewer.PDF:
                    _KinectCursor.SwipeDetected += SwitchPage;
                    break;

                default:
                    StopTracking();
                    break;
            }
        }

        private void PauseTracking()
        {
            var _KinectCursor = KinectCursorManager.Instance;
            _KinectCursor.GesturePoints.Clear();
            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(2000));
        }

        private void StopTracking()
        {
            var _KinectCursor = KinectCursorManager.Instance;
            _KinectCursor.SwipeDetected -= PauseVid;
            _KinectCursor.SwipeDetected -= SwitchPage;
            _KinectCursor.SwipeDetected -= SwitchPicture;
            _KinectCursor.SwipeOutOfBoundsDetected -= OutOfBounds;
            _KinectCursor.GesturePointTrackingStop();
        }

        private void OutOfBounds(object sender, KinectCursorEventArgs e)
        {
            var _kinect = KinectCursorManager.Instance;
            _kinect.GesturePoints.Clear();
        }
        #endregion KinectCursorManager


        #region Viewer OnClick Methods
        private void Clk_ChangeFolder(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog _changeFolder = new System.Windows.Forms.FolderBrowserDialog();
            if (_changeFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _Folder.Content = _changeFolder.SelectedPath;
            }

            // Following, we extract all the Files and separate them into respective Arrays
            _FilesPics = Directory.GetFiles(_Folder.Content.ToString(), "*.png");
            _FilesMP4 = Directory.GetFiles(_Folder.Content.ToString(), "*.wmv");
            _ThumbNails = Directory.GetFiles(_Folder.Content.ToString() + @"\Thumbs", "*.png");
            _FilesPDF = Directory.GetFiles(_Folder.Content.ToString(), "*.pdf");

            // Reset the _currentFile tracker
            _currentFile = 0;
        }

        private void Clk_Images(object sender, RoutedEventArgs e)
        {
            // Clear Canvas-Content and Load Home Tab image
            ClearContent();
            HomeTabImage();

            if (_FilesPics.Length <= 0)
            {
                TextBlock _NoPics = new TextBlock();
                _NoPics.Width = ImContent.Width / 1.5;
                _NoPics.Height = ImContent.Height / 2;
                _NoPics.FontSize = 26;
                _NoPics.Foreground = Brushes.White;
                _NoPics.TextAlignment = TextAlignment.Center;
                _NoPics.Text =
                    "There are no pictures in the current Folder.\n\n" +
                    "Please refer to the Home Button to change the Folder\n" +
                    "in use.  Thank you.";
                Canvas.SetZIndex(_NoPics, ZVisuals);
                Canvas.SetTop(_NoPics, ImContent.Height / 4);
                Canvas.SetLeft(_NoPics, ImContent.Width / 6);
                ImContent.Children.Add(_NoPics);
            }
            else
            {
                StartTracking(Theviewer.Image);
                #region Buttons
                PicViewPrevious _previous = new PicViewPrevious();
                _previous.Width = 420 * _percentWidth;
                _previous.Height = 200 * _percentWidth;
                _previous.Imagen.Width = 420 * _percentWidth;
                _previous.Imagen.Height = 200 * _percentWidth;
                _previous.Boton.Width = 420 * _percentWidth;
                _previous.Boton.Height = 200 * _percentWidth;
                Canvas.SetZIndex(_previous, ZButtons);
                Canvas.SetTop(_previous, _imageOffset);
                Canvas.SetLeft(_previous, 0);
                _previous.Boton.Click += PreviousPicture;
                ImContent.Children.Add(_previous);

                PicViewNext _next = new PicViewNext();
                _next.Width = 420 * _percentWidth;
                _next.Height = 80 * _percentWidth;
                _next.Imagen.Width = 420 * _percentWidth;
                _next.Imagen.Height = 200 * _percentWidth;
                _next.Boton.Width = 420 * _percentWidth;
                _next.Boton.Height = 200 * _percentWidth;
                Canvas.SetZIndex(_next, ZButtons);
                Canvas.SetTop(_next, _imageOffset);
                Canvas.SetRight(_next, 0);
                _next.Boton.Click += NextPicture;
                ImContent.Children.Add(_next);
                #endregion Buttons

                DisplayImage();
            }

        }

        private void Clk_PDF(object sender, RoutedEventArgs e)
        {
            ClearContent();
            HomeTabImage();
            DisplayPdfThumbs();
        }

        private void Clk_Videos(object sender, RoutedEventArgs e)
        {
            ClearContent();
            HomeTabImage();
            DisplayVidThumbs();
        }

        private void Clk_Home(object sender, RoutedEventArgs e)
        {
            // Go back to Main Menu
            LoadMainMenu();
        }

        private void Clk_Exit(object sender, RoutedEventArgs e)
        {
            ClearContent();
            ReduceOpacity();

            #region Buttons
            HoverButton _DenyExit = new HoverButton();
            _DenyExit.Width = 100 * _percentWidth;
            _DenyExit.Height = 100 * _percentWidth;
            _DenyExit.FontSize = 36;
            _DenyExit.Foreground = Brushes.DarkSlateGray;
            _DenyExit.Content = "No";
            _DenyExit.Click += OnDeny;
            Canvas.SetZIndex(_DenyExit, ZButtons);
            Canvas.SetTop(_DenyExit, ImLayout.Height / 2 + _DenyExit.Height / 2);
            Canvas.SetLeft(_DenyExit, ImLayout.Width / 2 + _DenyExit.Width);
            ImContent.Children.Add(_DenyExit);

            HoverButton _ConfirmExit = new HoverButton();
            _ConfirmExit.Width = 100 * _percentWidth;
            _ConfirmExit.Height = 100 * _percentWidth;
            _ConfirmExit.FontSize = 36;
            _ConfirmExit.Foreground = Brushes.DarkSlateGray;
            _ConfirmExit.Content = "Yes";
            _ConfirmExit.Click += OnConfirm;
            Canvas.SetZIndex(_ConfirmExit, ZButtons);
            Canvas.SetTop(_ConfirmExit, ImLayout.Height / 2 + _ConfirmExit.Height / 2);
            Canvas.SetRight(_ConfirmExit, ImLayout.Width / 2 + _ConfirmExit.Width);
            ImContent.Children.Add(_ConfirmExit);
            #endregion Buttons

            TextBlock _ChooseExit = new TextBlock();
            // Text for deciding to exit app
            _ChooseExit.Opacity = 1.0;
            _ChooseExit.Height = ImContent.Height / 8;
            _ChooseExit.Width = ImContent.Width / 2;
            _ChooseExit.Foreground = Brushes.White;
            _ChooseExit.FontSize = 36;
            _ChooseExit.FontFamily = new FontFamily("Century");
            _ChooseExit.TextAlignment = TextAlignment.Center;
            _ChooseExit.Text = "Are you sure you wish to exit?";
            Canvas.SetZIndex(_ChooseExit, ZVisuals);
            Canvas.SetTop(_ChooseExit, ImContent.Height / 4);
            Canvas.SetLeft(_ChooseExit, ImContent.Width / 4);
            ImContent.Children.Add(_ChooseExit);
        }

        private void OnConfirm(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void OnDeny(object sender, RoutedEventArgs e)
        {
            ClearContent();
            LoadMainMenu();
        }

        private void Clk_3D(object sender, RoutedEventArgs e)
        {
            // Clear Canvas-Content and Load Home Tab image
            ClearContent();
            HomeTabImage();

            TextBlock _3DNotReady = new TextBlock();
            _3DNotReady.Width = ImContent.Width / 1.5;
            _3DNotReady.Height = ImContent.Height / 2;
            _3DNotReady.FontSize = 26;
            _3DNotReady.Foreground = Brushes.White;
            _3DNotReady.TextAlignment = TextAlignment.Center;
            _3DNotReady.Text =
                "Hello.  The 3D Viewer is still in the works..." +
                "\nPlease be patient, as there will be other students" +
                "\nwho will improve on this project and make it" +
                "\nsomething far more than the original developer imagined." +
                "\n\n Please click on the Home button to browse other vieweres,\n" +
                "Thank you. \n\nJ.K.";
            Canvas.SetZIndex(_3DNotReady, ZVisuals);
            Canvas.SetTop(_3DNotReady, ImContent.Height / 4);
            Canvas.SetLeft(_3DNotReady, ImContent.Width / 6);
            ImContent.Children.Add(_3DNotReady);
        }
        #endregion Viewer OnClick Methods


        #region Image Viewer Methods
        private void NextPicture(object sender, RoutedEventArgs e)
        {
            if (_currentFile == (_FilesPics.Count() - 1))
            {
                _currentFile = 0;
            }
            else _currentFile++;

            DisplayImage();
        }

        private void PreviousPicture(object sender, RoutedEventArgs e)
        {
            if (_currentFile == 0)
            {
                _currentFile = _FilesPics.Count() - 1;
            }
            else _currentFile--;

            DisplayImage();
        }

        private void SwitchPicture(object sender, KinectCursorEventArgs e)
        {
            if (e.IsDirectionRight)
            {
                NextPicture(sender, e);
                PauseTracking();
            }
            else
            {
                PreviousPicture(sender, e);
                PauseTracking();
            }
        }

        private void DisplayImage()
        {
            // Delete last picture from memory
            if (ImContent.Children[ImContent.Children.Count - 1] is Image)
                ImContent.Children.RemoveAt(ImContent.Children.Count - 1);

            BitmapImage _Picture = new BitmapImage();
            _Picture.BeginInit();
            _Picture.UriSource = new Uri(_FilesPics[_currentFile].ToString(), UriKind.Absolute);
            _Picture.EndInit();
            Image ImPicture = new Image();
            ImPicture.Source = _Picture;
            ImPicture.Width = ImContent.Width / 1.5;
            ImPicture.Height = ImContent.Height / 1.5;
            Canvas.SetZIndex(ImPicture, ZVisuals);
            Canvas.SetBottom(ImPicture, _imageOffset + 78 * _percentWidth);
            Canvas.SetLeft(ImPicture, ImContent.Width / 6);
            ImContent.Children.Add(ImPicture);
        }
        #endregion Image Viewer Methods


        #region Video Viewer Methods
        private void DisplayVidThumbs()
        {
            HideVideo();
            
            for (int i = 0, j = 1; i < _ThumbNails.Length; i++)
            {
                string _pathvideo = _ThumbNails[i].Substring(_Folder.Content.ToString().Length + 7);
                _pathvideo = _pathvideo.Substring(0, _pathvideo.Length - 4) + ".wmv";

                VidViewThumb _thumbnail = new VidViewThumb();
                BitmapImage imagen = new BitmapImage();
                _thumbnail.Name = "thumb" + i.ToString();
                imagen.BeginInit();
                imagen.UriSource = new Uri(_ThumbNails[i], UriKind.Absolute);
                imagen.EndInit();
                _thumbnail.Imagen.Source = imagen;
                _thumbnail.Imagen.Width = 156.3 * _percentWidth;
                _thumbnail.Imagen.Height = 115.2 * _percentWidth;
                _thumbnail.Width = _thumbnail.Imagen.Width;
                _thumbnail.Height = _thumbnail.Imagen.Height;
                _thumbnail.Boton.Width = _thumbnail.Width;
                _thumbnail.Boton.Height = _thumbnail.Height;
                _thumbnail.Boton.Content = _pathvideo;
                _thumbnail.Boton.Click += OnSelectVideo;
                if (_thumbnail.Width / 2 + _thumbnail.Imagen.Width * 1.5 * i > ImContent.Width)
                    j++;
                Canvas.SetZIndex(_thumbnail, ZButtons);
                Canvas.SetTop(_thumbnail, _thumbnail.Height * j);
                Canvas.SetLeft(_thumbnail, _thumbnail.Width / 2 + _thumbnail.Imagen.Width * 1.5 * i);
                ImContent.Children.Add(_thumbnail);
            }
        }

        private void OnSelectVideo(object sender, RoutedEventArgs e)
        {
            ClearContent();
            ShowVideo();

            Button _selection = sender as Button;
            _VideoPlayer.Source = new Uri(_Folder.Content.ToString() + _selection.Content.ToString(), UriKind.Absolute);

            _VideoPlayer.Stop();
            _VideoPlayer.Play();
            _VideoPlayer.Pause();
            _VideoPlayer.Volume = 1;
            _VideoPlayer.SpeedRatio = 1;
            _VideoPlayer.Play();
            _VideoPlayer.Position += TimeSpan.FromMilliseconds(100);
            
            #region Buttons
            ReturnButton _Return = new ReturnButton();
            _Return.Height = 77 * _percentWidth;
            _Return.Width = 94 * _percentWidth;
            _Return.Imagen.Height = _Return.Height;
            _Return.Imagen.Width = _Return.Width;
            _Return.OnHover.Height = _Return.Height;
            _Return.OnHover.Width = _Return.Width;
            _Return.Boton.Height = _Return.Height;
            _Return.Boton.Width = _Return.Width;
            _Return.Boton.Click += Clk_Videos;
            Canvas.SetZIndex(_Return, ZButtons);
            Canvas.SetTop(_Return, _imageOffset + 7 * _percentWidth);
            Canvas.SetLeft(_Return, 12 * _percentWidth);
            ImContent.Children.Add(_Return);

            VidViewForward _FF = new VidViewForward();
            _FF.Height = 65 * _percentWidth;
            _FF.Width = 70 * _percentWidth;
            _FF.Imagen.Height = _FF.Height;
            _FF.Imagen.Width = _FF.Width;
            _FF.Boton.Height = _FF.Height;
            _FF.Boton.Width = _FF.Width;
            _FF.Boton.KinectCursorEnter += ForwardVidTrue;
            _FF.Boton.KinectCursorLeave += ForwardVidFalse;
            Canvas.SetZIndex(_FF, ZButtons);
            Canvas.SetBottom(_FF, _imageOffset);
            Canvas.SetLeft(_FF, 548 * _percentWidth);
            ImContent.Children.Add(_FF);

            VidViewRewind _RW = new VidViewRewind();
            _RW.Height = 65 * _percentWidth;
            _RW.Width = 70 * _percentWidth;
            _RW.Imagen.Height = _RW.Height;
            _RW.Imagen.Width = _RW.Width;
            _RW.Boton.Height = _RW.Height;
            _RW.Boton.Width = _RW.Width;
            _RW.Boton.KinectCursorEnter += RewindVidTrue;
            _RW.Boton.KinectCursorLeave += RewindVidFalse;
            Canvas.SetZIndex(_RW, ZButtons);
            Canvas.SetBottom(_RW, _imageOffset);
            Canvas.SetLeft(_RW, 413 * _percentWidth);
            ImContent.Children.Add(_RW);

            VidViewPause _Pause = new VidViewPause();
            _Pause.Height = 80 * _percentWidth;
            _Pause.Width = 65 * _percentWidth;
            _Pause.Imagen.Height = _Pause.Height;
            _Pause.Imagen.Width = _Pause.Width;
            _Pause.Boton.Height = _Pause.Height;
            _Pause.Boton.Width = _Pause.Width;
            _Pause.Boton.Click += PauseVid;
            Canvas.SetZIndex(_Pause, ZButtons);
            Canvas.SetBottom(_Pause, _imageOffset);
            Canvas.SetLeft(_Pause, 483 * _percentWidth);
            ImContent.Children.Add(_Pause);
            #endregion Buttons

            IsPlaying();
        }

        private void IsPlaying()
        {
            for ( int i = 3 ; i < ImContent.Children.Count; i++)
                ImContent.Children.RemoveAt(i);

            HomeTabVidMenuPause();
            StartTracking(Theviewer.Video);

            VidViewPause _Pause = new VidViewPause();
            _Pause.Height = 80 * _percentWidth;
            _Pause.Width = 65 * _percentWidth;
            _Pause.Imagen.Height = _Pause.Height;
            _Pause.Imagen.Width = _Pause.Width;
            _Pause.Boton.Height = _Pause.Height;
            _Pause.Boton.Width = _Pause.Width;
            _Pause.Boton.Click += PauseVid;
            Canvas.SetZIndex(_Pause, ZButtons);
            Canvas.SetBottom(_Pause, _imageOffset);
            Canvas.SetLeft(_Pause, 483 * _percentWidth);
            ImContent.Children.Add(_Pause);
        }

        private void IsPaused()
        {
            for (int i = 3; i < ImContent.Children.Count; i++)
                ImContent.Children.RemoveAt(i);

            HomeTabVidMenuPlay();
            StopTracking();

            VidViewPlay _Play = new VidViewPlay();
            _Play.Height = 80 * _percentWidth;
            _Play.Width = 65 * _percentWidth;
            _Play.Imagen.Height = _Play.Height;
            _Play.Imagen.Width = _Play.Width;
            _Play.Boton.Height = _Play.Height;
            _Play.Boton.Width = _Play.Width;
            _Play.Boton.Click += PlayVid;
            Canvas.SetZIndex(_Play, ZButtons);
            Canvas.SetBottom(_Play, _imageOffset);
            Canvas.SetLeft(_Play, 483 * _percentWidth);
            ImContent.Children.Add(_Play);
        }

        private void PauseVid(object sender, RoutedEventArgs e)
        {
            _VideoPlayer.Pause();
            IsPaused();
            PauseTracking();
        }

        private void PlayVid(object sender, RoutedEventArgs e)
        {
            _VideoPlayer.Play();
            IsPlaying();
        }

        private void ForwardVidTrue(object sender, KinectCursorEventArgs e)
        {
            _VideoPlayer.Pause();
            IsPaused();
            _FastForward.Start();
            _FastForward.Tick += _FastForward_Tick;
        }

        private void ForwardVidFalse(object sender, KinectCursorEventArgs e)
        {
            _FastForward.Tick -= _FastForward_Tick;
            _FastForward.Stop();
        }

        private void RewindVidTrue(object sender, KinectCursorEventArgs e)
        {
            _VideoPlayer.Pause();
            IsPaused();
            _Rewind.Start();
            _Rewind.Tick += _Rewind_Tick;
        }

        private void RewindVidFalse(object sender, KinectCursorEventArgs e)
        {
            _Rewind.Tick -= _Rewind_Tick;
            _Rewind.Stop();
        }

        private void _FastForward_Tick(object sender, EventArgs e)
        {
            _VideoPlayer.Position += TimeSpan.FromMilliseconds(300);
        }

        private void _Rewind_Tick(object sender, EventArgs e)
        {
            _VideoPlayer.Position -= TimeSpan.FromMilliseconds(100);
        }
        
        private void HideVideo()
        {
            _VideoPlayer.Source = null;
            _VideoPlayer.Opacity = 0;
            Canvas.SetZIndex(_VideoPlayer, 0);
        }

        private void ShowVideo()
        {
            _VideoPlayer.Opacity = 1;
            Canvas.SetZIndex(_VideoPlayer, ZVisuals);
            Canvas.SetBottom(_VideoPlayer, _imageOffset + 78 * _percentWidth);
            Canvas.SetLeft(_VideoPlayer, 0);
        }
        #endregion Video Viewer Methods


        #region PDF Viewer Methods
        private void DisplayPdfThumbs()
        {
            for (int i = 0, j = 0; i < _FilesPDF.Length; i++)
            {
                string _pathPDF = _FilesPDF[i].Substring(_Folder.Content.ToString().Length);
                string name = ((_pathPDF.Substring(0, _pathPDF.Length - 4)).Replace('_', ' ')).Replace('\\', ' ');
                
                PDFViewThumbs _thumbnail = new PDFViewThumbs();
                _thumbnail.Width = 152 * _percentWidth;
                _thumbnail.Height = 176 * _percentWidth;
                _thumbnail.Imagen.Width = _thumbnail.Width; ;
                _thumbnail.Imagen.Height = _thumbnail.Height;
                _thumbnail.Boton.Width = _thumbnail.Width;
                _thumbnail.Boton.Height = _thumbnail.Height;
                _thumbnail.Boton.Content = _pathPDF;
                _thumbnail.Boton.Click += OnSelectPDF;
                _thumbnail.Nombre.Width = 135 * _percentWidth;
                _thumbnail.Nombre.Height = 115 * _percentWidth;
                _thumbnail.Nombre.Text += name;
                if (_thumbnail.Width / 2 + _thumbnail.Width * 1.5 * i > ImContent.Width)
                    j++;
                Canvas.SetZIndex(_thumbnail, ZButtons);
                Canvas.SetTop(_thumbnail, _thumbnail.Height / 2 + _thumbnail.Height * j);
                Canvas.SetLeft(_thumbnail, _thumbnail.Width / 2 + _thumbnail.Imagen.Width * 1.5 * i);
                ImContent.Children.Add(_thumbnail);
            }
        }

        private void OnSelectPDF(object sender, RoutedEventArgs e)
        {
            ClearContent();
            for (int i = 0; i < ImLayout.Children.Count - 2; i++)
                ImLayout.Children[i].Opacity = 0;
            
            Button _selection = sender as Button;
            StartTracking(Theviewer.PDF);
            
            ReturnButton _Return = new ReturnButton();
            _Return.Height = 77 * _percentWidth;
            _Return.Width = 94 * _percentWidth;
            _Return.Imagen.Height = _Return.Height;
            _Return.Imagen.Width = _Return.Width;
            _Return.OnHover.Height = _Return.Height;
            _Return.OnHover.Width = _Return.Width;
            _Return.Boton.Height = _Return.Height;
            _Return.Boton.Width = _Return.Width;
            _Return.Boton.Click += Clk_PDF;
            Canvas.SetZIndex(_Return, ZButtons);
            Canvas.SetTop(_Return, 0);
            Canvas.SetLeft(_Return, 12 * _percentWidth);
            ImContent.Children.Add(_Return);

            WindowsFormsHost ImPDFViewer = new WindowsFormsHost();
            LoadedPDF = new AdobeReader(_Folder.Content.ToString() + _selection.Content.ToString());
            ImPDFViewer.Child = LoadedPDF;
            ImPDFViewer.Height = ImContent.Height * 0.9;
            ImPDFViewer.Width = ImContent.Width * 0.9;
            ImPDFViewer.Child.Size = new System.Drawing.Size((int)ImPDFViewer.Height, (int)ImPDFViewer.Width);
            Canvas.SetZIndex(ImPDFViewer, 0);
            Canvas.SetBottom(ImPDFViewer, 0);
            Canvas.SetLeft(ImPDFViewer, ImContent.Width * 0.05);
            ImContent.Children.Add(ImPDFViewer);
        }

        private void SwitchPage(object sender, KinectCursorEventArgs e)
        {
            if (e.IsDirectionRight)
            {
                LoadedPDF.PreviousPage();
                PauseTracking();
            }
            else
            {
                LoadedPDF.NextPage();
                PauseTracking();
            }
        }
        #endregion PDF Viewer Methods


        #region Loading and Clearing
        private void ClearContent()
        {
            // This function removes all the elements created after the Window_Loaded event
            ImContent.Children.Clear();

            // This will Clean the KinectCursorManager's Gesture Tracking events
            StopTracking();
        }

        private void ReduceOpacity()
        {
            for (int i = 0; i < ImLayout.Children.Count - 2; i++)
            {
                ImLayout.Children[i].Opacity = 0;
                ImLayout.Children[i].IsEnabled = false;
            }
            ImBackGround.Opacity = 0.2;
        }

        private void IncreaseOpacity()
        {
            for (int i = 0; i < ImLayout.Children.Count - 2; i++)
            {
                ImLayout.Children[i].Opacity = 1;
                ImLayout.Children[i].IsEnabled = true;
            }
        }

        private void LoadMainMenu()
        {
            // Clear Canvas-Content and Load Main Menu image
            ClearContent();
            IncreaseOpacity();
            MainMenuImage();
            HideVideo();

            //Buttons
            //Refer to the user control ButtonImage on the README for a description of how it works
            HoverButtonExit Bttn_Exit = new HoverButtonExit();
            Bttn_Exit.Height = 41 * _percentWidth;
            Bttn_Exit.Width = 41 * _percentWidth;
            Bttn_Exit._Texto.Height = Bttn_Exit._Texto.Height * _percentWidth;
            Bttn_Exit._Texto.Width = Bttn_Exit._Texto.Width * _percentWidth;
            Bttn_Exit._Texto.Text = "Quit";
            Bttn_Exit.Imagen.Height = Bttn_Exit.Imagen.Height * _percentWidth;
            Bttn_Exit.Imagen.Width = Bttn_Exit.Imagen.Width * _percentWidth;
            Bttn_Exit.Boton.Height = Bttn_Exit.Boton.Height * _percentWidth;
            Bttn_Exit.Boton.Width = Bttn_Exit.Boton.Width * _percentWidth;
            Bttn_Exit.Boton.Click += Clk_Exit;
            Canvas.SetZIndex(Bttn_Exit, ZButtons);
            Canvas.SetTop(Bttn_Exit, (11 * _percentWidth) + _imageOffset);
            Canvas.SetLeft(Bttn_Exit, 11 * _percentWidth);
            ImContent.Children.Add(Bttn_Exit);

            HoverButtonImages _BttnImages = new HoverButtonImages();
            _BttnImages.Boton.Click += Clk_Images;
            _BttnImages.Boton.Height *= _percentWidth;
            _BttnImages.Boton.Width *= _percentWidth;
            _BttnImages.Imagen.Height *= _percentWidth;
            _BttnImages.Imagen.Width *= _percentWidth;
            Canvas.SetZIndex(_BttnImages, ZButtons);
            Canvas.SetTop(_BttnImages, 228 * _percentWidth + _imageOffset);
            Canvas.SetLeft(_BttnImages, 285 * _percentWidth);
            ImContent.Children.Add(_BttnImages);

            HoverButtonVideos _BttnVideos = new HoverButtonVideos();
            _BttnVideos.Boton.Click += Clk_Videos;
            _BttnVideos.Boton.Height *= _percentWidth;
            _BttnVideos.Boton.Width *= _percentWidth;
            _BttnVideos.Imagen.Height *= _percentWidth;
            _BttnVideos.Imagen.Width *= _percentWidth;
            Canvas.SetZIndex(_BttnVideos, ZButtons);
            Canvas.SetTop(_BttnVideos, 349 * _percentWidth + _imageOffset);
            Canvas.SetLeft(_BttnVideos, 285 * _percentWidth);
            ImContent.Children.Add(_BttnVideos);

            HoverButtonPDF _BttnPDF = new HoverButtonPDF();
            _BttnPDF.Boton.Click += Clk_PDF;
            _BttnPDF.Boton.Height *= _percentWidth;
            _BttnPDF.Boton.Width *= _percentWidth;
            _BttnPDF.Imagen.Height *= _percentWidth;
            _BttnPDF.Imagen.Width *= _percentWidth;
            Canvas.SetZIndex(_BttnPDF, ZButtons);
            Canvas.SetTop(_BttnPDF, 464 * _percentWidth + _imageOffset);
            Canvas.SetLeft(_BttnPDF, 285 * _percentWidth);
            ImContent.Children.Add(_BttnPDF);

            HoverButton3D _Bttn3D = new HoverButton3D();
            _Bttn3D.Boton.Click += Clk_3D;
            _Bttn3D.Boton.Height *= _percentWidth;
            _Bttn3D.Boton.Width *= _percentWidth;
            _Bttn3D.Imagen.Height *= _percentWidth;
            _Bttn3D.Imagen.Width *= _percentWidth;
            Canvas.SetZIndex(_Bttn3D, ZButtons);
            Canvas.SetTop(_Bttn3D, 582 * _percentWidth + _imageOffset);
            Canvas.SetLeft(_Bttn3D, 285 * _percentWidth);
            ImContent.Children.Add(_Bttn3D);
        }

        private void MainMenuImage()
        {
            BitmapImage _Menu = new BitmapImage();
            _Menu.BeginInit();
            _Menu.UriSource = new Uri("Pictures/ImView_FullMenu.png", UriKind.Relative);
            _Menu.EndInit();
            Image ImMenu = new Image();
            ImMenu.Source = _Menu;
            ImMenu.Width = ImContent.Width;
            ImMenu.Height = ImContent.Height;
            Canvas.SetZIndex(ImMenu, ZVisuals);
            Canvas.SetTop(ImMenu, 0);
            Canvas.SetLeft(ImMenu, 0);
            ImContent.Children.Add(ImMenu);
        }

        private void HomeTabImage()
        {
            ReduceOpacity();

            BitmapImage _HomeTab = new BitmapImage();
            _HomeTab.BeginInit();
            _HomeTab.UriSource = new Uri("Pictures/ImView_HomeTab.png", UriKind.Relative);
            _HomeTab.EndInit();
            Image ImHomeTab = new Image();
            ImHomeTab.Source = _HomeTab;
            ImHomeTab.Width = ImContent.Width;
            ImHomeTab.Height = ImContent.Height;
            Canvas.SetZIndex(ImHomeTab, ZVisuals);
            Canvas.SetTop(ImHomeTab, 0);
            Canvas.SetLeft(ImHomeTab, 0);
            ImContent.Children.Add(ImHomeTab);

            ButtonHome _BttnHome = new ButtonHome();
            _BttnHome.Boton.Click += Clk_Home;
            _BttnHome.Boton.Height *= _percentWidth;
            _BttnHome.Boton.Width *= _percentWidth;
            _BttnHome.Imagen.Height *= _percentWidth;
            _BttnHome.Imagen.Width *= _percentWidth;
            Canvas.SetZIndex(_BttnHome, ZButtons);
            Canvas.SetTop(_BttnHome, 696 * _percentWidth + _imageOffset);
            Canvas.SetLeft(_BttnHome, 5 * _percentWidth);
            ImContent.Children.Add(_BttnHome);
        }

        private void HomeTabVidMenuPlay()
        {
            ReduceOpacity();

            BitmapImage _HomeTab = new BitmapImage();
            _HomeTab.BeginInit();
            _HomeTab.UriSource = new Uri("Pictures/VidView_MenuPlay.png", UriKind.Relative);
            _HomeTab.EndInit();
            Image ImHomeTab = new Image();
            ImHomeTab.Source = _HomeTab;
            ImHomeTab.Width = ImContent.Width;
            ImHomeTab.Height = ImContent.Height;
            Canvas.SetZIndex(ImHomeTab, ZVisuals);
            Canvas.SetTop(ImHomeTab, 0);
            Canvas.SetLeft(ImHomeTab, 0);
            ImContent.Children.Add(ImHomeTab);

            ButtonHome _BttnHome = new ButtonHome();
            _BttnHome.Boton.Click += Clk_Home;
            _BttnHome.Boton.Height *= _percentWidth;
            _BttnHome.Boton.Width *= _percentWidth;
            _BttnHome.Imagen.Height *= _percentWidth;
            _BttnHome.Imagen.Width *= _percentWidth;
            Canvas.SetZIndex(_BttnHome, ZButtons);
            Canvas.SetTop(_BttnHome, 696 * _percentWidth + _imageOffset);
            Canvas.SetLeft(_BttnHome, 5 * _percentWidth);
            ImContent.Children.Add(_BttnHome);
        }

        private void HomeTabVidMenuPause()
        {
            ReduceOpacity();

            BitmapImage _HomeTab = new BitmapImage();
            _HomeTab.BeginInit();
            _HomeTab.UriSource = new Uri("Pictures/VidView_MenuPause.png", UriKind.Relative);
            _HomeTab.EndInit();
            Image ImHomeTab = new Image();
            ImHomeTab.Source = _HomeTab;
            ImHomeTab.Width = ImContent.Width;
            ImHomeTab.Height = ImContent.Height;
            Canvas.SetZIndex(ImHomeTab, ZVisuals);
            Canvas.SetTop(ImHomeTab, 0);
            Canvas.SetLeft(ImHomeTab, 0);
            ImContent.Children.Add(ImHomeTab);

            ButtonHome _BttnHome = new ButtonHome();
            _BttnHome.Boton.Click += Clk_Home;
            _BttnHome.Boton.Height *= _percentWidth;
            _BttnHome.Boton.Width *= _percentWidth;
            _BttnHome.Imagen.Height *= _percentWidth;
            _BttnHome.Imagen.Width *= _percentWidth;
            Canvas.SetZIndex(_BttnHome, ZButtons);
            Canvas.SetTop(_BttnHome, 696 * _percentWidth + _imageOffset);
            Canvas.SetLeft(_BttnHome, 5 * _percentWidth);
            ImContent.Children.Add(_BttnHome);
        }
        #endregion Loading and Clearing
        

        #region Tilting the Kinect
        private void TiltUp(object sender, RoutedEventArgs e)
        {
            // This function adjusts the tilt on the Kinect up, to make it easier to manipulate
            Bttn_DownTilt.IsEnabled = false;
            Bttn_UpTilt.IsEnabled = false;

            if (_KinectSensor1.Kinect != null && _KinectSensor1.Kinect.IsRunning)
            {
                _KinectSensor1.Kinect.ElevationAngle = _KinectSensor1.Kinect.ElevationAngle + 3;
                TxtTilt.Text = _KinectSensor1.Kinect.ElevationAngle.ToString();
            }
            System.Threading.Thread.Sleep(new TimeSpan(hours: 0, minutes: 0, seconds: 1));

            Bttn_DownTilt.IsEnabled = true;
            Bttn_UpTilt.IsEnabled = true;
        }

        private void TiltDown(object sender, RoutedEventArgs e)
        {
            // This function adjusts the tilt on the Kinect down, to make it easier to manipulate
            Bttn_DownTilt.IsEnabled = false;
            Bttn_UpTilt.IsEnabled = false;

            if (_KinectSensor1.Kinect != null && _KinectSensor1.Kinect.IsRunning)
            {
                _KinectSensor1.Kinect.ElevationAngle = _KinectSensor1.Kinect.ElevationAngle - 3;
                TxtTilt.Text = _KinectSensor1.Kinect.ElevationAngle.ToString();
            }
            System.Threading.Thread.Sleep(new TimeSpan(hours: 0, minutes: 0, seconds: 1));

            Bttn_DownTilt.IsEnabled = true;
            Bttn_UpTilt.IsEnabled = true;
        }
        #endregion Tilting the Kinect


        #region (Un)Init
        private void Uninitsensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
                sensor.DepthStream.Disable();
                sensor.SkeletonStream.Disable();
                this._FrameSkeles = null;
            }
        }

        private void Initsensor(KinectSensor sensor)
        {
            var parameters = new TransformSmoothParameters
            {
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters);
            sensor.DepthStream.Enable();
            this._FrameSkeles = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];


            try
            {
                sensor.Start();
                TxtTilt.Text = _KinectSensor1.Kinect.ElevationAngle.ToString();
            }
            catch (System.IO.IOException)
            {
                _KinectSensor1.AppConflictOccurred();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_KinectSensor1 != null)
                Uninitsensor(_KinectSensor1.Kinect);
        }
        #endregion (Un)Init


        //#region Dummies
        //TextBlock asdasd;

        //private void dum1()
        //{
        //    ClearContent();

        //    asdasd = new TextBlock();
        //    asdasd.Width = ImContent.Width / 2;
        //    asdasd.Height = ImContent.Height;
        //    asdasd.FontSize = 32;
        //    asdasd.Background = Brushes.Black;
        //    asdasd.Foreground = Brushes.White;
        //    Canvas.SetTop(asdasd, _imageOffset);
        //    Canvas.SetLeft(asdasd, ImContent.Width / 4);
        //    Canvas.SetZIndex(asdasd, ZButtons);
        //    ImContent.Children.Add(asdasd);

        //    var _KinectCursor = KinectCursorManager.Instance;
        //    _KinectCursor.GesturePointTrackingInitialize(_SwpLngth, _SwpDeviate, _SwpTime, _XOff);
        //    _KinectCursor.GesturePointTrackingStart();
        //    _KinectCursor.SwipeDetected += ListPoints;
        //    _KinectCursor.SwipeOutOfBoundsDetected += OutBounds;
        //}

        //private void OutBounds(object sender, KinectCursorEventArgs e)
        //{
        //    asdasd.Text = "";
        //    var _Kinect = KinectCursorManager.Instance;

        //    for (int i = 0; i < _Kinect.GesturePoints.Count; i++)
        //    {
        //        asdasd.Text += "\n" + _Kinect.GesturePoints[i];
        //    }
        //    asdasd.Text += "\n\nThis was Out of Bounds";
        //}

        //private void ListPoints(object sender, KinectCursorEventArgs e)
        //{
        //    var _Kinect = KinectCursorManager.Instance;
        //    asdasd.Text = "";
        //    _Kinect.GesturePoints.Clear();

        //    for (int i = 0; i < _Kinect.GesturePoints.Count; i++)
        //    {
        //        asdasd.Text += "\n" + _Kinect.GesturePoints[i];
        //    }
        //    asdasd.Text += "\n\nThat is all.";
        //}
        //#endregion Dummies
    }
}
