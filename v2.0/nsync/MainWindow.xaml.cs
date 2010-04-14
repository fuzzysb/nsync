// Eugene

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Xml;

namespace nsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Class Variables
        private static int oldSelectedIndex = 0;
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructor for MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Takes a snapshot of an object element
        /// </summary>
        /// <param name="element">This parameter is an object that will be snapshot</param>
        /// <returns>Returns a bitmap of the snapshot</returns>
        public RenderTargetBitmap RenderBitmap(FrameworkElement element)
        {
            double topLeft = 0;
            double topRight = 0;
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;
            double dpiX = 96; // this is the magic number
            double dpiY = 96; // this is the magic number

            PixelFormat pixelFormat = PixelFormats.Default;
            VisualBrush elementBrush = new VisualBrush(element);
            DrawingVisual visual = new DrawingVisual();
            DrawingContext dc = visual.RenderOpen();

            dc.DrawRectangle(elementBrush, null, new Rect(topLeft, topRight, width, height));
            dc.Close();

            RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, dpiX, dpiY, pixelFormat);

            bitmap.Render(visual);
            return bitmap;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method is called when user clicks on the titlebar of MainWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// This method is called when user clicks on the exit button on MainWindow
        /// <para>nsync will exit after this</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// This method is called when user clicks on the minimize button on MainWindow
        /// <para>nsync will minimize to taskbar after this</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonMinimise_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// This method is called when users click on the left dot on MainWindow
        /// <para>Current page will be switched to SettingsPage</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPageSettings_Click(object sender, RoutedEventArgs e)
        {
            viewList.SelectedIndex = 0;
        }

        /// <summary>
        /// This method is called when users click on the right dot on MainWindow
        /// <para>Current page will be switched to TrackBackPage</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPageTrackBack_Click(object sender, RoutedEventArgs e)
        {
            viewList.SelectedIndex = 2;
        }

        /// <summary>
        /// This method is called when mouse pointer is moved near the sides of MainWindow
        /// <para>Slider bars will be appear when mouse pointer is near it</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMain_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            if (mousePos.X < 40 && mousePos.Y > 30 && mousePos.Y < this.Height - 30)
            {
                if (viewList.SelectedIndex != 0)
                {
                    ButtonSideTabLeft.Visibility = Visibility.Visible;
                }
            }
            else if (mousePos.X > this.Width - 40 && mousePos.Y > 30 && mousePos.Y < this.Height - 30)
            {
                if (viewList.SelectedIndex != viewList.Items.Count - 1)
                {
                    ButtonSideTabRight.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ButtonSideTabLeft.Visibility = Visibility.Hidden;
                ButtonSideTabRight.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// This method is called when viewList.SelectedIndex is changed
        /// <para>The respective page will be loaded</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void viewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            XmlElement root = (XmlElement)viewer.DataContext;
            XmlNodeList xnl = root.SelectNodes("Page");

            if (viewer.ActualHeight > 0 && viewer.ActualWidth > 0)
            {
                RenderTargetBitmap rtb = RenderBitmap(viewer);
                rectanglevisual.Fill = new ImageBrush(BitmapFrame.Create(rtb));
            }

            viewer.ItemsSource = xnl;

            if (oldSelectedIndex < viewList.SelectedIndex)
            {
                viewer.BeginStoryboard((Storyboard)this.Resources["slideLeftToRight"]);
            }
            else
            {
                viewer.BeginStoryboard((Storyboard)this.Resources["slideRightToLeft"]);
            }

            oldSelectedIndex = viewList.SelectedIndex;

            ButtonPageTrackBack.IsEnabled = true;
            ButtonPageHome.IsEnabled = true;
            ButtonPageSettings.IsEnabled = true;

            if(viewList.SelectedIndex == 0) 
            {
                ButtonPageSettings.IsEnabled = false;
            }
            else if (viewList.SelectedIndex == 1)
            {
                ButtonPageHome.IsEnabled = false;
            }
            else
            {
                ButtonPageTrackBack.IsEnabled = false;
            }

            //Change slider tooltips
            UpdateToolTips();
        }

        /// <summary>
        /// Updates the tooltips of the slider bars
        /// </summary>
        private void UpdateToolTips()
        {
            int leftIndex = viewList.SelectedIndex - 1;
            int rightIndex = viewList.SelectedIndex + 1;

            if (leftIndex == 0)
            {
                ButtonSideTabLeft.ToolTip = nsync.Properties.Resources.settingsToolTip;
            }
            else if (leftIndex == 1)
            {
                ButtonSideTabLeft.ToolTip = nsync.Properties.Resources.homeToolTip;
            }

            if (rightIndex == 1)
            {
                ButtonSideTabRight.ToolTip = nsync.Properties.Resources.homeToolTip;
            }
            else if (rightIndex == 2)
            {
                ButtonSideTabRight.ToolTip = nsync.Properties.Resources.trackBackToolTip;
            }
            
        }

        /// <summary>
        /// This method is called when users click on the center dot on MainWindow
        /// <para>Current page will be switched to HomePage</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPageHome_Click(object sender, RoutedEventArgs e)
        {
            viewList.SelectedIndex = 1;
        }

        /// <summary>
        /// This method is called when users click on the left sliderbar
        /// <para>Current page will be switched to the page on the left</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSideTabLeft_Click(object sender, RoutedEventArgs e)
        {
            if (viewList.SelectedIndex > 0)
            {
                viewList.SelectedIndex--;
            }
            if (viewList.SelectedIndex == 0)
            {
                ButtonSideTabLeft.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// This method is called when users click on the left sliderbar
        /// <para>Current page will be switched to the page on the left</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSideTabRight_Click(object sender, RoutedEventArgs e)
        {
            if (viewList.SelectedIndex < viewList.Items.Count)
            {
                viewList.SelectedIndex++;
            }
            if (viewList.SelectedIndex == viewList.Items.Count -1 || viewList.SelectedIndex==1)
            {
                ButtonSideTabRight.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// event when mouse enters settings circle, show the tooltip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPageSettings_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowPageToolTip(Properties.Resources.settingsToolTip);
        }

        /// <summary>
        /// method to display a tooltip for the navigation circles
        /// </summary>
        /// <param name="toolTip"></param>
        private void ShowPageToolTip(string toolTip)
        {
            PageToolTip.Visibility = Visibility.Visible;
            PageToolTipText.Text = toolTip;
        }

        /// <summary>
        /// event when mouse leaves any of the circles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPage_MouseLeave(object sender, MouseEventArgs e)
        {
            PageToolTip.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// event when mouse enters home circle, show the tooltip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPageHome_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowPageToolTip(Properties.Resources.homeToolTip);
        }

        /// <summary>
        /// event when mouse enters trackback circle, show the tooltip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPageTrackBack_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowPageToolTip(Properties.Resources.trackBackToolTip);
        }
        #endregion

      


    }
}