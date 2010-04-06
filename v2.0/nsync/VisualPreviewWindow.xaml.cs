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
        private ObservableCollection<PreviewItemData> previewCollection = new ObservableCollection<PreviewItemData>();
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
        public ObservableCollection<PreviewItemData> PreviewCollection
        { get { return previewCollection; } }

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
            foreach (FileData file in previewFileData)
            {
                string shortenedFileName = ShortenFileName(file.FileName,42);

                if (file.ChangeType == Changes.Create)
                {
                    if (file.RootPath == leftPath)
                    {
                        AddPreviewEntry(shortenedFileName, "Create", "");
                    }
                    else
                    {
                        AddPreviewEntry("", "Create", shortenedFileName);
                    }
                }
                if (file.ChangeType == Changes.Update)
                {
                    if (file.RootPath == leftPath)
                    {
                        AddPreviewEntry(shortenedFileName, "Update", "");
                    }
                    else
                    {
                        AddPreviewEntry("", "Update", shortenedFileName);
                    }

                }
                if (file.ChangeType == Changes.Delete)
                {
                    if (file.RootPath == leftPath)
                    {
                        AddPreviewEntry(shortenedFileName, "Delete", "");
                    }
                    else
                    {
                        AddPreviewEntry("", "Delete", shortenedFileName);
                    }
                }
                if (file.ChangeType == Changes.Rename)
                {
                    if (file.RootPath == leftPath)
                    {
                        AddPreviewEntry(shortenedFileName, "Rename", "");
                    }
                    else
                    {
                        AddPreviewEntry("", "Rename", shortenedFileName);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an entry into the preview list view
        /// </summary>
        /// <param name="previewLeftItem">string for left item column</param>
        /// <param name="previewRightItem">string for right item column</param>
        /// <param name="previewAction">string for action column</param>
        private void AddPreviewEntry(string previewLeftItem, string previewRightItem, string previewAction)
        {
            previewCollection.Add(new PreviewItemData
            {
                leftItem = previewLeftItem,
                action = previewRightItem,
                rightItem = previewAction
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

    }

    /// <summary>
    /// Simple data class of a preview item with 3 elements to fill the listview
    /// </summary>
    public class PreviewItemData
    {
        /// <summary>
        /// property for left item column
        /// </summary>
        public string leftItem { get; set; }
        /// <summary>
        /// property for action column
        /// </summary>
        public string action { get; set; }
        /// <summary>
        /// property for right item column
        /// </summary>
        public string rightItem { get; set; }
    }

}