using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace nsync
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private Settings settingsManager;

        /// <summary>
        /// Constructor for SettingsPage
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            CheckSettings();
        }

        /// <summary>
        /// Update the checkbox on SettingsPage
        /// </summary>
        private void CheckSettings()
        {
            settingsManager = Settings.Instance;
            if (!settingsManager.GetHelperWindowStatus())
            {
                CheckboxToggleHelperWindow.IsChecked = true;
            }
        }

        /// <summary>
        /// This method is called when user checks the checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckboxToggleHelperWindow_Checked(object sender, RoutedEventArgs e)
        {
            settingsManager = Settings.Instance;
            settingsManager.SetHelperWindowStatus(false);
        }

        /// <summary>
        /// This method is called when user unchecks the checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckboxToggleHelperWindow_UnChecked(object sender, RoutedEventArgs e)
        {
            settingsManager = Settings.Instance;
            settingsManager.SetHelperWindowStatus(true);
        }
    }
}
