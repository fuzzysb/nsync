using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nsync
{
    public class HelperManager
    {
        private HelperWindow windowHelper;
        private Settings settingsManager;

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
        /// <param name="time">The duration for which the notification window should be active</param>
        /// <param name="windowPosition">The position for which the notification window should be placed</param>
        public void Show(string helpString, int time, HelperWindow.windowStartPosition windowPosition)
        {
            if (true == helperWindowIsOn())
            {
                windowHelper.SetSettings(helpString, time, windowPosition);
                if (windowHelper.Visibility != Visibility.Visible)
                {
                    windowHelper.Visibility = Visibility.Visible;
                    windowHelper.FormFade.Begin();
                }
            }
        }

        /// <summary>
        /// Checks if the notification window should be on/off
        /// </summary>
        /// <returns>The result is returned as a boolean</returns>
        private bool helperWindowIsOn()
        {
            return settingsManager.GetHelperWindowStatus();
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
    }
}
