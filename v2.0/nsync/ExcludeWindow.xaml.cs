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
        private int lastStateItemsCount = 0;
        private List<string> excludeFolders;
        private List<string> excludeFiles;
        private List<string> excludeFileTypes;
        private List<string> oldExcludeFolders;
        private List<string> oldExcludeFiles;
        private List<string> oldExcludeFileTypes;
        private List<string> availableFileTypes;
        private readonly int MAX_STRING_LENGTH = 90;

        public ExcludeWindow()
        {
            InitializeComponent();
            excludeFolders = new List<string>();
            excludeFiles = new List<string>();
            excludeFileTypes = new List<string>();
            oldExcludeFolders = new List<string>();
            oldExcludeFiles = new List<string>();
            oldExcludeFileTypes = new List<string>();
            availableFileTypes = new List<string>();
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
            LabelLeftPath.Content = leftPath;
            LabelRightPath.Content = rightPath;

            PopulateFileTypes();
            

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
            if (!availableFileTypes.Contains(fileExtension))
            {
                availableFileTypes.Add(fileExtension);
            }
        }

        private void BoxExclude_DragEnter(object sender, DragEventArgs e)
        {
            SaveLastState();

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (string i in fileNames)
                {
                    DirectoryInfo dirTemp = new DirectoryInfo(i);
                    //FileInfo fileTemp = new FileInfo(i);
                    if (dirTemp.Exists)
                    {
                        if(IsNotInList(excludeFolders,i))
                            excludeFolders.Add(i);
                    }
                    else
                    {
                        if (IsNotInList(excludeFiles, i))
                            excludeFiles.Add(i);
                    }
                }
                ClearListBox();
                UpdateListBox();
            }
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
                AddListBoxItem("Exclude Folder: " + excludeFolders[i]);
            }
            for (int i = 0; i < excludeFiles.Count; i++)
            {
                AddListBoxItem("Exclude File: " + excludeFiles[i]);
            }
            for (int i = 0; i < excludeFileTypes.Count; i++)
            {
                AddListBoxItem("Exclude File Type: " + excludeFileTypes[i]);
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

        private void AddListBoxItem(string excludeStatement)
        {
            ListBoxItem excludeListBoxItem = new ListBoxItem();
            excludeListBoxItem.Content = ShortenPath(excludeStatement);
            excludeListBoxItem.ToolTip = excludeStatement;

            //listBoxLeft.MouseUp += new MouseButtonEventHandler(ListBoxLeft_MouseUp);
            //listBoxLeft.MouseEnter += new MouseEventHandler(listBoxLeft_MouseEnter);
            //listBoxLeft.MouseLeave += new MouseEventHandler(listBoxLeft_MouseLeave);
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
            RestoreLastState();
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
            //}

            ClearListBox();
            UpdateListBox();
        }

        private void ClearListBox()
        {
            ListBoxExclude.Items.Clear();
        }

        private void BoxExclude_Drop(object sender, DragEventArgs e)
        {

        }

        private void ComboBoxFileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
}
