using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Xml;
using System.ComponentModel;
using System.Windows.Data;


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
        private string leftFolderPath, rightFolderPath;
        private TrackBackEngine trackback;
        private HelperManager helper;
        private Window mainWindow = Application.Current.MainWindow;

        private readonly string SETTINGS_FILE_NAME = "settings.xml";
        private readonly string PATH_MRU_LEFT_FOLDER = "/nsync/MRU/left1";
        private readonly string PATH_MRU_RIGHT_FOLDER = "/nsync/MRU/right1";
        private readonly string MESSAGE_RESTORING_FOLDERS = "Restoring folders...";
        private readonly string MESSAGE_RESTORE_COMPLETED = "Restore completed";
        private readonly string MESSAGE_ERROR_DETECTED = "Error detected";
        private readonly int HELPER_WINDOW_HIGH_PRIORITY = 0;
        private readonly int HELPER_WINDOW_LOW_PRIORITY = 1;
        #endregion

        /// <summary>
        /// TrackBackPage Contructor
        /// </summary>
        public TrackBackPage()
        {
            InitializeComponent();

            LoadTrackBackXML();

            helper = new HelperManager(mainWindow);

            trackback = new TrackBackEngine();
            trackback.LeftFolderPath = leftFolderPath;
            trackback.RightFolderPath = rightFolderPath;

            trackback.backgroundWorkerForTrackBackRestore.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(backgroundWorkerForTrackBackRestore_RunWorkerCompleted);
        }

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

        private void LoadTrackBackXML()
        {
            XmlDocument document = new XmlDocument();
            document.Load(SETTINGS_FILE_NAME);

            leftFolderPath = document.SelectSingleNode(PATH_MRU_LEFT_FOLDER).InnerText;
            rightFolderPath = document.SelectSingleNode(PATH_MRU_RIGHT_FOLDER).InnerText;
        }

        /// <summary>
        /// Adds an entry into the trackback list view
        /// </summary>
        /// <param name="trackBackName"></param>
        /// <param name="trackBackDate"></param>
        /// <param name="trackBackFolder"></param>
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
            LoadSourceFolders();

            if (GetSelectedComboBoxItem() == leftFolderPath)
                LoadTrackBackEntriesForLeftFolder();
            else
                LoadTrackBackEntriesForRightFolder();
            
            //Sort left and right lists according to date/time
            SortList("dateItem", ListSortDirection.Descending, ListViewForLeftFolder);
            SortList("dateItem", ListSortDirection.Descending, ListViewForRightFolder);
        }

        /// <summary>
        /// Sorting method to sort a listview
        /// </summary>
        /// <param name="sortBy">data name/parameter to sort by as a string</param>
        /// <param name="direction">ascending or descending order</param>
        /// <param name="lv">listview to be sorted</param>
        private void SortList(string sortBy, ListSortDirection direction, ListView lv)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(lv.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }


        private void LoadSourceFolders()
        {
            AddComboBoxItem(leftFolderPath);
            AddComboBoxItem(rightFolderPath);
            ComboBoxSourceFolder.SelectedIndex = 0;
        }

        /// <summary>
        /// adds an item to the combobox
        /// </summary>
        /// <param name="itemName"></param>
        private void AddComboBoxItem(string itemName)
        {
            ComboBoxItem SourceFolderComboBoxItem = new ComboBoxItem();
            SourceFolderComboBoxItem.Content = itemName;
            SourceFolderComboBoxItem.Style = (Style)FindResource("ComboBoxDarkItem");
            ComboBoxSourceFolder.Items.Add(SourceFolderComboBoxItem);
        }

        /// <summary>
        /// Loads the trackback entries for the folder into the listview
        /// </summary>
        private void LoadTrackBackEntriesForLeftFolder()
        {
            trackBackCollectionForLeftFolder.Clear();
            ListViewForLeftFolder.Visibility = Visibility.Visible;
            ListViewForRightFolder.Visibility = Visibility.Collapsed;

            if (trackback.hasTrackBackData(leftFolderPath))
            {
                string[] folderList = trackback.GetFolderVersions(leftFolderPath);
                string[] destinationList = trackback.GetFolderDestinations(leftFolderPath);
                string[] timeStampList = trackback.GetFolderTimeStamps(leftFolderPath);

                for (int i = 0; i < folderList.Length; i++)
                    AddTrackBackEntryForLeftFolder(folderList[i], timeStampList[i], destinationList[i]);
            }
        }

        private void LoadTrackBackEntriesForRightFolder()
        {
            trackBackCollectionForRightFolder.Clear();

            ListViewForLeftFolder.Visibility = Visibility.Collapsed;
            ListViewForRightFolder.Visibility = Visibility.Visible;

            if (trackback.hasTrackBackData(rightFolderPath))
            {
                string[] folderList = trackback.GetFolderVersions(rightFolderPath);
                string[] destinationList = trackback.GetFolderDestinations(rightFolderPath);
                string[] timeStampList = trackback.GetFolderTimeStamps(rightFolderPath);

                for (int j = 0; j < folderList.Length; j++)
                    AddTrackBackEntryForRightFolder(folderList[j], timeStampList[j], destinationList[j]);
            }
        }

        private void ComboBoxSourceFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GetSelectedComboBoxItem() == leftFolderPath)
                LoadTrackBackEntriesForLeftFolder();
            else
                LoadTrackBackEntriesForRightFolder();
        }

        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            EnableInterface(false);
            LabelProgress.Visibility = Visibility.Visible;
            LabelProgress.Content = MESSAGE_RESTORING_FOLDERS;

            if (GetSelectedComboBoxItem() == leftFolderPath)
                trackback.StartRestore(leftFolderPath, GetSelectedListViewItem(ListViewForLeftFolder));
            else
                trackback.StartRestore(rightFolderPath, GetSelectedListViewItem(ListViewForRightFolder));
        }

        private string GetSelectedComboBoxItem()
        {
            ComboBoxItem selectedComboBoxItem = new ComboBoxItem();
            selectedComboBoxItem = (ComboBoxItem)ComboBoxSourceFolder.SelectedItem;
            return selectedComboBoxItem.Content.ToString();
        }

        private string GetSelectedListViewItem(ListView listView)
        {
            TrackBackItemData selectedListViewItem = (TrackBackItemData)listView.SelectedItem;
            return selectedListViewItem.dateItem;
        }

        private void EnableInterface(bool status)
        {
            double opacityValue;
            bool enableButtons;

            if (status)
            {
                opacityValue = 1;
                enableButtons = true;
                ButtonRestore.IsEnabled = true;
            }
            else
            {
                enableButtons = false;
                opacityValue = 0.5;
                ButtonRestore.IsEnabled = false;
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

        void backgroundWorkerForTrackBackRestore_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((bool) e.Result)
            {
                EnableInterface(true);
                LabelProgress.Visibility = Visibility.Visible;
                LabelProgress.Content = MESSAGE_RESTORE_COMPLETED;
                helper.Show(nsync.Properties.Resources.restoreComplete, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
            }
            else
            {
                EnableInterface(true);
                LabelProgress.Visibility = Visibility.Visible;
                LabelProgress.Content = MESSAGE_ERROR_DETECTED;
                helper.Show(nsync.Properties.Resources.defaultErrorMessage, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
            }
        }
    }

    /// <summary>
    /// Class of trackback item data, for binding
    /// </summary>
    public class TrackBackItemData
    {
        /// <summary>
        /// property for left item column
        /// </summary>
        public string nameItem { get; set; }
        /// <summary>
        /// property for action column
        /// </summary>
        public string dateItem { get; set; }
        /// <summary>
        /// property for right item column
        /// </summary>
        public string folderItem { get; set; }
    }
}
