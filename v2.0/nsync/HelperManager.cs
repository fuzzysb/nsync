using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace nsync
{
    /// <summary>
    /// Helper Window Manager Class
    /// </summary>
    public class HelperManager
    {
        #region Class Variables
        private HelperWindow windowHelper;
        private Settings settingsManager;
        private int timer;
        private int errorCount;
        private int conflictCount;
        private string logPath;
        private bool isRevertPathDialog = false;
        private Window mainWindow;
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructor for HelperManager
        /// </summary>
        /// <param name="ownerWindow">Setting the owner of windowHelper to ownerWindow</param>
        public HelperManager(Window ownerWindow)
        {
            settingsManager = Settings.Instance;

            windowHelper = new HelperWindow();
            windowHelper.Owner = ownerWindow;
            mainWindow = ownerWindow;
            windowHelper.Show();
            windowHelper.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Tell windowHelper to display the notification
        /// </summary>
        /// <param name="helpString">The string to be displayed in the notifiation window</param>
        /// <param name="priority">The piority of the window to be displayed</param>
        /// <param name="windowPosition">The position for which the notification window should be placed</param>
        public void Show(string helpString, int priority, HelperWindow.windowStartPosition windowPosition)
        {
            if (priority == -2)
            {
                windowHelper.MainWindow = mainWindow;
                string extraHelpString = "Click here to quit nsync.";
                windowHelper.SetSettings(helpString, -1, windowPosition, "QUIT_NSYNC", extraHelpString);
                windowHelper.ButtonClose.Visibility = Visibility.Hidden;

                if (windowHelper.Visibility != Visibility.Visible && windowHelper.IsLoaded)
                {
                    windowHelper.Visibility = Visibility.Visible;
                    windowHelper.FormFade.Begin();
                }
            }

            else
            {
                if (helperWindowIsOn() || (priority == -1) || (priority == 0) || (priority != -2))
                {
                    if (priority == -1)
                    {
                        if ((errorCount == 0) && (conflictCount == 0))
                            windowHelper.SetSettings(helpString, determineTimer(priority), windowPosition, null, null);
                        else
                        {
                            if (errorCount == 1)
                                helpString += " " + errorCount + " file not synchronized.";
                            else if (errorCount > 1)
                                helpString += " " + errorCount + " files not synchronized.";
                            if (conflictCount == 0)
                                helpString += " " + conflictCount + " file conflicted.";
                            else if (conflictCount > 1)
                                helpString += " " + conflictCount + " files conflicted.";
                            string extraHelpString = "Click here to view Log.";
                            windowHelper.SetSettings(helpString, determineTimer(priority), windowPosition, logPath, extraHelpString);
                        }
                    }

                    else
                    {
                        if (isRevertPathDialog)
                        {
                            string extraHelpString = "Click here to revert changes.";
                            windowHelper.HyperTextMouseDown += new System.Windows.Input.MouseButtonEventHandler(windowHelper_HyperTextMouseDown);
                            windowHelper.SetSettings(helpString, determineTimer(priority), windowPosition, null, extraHelpString);
                        }
                        else
                            windowHelper.SetSettings(helpString, determineTimer(priority), windowPosition, null, null);
                    }

                    if (windowHelper.Visibility != Visibility.Visible && windowHelper.IsLoaded)
                    {
                        windowHelper.Visibility = Visibility.Visible;
                        windowHelper.FormFade.Begin();
                    }
                }
            }
        }

        /// <summary>
        /// event when the mouse is down on the hypertext portion of the helper window
        /// </summary>
        public event MouseButtonEventHandler HyperTextMouseDown;

        /// <summary>
        /// event handler for the mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void windowHelper_HyperTextMouseDown(object sender, MouseButtonEventArgs e)
        {
            HyperTextMouseDown(sender,e);
        }

        /// <summary>
        /// Closes the notification window
        /// </summary>
        public void CloseWindow()
        {
            windowHelper.Close();
        }

        /// <summary>
        /// Moves the notification window accordingly when its position is changed
        /// </summary>
        public void UpdateMove()
        {
            windowHelper.MoveWindow();
        }

        /// <summary>
        /// Hides the notification window
        /// </summary>
        public void HideWindow()
        {
            windowHelper.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Setter and Getter method for errorCount
        /// </summary>
        public int ErrorCount
        {
            get { return errorCount; }
            set { errorCount = value; }
        }

        /// <summary>
        /// Setter and Getter method for conflictCount
        /// </summary>
        public int ConflictCount
        {
            get { return conflictCount; }
            set { conflictCount = value; }
        }

        /// <summary>
        /// Setter and Getter method for getting log path
        /// </summary>
        public string LogPath
        {
            get { return logPath; }
            set { logPath = value; }
        }

        /// <summary>
        /// Setter and Getter method for getting log path
        /// </summary>
        public bool IsRevertPathDialog
        {
            get { return isRevertPathDialog; }
            set { isRevertPathDialog = value; }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Determines the duration for displaying the helper message
        /// </summary>
        /// <returns>The result is returned as an int</returns>
        private int determineTimer(int priority)
        {
            if ((priority == 0) || (priority == -1))
            {
                if ((timer < 5) && (timer > -1))
                {
                    return 5;
                }
                return timer;
            }
            else
                return timer;
        }

        /// <summary>
        /// Checks if the notification window should be on/off
        /// </summary>
        /// <returns>The result is returned as a boolean</returns>
        private bool helperWindowIsOn()
        {
            timer = settingsManager.GetHelperWindowStatus();
            if (timer == 0)
                return false;
            return true;
        }
        #endregion
    }
}
