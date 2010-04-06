using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;

namespace nsync
{
    /// <summary>
    /// Interaction logic for TrackBackPage.xaml
    /// </summary>
    public partial class TrackBackPage : Page
    {
        private ObservableCollection<TrackBackItemData> trackBackCollection = new ObservableCollection<TrackBackItemData>();

        /// <summary>
        /// TrackBackPage Contructor
        /// </summary>
        public TrackBackPage()
        {
            InitializeComponent();
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
        /// property of the trackback collection used in binding
        /// </summary>
        public ObservableCollection<TrackBackItemData> TrackBackCollection
        { get { return trackBackCollection; } }

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
            AddComboBoxItem("C:\\");
            AddComboBoxItem("C:\\2");
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
            AddTrackBackEntry("nametest", "datetest", "foldertest");
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
