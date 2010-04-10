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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace nsync
{
    /// <summary>
    /// Interaction logic for VisualPreviewWindow.xaml
    /// </summary>
    public partial class VisualPreviewWindow : Window
    {
        #region Class Variables
        private ObservableCollection<BothPreviewItemData> bothPreviewCollection = new ObservableCollection<BothPreviewItemData>();
        private ObservableCollection<LeftRightPreviewItemData> leftRightPreviewCollection = new ObservableCollection<LeftRightPreviewItemData>();
        private string leftPath;
        private string rightPath;
        private List<FileData> previewFileData = new List<FileData>();
        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for VisualPreviewWindow
        /// </summary>
        public VisualPreviewWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        /// <summary>
        /// property of the preview collection used in binding
        /// </summary>
        public ObservableCollection<BothPreviewItemData> BothPreviewCollection
        { get { return bothPreviewCollection; } }

        /// <summary>
        /// property of the preview collection used in binding
        /// </summary>
        public ObservableCollection<LeftRightPreviewItemData> LeftRightPreviewCollection
        { get { return leftRightPreviewCollection; } }

        /// <summary>
        /// Property for left path
        /// </summary>
        public string LeftPath
        {
            get { return leftPath; }
            set { leftPath = value; }
        }

        /// <summary>
        /// Property for right path
        /// </summary>
        public string RightPath
        {
            get { return rightPath; }
            set { rightPath = value; }
        }

        /// <summary>
        /// Property for filedata list
        /// </summary>
        public List<FileData> PreviewFileData
        {
            get { return previewFileData; }
            set { previewFileData = value; }
        }

        #endregion

        #region Path Shortener
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
        /// Method to shorten paths to a certain length
        /// </summary>
        /// <param name="path">the full path</param>
        /// <param name="length">the length to shorten to</param>
        /// <returns>shortened string</returns>
        static string PathShortener(string path, int length)
        {
            StringBuilder sb = new StringBuilder();
            PathCompactPathEx(sb, path, length, 0);
            return sb.ToString();
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Enable the window to be dragged and moved on mousedown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// when the visual preview is loaded, fill the labels with the left and right folder paths
        /// display the preview information in the listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowVisualPreview_Loaded(object sender, RoutedEventArgs e)
        {
            FixWindowPosition();

            LabelLeftPath.Content = PathShortener(leftPath,64);
            LabelLeftPath.ToolTip = leftPath;
            LabelRightPath.Content = PathShortener(rightPath,64);
            LabelRightPath.ToolTip = rightPath;

            DisplayInfo();
        }

        /// <summary>
        /// Method to display no changes if there aren't any, change the combobox entry so preview listview loads
        /// </summary>
        private void DisplayInfo()
        {
            //todo: load a favourite filter
            ComboBoxFilter.SelectedIndex = 0;

            CheckEmptyList();
        }

        /// <summary>
        /// Checks if list is empty, displays no changes message
        /// </summary>
        private void CheckEmptyList()
        {
            if (ListViewBoth.Visibility == Visibility.Visible)
            {
                if (ListViewBoth.Items.Count == 0)
                    LabelNoChanges.Visibility = Visibility.Visible;
                else
                    LabelNoChanges.Visibility = Visibility.Hidden;
            }
            else if (ListViewLeftRight.Visibility == Visibility.Visible)
            {
                if (ListViewLeftRight.Items.Count == 0)
                    LabelNoChanges.Visibility = Visibility.Visible;
                else
                    LabelNoChanges.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Adds an entry into the preview both list view
        /// </summary>
        /// <param name="previewLeftItem"></param>
        /// <param name="previewFileItem"></param>
        /// <param name="previewRightItem"></param>
        private void AddBothPreviewEntry(string previewLeftItem, string previewFileItem, string previewRightItem , string previewToolTip)
        {
            bothPreviewCollection.Add(new BothPreviewItemData
            {
                bothLeft = previewLeftItem,
                bothFileName = previewFileItem,
                bothRight = previewRightItem,
                bothToolTip = previewToolTip
            });
        }

        /// <summary>
        /// Adds an entry into the preview leftright list view
        /// </summary>
        /// <param name="previewLeftRightFileName"></param>
        /// <param name="previewLeftRightAction"></param>
        private void AddLeftRightPreviewEntry(string previewLeftRightFileName, string previewLeftRightAction, string previewLeftRightToolTip)
        {
            leftRightPreviewCollection.Add(new LeftRightPreviewItemData
            {
                leftRightFileName = previewLeftRightFileName,
                leftRightAction = previewLeftRightAction,
                leftRightToolTip = previewLeftRightToolTip
            });
        }

        /// <summary>
        /// event handler when the filter combobox is changed, change the view of the list and the interface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ComboBoxFilter.SelectedIndex)
            {
                case 0:
                    LoadBothFilter();
                    break;
                case 1:
                    LoadLeftFilter();
                    break;
                case 2:
                    LoadRightFilter();
                    break;
                default:
                    //Error, unexpected behavior
                    MessageBox.Show("Error!");
                    break;
            }

            CheckEmptyList();
        }

        /// <summary>
        /// Load the right filtered list and interface
        /// </summary>
        private void LoadRightFilter()
        {
            BoxLeftPath.IsEnabled = false;
            BoxRightPath.IsEnabled = true;
            ListViewBoth.Visibility = Visibility.Collapsed;
            ListViewLeftRight.Visibility = Visibility.Visible;

            LoadLeftRightData(false);
        }

        /// <summary>
        /// Load the left filtered list and interface
        /// </summary>
        private void LoadLeftFilter()
        {
            BoxLeftPath.IsEnabled = true;
            BoxRightPath.IsEnabled = false;
            ListViewBoth.Visibility = Visibility.Collapsed;
            ListViewLeftRight.Visibility = Visibility.Visible;

            LoadLeftRightData(true);
        }

        /// <summary>
        /// Load the both filtered view of the list and the corresponding interface
        /// </summary>
        private void LoadBothFilter()
        {
            BoxLeftPath.IsEnabled = true;
            BoxRightPath.IsEnabled = true;
            ListViewBoth.Visibility = Visibility.Visible;
            ListViewLeftRight.Visibility = Visibility.Collapsed;

            LoadBothData();
        }

        /// <summary>
        /// Load in the data to the data binding collection for the both filter
        /// </summary>
        private void LoadBothData()
        {
            bothPreviewCollection.Clear();

            foreach (FileData file in previewFileData)
            {
                string shortenedFileName;
                string fullFileName = file.RootPath + file.RelativePath;
                if (file.IsFolder == true)
                {
                    shortenedFileName = PathShortener(file.RelativePath, 60);
                    shortenedFileName = shortenedFileName + " [Folder]";
                    fullFileName += " [Folder]";
                }
                else
                {
                    shortenedFileName = PathShortener(file.RelativePath, 70);
                }
                if (file.RootPath == leftPath)
                {
                    AddBothPreviewEntry(file.ChangeType.ToString(), shortenedFileName, "", fullFileName);
                }
                else
                {
                    AddBothPreviewEntry("", shortenedFileName, file.ChangeType.ToString(), fullFileName);
                }
            }
        }

        /// <summary>
        /// Load in the data to the data binding collection for the left and right filter
        /// </summary>
        /// <param name="IsLeft"></param>
        private void LoadLeftRightData(bool IsLeft)
        {
            leftRightPreviewCollection.Clear();

            foreach (FileData file in previewFileData)
            {
                string shortenedFileName;
                string fullFileName = file.RelativePath;
                if (file.IsFolder == true)
                {
                    shortenedFileName = PathShortener(file.RelativePath, 60);
                    shortenedFileName = shortenedFileName + " [Folder]";
                    fullFileName += " [Folder]";
                }
                else
                {
                    shortenedFileName = PathShortener(file.RelativePath, 70);
                }
                if (file.RootPath == leftPath && IsLeft || file.RootPath == rightPath && !IsLeft)
                {
                    AddLeftRightPreviewEntry(shortenedFileName, file.ChangeType.ToString(), fullFileName);
                }
            }
        }

        /// <summary>
        /// Fix the window postition so it's not out of the screen
        /// </summary>
        private void FixWindowPosition()
        {
            if (this.Left < 0)
                this.Left = 0;

            if (this.Top < 0)
                this.Top = 0;

            if (this.Left > SystemParameters.PrimaryScreenWidth - (double)GetValue(WidthProperty))
                this.Left = SystemParameters.PrimaryScreenWidth - (double)GetValue(WidthProperty);

            if (this.Top > SystemParameters.PrimaryScreenHeight - (double)GetValue(HeightProperty))
                this.Top = SystemParameters.PrimaryScreenHeight - (double)GetValue(HeightProperty);
        }
        #endregion

        /// <summary>
        /// event called on clicking on the header of a column
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
                    if (ListViewBoth.Visibility == Visibility.Visible)
                        SortList(header, direction, ListViewBoth);
                    else if (ListViewLeftRight.Visibility == Visibility.Visible)
                        SortList(header, direction, ListViewLeftRight);
                    else
                        throw new Exception("Error: No listview visible!");

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        /// <summary>
        /// method to sort a listview list by a column
        /// </summary>
        /// <param name="sortBy">data name/parameter to sort by as a string</param>
        /// <param name="direction">Ascending or descending order</param>
        /// <param name="listView">Listview to be sorted</param>
        private void SortList(string sortBy, ListSortDirection direction, ListView listView)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(listView.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }

    #region DataClass for ListView
    /// <summary>
    /// Simple data class of a preview item with 3 elements to fill the both filtered listview
    /// </summary>
    public class BothPreviewItemData
    {
        /// <summary>
        /// property for left item column
        /// </summary>
        public string bothLeft { get; set; }
        /// <summary>
        /// property for right tiem column
        /// </summary>
        public string bothRight { get; set; }
        /// <summary>
        /// property for filename column
        /// </summary>
        public string bothFileName { get; set; }
        /// <summary>
        /// property for tooltip
        /// </summary>
        public string bothToolTip { get; set; }
    }

    /// <summary>
    /// Simple data class of a preview item with 2 elements to fill the leftright filtered listview
    /// </summary>
    public class LeftRightPreviewItemData
    {
        /// <summary>
        /// property for filename column
        /// </summary>
        public string leftRightFileName { get; set; }
        /// <summary>
        /// property for action column
        /// </summary>
        public string leftRightAction { get; set; }
        /// <summary>
        /// property for tooltip
        /// </summary>
        public string leftRightToolTip { get; set; }
    }
    #endregion
}