using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace nsync
{
    /// <summary>
    /// Interaction logic for ExcludeWindow.xaml
    /// </summary>
    public partial class ExcludeWindow : Window
    {
        private string leftPath;
        private string rightPath;
        //private int lastStateItemsCount = 0;
        private List<string> excludeFolders;
        private List<string> excludeFiles;
        private List<string> excludeFileTypes;
        private List<string> oldExcludeFolders;
        private List<string> oldExcludeFiles;
        private List<string> oldExcludeFileTypes;
        private List<string> availableFileTypes;
        private List<string> excludeInvalid;
        private List<string> oldExcludeInvalid;
        private readonly int MAX_STRING_LENGTH = 90;
        bool reallyLeft = true;

        /// <summary>
        /// Constructor for ExcludeWindow
        /// </summary>
        public ExcludeWindow()
        {
            InitializeComponent();
            excludeFolders = new List<string>();
            excludeFiles = new List<string>();
            excludeFileTypes = new List<string>();
            oldExcludeFolders = new List<string>();
            oldExcludeFiles = new List<string>();
            oldExcludeFileTypes = new List<string>();
            oldExcludeInvalid = new List<string>();
            availableFileTypes = new List<string>();
            excludeInvalid = new List<string>();
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

        private void titleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            //todo: handle close window

            this.Close();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ContinueSync();
        }

        private void ContinueSync()
        {
            //todo: handle continue sync

            CloseWindow();
        }

        private void WindowExclude_Loaded(object sender, RoutedEventArgs e)
        {
            LabelLeftPath.Content = ShortenPath(leftPath, 130);
            LabelLeftPath.ToolTip = leftPath;
            LabelRightPath.Content = ShortenPath(rightPath, 130);
            LabelRightPath.ToolTip = rightPath;

            PopulateFileTypes();
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

        private void PopulateFileTypes()
        {
            string[] filePathsLeft = Directory.GetFiles(leftPath, "*.*",
                                         SearchOption.AllDirectories);
            string[] filePathsRight = Directory.GetFiles(rightPath, "*.*",
                                         SearchOption.AllDirectories);
            foreach(string paths in filePathsLeft) {
                AddToFileTypesList(System.IO.Path.GetExtension(paths));
            }
            foreach (string paths in filePathsRight)
            {
                AddToFileTypesList(System.IO.Path.GetExtension(paths));
            }
            PopulateFileTypesComboBox();
        }

        private void PopulateFileTypesComboBox()
        {
            foreach (string fileExtension in availableFileTypes)
            {
                AddComboBoxItem(fileExtension);
            }
        }

        private void AddComboBoxItem(string fileExtension)
        {
            ComboBoxItem fileTypeComboBoxItem = new ComboBoxItem();
            fileTypeComboBoxItem.Content = fileExtension;
            ComboBoxFileType.Items.Add(fileTypeComboBoxItem);
        }

        private void AddToFileTypesList(string fileExtension)
        {
            if (fileExtension != Properties.Resources.metaDataFileExtension)
            {
                if (!availableFileTypes.Contains(fileExtension))
                {
                    availableFileTypes.Add(fileExtension);
                }
            }
        }

        private void BoxExclude_DragEnter(object sender, DragEventArgs e)
        {
            
            if (reallyLeft)
            {
                SaveLastState();

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {

                    string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                    foreach (string i in fileNames)
                    {
                        if (IsAChildPath(i, leftPath) || IsAChildPath(i, rightPath))
                        {
                            DirectoryInfo dirTemp = new DirectoryInfo(i);
                            //FileInfo fileTemp = new FileInfo(i);
                            if (dirTemp.Exists)
                            {
                                if (IsNotInList(excludeFolders, i))
                                    excludeFolders.Add(i);
                            }
                            else
                            {
                                if (IsNotInList(excludeFiles, i))
                                    excludeFiles.Add(i);
                            }
                        }
                        else
                        {
                            if (IsNotInList(excludeInvalid, i))
                                excludeInvalid.Add(i);
                        }
                    }
                    ClearListBox();
                    UpdateListBox();
                }
                reallyLeft = false;
            }
        }

        private bool IsAChildPath(string childPath, string parentPath)
        {
            string[] childPathArray = childPath.Split(new char[] { '\\' });
            string[] parentPathArray = parentPath.Split(new char[] { '\\' });

            return IsSubPath(childPathArray, parentPathArray);
        }

        /// <summary>
        /// Compares the folder paths of the input arrays and determine if they
        /// have the same path from the root directory.
        /// </summary>
        /// <param name="sourceArray">Array of a folder path to be checked</param>
        /// <param name="destinationArray">Array of a folder path to be checked</param>
        /// <returns></returns>
        private bool IsSubPath(string[] sourceArray, string[] destinationArray)
        {
            if (sourceArray.Length > destinationArray.Length)
            {
                string[] tmp = sourceArray;
                sourceArray = destinationArray;
                destinationArray = tmp;
            }

            for (int i = 0; i < sourceArray.Length; i++)
            {
                if (sourceArray[i] != destinationArray[i])
                    return false;
            }
            return true;
        }

        private bool IsNotInList(List<string> ExcludeList, string path)
        {
            for (int i = 0; i < ExcludeList.Count; i++)
            {
                if (ExcludeList[i] == path)
                    return false;
            }
            return true;
        }

        private void UpdateListBox()
        {
            // Setup listbox items
            for (int i = 0; i < excludeFolders.Count; i++)
            {
                AddListBoxItem("Exclude Folder: " + excludeFolders[i], new SolidColorBrush(Colors.SkyBlue), excludeFolders[i]);
            }
            for (int i = 0; i < excludeFiles.Count; i++)
            {
                AddListBoxItem("Exclude File: " + excludeFiles[i], new SolidColorBrush(Colors.White), excludeFiles[i]);
            }
            for (int i = 0; i < excludeFileTypes.Count; i++)
            {
                AddListBoxItem("Exclude File Type: " + excludeFileTypes[i], new SolidColorBrush(Colors.Orange), excludeFileTypes[i]);
            }
            for (int i = 0; i < excludeInvalid.Count; i++)
            {
                AddListBoxItem("Not in synchronized folders: " + excludeInvalid[i], new SolidColorBrush(Colors.LightPink), excludeInvalid[i]);
            }

            RefreshInterface();
        }

        private void RefreshInterface()
        {
            if (ListBoxExclude.Items.Count > 0)
            {
                LabelStatus.Content = "Click on entries to remove them from the exclude list.";
                ButtonClear.IsEnabled = true;
            }
            else
            {
                ButtonClear.IsEnabled = false;
                LabelStatus.Content = "Drag & drop files and folders to exclude.";
            }
        }

        private string ShortenPath(string fullPath)
        {
            if (fullPath.Length > MAX_STRING_LENGTH)
            {
                return fullPath.Substring(0, 50) + "..." + fullPath.Substring(fullPath.Length - 40, 40);
            }
            return fullPath;
        }

        private void AddListBoxItem(string excludeStatement, Brush itemColor, string tag)
        {
            ListBoxItem excludeListBoxItem = new ListBoxItem();
            excludeListBoxItem.Content = ShortenPath(excludeStatement);
            excludeListBoxItem.ToolTip = excludeStatement;
            excludeListBoxItem.Foreground = itemColor;
            excludeListBoxItem.Tag = tag;

            //listBoxLeft.MouseUp += new MouseButtonEventHandler(ListBoxLeft_MouseUp);
            //listBoxLeft.Tag = i;

            ListBoxExclude.Items.Add(excludeListBoxItem);
        }



        private void SaveLastState()
        {
            if (HintText.Visibility == Visibility.Visible)
            {
                HintText.Visibility = Visibility.Collapsed;
                HintIcon.Visibility = Visibility.Collapsed;
                ListBoxExclude.Visibility = Visibility.Visible;
            }
            else
            {
                //backup lists
                CopyList(excludeFolders, oldExcludeFolders);
                CopyList(excludeFiles, oldExcludeFiles);
                CopyList(excludeFileTypes, oldExcludeFileTypes);
                CopyList(excludeInvalid, oldExcludeInvalid);
            }            
        }

        private void CopyList(List<string> source, List<string> destination)
        {
            destination.Clear();
            foreach(string i in source) {
                destination.Add(i);
            }
        }

        private void BoxExclude_DragLeave(object sender, DragEventArgs e)
        {
            double x = MouseUtilities.CorrectGetPosition(WindowExclude).X;
            double y = MouseUtilities.CorrectGetPosition(WindowExclude).Y;
            if (x < 30 || y < 155 || y > 550 || x > 510)
            {
                RestoreLastState();
                reallyLeft = true;
            }
        }

        private void RestoreLastState()
        {
            //if (lastStateItemsCount == 0)
            //{
            //    HintText.Visibility = Visibility.Visible;
            //    HintIcon.Visibility = Visibility.Visible;
            //    ListBoxExclude.Visibility = Visibility.Hidden;
            //    lastStateItemsCount = ListBoxExclude.Items.Count;
            //}
            //else
            //{
                //restore old lists
                CopyList(oldExcludeFolders, excludeFolders);
                CopyList(oldExcludeFiles, excludeFiles);
                CopyList(oldExcludeFileTypes, excludeFileTypes);
                CopyList(oldExcludeInvalid, excludeInvalid);
            //}

            ClearListBox();
            UpdateListBox();
        }

        /// <summary>
        /// Clear the listbox in the gui
        /// </summary>
        private void ClearListBox()
        {
            ListBoxExclude.Items.Clear();
        }

        private void BoxExclude_Drop(object sender, DragEventArgs e)
        {
            reallyLeft = true;
        }

        private void ComboBoxFileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxFileType.SelectedIndex >= 0)
            {
                if (HintText.Visibility == Visibility.Visible)
                {
                    HintText.Visibility = Visibility.Collapsed;
                    HintIcon.Visibility = Visibility.Collapsed;
                    ListBoxExclude.Visibility = Visibility.Visible;
                }

                ComboBoxItem selectedComboBoxItem = new ComboBoxItem();
                selectedComboBoxItem = (ComboBoxItem)ComboBoxFileType.SelectedItem;
                string fileExtension = selectedComboBoxItem.Content.ToString();
                if (IsNotInList(excludeFileTypes, fileExtension))
                    excludeFileTypes.Add(fileExtension);
                ClearListBox();
                UpdateListBox();
                //This is a fix for the problem of trying to select an already selected item
                //ComboBoxFileType.SelectedIndex = -1;
            }
        }

        private void ListBoxExclude_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxExclude.SelectedIndex >= 0)
            {
                ListBoxItem selectedListBoxItem = new ListBoxItem();
                selectedListBoxItem = (ListBoxItem)ListBoxExclude.SelectedItem;
                string path = selectedListBoxItem.Tag.ToString();
                ListBoxExclude.SelectedIndex = -1;

                excludeFolders.Remove(path);
                excludeFiles.Remove(path);
                excludeFileTypes.Remove(path);
                excludeInvalid.Remove(path);

                ClearListBox();
                UpdateListBox();

                //ListBoxExclude.Items.Remove(ListBoxExclude.SelectedItem);
                //ListBoxExclude.SelectedIndex = -1;

            }
        }

        /// <summary>
        /// Event handler for clear button click, clears backend lists and listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            ClearLists();
            ClearListBox();
            RefreshInterface();
        }

        /// <summary>
        /// Functions to clear the backend lists
        /// </summary>
        private void ClearLists()
        {
            excludeFiles.Clear();
            excludeFileTypes.Clear();
            excludeFolders.Clear();
            excludeInvalid.Clear();
        }
    }
}
