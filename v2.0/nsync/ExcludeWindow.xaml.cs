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
using System.Runtime.InteropServices;


namespace nsync
{
    /// <summary>
    /// Interaction logic for ExcludeWindow.xaml
    /// </summary>
    public partial class ExcludeWindow : Window
    {
        #region Class Variables
        private string leftPath;
        private string rightPath;
        private List<string> excludeFolders;
        private List<string> excludeFileNames;
        private List<string> excludeFileTypes;
        private List<string> excludeSubFolders;
        private List<string> oldExcludeSubFolders;
        private List<string> oldExcludeFolders;
        private List<string> oldExcludeFileNames;
        private List<string> oldExcludeFileTypes;
        private List<string> availableFileTypes;
        private List<string> excludeInvalid;
        private List<string> oldExcludeInvalid;
        private readonly int MAX_STRING_LENGTH = 90;
        bool reallyLeft = true;
        private bool cancel = false;
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructor for ExcludeWindow
        /// </summary>
        public ExcludeWindow()
        {
            InitializeComponent();
            excludeFolders = new List<string>();
            excludeFileNames = new List<string>();
            excludeFileTypes = new List<string>();
            excludeSubFolders = new List<string>();
            oldExcludeSubFolders = new List<string>();
            oldExcludeFolders = new List<string>();
            oldExcludeFileNames = new List<string>();
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

        /// <summary>
        /// Property for cancel to check;
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
        }

        /// <summary>
        /// Function to return list of File Types in Exclude Box
        /// </summary>
        /// <returns>List of File Types to be excluded in sync</returns>
        public List<string> GetFileTypeList()
        {
            return excludeFileTypes;
        }

        /// <summary>
        /// Function to return list of File Names in Exclude Box
        /// </summary>
        /// <returns>List of File Names to be excluded in sync</returns>
        public List<string> GetFileNameList()
        {
            return excludeFileNames;
        }

        /// <summary>
        /// Function to return list of Folders in Exclude Box
        /// </summary>
        /// <returns>List of Folders to be excluded in sync</returns>
        public List<string> GetFolderList()
        {
            return excludeFolders;
        }
        #endregion

        #region Private Methods
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
        /// event handler when close window button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            cancel = true;

            this.Close();
        }

