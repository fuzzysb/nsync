using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            windowHelper.Show();
            windowHelper.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Tell windowHelper to display the notification
        /// </summary>
        /// <param name="helpString">The string to be displayed in the notifiation window</param>
        /// <param name="piority">The piority of the window to be displayed</param>
        /// <param name="windowPosition">The position for which the notification window should be placed</param>
        public void Show(string helpString, int priority, HelperWindow.windowStartPosition windowPosition)
        {
            if ((helperWindowIsOn()) || ((!helperWindowIsOn()) && (priority == 0)))
            {
                windowHelper.SetSettings(helpString, determineTimer(priority), windowPosition);
                if (windowHelper.Visibility != Visibility.Visible && windowHelper.IsLoaded)
                {
                    windowHelper.Visibility = Visibility.Visible;
                    windowHelper.FormFade.Begin();
                }
            }
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
        #endregion

        #region Private Methods
        /// <summary>
        /// Determines the duration for displaying the helper message
        /// </summary>
        /// <returns>The result is returned as an int</returns>
        private int determineTimer(int piority)
        {
            if (piority == 0)
            {
                if (timer < 5)
                    return 5;
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
