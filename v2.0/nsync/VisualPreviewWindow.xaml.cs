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

        private void WindowVisualPreview_Loaded(object sender, RoutedEventArgs e)
        {
            LabelLeftPath.Content = ShortenPath(leftPath,130);
            LabelLeftPath.ToolTip = leftPath;
            LabelRightPath.Content = ShortenPath(rightPath,130);
            LabelRightPath.ToolTip = rightPath;

            DisplayInfo();          
        }

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
        /// Better shorten path algorithm
        /// </summary>
        /// <param name="fullPath">original long path name</param>
        /// <param name="maxLength">max length before shortening</param>
        /// <returns>shortened path name</returns>
        private string ShortenPath(string fullPath, int maxLength)
        {
            string[] fullPathArray = fullPath.Split(new char[] { '\\' });

            if (fullPath.Length > maxLength)
            {
                return fullPathArray[0] + '\\' + "..." + '\\' + fullPathArray[fullPathArray.Length - 1];
            }
            return fullPath;
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