        /// <summary>
        /// event handler when continue/next button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            cancel = false;
            this.Close();
        }

        /// <summary>
        /// event handler when the window is loaded. loads the paths of the left and right folders into the labels. populates file types.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowExclude_Loaded(object sender, RoutedEventArgs e)
        {
            LabelLeftPath.Content = PathShortener(leftPath, 46);
            LabelLeftPath.ToolTip = leftPath;
            LabelRightPath.Content = PathShortener(rightPath, 46);
            LabelRightPath.ToolTip = rightPath;

            try
            {
                PopulateFileTypes();
            }
            catch (System.UnauthorizedAccessException)
            {
                //just close as homepage will handle the file access issues
                this.Close();
            }
            catch (Exception exceptionError)
            {
                MessageBox.Show(exceptionError.Message);
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
        /// populates the combobox with the file types present in the left and right folders
        /// </summary>
        private void PopulateFileTypes()
        {
            try
            {
                string[] filePathsLeft = Directory.GetFiles(leftPath, "*.*",
                                             SearchOption.AllDirectories);
                string[] filePathsRight = Directory.GetFiles(rightPath, "*.*",
                                             SearchOption.AllDirectories);
                foreach (string paths in filePathsLeft)
                {
                    AddToFileTypesList(System.IO.Path.GetExtension(paths));
                }
                foreach (string paths in filePathsRight)
                {
                    AddToFileTypesList(System.IO.Path.GetExtension(paths));
                }
                PopulateFileTypesComboBox();
            }
            catch (System.UnauthorizedAccessException e)
            {
                //Cannot access files; locked
                throw e;
            }
            catch (Exception e)
            {
                throw new Exception("Error:\n" + e.Message);
            }
        }

        /// <summary>
        /// adds entries from the list to the combobox.
        /// </summary>
        private void PopulateFileTypesComboBox()
        {
            foreach (string fileExtension in availableFileTypes)
            {
                AddComboBoxItem(fileExtension);
            }
        }

        /// <summary>
        /// adds an item to the combobox
        /// </summary>
        /// <param name="fileExtension">string of the item to add</param>
        private void AddComboBoxItem(string fileExtension)
        {
            ComboBoxItem fileTypeComboBoxItem = new ComboBoxItem();
            fileTypeComboBoxItem.Content = fileExtension;
            ComboBoxFileType.Items.Add(fileTypeComboBoxItem);
        }

        /// <summary>
        /// adds the file type to a list object
        /// </summary>
        /// <param name="fileExtension">string of the file type to be added to the list</param>
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

        /// <summary>
        /// event handler on dragging and object into the exclude box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                            if (dirTemp.Exists)
                            {
                                if (IsNotInList(excludeFolders, i))
                                {
                                    if (!IsSubFolder(excludeFolders, i))
                                    {
                                        RemoveSubFolders(i);
                                        excludeFolders.Add(i);
                                    }
                                    else
                                    {
                                        if (IsNotInList(excludeSubFolders, i))
                                            excludeSubFolders.Add(i);
                                    }
                                }
                            }
                            else
                            {
                                string fileName = System.IO.Path.GetFileName(i);
                                if (IsNotInList(excludeFileNames, fileName))
                                    excludeFileNames.Add(fileName);
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

        private void RemoveSubFolders(string folderPath)
        {
            List<string> pathsToRemove = new List<string>();

            foreach (string singlePath in excludeFolders)
            {
                if (IsSubFolderCheck(singlePath, folderPath))
                {
                    if (singlePath != folderPath)
                    {
                        excludeSubFolders.Add(singlePath);
                        pathsToRemove.Add(singlePath);
                    }
                }
            }

            foreach(string singlePath in pathsToRemove) {
                excludeFolders.Remove(singlePath);
            }
        }

        private bool IsSubFolder(List<string> excludeFolderPaths, string folderPath)
        {
            foreach (string singlePath in excludeFolderPaths)
            {
                if (IsSubFolderCheck(folderPath, singlePath))
                    return true;
            }
            return false;
        }

        private bool IsSubFolderCheck(string childPath, string parentPath)
        {
            string[] childPathArray = childPath.Split(new char[] { '\\' });
            string[] parentPathArray = parentPath.Split(new char[] { '\\' });

            if (parentPathArray.Length > childPathArray.Length)
                return false;

            for (int i = 0; i < parentPathArray.Length; i++)
            {
                if (parentPathArray[i] == "")
                    continue;

                if (parentPathArray[i] != childPathArray[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// method to check if one path is the root of another
        /// </summary>
        /// <param name="childPath">the longer child path</param>
        /// <param name="parentPath">the shorter parent path</param>
        /// <returns>bool whether it is a child path</returns>
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
                if (sourceArray[i] == "")
                    continue;

                if (destinationArray[i] == "")
                    continue;

                if (sourceArray[i] != destinationArray[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// check if a string is in a string list or not
        /// </summary>
        /// <param name="ExcludeList">list of strings to check against</param>
        /// <param name="path">string to check</param>
        /// <returns>bool, true if string is not in list</returns>
        private bool IsNotInList(List<string> ExcludeList, string path)
        {
            for (int i = 0; i < ExcludeList.Count; i++)
            {
                if (ExcludeList[i] == path)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// updates the listbox with items from the backend list
        /// </summary>
        private void UpdateListBox()
        {
            // Setup listbox items
            for (int i = 0; i < excludeFolders.Count; i++)
            {
                AddListBoxItem("Exclude Folder: ", new SolidColorBrush(Colors.SkyBlue), excludeFolders[i]);
            }
            for (int i = 0; i < excludeFileNames.Count; i++)
            {
                AddListBoxItem("Exclude All Files: ", new SolidColorBrush(Colors.White), excludeFileNames[i]);
            }
            for (int i = 0; i < excludeFileTypes.Count; i++)
            {
                AddListBoxItem("Exclude File Type: ", new SolidColorBrush(Colors.Orange), excludeFileTypes[i]);
            }
            for (int i = 0; i < excludeInvalid.Count; i++)
            {
                AddListBoxItem("Not in synchronized folders: ", new SolidColorBrush(Colors.LightPink), excludeInvalid[i]);
            }
            for (int i = 0; i < excludeSubFolders.Count; i++)
            {
                AddListBoxItem("Parent Folder Included: ", new SolidColorBrush(Colors.LightPink), excludeSubFolders[i]);
            }

            RefreshInterface();
        }

        /// <summary>
        /// refresh the message displayed in the status label
        /// </summary>
        private void RefreshInterface()
        {
            if (ListBoxExclude.Items.Count > 0)
            {
                LabelStatus.Content = "Click on entries to remove them from the exclude list.";
                ButtonClear.IsEnabled = true;
                ListBoxExclude.Visibility = Visibility.Visible;
            }
            else
            {
                ListBoxExclude.Visibility = Visibility.Hidden;
                ButtonClear.IsEnabled = false;
                LabelStatus.Content = "Drag & drop files and folders to exclude.";
            }
        }

        /// <summary>
        /// add an item to the listbox
        /// </summary>
        /// <param name="excludeStatement">string of the statement to add</param>
        /// <param name="itemColor">color of the string</param>
        /// <param name="tag">the actual path to be added to the tag property of the listbox item</param>
        private void AddListBoxItem(string excludeStatement, Brush itemColor, string tag)
        {
            ListBoxItem excludeListBoxItem = new ListBoxItem();
            excludeListBoxItem.Content = excludeStatement + PathShortener(tag, 90 - excludeStatement.Length);
            excludeListBoxItem.ToolTip = excludeStatement + tag;
            excludeListBoxItem.Foreground = itemColor;
            excludeListBoxItem.Tag = tag;

            ListBoxExclude.Items.Add(excludeListBoxItem);
        }

        /// <summary>
        /// saves the previous state of all the backend lists
        /// </summary>
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
                CopyList(excludeFileNames, oldExcludeFileNames);
                CopyList(excludeFileTypes, oldExcludeFileTypes);
                CopyList(excludeInvalid, oldExcludeInvalid);
                CopyList(excludeSubFolders, oldExcludeSubFolders);
            }
        }

        /// <summary>
        /// copy one list to another
        /// </summary>
        /// <param name="source">the list to copy from</param>
        /// <param name="destination">the list to copy to</param>
        private void CopyList(List<string> source, List<string> destination)
        {
            destination.Clear();
            foreach (string i in source)
            {
                destination.Add(i);
            }
        }

        /// <summary>
        /// event handler when items are dragged out without being dropped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// restores the backend lists to their previous state
        /// </summary>
        private void RestoreLastState()
        {
            //restore old lists
            CopyList(oldExcludeFolders, excludeFolders);
            CopyList(oldExcludeFileNames, excludeFileNames);
            CopyList(oldExcludeFileTypes, excludeFileTypes);
            CopyList(oldExcludeInvalid, excludeInvalid);
            CopyList(oldExcludeSubFolders, excludeSubFolders);

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

        /// <summary>
        /// event handler when items are dropped in the exclude box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxExclude_Drop(object sender, DragEventArgs e)
        {
            reallyLeft = true;
        }

        /// <summary>
        /// event handler when the selection is changed in the combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            }
        }

        /// <summary>
        /// event handler when the mouse is clicked on an item in the exclude list box. delete the item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxExclude_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxExclude.SelectedIndex >= 0)
            {
                ListBoxItem selectedListBoxItem = new ListBoxItem();
                selectedListBoxItem = (ListBoxItem)ListBoxExclude.SelectedItem;
                string path = selectedListBoxItem.Tag.ToString();
                ListBoxExclude.SelectedIndex = -1;

                if (excludeFolders.Contains(path))
                {
                    RestoreSubFolders(path);
                }

                excludeFolders.Remove(path);
                excludeFileNames.Remove(path);
                excludeFileTypes.Remove(path);
                excludeInvalid.Remove(path);
                excludeSubFolders.Remove(path);

                ClearListBox();
                UpdateListBox();
            }
        }

        private void RestoreSubFolders(string folderPath)
        {
            List<string> pathsToRemove = new List<string>();

            foreach (string singlePath in excludeSubFolders)
            {
                if (IsSubFolderCheck(singlePath, folderPath))
                {
                    if (singlePath != folderPath)
                    {
                        excludeFolders.Add(singlePath);
                        pathsToRemove.Add(singlePath);
                    }
                }
            }

            foreach (string singlePath in pathsToRemove)
            {
                excludeSubFolders.Remove(singlePath);
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
            excludeFileNames.Clear();
            excludeFileTypes.Clear();
            excludeFolders.Clear();
            excludeInvalid.Clear();
            excludeSubFolders.Clear();
        }

        private void ComboBoxFileType_DropDownOpened(object sender, EventArgs e)
        {
            ComboBoxFileType.SelectedIndex = -1;
        }
        #endregion
    }
}