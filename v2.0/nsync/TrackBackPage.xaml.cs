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
        private ObservableCollection<TrackBackItemData> trackBackCollection = new ObservableCollection<TrackBackItemData>();
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
        /// property of the trackback collection used in binding
        /// </summary>
        public ObservableCollection<TrackBackItemData> TrackBackCollection
        { 
            get { return trackBackCollection; } 
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
        private void AddTrackBackEntry(string trackBackName, string trackBackDate, string trackBackFolder)
        {
            trackBackCollection.Add(new TrackBackItemData
            {
                nameItem = trackBackName,
                dateItem = trackBackDate,
                folderItem = trackBackFolder
            });
        }

        /// <summary>
        /// Page is loaded, initialise listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadSourceFolders();
            LoadTrackBackEntries();
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
        private void LoadTrackBackEntries()
        {
            string[] folderList, destinationList, timeStampList;
            trackback.LeftFolderPath = leftFolderPath;
            trackback.RightFolderPath = rightFolderPath;

            ComboBoxItem selectedComboBoxItem = new ComboBoxItem();
            selectedComboBoxItem = (ComboBoxItem)ComboBoxSourceFolder.SelectedItem;
            string folderSelected = selectedComboBoxItem.Content.ToString();

            if (folderSelected == leftFolderPath && trackback.hasTrackBackData(leftFolderPath))
            {
                folderList = trackback.GetFolderVersions(leftFolderPath);
                destinationList = trackback.GetFolderDestinations(leftFolderPath);
                timeStampList = trackback.GetFolderTimeStamps(leftFolderPath);

                for (int i = 0; i < folderList.Length; i++)
                    AddTrackBackEntry(folderList[i], timeStampList[i], destinationList[i]);
            }
            else if (folderSelected == rightFolderPath && trackback.hasTrackBackData(rightFolderPath))
            {
                folderList = trackback.GetFolderVersions(rightFolderPath);
                destinationList = trackback.GetFolderDestinations(rightFolderPath);
                timeStampList = trackback.GetFolderTimeStamps(rightFolderPath);

                for (int j = 0; j < folderList.Length; j++)
                    AddTrackBackEntry(folderList[j], timeStampList[j], destinationList[j]);
            }
        }

        private void ComboBoxSourceFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTrackBackEntries();
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
