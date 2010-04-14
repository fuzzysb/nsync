// Jiewei

using System;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Xml;
using System.ComponentModel;
using System.Windows.Data;
using System.Runtime.InteropServices;
using System.IO;


namespace nsync
{
    /// <summary>
    /// Interaction logic for TrackBackPage.xaml
    /// </summary>
    public partial class TrackBackPage : Page
    {
        #region Class Variables
        private ObservableCollection<TrackBackItemData> trackBackCollectionForLeftFolder = new ObservableCollection<TrackBackItemData>();
        private ObservableCollection<TrackBackItemData> trackBackCollectionForRightFolder = new ObservableCollection<TrackBackItemData>();
        private string actualLeftFolderPath, actualRightFolderPath;
        private string leftFolderPath, rightFolderPath;
        private string[] shortenedFolderPaths;
        private TrackBackEngine trackback;
        private HelperManager helper;
        private Window mainWindow = Application.Current.MainWindow;
        private Settings settingsManager;
        private DebugLogger debugLogger;
        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private string[] folderList;
        private string[] destinationList;
        private string[] timeStampList;

        private readonly string SETTINGS_FILE_NAME = Environment.GetEnvironmentVariable("APPDATA") + Properties.Resources.settingsFilePath;
        private readonly string PATH_MRU_LEFT_FOLDER = Properties.Resources.leftFolderMRUPath;
        private readonly string PATH_MRU_RIGHT_FOLDER = Properties.Resources.rightFolderMRUPath;
        private readonly string MESSAGE_RESTORING_FOLDERS = Properties.Resources.messageRestoringFolders;
        private readonly string MESSAGE_RESTORE_COMPLETED = Properties.Resources.messageRestoreCompleted;
        private readonly string MESSAGE_ERROR_DETECTED = Properties.Resources.messageErrorDetected;
        private readonly string MESSAGE_NO_LISTVIEW = Properties.Resources.messageNoListView;
        private readonly int HELPER_WINDOW_HIGH_PRIORITY = 0;
        private readonly int HELPER_WINDOW_LOW_PRIORITY = 1;
        #endregion

