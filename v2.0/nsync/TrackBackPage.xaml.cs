using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Xml;

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

        private readonly string SETTINGS_FILE_NAME = "settings.xml";
        private readonly string PATH_MRU_LEFT_FOLDER = "/nsync/MRU/left1";
        private readonly string PATH_MRU_RIGHT_FOLDER = "/nsync/MRU/right1";
        #endregion

        /// <summary>
        /// TrackBackPage Contructor
        /// </summary>
        public TrackBackPage()
        {
            InitializeComponent();

            LoadTrackBackXML();

            trackback = new TrackBackEngine();
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

            ComboBoxItem selectedComboBoxItem = new ComboBoxItem();
            selectedComboBoxItem = (ComboBoxItem)ComboBoxSourceFolder.SelectedItem;
            string folderSelected = selectedComboBoxItem.Content.ToString();

            if (folderSelected == leftFolderPath)
                LoadTrackBackEntriesForLeftFolder();
            else
                LoadTrackBackEntriesForRightFolder();
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

            trackback.LeftFolderPath = leftFolderPath;
            trackback.RightFolderPath = rightFolderPath;

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

            trackback.LeftFolderPath = leftFolderPath;
            trackback.RightFolderPath = rightFolderPath;

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
            ComboBoxItem selectedComboBoxItem = new ComboBoxItem();
            selectedComboBoxItem = (ComboBoxItem)ComboBoxSourceFolder.SelectedItem;
            string folderSelected = selectedComboBoxItem.Content.ToString();

            if (folderSelected == leftFolderPath)
                LoadTrackBackEntriesForLeftFolder();
            else
                LoadTrackBackEntriesForRightFolder();
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
