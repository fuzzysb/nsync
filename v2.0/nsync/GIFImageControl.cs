///////////////////////////////////////////////////////
//                                        
//   Title:     GIFImageControl.cs
//   Author:    Siow Boon Lin Eugene
//
///////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace nsync
{
    // Class Credited to 
    // http://www.solidrockstable.com/blogs/PragmaticTSQL/Lists/Posts/Post.aspx?ID=37
    /// <summary>
    /// GIFImageControl handles the GUI components for GIF images in the other pages.
    /// </summary>
    public class GIFImageControl : Image
    {
        #region Class Variables
        private Bitmap _Bitmap;
        private bool _mouseClickStarted;
        #endregion

        #region Properties and Dependency Properties
        /// <summary>
        /// dependency property that can be set through styling, attached to the property to allow pause on click
        /// </summary>
        public static readonly DependencyProperty AllowClickToPauseProperty =
            DependencyProperty.Register("AllowClickToPause", typeof(bool), typeof(GIFImageControl),
                                        new UIPropertyMetadata(true));
        
        /// <summary>
        /// dependency property attached to the gifsource property
        /// </summary>
        public static readonly DependencyProperty GIFSourceProperty =
            DependencyProperty.Register("GIFSource", typeof(string), typeof(GIFImageControl),
                                        new UIPropertyMetadata("", GIFSource_Changed));
        
        /// <summary>
        /// dependency property attached to the playanimation property
        /// </summary>
        public static readonly DependencyProperty PlayAnimationProperty =
            DependencyProperty.Register("PlayAnimation", typeof(bool), typeof(GIFImageControl),
                                        new UIPropertyMetadata(true, PlayAnimation_Changed));

        /// <summary>
        /// Property whether click pauses or not
        /// </summary>
        public bool AllowClickToPause
        {
            get { return (bool)GetValue(AllowClickToPauseProperty); }
            set { SetValue(AllowClickToPauseProperty, value); }
        }

        /// <summary>
        /// property to play the animation
        /// </summary>
        public bool PlayAnimation
        {
            get { return (bool)GetValue(PlayAnimationProperty); }
            set { SetValue(PlayAnimationProperty, value); }
        }

        /// <summary>
        /// property of the source of the gif
        /// </summary>
        public string GIFSource
        {
            get { return (string)GetValue(GIFSourceProperty); }
            set { SetValue(GIFSourceProperty, value); }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for GIFImageControl
        /// </summary>
        public GIFImageControl()
        {
            MouseLeftButtonDown += GIFImageControl_MouseLeftButtonDown;
            MouseLeftButtonUp += GIFImageControl_MouseLeftButtonUp;
            MouseLeave += GIFImageControl_MouseLeave;
            Click += GIFImageControl_Click;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// event handler when gif image control is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GIFImageControl_Click(object sender, RoutedEventArgs e)
        {
            if (AllowClickToPause)
                PlayAnimation = !PlayAnimation;
        }

        /// <summary>
        /// event handler when mouse leaves the gif image control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GIFImageControl_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseClickStarted = false;
        }

        /// <summary>
        /// event handler when the the left mouse button is up (after a click)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GIFImageControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_mouseClickStarted)
                FireClickEvent(sender, e);
            _mouseClickStarted = false;
        }

        /// <summary>
        /// event handler when the the left mouse button is down (after a click)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GIFImageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseClickStarted = true;
        }

        /// <summary>
        /// route the click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FireClickEvent(object sender, RoutedEventArgs e)
        {
            if (null != Click)
                Click(sender, e);
        }

        /// <summary>
        /// event handler when playanimation is changed
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void PlayAnimation_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var gic = (GIFImageControl)d;
            if ((bool)e.NewValue)
            {
                //StartAnimation if GIFSource is properly set
                if (null != gic._Bitmap)
                    ImageAnimator.Animate(gic._Bitmap, gic.OnFrameChanged);
            }
            else
                //Pause Animation
                ImageAnimator.StopAnimate(gic._Bitmap, gic.OnFrameChanged);
        }

        /// <summary>
        /// Gets bitmaps for a resource assembly
        /// </summary>
        /// <param name="assemblyToSearch">the assembly to search for bitmaps</param>
        /// <returns>a gdi bitmap</returns>
        private Bitmap GetBitmapResourceFromAssembly(Assembly assemblyToSearch)
        {
            string[] resourselist = assemblyToSearch.GetManifestResourceNames();
            if (null != assemblyToSearch.FullName)
            {
                string searchName = String.Format("{0}.{1}", assemblyToSearch.FullName.Split(',')[0], GIFSource);
                if (resourselist.Contains(searchName))
                {
                    Stream bitmapStream = assemblyToSearch.GetManifestResourceStream(searchName);
                    if (null != bitmapStream)
                        return (Bitmap)System.Drawing.Image.FromStream(bitmapStream);
                }
            }
            return null;
        }

        /// <summary>
        /// Checks and sets the gifsource 
        /// </summary>
        private void SetImageGIFSource()
        {
            if (null != _Bitmap)
            {
                ImageAnimator.StopAnimate(_Bitmap, OnFrameChanged);
                _Bitmap = null;
            }
            if (String.IsNullOrEmpty(GIFSource))
            {
                //Turn off if GIF set to null or empty
                Source = null;
                InvalidateVisual();
                return;
            }
            if (File.Exists(GIFSource))
                _Bitmap = (Bitmap)System.Drawing.Image.FromFile(GIFSource);
            else
            {
                //Support looking for embedded resources
                Assembly assemblyToSearch = Assembly.GetAssembly(GetType());
                _Bitmap = GetBitmapResourceFromAssembly(assemblyToSearch);
                if (null == _Bitmap)
                {
                    assemblyToSearch = Assembly.GetCallingAssembly();
                    _Bitmap = GetBitmapResourceFromAssembly(assemblyToSearch);
                    if (null == _Bitmap)
                    {
                        assemblyToSearch = Assembly.GetEntryAssembly();
                        _Bitmap = GetBitmapResourceFromAssembly(assemblyToSearch);
                        if (null == _Bitmap)
                            throw new FileNotFoundException("GIF Source was not found.", GIFSource);
                    }
                }
            }
            if (PlayAnimation)
                ImageAnimator.Animate(_Bitmap, OnFrameChanged);
        }

        /// <summary>
        /// event handler when the gifsource is changed
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void GIFSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GIFImageControl)d).SetImageGIFSource();
        }

        /// <summary>
        /// event handler when frame is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFrameChanged(object sender, EventArgs e)
        {
            //delegates the task to OnFrameChangedInMainThread
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                   new OnFrameChangedDelegate(OnFrameChangedInMainThread));
        }

        /// <summary>
        /// method to carry out actions on frame changed, update frames
        /// </summary>
        private void OnFrameChangedInMainThread()
        {
            if (PlayAnimation)
            {
                ImageAnimator.UpdateFrames(_Bitmap);
                Source = GetBitmapSource(_Bitmap);
                InvalidateVisual();
            }
        }

        /// <summary>
        /// method to get a bitmap source object from a gdi bitmap
        /// </summary>
        /// <param name="gdiBitmap">gdi 'bitmap' object</param>
        /// <returns>bitmap source object</returns>
        private static BitmapSource GetBitmapSource(Bitmap gdiBitmap)
        {
            IntPtr hBitmap = gdiBitmap.GetHbitmap();
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                                                                              IntPtr.Zero,
                                                                              Int32Rect.Empty,
                                                                              BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);
            return bitmapSource;
        }

        private delegate void OnFrameChangedDelegate();
        #endregion

        #region Public Methods
        /// <summary>
        /// routed event handler for click
        /// </summary>
        public event RoutedEventHandler Click;

        /// <summary>
        /// GDI API call to delete (visual) object 
        /// </summary>
        /// <param name="hDc"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern IntPtr DeleteObject(IntPtr hDc);
        #endregion
        
    }
}