        #region Constructor
        /// <summary>
        /// TrackBackPage Contructor
        /// </summary>
        public TrackBackPage()
        {
            InitializeComponent();

            settingsManager = Settings.Instance;

            // Get the debugLogger class instance
            debugLogger = DebugLogger.Instance;

            helper = new HelperManager(mainWindow);

            // Disables the TrackBackPage interface if no folder pairs are loaded in HomePage
            if (!File.Exists(SETTINGS_FILE_NAME) || settingsManager.LoadFolderPaths()[0] == "")
            {
                HideListViewForBothFolders(false);
                return;
            }
            else
            {
                LoadTrackBackXML();

                trackback = new TrackBackEngine();
                trackback.LeftFolderPath = actualLeftFolderPath;
                trackback.RightFolderPath = actualRightFolderPath;

                // Displays the message 'No TrackBack Data' if the folder does not have any previously backed up folders
                if (trackback.hasTrackBackData(actualLeftFolderPath) && trackback.hasTrackBackData(actualRightFolderPath))
                {
                    GridTrackBack.IsEnabled = true;
                }
                else if (trackback.hasTrackBackData(actualLeftFolderPath))
                {
                    DisplayListViewForLeftFolderOnly(true);
                }
                else if (trackback.hasTrackBackData(actualRightFolderPath))
                {
                    DisplayListViewForRightFolderOnly(true);
                }
                else
                {
                    HideListViewForBothFolders(false);
                }

                // Adds the event handler for Restore
                trackback.backgroundWorkerForTrackBackRestore.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorkerForTrackBackRestore_RunWorkerCompleted);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Property of the trackback collection (for left folder) used in binding
        /// </summary>
        public ObservableCollection<TrackBackItemData> TrackBackCollectionForLeftFolder
        { 
            get { return trackBackCollectionForLeftFolder; } 
        }

        /// <summary>
        /// Property of the trackback collection (for right folder) used in binding
        /// </summary>
        public ObservableCollection<TrackBackItemData> TrackBackCollectionForRightFolder
        {
            get { return trackBackCollectionForRightFolder; }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads the TrackBack XML document
        /// </summary>
        private void LoadTrackBackXML()
        {
            XmlDocument document = new XmlDocument();
            document.Load(SETTINGS_FILE_NAME);

            shortenedFolderPaths = new string[2];

            actualLeftFolderPath = document.SelectSingleNode(PATH_MRU_LEFT_FOLDER).InnerText;
            shortenedFolderPaths[0] = leftFolderPath = ShortenPath(actualLeftFolderPath, 65);

            actualRightFolderPath = document.SelectSingleNode(PATH_MRU_RIGHT_FOLDER).InnerText;
            shortenedFolderPaths[1] = rightFolderPath = ShortenPath(actualRightFolderPath, 65);
        }

        /// <summary>
        /// Adds an entry into the trackback list view
        /// </summary>
        /// <param name="trackBackName">Name of the folder</param>
        /// <param name="trackBackDate">Date and time of the folder</param>
        /// <param name="trackBackFolder">Destination folder it was synced with</param>
        private void AddTrackBackEntryForLeftFolder(string trackBackName, string trackBackDate, string trackBackFolder)
        {
            TrackBackItemData data = new TrackBackItemData
                {
                    nameItem = trackBackName,
                    dateItem = trackBackDate,
                    folderItem = trackBackFolder
                };
            if (trackBackName != null && trackBackFolder != null && trackBackDate != null)
                trackBackCollectionForLeftFolder.Add(data); 
        }

        /// <summary>
        /// Adds an entry into the trackback list view
        /// </summary>
        /// <param name="trackBackName">Name of the folder</param>
        /// <param name="trackBackDate">Date and time of the folder</param>
        /// <param name="trackBackFolder">Destination folder it was synced with</param>
        private void AddTrackBackEntryForRightFolder(string trackBackName, string trackBackDate, string trackBackFolder)
        {
            TrackBackItemData data = new TrackBackItemData
            {
                nameItem = trackBackName,
                dateItem = trackBackDate,
                folderItem = trackBackFolder
            };
            if (trackBackName != null && trackBackFolder != null && trackBackDate != null)
                trackBackCollectionForRightFolder.Add(data);
        }

        /// <summary>
        /// Page is loaded, initialise listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            int trackbackStatus = settingsManager.GetTrackBackStatus(); // 0 for disabled, 1 for enabled, -1 is an error
            
            if (trackbackStatus != -1)
            {
                if (trackbackStatus == 0)
                {
                    GridTrackBack.IsEnabled = false;
                    GridTrackBack.Opacity = 0.5;
                    LabelDisabled.Visibility = Visibility.Visible;
                }

                LabelProgress.Visibility = Visibility.Hidden;
                ButtonRestore.Visibility = Visibility.Hidden;

                if ((File.Exists(SETTINGS_FILE_NAME) && settingsManager.LoadFolderPaths()[0] != "") &&
                    trackback.hasTrackBackData(actualLeftFolderPath) && trackback.hasTrackBackData(actualRightFolderPath))
                {
                    LoadSourceFolders();

                    if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualLeftFolderPath)
                        LoadTrackBackEntriesForLeftFolder();
                    else if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualRightFolderPath)
                        LoadTrackBackEntriesForRightFolder();

                    //Sort left and right lists according to date/time
                    SortList("dateItem", ListSortDirection.Descending, ListViewForLeftFolder);
                    SortList("dateItem", ListSortDirection.Descending, ListViewForRightFolder);
                }
                else if ((File.Exists(SETTINGS_FILE_NAME) && settingsManager.LoadFolderPaths()[0] != "") &&
                    trackback.hasTrackBackData(actualLeftFolderPath))
                {
                    LoadSourceFolders();

                    if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualLeftFolderPath)
                        LoadTrackBackEntriesForLeftFolder();

                    //Sort left lists according to date/time
                    SortList("dateItem", ListSortDirection.Descending, ListViewForLeftFolder);
                }
                else if ((File.Exists(SETTINGS_FILE_NAME) && settingsManager.LoadFolderPaths()[0] != "") &&
                    trackback.hasTrackBackData(actualRightFolderPath))
                {
                    LoadSourceFolders();

                    if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualRightFolderPath)
                        LoadTrackBackEntriesForRightFolder();

