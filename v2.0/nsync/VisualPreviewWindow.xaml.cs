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
using System.Runtime.InteropServices;

namespace nsync
{
    /// <summary>
    /// Interaction logic for VisualPreviewWindow.xaml
    /// </summary>
    public partial class VisualPreviewWindow : Window
    {
        private ObservableCollection<BothPreviewItemData> bothPreviewCollection = new ObservableCollection<BothPreviewItemData>();
        private string leftPath;
        private string rightPath;
        private List<FileData> previewFileData = new List<FileData>();

        /// <summary>
        /// Constructor for VisualPreviewWindow
        /// </summary>
        public VisualPreviewWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// enable the window to be dragged and moved on mousedown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// property of the preview collection used in binding
        /// </summary>
        public ObservableCollection<BothPreviewItemData> BothPreviewCollection
        { get { return bothPreviewCollection; } }

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
            LabelLeftPath.Content = PathShortener(leftPath,64);
            LabelLeftPath.ToolTip = leftPath;
            LabelRightPath.Content = PathShortener(rightPath,64);
            LabelRightPath.ToolTip = rightPath;

            DisplayInfo();          
        }

        /// <summary>
        /// Method to display the preview information in the listview
        /// </summary>
        private void DisplayInfo()
        {
            if (previewFileData.Count == 0)
                LabelNoChanges.Visibility = Visibility.Visible;
            else
                LabelNoChanges.Visibility = Visibility.Hidden;

            foreach (FileData file in previewFileData)
            {
                string shortenedFileName = ShortenFileName(file.FileName,42);

                if (file.ChangeType == Changes.Create)
                {
                    if (file.RootPath == leftPath)
                    {
                        AddBothPreviewEntry("Create", shortenedFileName, "");
                    }
                    else
                    {
                        AddBothPreviewEntry("", shortenedFileName, "Create");
                    }
                }
                if (file.ChangeType == Changes.Update)
                {
                    if (file.RootPath == leftPath)
                    {
                        AddBothPreviewEntry("Update", shortenedFileName, "");
                    }
                    else
                    {
                        AddBothPreviewEntry("", shortenedFileName, "Update");
                    }

                }
                if (file.ChangeType == Changes.Delete)
                {
                    if (file.RootPath == leftPath)
                    {
                        AddBothPreviewEntry("Delete", shortenedFileName, "");
                    }
                    else
                    {
                        AddBothPreviewEntry("", shortenedFileName, "Delete");
                    }
                }
                if (file.ChangeType == Changes.Rename)
                {
                    if (file.RootPath == leftPath)
                    {
                        AddBothPreviewEntry("Rename", shortenedFileName, "");
                    }
                    else
                    {
                        AddBothPreviewEntry("", shortenedFileName, "Rename");
                    }
                }
            }
        }

        /// <summary>
        /// Adds an entry into the preview list view
        /// </summary>
        /// <param name="previewLeftItem"></param>
        /// <param name="previewFileItem"></param>
        /// <param name="previewRightItem"></param>
        private void AddBothPreviewEntry(string previewLeftItem, string previewFileItem, string previewRightItem )
        {
            bothPreviewCollection.Add(new BothPreviewItemData
            {
                bothLeft = previewLeftItem,
                bothFileName = previewFileItem,
                bothRight = previewRightItem
            });
        }

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

        /// <summary>
        /// Shortens file name if its too long
        /// </summary>
        /// <param name="fullPath">original long filename</param>
        /// <param name="maxLength">max length before shortening</param>
        /// <returns>shortened file name</returns>
        private string ShortenFileName(string fullPath, int maxLength)
        {
            if (fullPath.Length > maxLength)
            {
                return fullPath.Substring(0, (maxLength / 2)-3) + "..." + fullPath.Substring(fullPath.Length - (maxLength / 2), maxLength / 2);
            }
            return fullPath;
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
                    break;
            }
        }

        /// <summary>
        /// Load the right filtered list and interface
        /// </summary>
        private void LoadRightFilter()
        {
            BoxLeftPath.IsEnabled = false;
            BoxRightPath.IsEnabled = true;
        }

        /// <summary>
        /// Load the left filtered list and interface
        /// </summary>
        private void LoadLeftFilter()
        {
            BoxLeftPath.IsEnabled = true;
            BoxRightPath.IsEnabled = false;
        }

        /// <summary>
        /// Load the both filtered view of the list and the corresponding interface
        /// </summary>
        private void LoadBothFilter()
        {
            BoxLeftPath.IsEnabled = true;
            BoxRightPath.IsEnabled = true;
        }

    }

    /// <summary>
    /// Simple data class of a preview item with 3 elements to fill the listview
    /// </summary>
    public class BothPreviewItemData
    {
        /// <summary>
        /// property for left item column
        /// </summary>
        public string bothLeft { get; set; }
        /// <summary>
        /// property for action column
        /// </summary>
        public string bothRight { get; set; }
        /// <summary>
        /// property for right item column
        /// </summary>
        public string bothFileName { get; set; }
    }

}