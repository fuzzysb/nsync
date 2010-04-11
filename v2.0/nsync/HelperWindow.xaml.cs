using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace nsync
{
    /// <summary>
    /// Interaction logic for HelperWindow.xaml
    /// </summary>
    public partial class HelperWindow : Window
    {
        #region Class Variables
        private int delayTime=10;
        private DispatcherTimer dispatcherTimer;
        private windowStartPosition windowPostionType;
        private bool windowActive;
        private string hyperLinkPath;
        #endregion

        #region Enumeration
        /// <summary>
        /// A list of enumeration of the available positions for notification window
        /// </summary>
        public enum windowStartPosition
        {
            /// <summary>
            /// Position window top left
            /// </summary>
            topLeft, 
            /// <summary>
            /// Position window top right
            /// </summary>
            topRight,
            /// <summary>
            /// Position window bottom left
            /// </summary>
            bottomLeft, 
            /// <summary>
            /// Position window bottom right
            /// </summary>
            bottomRight, 
            /// <summary>
            /// Position window centre
            /// </summary>
            center, 
            /// <summary>
            /// Position window top
            /// </summary>
            windowTop
        };
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructor for HelperWindow class
        /// </summary>
        public HelperWindow()
        {
            InitializeComponent();
            SetTime();
        }   

        /// <summary>
        /// Set the settings for notification window
        /// </summary>
        /// <param name="helpText">This is the string to be displayed</param>
        /// <param name="helpDuration">Duration for which the notification window should be active</param>
        /// <param name="windowPosition">The position where notification window should be displayed</param>
        /// <param name="hyperLinkPath"></param>
        /// <param name="hyperLinkText"></param>
        public void SetSettings(string helpText, int helpDuration, windowStartPosition windowPosition, string hyperLinkPath, string hyperLinkText)
        {
            this.hyperLinkPath = hyperLinkPath;
            windowActive = true;
            ContentText.Text = helpText;
            delayTime = helpDuration;
            windowPostionType = windowPosition;
            dispatcherTimer.Start();

            if (hyperLinkText == null)
                HyperLinkText.Visibility = Visibility.Collapsed;
            else
            {
                HyperLinkText.Visibility = Visibility.Visible;
                HyperLinkText.Text = hyperLinkText;
            }

            switch (windowPosition)
            {
                case windowStartPosition.topLeft:
                    this.Left = 10;
                    this.Top = 10;
                    break;
                case windowStartPosition.topRight:
                    this.Left = SystemParameters.PrimaryScreenWidth - (double)GetValue(WidthProperty) - 10;
                    this.Top = 10;
                    break;
                case windowStartPosition.bottomLeft:
                    this.Left = 10;
                    this.Top = SystemParameters.PrimaryScreenHeight - (double)GetValue(HeightProperty) - 50;
                    break;
                case windowStartPosition.bottomRight:
                    this.Left = SystemParameters.PrimaryScreenWidth - (double)GetValue(WidthProperty) - 10;
                    this.Top = SystemParameters.PrimaryScreenHeight - (double)GetValue(HeightProperty) - 50;
                    break;
                case windowStartPosition.center:
                    this.Left = (SystemParameters.PrimaryScreenWidth / 2) - ((double)GetValue(WidthProperty) / 2);
                    this.Top = (SystemParameters.PrimaryScreenHeight / 2) - ((double)GetValue(HeightProperty) / 2);
                    break;
                case windowStartPosition.windowTop:
                    MoveWindow();
                    break;
            }

            if (this.HyperLinkText!=null)
                HyperLinkText.Cursor = Cursors.Hand;
            else
                HyperLinkText.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Get the state of the notification window
        /// </summary>
        public bool WindowActiveState
        {
            get { return windowActive; }
            set { windowActive = value; }
        }

        /// <summary>
        /// Move the notification window accordingly when its position is changed
        /// </summary>
        public void MoveWindow()
        {
            if (windowPostionType == windowStartPosition.windowTop)
            {
                double leftPos = Owner.Left + ((double)GetValue(WidthProperty) / 4);
                double rightPos = leftPos + (double)GetValue(WidthProperty);
                double rightMostLeftPos = SystemParameters.PrimaryScreenWidth - (double)GetValue(WidthProperty);
                double topPos = Owner.Top - (double)GetValue(HeightProperty) - 10;
                double bottomPos = Owner.Top + Owner.Height + 10;

                if (rightPos > SystemParameters.PrimaryScreenWidth)
                    this.Left = rightMostLeftPos;
                else if (leftPos > 0)
                    this.Left = leftPos;
                else
                    this.Left = 1;

                if (topPos > 0)
                    this.Top = topPos;
                else
                    this.Top = bottomPos;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Close the notification window
        /// </summary>
        private void CloseWindow()
        {
            if (windowActive == true)
            {
                windowActive = false;
                FormFadeOut.Begin();
            }
        }

        /// <summary>
        /// Setup the settings for timer of notification window
        /// </summary>
        private void SetTime()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        /// <summary>
        /// This method is called to start the timer and will close the notification window when time is up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Application.Current.MainWindow.Focus();
            if (delayTime!=0)
            {
                delayTime--;
            }
            else
            {
                CloseWindow();
            }
        }

        /// <summary>
        /// This method is called when a right mouse click is done on the notification window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void windowHelper_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseWindow();
        }

        /// <summary>
        /// This method is called when the notification window has finished fading out
        /// <para>Notifcation window will be hidden after this</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormFadeOut_Completed(object sender, EventArgs e)
        {
            if (!windowActive)
            {
                this.Visibility = Visibility.Hidden;
                dispatcherTimer.Stop();
            }
        }

        /// <summary>
        /// This method is called when the close button on the notification window is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        /// <summary>
        /// event when the mouse is down on the hypertext portion of the helper window
        /// </summary>
        public event MouseButtonEventHandler HyperTextMouseDown;

        /// <summary>
        /// This method is called when the the helperWindow textBlock is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HyperLinkText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.hyperLinkPath != null)
                System.Diagnostics.Process.Start(@hyperLinkPath);
            else if (this.HyperLinkText != null)
            {
                HyperTextMouseDown(sender, e);
                CloseWindow();
            }
        }
        #endregion

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            dispatcherTimer.Stop();
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            dispatcherTimer.Start();
        }
    }
}