                    //Sort right lists according to date/time
                    SortList("dateItem", ListSortDirection.Descending, ListViewForRightFolder);
                }

                //Add event handler to check when main window is moved, move helper window too
                mainWindow.LocationChanged += new EventHandler(mainWindow_LocationChanged);

            }
        }

        /// <summary>
        /// This method will be called when the position of mainWindow is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_LocationChanged(object sender, EventArgs e)
        {
            helper.UpdateMove();
        }

        /// <summary>
        /// Event called on clicking on the header of a column
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
              e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Tag as string;

                    if (ListViewForLeftFolder.Visibility == Visibility.Visible)
                        SortList(header, direction, ListViewForLeftFolder);
                    else if (ListViewForRightFolder.Visibility == Visibility.Visible)
                        SortList(header, direction, ListViewForRightFolder);
                    else
                        throw new Exception(MESSAGE_NO_LISTVIEW);

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        /// <summary>
        /// Sorting method to sort a listview
        /// </summary>
        /// <param name="sortBy">Data name/parameter to sort by as a string</param>
        /// <param name="direction">Ascending or descending order</param>
        /// <param name="listView">List View to be sorted</param>
        private void SortList(string sortBy, ListSortDirection direction, ListView listView)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(listView.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription description = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(description);
            dataView.Refresh();
        }

        /// <summary>
        /// Loads the folder names into the combo box
        /// </summary>
        private void LoadSourceFolders()
        {
            AddComboBoxItem(leftFolderPath);
            AddComboBoxItem(rightFolderPath);
            ComboBoxSourceFolder.SelectedIndex = 0;
        }

        /// <summary>
        /// Adds an item to the combo box
        /// </summary>
        /// <param name="itemName">The name of the item to be added</param>
        private void AddComboBoxItem(string itemName)
        {
            ComboBoxItem SourceFolderComboBoxItem = new ComboBoxItem();
            SourceFolderComboBoxItem.Content = itemName;
            SourceFolderComboBoxItem.Style = (Style)FindResource("ComboBoxDarkItem");
            ComboBoxSourceFolder.Items.Add(SourceFolderComboBoxItem);
        }

        /// <summary>
        /// Loads the trackback entries for the left folder into the listview
        /// </summary>
        private void LoadTrackBackEntriesForLeftFolder()
        {
            // Refresh the list view
            trackBackCollectionForLeftFolder.Clear();

            ListViewForLeftFolder.Visibility = Visibility.Visible;
            ListViewForRightFolder.Visibility = Visibility.Collapsed;

            if (trackback.hasTrackBackData(actualLeftFolderPath))
            {
                LoadDataFromTrackBackData(actualLeftFolderPath);

                for (int i = 0; i < folderList.Length; i++)
                    AddTrackBackEntryForLeftFolder(folderList[i], timeStampList[i], destinationList[i]);
            }
        }

        /// <summary>
        /// Loads the trackback entries for the right folder into the listview
        /// </summary>
        private void LoadTrackBackEntriesForRightFolder()
        {
            // Refresh the list view
            trackBackCollectionForRightFolder.Clear();

            ListViewForLeftFolder.Visibility = Visibility.Collapsed;
            ListViewForRightFolder.Visibility = Visibility.Visible;

            if (trackback.hasTrackBackData(actualRightFolderPath))
            {
                LoadDataFromTrackBackData(actualRightFolderPath);

                for (int j = 0; j < folderList.Length; j++)
                    AddTrackBackEntryForRightFolder(folderList[j], timeStampList[j], destinationList[j]);
            }
        }

        /// <summary>
        /// Loads the data from TrackBack XML document into the arrays for storing
        /// </summary>
        /// <param name="folderPath"></param>
        private void LoadDataFromTrackBackData(string folderPath)
        {
            folderList = trackback.GetFolderVersions(folderPath);
            destinationList = trackback.GetFolderDestinations(folderPath);
            timeStampList = trackback.GetFolderTimeStamps(folderPath);
        }

        /// <summary>
        /// Handles the event when the combo box selected item is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxSourceFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (actualLeftFolderPath == actualRightFolderPath)
            {
                ComboBoxSourceFolder.IsEnabled = false;
            }
            else if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualLeftFolderPath)
            {
                ComboBoxSourceFolder.IsEnabled = true;
                ComboBoxSourceFolder.ToolTip = actualLeftFolderPath;
                LoadTrackBackEntriesForLeftFolder();
                if (ListViewForLeftFolder.Items.Count == 0)
                {
                    ListViewForLeftFolder.Visibility = ListViewForRightFolder.Visibility = Visibility.Collapsed;
                    LabelNoChanges.Visibility = Visibility.Visible;
                }
                else
                    LabelNoChanges.Visibility = Visibility.Hidden;
            }
            else if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualRightFolderPath)
            {
                ComboBoxSourceFolder.IsEnabled = true;
                ComboBoxSourceFolder.ToolTip = actualRightFolderPath;
                LoadTrackBackEntriesForRightFolder();
                if (ListViewForRightFolder.Items.Count == 0)
                {
                    ListViewForLeftFolder.Visibility = ListViewForRightFolder.Visibility = Visibility.Collapsed;
                    LabelNoChanges.Visibility = Visibility.Visible;
                }
                else
                    LabelNoChanges.Visibility = Visibility.Hidden;
            }

            LabelProgress.Visibility = Visibility.Hidden;
            ButtonRestore.Visibility = Visibility.Hidden;

            //Add event handler to check when main window is moved, move helper window too
            mainWindow.LocationChanged += new EventHandler(mainWindow_LocationChanged);
        }
        /// <summary>
        /// This method is called when user clicks on the Restore button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - ButtonRestore_Click()", "Trying to restore folders");
            try
            {
                if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualLeftFolderPath && !trackback.hasEnoughDiskSpaceInLeftFolder())
                {
                    DisplayErrorInterface();
                    helper.Show(Properties.Resources.leftFolderInsufficientDiskSpace, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - ButtonRestore_Click()", nsync.Properties.Resources.leftFolderInsufficientDiskSpace);
                }
                else if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualRightFolderPath && !trackback.hasEnoughDiskSpaceInRightFolder())
                {
                    DisplayErrorInterface();
                    helper.Show(Properties.Resources.rightFolderInsufficientDiskSpace, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - ButtonRestore_Click()", nsync.Properties.Resources.rightFolderInsufficientDiskSpace);
                }
                else
                {
                    EnableInterface(false);
                    LabelProgress.Visibility = Visibility.Visible;
                    LabelProgress.Content = MESSAGE_RESTORING_FOLDERS;

                    if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualLeftFolderPath)
                    {
                        trackback.StartRestore(actualLeftFolderPath, GetSelectedListViewItem(ListViewForLeftFolder));
                        debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - ButtonRestore_Click()", "Restoring left folder in progress");
                    }
                    else if (GetOriginalFolderPath(GetSelectedComboBoxItem()) == actualRightFolderPath)
                    {
                        trackback.StartRestore(actualRightFolderPath, GetSelectedListViewItem(ListViewForRightFolder));
                        debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - ButtonRestore_Click()", "Restoring right folder in progress");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                DisplayErrorInterface();
                helper.Show(Properties.Resources.accessRightsInsufficient, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - ButtonRestore_Click()", nsync.Properties.Resources.accessRightsInsufficient);
                return;
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                DisplayErrorInterface();
                helper.Show(fileNotFoundException.Message, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - ButtonRestore_Click()", fileNotFoundException.Message);
                return;
            }
        }

        /// <summary>
        /// Gets the value of the selected item in combo box
        /// </summary>
        /// <returns>The string representation of the selected item</returns>
        private string GetSelectedComboBoxItem()
        {
            ComboBoxItem selectedComboBoxItem = new ComboBoxItem();
            selectedComboBoxItem = (ComboBoxItem)ComboBoxSourceFolder.SelectedItem;
            return selectedComboBoxItem.Content.ToString();
        }

        /// <summary>
        /// Gets the value of the selected item in list view
        /// </summary>
        /// <param name="listView">The list view selected</param>
        /// <returns>The string representation of the selected item</returns>
        private string GetSelectedListViewItem(ListView listView)
        {
            TrackBackItemData selectedListViewItem = (TrackBackItemData)listView.SelectedItem;
            return selectedListViewItem.dateItem;
        }

        /// <summary>
        /// Gets the orignial path of the folder
        /// </summary>
        /// <param name="shortenedFolderPath">The shortened folder path</param>
        /// <returns>The original folder path</returns>
        private string GetOriginalFolderPath(string shortenedFolderPath)
        {
            if (shortenedFolderPaths[0] == shortenedFolderPath)
                return actualLeftFolderPath;
            else if (shortenedFolderPaths[1] == shortenedFolderPath)
                return actualRightFolderPath;
            return "";
        }

        /// <summary>
        /// Enables or disables the interface after and during restoring of folders
        /// </summary>
        /// <param name="status"></param>
        private void EnableInterface(bool status)
        {
            double opacityValue;
            bool enableButtons;

            if (status)
            {
                opacityValue = 1;
                enableButtons = true;
                ButtonRestore.IsEnabled = true;
                RestoringImage.Visibility = Visibility.Hidden;
                ButtonRestore.Visibility = Visibility.Visible;
            }
            else
            {
                enableButtons = false;
                opacityValue = 0.5;
                ButtonRestore.IsEnabled = false;
                RestoringImage.Visibility = Visibility.Visible;
                ButtonRestore.Visibility = Visibility.Hidden;
            }

            //Enable/Disable the interface
            Button ButtonClose = (Button)mainWindow.FindName("ButtonClose");
            ButtonClose.IsEnabled = enableButtons;
            ComboBoxSourceFolder.IsEnabled = enableButtons;
            BoxTrackBack.IsEnabled = enableButtons;


            //Enable/Disable the scroller
            Button ButtonSideTabLeft = (Button)mainWindow.FindName("ButtonSideTabLeft");
            ButtonSideTabLeft.IsEnabled = enableButtons;

            //Enable/Disable the dotmenu
            Button ButtonPageSettings = (Button)mainWindow.FindName("ButtonPageSettings");
            ButtonPageSettings.IsEnabled = enableButtons;
            Button ButtonPageHome = (Button)mainWindow.FindName("ButtonPageHome");
            Button ButtonPageTrackBack = (Button)mainWindow.FindName("ButtonPageTrackBack");
            ButtonPageHome.IsEnabled = enableButtons;

            //Set Opacity
            ButtonSideTabLeft.Opacity = opacityValue;
            ButtonPageSettings.Opacity = ButtonPageHome.Opacity = ButtonPageTrackBack.Opacity = opacityValue;
        }

        /// <summary>
        /// Displays the interface components when there is an error
        /// </summary>
        private void DisplayErrorInterface()
        {
            EnableInterface(true);
            LabelProgress.Visibility = Visibility.Visible;
            LabelProgress.Content = MESSAGE_ERROR_DETECTED;
        }

        /// <summary>
        /// Displays the list view for left folder only
        /// </summary>
        /// <param name="status">If true, enable the interface. Else, disable the interface.</param>
        private void DisplayListViewForLeftFolderOnly(bool status)
        {
            GridTrackBack.IsEnabled = status;
            ListViewForLeftFolder.Visibility = Visibility.Visible;
            ListViewForRightFolder.Visibility = Visibility.Collapsed;
            LabelNoChanges.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Displays the list view for right folder only
        /// </summary>
        /// <param name="status">If true, enable the interface. Else, disable the interface.</param>
        private void DisplayListViewForRightFolderOnly(bool status)
        {
            GridTrackBack.IsEnabled = status;
            ListViewForRightFolder.Visibility = Visibility.Visible;
            ListViewForLeftFolder.Visibility = Visibility.Collapsed;
            LabelNoChanges.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the list view for both folders
        /// </summary>
        /// <param name="status">If true, enable the interface. Else, disable the interface.</param>
        private void HideListViewForBothFolders(bool status)
        {
            GridTrackBack.IsEnabled = status;
            ListViewForRightFolder.Visibility = ListViewForLeftFolder.Visibility = Visibility.Collapsed;
            LabelNoChanges.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// This method is called when the background worker has finished restoring the folders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForTrackBackRestore_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((int) e.Result == 0)
            {
                EnableInterface(true);
                LabelProgress.Visibility = Visibility.Visible;
                LabelProgress.Content = MESSAGE_RESTORE_COMPLETED;
                helper.Show(Properties.Resources.restoreComplete, HELPER_WINDOW_LOW_PRIORITY, HelperWindow.windowStartPosition.windowTop);

                debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - backgroundWorkerForTrackBackRestore_RunWorkerCompleted()", nsync.Properties.Resources.restoreComplete);
            }
            else
            {
                DisplayErrorInterface();
                
                switch ((int) e.Result)
                {
                    case 1:
                        helper.Show(Properties.Resources.folderRestrictedAccess, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 2:
                        helper.Show(Properties.Resources.folderNotExists, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 3:
                        helper.Show(Properties.Resources.folderPathTooLong, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 4:
                        helper.Show(Properties.Resources.folderIOException, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    default:
                        helper.Show(Properties.Resources.defaultErrorMessage, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;                    
                }
                debugLogger.LogMessage(actualLeftFolderPath, actualRightFolderPath, "TrackBackPage.xaml.cs - backgroundWorkerForTrackBackRestore_RunWorkerCompleted()", "Error #" + e.Result.ToString());
            }
        }

        /// <summary>
        /// Use Win32 Api for shortening paths
        /// </summary>
        /// <param name="pszOut"></param>
        /// <param name="szPath"></param>
        /// <param name="cchMax"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        /// <summary>
        /// Shortens folder path for MRU list
        /// </summary>
        /// <param name="oldPath">The path that is to be shortened is passed in</param>
        /// <param name="maxLength">The maximum length that the path should be shortened</param>
        /// <returns>A string containing the new folder path is returned</returns>
        private string ShortenPath(string oldPath, int maxLength)
        {
            StringBuilder sb = new StringBuilder();
            PathCompactPathEx(sb, oldPath, maxLength, 0);
            return sb.ToString();
        }

        /// <summary>
        /// This method is called when the left folder list view selection is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewForLeftFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LabelProgress.Visibility = Visibility.Hidden;
            ButtonRestore.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// This method is called when the right folder list view selection is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewForRightFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LabelProgress.Visibility = Visibility.Hidden;
            ButtonRestore.Visibility = Visibility.Visible;
        }
        #endregion
    }

    /// <summary>
    /// Class of trackback item data, for binding
    /// </summary>
    public class TrackBackItemData
    {
        /// <summary>
        /// Property for left item column
        /// </summary>
        public string nameItem { get; set; }
        /// <summary>
        /// Property for action column
        /// </summary>
        public string dateItem { get; set; }
        /// <summary>
        /// Property for right item column
        /// </summary>
        public string folderItem { get; set; }
    }
}
