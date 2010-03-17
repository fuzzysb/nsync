using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Management;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Xml;

namespace nsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private static int oldSelectedIndex = 0;
        
        /// <summary>
        /// Constructor for MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

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
        private void ButtonPageBackTrack_Click(object sender, RoutedEventArgs e)
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
            if (mousePos.X < 40)
            {
                if (viewList.SelectedIndex != 0)
                {
                    ButtonSideTabLeft.Visibility = Visibility.Visible;
                }
            }
            else if (mousePos.X > this.Width - 40)
            {
                if (viewList.SelectedIndex != viewList.Items.Count - 1)
                {
                    if(viewList.SelectedIndex != 1)
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

            ButtonPageBackTrack.IsEnabled = true;
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
                ButtonPageBackTrack.IsEnabled = false;
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
        /// This method is called when user click on the letter 'n' of nsync logo
        /// <para>The testing window will appear</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonTesting_Click(object sender, RoutedEventArgs e)
        {
            TestEngine testEngine = new TestEngine();
            testEngine.ShowDialog();
        }

    }
}