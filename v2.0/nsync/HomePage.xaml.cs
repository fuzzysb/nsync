using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Management;
using System.Runtime.InteropServices;

namespace nsync
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        #region Class Variables
        private string previousTextLeft;
        private ImageSource previousImageLeft;
        private string previousTextRight;
        private ImageSource previousImageRight;
        private bool hasLeftPath = false;
        private bool hasRightPath = false;
        private string settingsFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + nsync.Properties.Resources.settingsFilePath;
        private SyncEngine synchronizer;
        private TrackBackEngine trackback;
        private LinearGradientBrush blankOpacityMask;
        private HelperManager helper;
        private Window mainWindow = Application.Current.MainWindow;
        private Settings settingsManager;
        private string actualLeftPath;
        private string actualRightPath;
        private string oldLeftPath;
        private string oldRightPath;
        private bool isInterfaceEnabled;
        private string errorDirectory = null;
        private bool isErrorClosing = false;
        
        private string[] originalFolderPaths;

        private string NULL_STRING = nsync.Properties.Resources.nullString;
        private string ICON_LINK_REMOVABLE_DRIVE = nsync.Properties.Resources.thumbdriveIconPath;
        private string ICON_LINK_FOLDER = nsync.Properties.Resources.folderIconPath;
        private string ICON_LINK_FOLDER_MISSING = nsync.Properties.Resources.folderMissingIconPath;

        private string MESSAGE_ACCESS_DENIED_ERROR = "Access is denied. ";
        private string MESSAGE_SYNC_COMPLETED = "Sync completed";
        private string MESSAGE_ERROR_DETECTED = "Error detected";
        private string MESSAGE_SYNCING_FOLDERS = "Syncing folders...";
        private string MESSAGE_PREPARING_FOLDERS = "Preparing folders...";
        private string MESSAGE_SYNC_TERMINATED = "Sync terminated";
        private string MESSAGE_BACKING_UP_FOLDERS = "Backing up folders...";

        private int HELPER_WINDOW_HIGH_PRIORITY = 0;
        private int HELPER_WINDOW_LOW_PRIORITY = 1;
        private int HELPER_WINDOW_SYNC_COMPLETE_PRIORITY = -1;

        private List<FileData> fileData;
        private Preview previewSync;
        private SummaryReport summaryReport;
        private ExcludeWindow excludeWindow;
        private ExcludeData excludeData = new ExcludeData();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for HomePage class
        /// </summary>
        public HomePage()
        {
            InitializeComponent();

            // Initialise Helper
            helper = new HelperManager(mainWindow);

            // Get the settings class instance
            settingsManager = Settings.Instance;
            settingsManager.SetHomePage(this);
            
            mainWindow.Closing += new CancelEventHandler(mainWindow_Closing);

            actualLeftPath = nsync.Properties.Resources.panelText;
            actualRightPath = nsync.Properties.Resources.panelText;
        }

        /// <summary>
        /// Sets isErrorClosing to true when there is error
        /// </summary>
        public void IsErrorClosing()
        {
            isErrorClosing = true;
        }
        #endregion

        #region HomePage Controls
        /// <summary>
        /// This method will be called when HomePage is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Create blank opacity mask
            blankOpacityMask = new LinearGradientBrush();
            blankOpacityMask.GradientStops.Add(new GradientStop(Colors.Transparent, 0));
            blankOpacityMask.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
            ImageTeam14Over.OpacityMask = blankOpacityMask;

            //Create SyncEngine object
            synchronizer = new SyncEngine();
            trackback = new TrackBackEngine();

            // Initialize folder path array
            originalFolderPaths = new string[10];
            for (int i = 0; i < 10; i++)
            {
                originalFolderPaths[i] = "";
            }

            // Create event handlers for backgroundWorkerForSync
            synchronizer.backgroundWorkerForSync.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerForSync_RunWorkerCompleted);
            synchronizer.backgroundWorkerForSync.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerForSync_ProgressChanged);

            synchronizer.backgroundWorkerForPreSync.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerForPreSync_RunWorkerCompleted);

            trackback.backgroundWorkerForTrackBackBackup.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerForTrackBackBackup_RunWorkerCompleted);

            //Load the previous folder paths from settings.xml
            LoadFolderPaths();

            //Add event handler to check when main window is moved, move helper window too
            mainWindow.LocationChanged += new EventHandler(mainWindow_LocationChanged);
        }     

        /// <summary>
        /// This method will be called when the position of mainWindow is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_LocationChanged(object sender, EventArgs e)
        {
            helper.UpdateMove(); 
        }

        /// <summary>
        /// Update tooltips
        /// </summary>
        private void RefreshToolTips()
        {
            //update tooltips
            LeftText.ToolTip = actualLeftPath;
            RightText.ToolTip = actualRightPath;
        }

        /// <summary>
        /// This method is called when HomePage is unloaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveFolderPaths();
            helper.CloseWindow();
        }

        /// <summary>
        /// This method is called when nsync is exited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!isErrorClosing)
                SaveFolderPaths();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method resolves a shortcut link
        /// </summary>
        /// <param name="path">This parameter is a string which is the shortcut path to be resolved</param>
        /// <returns>Returns a string which contains the path of the resolved shortcut</returns>
        private string ResolveShortcut(string path)
        {
            IWshRuntimeLibrary.IWshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(path);
            return shortcut.TargetPath;
        }

        /// <summary>
        /// This method checks if the path is a shortcut link
        /// </summary>
        /// <param name="path">This parameter is the path to be checked</param>
        /// <returns>Returns a boolean which indicates whether the path is a shortcut link</returns>
        private bool IsPathShortcut(string path)
        {
            // To handle the case when user drag in C:\ shortcut
            if (path.Length < 4) 
                return false;

            if (path.Substring(path.Length - 4) == ".lnk")
                return true;
            else
                return false;
        }

        /// <summary>
        /// This method is called when user drag and drop something into the left box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxLeft_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                hasLeftPath = true;
                e.Handled = true;

                SyncToTheSameFolderHierarchy("left");
                RememberLastRemoveableDiskSync("left");
                ShowSync();
            }
        }

        /// <summary>
        /// This method is called when user drag and drop something into the right box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxRight_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                hasRightPath = true;
                e.Handled = true;

                SyncToTheSameFolderHierarchy("right");
                RememberLastRemoveableDiskSync("right");
                ShowSync();
            }
        }

        /// <summary>
        /// This method is called when user drag, but did not drop, something into the left box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxLeft_DragEnter(object sender, DragEventArgs e)
        {
            if (hasLeftPath && hasRightPath) SaveCurrentFolderPair();

            previousImageLeft = LeftIcon.Source;
            previousTextLeft = actualLeftPath;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (string i in fileNames)
                {
                    string actualPath = i;
                    if (IsPathShortcut(i))
                        actualPath = ResolveShortcut(i);

                    DirectoryInfo dirTemp = new DirectoryInfo(actualPath);
                    FileInfo fileTemp = new FileInfo(actualPath);
                    if (dirTemp.Exists)
                    {
                        actualLeftPath = actualPath;
                        LeftText.Text = ShortenPath(actualLeftPath, 90);
                    }
                    else
                    {
                        actualLeftPath = fileTemp.DirectoryName;
                        LeftText.Text = ShortenPath(actualLeftPath, 90);
                    }
                }

                synchronizer.LeftPath = actualLeftPath;

                DisplayCorrectIcons();
            }
        }

        /// <summary>
        /// This method is called when user drag, but did not drop, something into the right box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxRight_DragEnter(object sender, DragEventArgs e)
        {
            if (hasLeftPath && hasRightPath) SaveCurrentFolderPair();

            previousImageRight = RightIcon.Source;
            previousTextRight = actualRightPath;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (string i in fileNames)
                {
                    string actualPath = i;
                    if (IsPathShortcut(i))
                        actualPath = ResolveShortcut(i);

                    DirectoryInfo dirTemp = new DirectoryInfo(actualPath);
                    FileInfo fileTemp = new FileInfo(actualPath);
                    if (dirTemp.Exists)
                    {
                        actualRightPath = actualPath;
                        RightText.Text = ShortenPath(actualRightPath, 90);
                    }
                    else
                    {
                        actualRightPath = fileTemp.DirectoryName;
                        RightText.Text = ShortenPath(actualRightPath, 90);
                    }
                }

                synchronizer.RightPath = actualRightPath;

                DisplayCorrectIcons();
            }
        }

        /// <summary>
        /// This method is called when user drag, but did not drop, and instead drag out of the right box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxRight_DragLeave(object sender, DragEventArgs e)
        {
            actualRightPath = previousTextRight;
            RightText.Text = ShortenPath(actualRightPath, 90);
            synchronizer.RightPath = actualRightPath;
            RightIcon.Source = previousImageRight;

            if (hasLeftPath && hasRightPath) SaveCurrentFolderPair();
        }

        /// <summary>
        /// This method is called when user drag, but did not drop, and instead drag out of the left box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxLeft_DragLeave(object sender, DragEventArgs e)
        {
            actualLeftPath = previousTextLeft;
            LeftText.Text = ShortenPath(actualLeftPath, 90);
            synchronizer.LeftPath = actualLeftPath;
            LeftIcon.Source = previousImageLeft;

            if (hasLeftPath && hasRightPath) SaveCurrentFolderPair();
        }

        /// <summary>
        /// This method is called when user clicks on the left box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string currentPath = NULL_STRING;
            if (hasLeftPath)
            {
                currentPath = actualLeftPath;
                SaveCurrentFolderPair();
            }

            string directoryPath = FolderSelect(currentPath);
            if (directoryPath != actualLeftPath)
            {
                if (directoryPath != NULL_STRING)
                {
                    actualLeftPath = directoryPath;
                    LeftText.Text = ShortenPath(actualLeftPath, 90);
                    hasLeftPath = true;

                    SyncToTheSameFolderHierarchy("left");
                    RememberLastRemoveableDiskSync("left");
                }
            }

            DisplayCorrectIcons();

            synchronizer.LeftPath = actualLeftPath;
            synchronizer.RightPath = actualRightPath;

            ShowSync();
        }

        /// <summary>
        /// This method is called when user clicks on the right box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string currentPath = NULL_STRING;
            if (hasRightPath)
            {
                currentPath = actualRightPath;
                SaveCurrentFolderPair();
            }
            string directoryPath = FolderSelect(currentPath);
            if (directoryPath != actualRightPath)
            {
                if (directoryPath != NULL_STRING)
                {
                    actualRightPath = directoryPath;
                    RightText.Text = ShortenPath(actualRightPath, 90);
                    hasRightPath = true;

                    SyncToTheSameFolderHierarchy("right");
                    RememberLastRemoveableDiskSync("right");
                }
            }

            DisplayCorrectIcons();

            synchronizer.LeftPath = actualLeftPath;
            synchronizer.RightPath = actualRightPath;

            ShowSync();
        }

        /// <summary>
        /// Opens the browser dialog for user to choose a folder path
        /// </summary>
        /// <param name="originalPath">This parameter provides the starting point for the browser dialog</param>
        /// <returns>Returns the selected folder path from the browser dialog</returns>
        private string FolderSelect(string originalPath)
        {
            System.Windows.Forms.FolderBrowserDialog FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            FolderDialog.Description = nsync.Properties.Resources.folderExplorerText;

            if (originalPath != NULL_STRING)
            {
                FolderDialog.SelectedPath = originalPath;
            }
            FolderDialog.ShowDialog();
            return FolderDialog.SelectedPath;
        }

        /// <summary>
        /// This method is called when the mouse pointer enters leftbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            if (BarMRULeft.IsEnabled == true)
            {
                BarMRULeft.Visibility = Visibility.Visible;
                BarMRURight.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// This method is called when the mouse pointer leaves leftbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxLeft_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!LeftListBox.IsVisible)
            {
                BarMRULeft.Visibility = Visibility.Hidden;
                BarMRURight.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// This method is called when the mouse pointer enters left MRU bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarMRULeft_MouseEnter(object sender, MouseEventArgs e)
        {
                BarMRULeft.Opacity = 0.5;
                BarMRULeft.Cursor = Cursors.Hand;
                BarMRURight.Opacity = 0.5;
        }

        /// <summary>
        /// This method is called when the mouse pointer leaves left MRU bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarMRULeft_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!LeftListBox.IsVisible)
            {
                BarMRULeft.Opacity = 0.2;
                BarMRURight.Opacity = 0.2;
            }
        }

        /// <summary>
        /// This method is called when the mouse pointer enters rightbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxRight_MouseEnter(object sender, MouseEventArgs e)
        {
            if (BarMRURight.IsEnabled == true)
            {
                BarMRURight.Visibility = Visibility.Visible;
                BarMRULeft.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// This method is called when the mouse pointer leaves rightbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxRight_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!RightListBox.IsVisible)
            {
                BarMRURight.Visibility = Visibility.Hidden;
                BarMRULeft.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// This method is called when the mouse pointer enters right MRU bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarMRURight_MouseEnter(object sender, MouseEventArgs e)
        {
                BarMRURight.Opacity = 0.5;
                BarMRURight.Cursor = Cursors.Hand;
                BarMRULeft.Opacity = 0.5;
        }

        /// <summary>
        /// This method is called when the mouse pointer leaves right MRU bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarMRURight_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!RightListBox.IsVisible)
            {
                BarMRURight.Opacity = 0.2;
                BarMRULeft.Opacity = 0.2;
            }
        }

        /// <summary>
        /// This method is called when user clicks on the right MRU bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarMRURight_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (RightListBox.IsVisible)
            {
                RightListBox.Visibility = Visibility.Hidden;
                LeftListBox.Visibility = Visibility.Hidden;
            }
            else
            {
                RightListBox.Visibility = Visibility.Visible;
                LeftListBox.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// This method is called when user clicks on the left MRU bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarMRULeft_MouseUp(object sender, MouseButtonEventArgs e)
        {            
            if (LeftListBox.IsVisible)
            {
                RightListBox.Visibility = Visibility.Hidden;
                LeftListBox.Visibility = Visibility.Hidden;
            }
            else
            {
                RightListBox.Visibility = Visibility.Visible;
                LeftListBox.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Change the leftbox/rightbox icon if folder path is a removeable drive
        /// </summary>
        /// <param name="path"></param>
        /// <param name="leftOrRight"></param>
        private void ShowRemovableDrives(string path, string leftOrRight)
        {
            if (synchronizer.IsPathRemovableDisk(path))
            {
                if (leftOrRight == "left" || leftOrRight == "Left")
                {
                    try
                    {
                        LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_REMOVABLE_DRIVE));
                    }
                    catch (Exception exceptionError)
                    {
                        helper.Show(exceptionError.Message, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    }
                }
                else if (leftOrRight == "right" || leftOrRight == "Right")
                {
                    try
                    {
                        RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_REMOVABLE_DRIVE));
                    }
                    catch (Exception exceptionError)
                    {
                        helper.Show(exceptionError.Message, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if folder paths exist
        /// </summary>
        /// <returns>Return a boolean to determine if folder paths exist</returns>
        private bool IsFolderExist()
        {
            bool rightFolderExists = synchronizer.IsFolderExists("right");
            bool leftFolderExists = synchronizer.IsFolderExists("left");

            DisplayCorrectIcons();

            if (!rightFolderExists && !leftFolderExists)
            {
                helper.Show(nsync.Properties.Resources.bothFoldersNotExist, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                return false;
            }
            else if (!rightFolderExists)
            {
                helper.Show(nsync.Properties.Resources.rightFolderNotExist, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                return false;
            }
            else if (!leftFolderExists)
            {
                helper.Show(nsync.Properties.Resources.leftFolderNotExist, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks if folders are similar
        /// </summary>
        /// <returns>Return a boolean to determine if folder paths are similar</returns>
        private bool IsFoldersSimilar()
        {
            if (synchronizer.IsFoldersSimilar())
            {
                helper.Show(nsync.Properties.Resources.similarFolders, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if one folder is a subfolder of another
        /// </summary>
        /// <returns>Return a boolean to determine if one folder is subfolder of another</returns>
        private bool IsFolderSubfolder()
        {
            if (synchronizer.IsFolderSubfolder())
            {
                helper.Show(nsync.Properties.Resources.subfolderOfFolder, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the sync button should appear
        /// </summary>
        /// <returns>Return a boolean to determine if sync button should appear</returns>
        private bool ShowSync()
        {
            RefreshToolTips();

            LabelProgress.Visibility = Visibility.Hidden;
            LabelProgressPercent.Visibility = Visibility.Hidden;

            DisplayCorrectIcons();

            // Only if both boxes are filled with folder paths, then we need to check validity
            if (!hasLeftPath || !hasRightPath)
                return false;
                
            // Do a series of validity testing to check if the sync + preview buttons should appear
            if (!IsFolderExist() || IsFoldersSimilar() || IsFolderSubfolder())
            {
                DisplayCorrectIcons();
                ButtonSync.Visibility = Visibility.Hidden;
                ButtonPreview.Visibility = Visibility.Hidden;
                return false;
            }

            DisplayCorrectIcons();
            ButtonSync.Visibility = Visibility.Visible;
            ButtonPreview.Visibility = Visibility.Visible;
            return true;
        }

        /// <summary>
        /// Saves folder paths to settings.xml
        /// </summary>
        private void SaveFolderPaths()
        {
            if (hasLeftPath && hasRightPath)
            {
                settingsManager.ExcludedData = excludeData;
                settingsManager.SaveFolderPaths(actualLeftPath, actualRightPath);
                actualLeftPath = actualRightPath = NULL_STRING;
                hasLeftPath = hasRightPath = false;
            }
            else
                return;
        }

        /// <summary>
        /// Reload folder paths on MRU list
        /// </summary>
        private void ReloadFolderPaths()
        {
            LeftListBox.Items.Clear();
            RightListBox.Items.Clear();
            LoadFolderPaths();
        }

        /// <summary>
        /// Load folder paths from settings.xml
        /// </summary>
        private void LoadFolderPaths()
        {
            List<string> folderPaths = settingsManager.LoadFolderPaths();
            int counter;

            // Do nothing if settings.xml does not exists
            if (folderPaths.Count == 0)
                return;

            // Setup folder paths in SyncEngine and in LeftText+RightText
            actualLeftPath = folderPaths[0];
            LeftText.Text = ShortenPath(actualLeftPath, 90);
            synchronizer.LeftPath = actualLeftPath;
            actualRightPath = folderPaths[1];
            RightText.Text = ShortenPath(actualRightPath, 90);
            synchronizer.RightPath = actualRightPath;

            if (actualLeftPath == NULL_STRING)
            {
                actualLeftPath = nsync.Properties.Resources.panelText;
                LeftText.Text = nsync.Properties.Resources.panelText;
                hasLeftPath = false;
            }
            else
                hasLeftPath = true;

            if (actualRightPath == NULL_STRING)
            {
                actualRightPath = nsync.Properties.Resources.panelText;
                RightText.Text = nsync.Properties.Resources.panelText;
                hasRightPath = false;
            }
            else
                hasRightPath = true;
                
            // If first pair of folder paths in settings.xml is already empty, it's guranteed settings.xml is empty. No point trying to load MRU
            if (LeftText.Text == nsync.Properties.Resources.panelText && RightText.Text == nsync.Properties.Resources.panelText)
                return;

            counter = 0;
            // Setup MRU listbox items
            for (int i = 1; i <= 5; i++)
            {
                ListBoxItem listBoxLeft = new ListBoxItem();
                if (folderPaths[i + (i-2)] == NULL_STRING)
                    continue;

                listBoxLeft.Content = ShortenPath(folderPaths[i + (i-2)], 40);
                listBoxLeft.ToolTip = folderPaths[i + (i-2)];

                originalFolderPaths[counter] = folderPaths[i + (i-2)];
                counter += 2;

                listBoxLeft.MouseUp += new MouseButtonEventHandler(ListBoxLeft_MouseUp);
                listBoxLeft.MouseEnter += new MouseEventHandler(listBoxLeft_MouseEnter);
                listBoxLeft.MouseLeave += new MouseEventHandler(listBoxLeft_MouseLeave);
                listBoxLeft.Tag = i;

                LeftListBox.Items.Add(listBoxLeft);
            }
            counter = 1;

            for (int i = 1; i <= 5; i++)
            {
                ListBoxItem listBoxRight = new ListBoxItem();
                if (folderPaths[i + (i-1)] == NULL_STRING)
                    continue;

                listBoxRight.Content = ShortenPath(folderPaths[i + (i-1)], 40);
                listBoxRight.ToolTip = folderPaths[i + (i-1)];

                originalFolderPaths[counter] = folderPaths[i + (i-1)];
                counter += 2;

                listBoxRight.MouseUp += new MouseButtonEventHandler(ListBoxRight_MouseUp);
                listBoxRight.MouseEnter += new MouseEventHandler(listBoxRight_MouseEnter);
                listBoxRight.MouseLeave += new MouseEventHandler(listBoxRight_MouseLeave);
                listBoxRight.Tag = i;

                RightListBox.Items.Add(listBoxRight);
            }

            ShowSync();
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
        /// Shortens folder path for MRU list
        /// </summary>
        /// <param name="oldPath">The path that is to be shortened is passed in</param>
        /// <param name="maxLength">The maximum length to allow for the path</param>
        /// <returns>A string containing the new folder path is returned</returns>
        private string ShortenPath(string oldPath, int maxLength)
        {
            StringBuilder sb = new StringBuilder();
            PathCompactPathEx(sb, oldPath, maxLength, 0);
            return sb.ToString();
        }

        /// <summary>
        /// This method is called when mouse pointer leaves right listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxRight_MouseLeave(object sender, MouseEventArgs e)
        {
            LeftListBox.SelectedIndex = -1;
        }

        /// <summary>
        /// This method is called when mouse pointer leaves left listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxLeft_MouseLeave(object sender, MouseEventArgs e)
        {
            RightListBox.SelectedIndex = -1;
        }

        /// <summary>
        /// This method is called when mouse pointer enters left listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            ListBoxItem lb = new ListBoxItem();
            lb = (ListBoxItem)sender;
            int index = Convert.ToInt32(lb.Tag);
            RightListBox.SelectedIndex = index - 1;
        }

        /// <summary>
        /// This method is called when mouse pointer enters right listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxRight_MouseEnter(object sender, MouseEventArgs e)
        {
            ListBoxItem lb = new ListBoxItem();
            lb = (ListBoxItem)sender;
            int index = Convert.ToInt32(lb.Tag);
            LeftListBox.SelectedIndex = index - 1;
        }

        /// <summary>
        /// This method is called when user click on left listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxLeft_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Update left and right path with the newly selected paths
            // Left
            ListBoxItem lb = new ListBoxItem();
            lb = (ListBoxItem)e.Source;
            actualLeftPath = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 2)];
            LeftText.Text = ShortenPath(actualLeftPath, 90);
            // Right
            int index = Convert.ToInt32(lb.Tag);
            RightListBox.SelectedIndex = index - 1;
            lb = (ListBoxItem)RightListBox.SelectedItem;
            actualRightPath = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 1)];
            RightText.Text = ShortenPath(actualRightPath, 90);

            // Update SyncEngine with the newly selected paths
            synchronizer.LeftPath = actualLeftPath;
            synchronizer.RightPath = actualRightPath;

            // After all's done, close the listbox
            LeftListBox.SelectedIndex = -1;
            LeftListBox.Visibility = Visibility.Hidden;
            RightListBox.Visibility = Visibility.Hidden;

            ShowSync();
        }

        /// <summary>
        /// This method is called when user click on right listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxRight_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Update left and right path with the newly selected paths
            // Right
            ListBoxItem lb = new ListBoxItem();
            lb = (ListBoxItem)e.Source;
            actualRightPath = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 1)];
            RightText.Text = ShortenPath(actualRightPath, 90);
            // Left
            int index = Convert.ToInt32(lb.Tag);
            LeftListBox.SelectedIndex = index - 1;
            lb = (ListBoxItem)LeftListBox.SelectedItem;
            actualLeftPath = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 2)];
            LeftText.Text = ShortenPath(actualLeftPath, 90);

            // Update SyncEngine with the newly selected paths
            synchronizer.LeftPath = actualLeftPath;
            synchronizer.RightPath = actualRightPath;

            // After all's done, close the listbox
            RightListBox.SelectedIndex = -1;
            LeftListBox.Visibility = Visibility.Hidden;
            RightListBox.Visibility = Visibility.Hidden;

            ShowSync();
        }

        /// <summary>
        /// This method is called when user click on the sync button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
            // do a check one more time
            // handle the situation when after a sync job is setup,
            // user deletes the 2 folders n click sync again
            if (!ShowSync())
                return;

            // Check if exclude window is enabled in settings
            int excludeWindowStatus = settingsManager.GetExcludeWindowStatus(); // 0 for disabled, 1 for enabled, -1 for error
            if (excludeWindowStatus == 1)
            {
                bool leftDirectoryAccessible = IsDirectoryAccessible(actualLeftPath);
                bool rightDirectoryAccessible = IsDirectoryAccessible(actualRightPath);
                if (leftDirectoryAccessible && rightDirectoryAccessible)
                {
                    EnableInterface(false);
                    excludeWindow = new ExcludeWindow();
                    excludeWindow.Closing += new CancelEventHandler(excludeWindow_Closing);
                    excludeWindow.LeftPath = actualLeftPath;
                    excludeWindow.RightPath = actualRightPath;
                    excludeWindow.Owner = mainWindow;
                    excludeWindow.LoadExcludeData();
                    excludeWindow.LogError += new ExcludeWindow.LogHandler(excludeWindow_LogError);
                    mainWindow.Opacity = 0.2;
                    excludeWindow.ShowDialog();
                }
                else
                {
                    string rightsString = nsync.Properties.Resources.accessRightsInsufficient;
                    if (!leftDirectoryAccessible)
                        rightsString += "\n" + ShortenPath(actualLeftPath,50);
                    if(!rightDirectoryAccessible)
                        rightsString += "\n" + ShortenPath(actualRightPath,50);
                    helper.Show(rightsString, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                }
            }
            else if (excludeWindowStatus == 0)
            {
                // Make necessary changes to UI to prepare for sync
                LeftListBox.Visibility = Visibility.Hidden;
                RightListBox.Visibility = Visibility.Hidden;
                LabelProgress.Visibility = Visibility.Visible;
                LabelProgress.Content = MESSAGE_PREPARING_FOLDERS;
                EnableInterface(false);

                // Feed the actualleftpath and actualrightpath into SyncEngine again
                // Safety precaution
                synchronizer.LeftPath = actualLeftPath;
                synchronizer.RightPath = actualRightPath;

                // Do PreSync Calculations: count how many changes need to be done
                // If not enough disk space, return
                // If enough, continue to start the real sync
                excludeData = new ExcludeData();
                synchronizer.ExcludeData = excludeData;
                synchronizer.PreSync();
            }
            else
            {
                //Do nothing if -1
            }
        }

        private bool IsDirectoryAccessible(string directoryPath)
        {
            try
            {
                Directory.GetFiles(directoryPath);
            }
            catch (System.UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        /// <summary>
        /// Error handler when exclude windows background worker encounters an error
        /// </summary>
        /// <param name="message"></param>
        private void excludeWindow_LogError(string message)
        {
            try
            {
                string[] directory = message.Split(new char[] { '\'' });
                if (directory[0] == "Access to the path ")
                {
                    errorDirectory = directory[1];
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// This method is called when exclude window closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void excludeWindow_Closing(object sender, CancelEventArgs e)
        {
            if (excludeWindow.Cancel == true)
            {
                mainWindow.Opacity = 1;
                EnableInterface(true);
                excludeData = new ExcludeData();
                excludeData.ExcludeFileNameList = excludeWindow.GetFileNameList();
                excludeData.ExcludeFileTypeList = excludeWindow.GetFileTypeList();
                excludeData.ExcludeFolderList = excludeWindow.GetFolderList();
            }
            else
            {
                // Make necessary changes to UI to prepare for sync
                mainWindow.Opacity = 1;
                LeftListBox.Visibility = Visibility.Hidden;
                RightListBox.Visibility = Visibility.Hidden;
                LabelProgress.Visibility = Visibility.Visible;
                LabelProgress.Content = MESSAGE_PREPARING_FOLDERS;
                EnableInterface(false);

                // Feed the actualleftpath and actualrightpath into SyncEngine again
                // Safety precaution
                synchronizer.LeftPath = actualLeftPath;
                synchronizer.RightPath = actualRightPath;

                // Do PreSync Calculations: count how many changes need to be done
                // If not enough disk space, return
                // If enough, continue to start the real sync
                excludeData = new ExcludeData();
                excludeData.ExcludeFileNameList = excludeWindow.GetFileNameList();
                excludeData.ExcludeFileTypeList = excludeWindow.GetFileTypeList();
                excludeData.ExcludeFolderList = excludeWindow.GetFolderList();
                synchronizer.ExcludeData = excludeData;
                synchronizer.PreSync();
            }
        }

        /// <summary>
        /// Revert both paths back to their original paths before they were changed
        /// </summary>
        private void RevertBackToOldFolderPair()
        {
            actualLeftPath = oldLeftPath;
            actualRightPath = oldRightPath;
            LeftText.Text = ShortenPath(actualLeftPath, 90);
            RightText.Text = ShortenPath(actualRightPath, 90);
            synchronizer.LeftPath = actualLeftPath;
            synchronizer.RightPath = actualRightPath;
        }

        /// <summary>
        /// Saves the current paths for later use
        /// </summary>
        private void SaveCurrentFolderPair()
        {
            oldLeftPath = actualLeftPath;
            oldRightPath = actualRightPath;
        }

        /// <summary>
        /// Try to change the 2 folder path to the appropriate folder hierarchy, if possible
        /// </summary>
        /// <param name="leftOrRight">This parameter indicates if the target path is leftPath or rightPath</param>
        private void SyncToTheSameFolderHierarchy(string leftOrRight)
        {
            string newTargetPath;
            if (leftOrRight == "left" || leftOrRight == "Left") 
                newTargetPath = actualLeftPath;
            else if (leftOrRight == "right" || leftOrRight == "Right") 
                newTargetPath = actualRightPath;
            else // any other weird input, reject
                return;

            // Gets the new folder path with the correct folder hierarchy
            string[] newFolderPaths = new string[2];
            string[] oldPath = new string[2];
            if (oldLeftPath == null || oldRightPath == null) 
                return;
            oldPath[0] = oldLeftPath;
            oldPath[1] = oldRightPath;
            newFolderPaths = synchronizer.SyncToTheSameFolderHierarchy(newTargetPath, oldPath, leftOrRight);
            
            SaveCurrentFolderPair();
            
            // Make changes to the paths using the new folder paths
            if (newFolderPaths == null || newFolderPaths.Length != 2)
                return;
            else
            {
                if (leftOrRight == "left" || leftOrRight == "Left")
                {
                    helper.IsRevertPathDialog = true;
                    helper.HyperTextMouseDown += new MouseButtonEventHandler(helper_HyperTextMouseDown);
                    helper.Show(nsync.Properties.Resources.modifiedRightPath, HELPER_WINDOW_LOW_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    helper.IsRevertPathDialog = false;
                }
                else if (leftOrRight == "right" || leftOrRight == "Right")
                {
                    helper.IsRevertPathDialog = true;
                    helper.HyperTextMouseDown += new MouseButtonEventHandler(helper_HyperTextMouseDown);
                    helper.Show(nsync.Properties.Resources.modifiedLeftPath, HELPER_WINDOW_LOW_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    helper.IsRevertPathDialog = false;
                }
                actualLeftPath = newFolderPaths[0];
                actualRightPath = newFolderPaths[1];
                LeftText.Text = ShortenPath(actualLeftPath, 90);
                RightText.Text = ShortenPath(actualRightPath, 90);
                synchronizer.LeftPath = actualLeftPath;
                synchronizer.RightPath = actualRightPath;
            }

            DisplayCorrectIcons();
        }

        /// <summary>
        /// This method is called when user clicks on the hyperlink to revert the original folder path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helper_HyperTextMouseDown(object sender, MouseButtonEventArgs e)
        {
            RevertBackToOldFolderPair();
        }

        /// <summary>
        /// Displays the correct images for the left and right box accordingly
        /// </summary>
        private void DisplayCorrectIcons()
        {
            // left box
            if (actualLeftPath != nsync.Properties.Resources.panelText)
            {
                if (!Directory.Exists(actualLeftPath))
                {
                    try
                    {
                        LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER_MISSING));
                    }
                    catch (Exception exceptionError)
                    {
                        helper.Show(exceptionError.Message, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    }
                }
                else
                {
                    try
                    {
                        LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
                    }
                    catch (Exception exceptionError)
                    {
                        helper.Show(exceptionError.Message, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    }
                    ShowRemovableDrives(actualLeftPath, "left");
                }
            }

            // right box
            if (actualRightPath != nsync.Properties.Resources.panelText)
            {
                if (!Directory.Exists(actualRightPath))
                {
                    try
                    {
                        RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER_MISSING));
                    }
                    catch (Exception exceptionError)
                    {
                        helper.Show(exceptionError.Message, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    }
                }
                else
                {
                    try
                    {
                        RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
                    }
                    catch (Exception exceptionError)
                    {
                        helper.Show(exceptionError.Message, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    }
                    ShowRemovableDrives(actualRightPath, "right");
                }
            }
        }

        /// <summary>
        /// Save the folder paths into settings.xml if the folder path involves a removeable disk
        /// </summary>
        private void SaveFolderPathsForRemoveableDisk()
        {
            // check if actualLeftPath and actualRightPath is removeable disk first
            bool isLeftPathRemoveableDisk, isRightPathRemoveableDisk;
            isLeftPathRemoveableDisk = synchronizer.IsPathRemovableDisk(Directory.GetDirectoryRoot(actualLeftPath));
            isRightPathRemoveableDisk = synchronizer.IsPathRemovableDisk(Directory.GetDirectoryRoot(actualRightPath));
            if (isLeftPathRemoveableDisk || isRightPathRemoveableDisk)
            {
                // if they are removeable disk, get their serial number
                // and ask settings class to save the folder pair
                if (isLeftPathRemoveableDisk)
                {
                    settingsManager.SaveFolderPathForRemoveableDisk(synchronizer.GetRemovableDiskSerialNumber(actualLeftPath), actualLeftPath, actualRightPath);
                }
                if(isRightPathRemoveableDisk)
                {
                    settingsManager.SaveFolderPathForRemoveableDisk(synchronizer.GetRemovableDiskSerialNumber(actualRightPath), actualLeftPath, actualRightPath);
                }
            }
        }

        /// <summary>
        /// Try to get and replace actualLeftPath and actualRightPath with the last synced folder pair for a removeable disk
        /// </summary>
        /// <param name="leftOrRight">This parameter indicates if the removeable disk is actualLeftPath or actualRightPath</param>
        private void RememberLastRemoveableDiskSync(string leftOrRight)
        {
            // The left and right path will be changed if it is a removeable disk
            // The new path will be the last sync folder path for that removeable disk
            // ( e.g. BEFORE actualLeftPath == F:\ --> AFTER actualLeftPath == F:\LastSyncFolder )
            string[] newPath = new string[2];
            string serialNumber;
            if (leftOrRight == "left" || leftOrRight == "Left")
            {
                if ((serialNumber = synchronizer.GetRemovableDiskSerialNumberWithChecks(actualLeftPath)) != null)
                {
                    if ((newPath = settingsManager.GetLastRemoveableDiskSync(serialNumber)) != null)
                    {
                        hasLeftPath = hasRightPath = true;
                        actualLeftPath = newPath[0];
                        LeftText.Text = ShortenPath(actualLeftPath, 90);
                        actualRightPath = newPath[1];
                        RightText.Text = ShortenPath(actualRightPath, 90);
                        synchronizer.LeftPath = actualLeftPath;
                        synchronizer.RightPath = actualRightPath;

                        helper.IsRevertPathDialog = true;
                        helper.HyperTextMouseDown += new MouseButtonEventHandler(helper_HyperTextMouseDown);
                        helper.Show(nsync.Properties.Resources.folderOnRemovableDiskRestored, HELPER_WINDOW_LOW_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        helper.IsRevertPathDialog = false;
                    }
                }
            }
            else if (leftOrRight == "right" || leftOrRight == "Right")
            {
                if ((serialNumber = synchronizer.GetRemovableDiskSerialNumberWithChecks(actualRightPath)) != null)
                {
                    if ((newPath = settingsManager.GetLastRemoveableDiskSync(serialNumber)) != null)
                    {
                        hasLeftPath = hasRightPath = true;
                        actualLeftPath = newPath[0];
                        LeftText.Text = ShortenPath(actualLeftPath, 90);
                        actualRightPath = newPath[1];
                        RightText.Text = ShortenPath(actualRightPath, 90);
                        synchronizer.LeftPath = actualLeftPath;
                        synchronizer.RightPath = actualRightPath;

                        helper.IsRevertPathDialog = true;
                        helper.HyperTextMouseDown += new MouseButtonEventHandler(helper_HyperTextMouseDown);
                        helper.Show(nsync.Properties.Resources.folderOnRemovableDiskRestored, HELPER_WINDOW_LOW_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        helper.IsRevertPathDialog = false;
                    }
                }
            }
            else
                return;
        }
        
        /// <summary>
        /// This method is called when progress percentage has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForSync_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            double percentage = (double)e.ProgressPercentage / 100;

            //Set team14 progress bar
            LinearGradientBrush opacityMask = new LinearGradientBrush();
            opacityMask.StartPoint = new Point(percentage, 0);
            opacityMask.EndPoint = new Point(1, 0);
            opacityMask.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 0, 0), 0));
            opacityMask.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 0.2));
            ImageTeam14Over.OpacityMask = opacityMask;

            LabelProgressPercent.Content = e.ProgressPercentage.ToString() + " %";
        }

        /// <summary>
        /// Enable or disable the user interface after and during synchronization
        /// </summary>
        /// <param name="enableOrDisable">This boolean determines whether to disable or enable the interface</param>
        private void EnableInterface(bool enableOrDisable)
        {
            double opacityValue;
            bool enableButtons;

            if (enableOrDisable)
            {
                opacityValue = 1;
                enableButtons = true;
                ButtonSync.Visibility = Visibility.Visible;
                ButtonPreview.IsEnabled = true;
                SyncingImage.Visibility = Visibility.Hidden;
                ButtonStop.Visibility = Visibility.Hidden;
                isInterfaceEnabled = true;
            }
            else
            {
                enableButtons = false;
                opacityValue = 0.5;
                ButtonSync.Visibility = Visibility.Hidden;
                ButtonPreview.IsEnabled = false;
                SyncingImage.Visibility = Visibility.Visible;
                isInterfaceEnabled = false;
            }

            //Enable/Disable the interface
            BoxLeft.IsEnabled = BoxRight.IsEnabled = ButtonSync.IsEnabled = enableButtons;
            BarMRULeft.IsEnabled = BarMRURight.IsEnabled = enableButtons;
            Button ButtonClose = (Button)mainWindow.FindName("ButtonClose");
            ButtonClose.IsEnabled = enableButtons;

            //Enable/Disable the scroller
            Button ButtonSideTabLeft = (Button)mainWindow.FindName("ButtonSideTabLeft");
            ButtonSideTabLeft.IsEnabled = enableButtons;
            Button ButtonSideTabRight = (Button)mainWindow.FindName("ButtonSideTabRight");
            ButtonSideTabRight.IsEnabled = enableButtons;

            //Enable/Disable the dotmenu
            Button ButtonPageSettings = (Button)mainWindow.FindName("ButtonPageSettings");
            ButtonPageSettings.IsEnabled = enableButtons;
            Button ButtonPageHome = (Button)mainWindow.FindName("ButtonPageHome");
            Button ButtonPageTrackBack = (Button)mainWindow.FindName("ButtonPageTrackBack");
            ButtonPageTrackBack.IsEnabled = enableButtons;

            //Set Opacity
            helper.HideWindow();
            BoxLeft.Opacity = BoxRight.Opacity = opacityValue;
            ButtonSideTabLeft.Opacity = ButtonSideTabRight.Opacity = opacityValue;
            ButtonPageSettings.Opacity = ButtonPageHome.Opacity = ButtonPageTrackBack.Opacity = opacityValue;
        }

        /// <summary>
        /// This method is called when when synchronization is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableInterface(true);

            // This condition eliminates the scenario when e.Result throws an exception when the background
            // worker encounters an error or gets cancelled
            if (e.Cancelled || e.Error != null)
            {
                // This condition is met when the background worker is cancelled
                if (e.Cancelled)
                {
                    helper.Show(nsync.Properties.Resources.syncTerminated, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    LabelProgress.Content = MESSAGE_SYNC_TERMINATED;
                    ButtonSync.Visibility = Visibility.Visible;
                    ButtonPreview.Visibility = Visibility.Visible;
                }
                // This condition is met when the background worker encounters an error
                else if (e.Error != null)
                {
                    helper.Show(nsync.Properties.Resources.defaultErrorMessage, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    LabelProgress.Content = MESSAGE_ERROR_DETECTED;
                    ButtonSync.Visibility = Visibility.Hidden;
                    ButtonPreview.Visibility = Visibility.Hidden;
                }
                LabelProgressPercent.Visibility = Visibility.Hidden;
                ImageTeam14Over.OpacityMask = blankOpacityMask;
                
                IsFolderExist();
                return;
            }
            else
            {
                if (!(bool)e.Result)
                {
                    helper.Show(nsync.Properties.Resources.insufficientDiskSpace, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    LabelProgress.Content = MESSAGE_ERROR_DETECTED;
                    LabelProgressPercent.Visibility = Visibility.Hidden;
                    ImageTeam14Over.OpacityMask = blankOpacityMask;
                    ButtonSync.Visibility = Visibility.Hidden;
                    ButtonPreview.Visibility = Visibility.Hidden;
                    IsFolderExist();
                    return;
                }

                if (synchronizer.IsFoldersSync())
                {
                    ImageTeam14Over.OpacityMask = blankOpacityMask;
                    
                    // Update MRU
                    SaveFolderPaths();
                    ReloadFolderPaths();

                    LabelProgress.Content = MESSAGE_SYNC_COMPLETED;
                    LabelProgressPercent.Content = "100 %";
                    helper.Show(nsync.Properties.Resources.synchronizedFolders, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    return;
                }

                previewSync = new Preview();
                previewSync.LeftPath = actualLeftPath;
                previewSync.RightPath = actualRightPath;
                // If filters were activated, let summary sync know as well.
                int excludeWindowStatus = settingsManager.GetExcludeWindowStatus();
                if (excludeWindowStatus == 1)
                {
                    previewSync.ExcludeData = excludeData;
                }
                else if (excludeWindowStatus == 0)
                {
                    excludeData = new ExcludeData();
                    previewSync.ExcludeData = excludeData;
                }

                previewSync.backgroundWorkerForSummary.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerForSummary_RunWorkerCompleted);
                previewSync.SummarySync(); 
            }

        }

        /// <summary>
        /// This method is called when when summary sychronization is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void backgroundWorkerForSummary_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            fileData = new List<FileData>();
            fileData = previewSync.GetData();

            // When all sync job done, save the folder pairs to MRU and settings.xml
            SaveFolderPaths();
            ReloadFolderPaths();

            // Update the UI
            LabelProgress.Content = MESSAGE_SYNC_COMPLETED;
            LabelProgressPercent.Content = "100 %";
            ImageTeam14Over.OpacityMask = blankOpacityMask;

            // Creation of summary report
            if (fileData.Count == 0)
            {
                summaryReport = new SummaryReport(true, synchronizer.ErrorMessageForSummaryReport);
                summaryReport.LeftPath = actualLeftPath;
                summaryReport.RightPath = actualRightPath;
                summaryReport.CreateLog();
            }
            else
            {
                summaryReport = new SummaryReport(fileData, synchronizer.ErrorMessageForSummaryReport);
                summaryReport.LeftPath = actualLeftPath;
                summaryReport.RightPath = actualRightPath;
                summaryReport.CreateLog();
            }

            // Display a notification to the user
            helper.ErrorCount = fileData.Count;
            helper.ConflictCount = synchronizer.ErrorMessageForSummaryReport.Count;
            helper.LogPath = summaryReport.LogPath;
            helper.Show(nsync.Properties.Resources.syncComplete, HELPER_WINDOW_SYNC_COMPLETE_PRIORITY, HelperWindow.windowStartPosition.windowTop);

            // Save folder pair if the sync involves a removeable disk
            SaveFolderPathsForRemoveableDisk();
        }

        /// <summary>
        /// This method is called when the TrackBack has finished backing up the folders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForTrackBackBackup_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((int) e.Result != 0)
            {
                EnableInterface(true);
                switch ((int) e.Result)
                {
                    case 1:
                        helper.Show(nsync.Properties.Resources.leftFolderRestrictedAccess, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case -1:
                        helper.Show(nsync.Properties.Resources.rightFolderRestrictedAccess, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 2:
                        helper.Show(nsync.Properties.Resources.leftFolderNotExistsForTrackBack, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case -2:
                        helper.Show(nsync.Properties.Resources.rightFolderNotExistsForTrackBack, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 3:
                        helper.Show(nsync.Properties.Resources.leftFolderPathTooLong, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case -3:
                        helper.Show(nsync.Properties.Resources.rightFolderPathTooLong, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 4:
                        helper.Show(nsync.Properties.Resources.leftFolderIOException, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case -4:
                        helper.Show(nsync.Properties.Resources.rightFolderIOException, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    default:
                        helper.Show(nsync.Properties.Resources.defaultErrorMessage, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                }
                
                // Unsuccessful sync jobs folders will be saved in MRU
                // This is to give users convenience so that
                // They can just click on MRU to retrieve the folder pairs again
                SaveFolderPaths();
                ReloadFolderPaths();

                LabelProgress.Visibility = Visibility.Visible;
                LabelProgress.Content = MESSAGE_ERROR_DETECTED;
                LabelProgressPercent.Visibility = Visibility.Hidden;
                // Users should be able click the sync button again,
                // in case they've freed some disk space
                ButtonSync.Visibility = Visibility.Visible;
                ButtonPreview.Visibility = Visibility.Visible;
                return;
            }

            // If trackback backups was successful, continue with sync job
            // Update UI
            EnableInterface(false);
            LabelProgress.Content = MESSAGE_SYNCING_FOLDERS;
            LabelProgressPercent.Visibility = Visibility.Visible;
            LabelProgressPercent.Content = "0 %";

            synchronizer.StartSync();
        }

        /// <summary>
        /// This method is called when presync calculations are completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForPreSync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((int)e.Result > 0)
            {
                EnableInterface(true);
                switch ((int)e.Result)
                {
                    case 1:
                        helper.Show(nsync.Properties.Resources.rightFolderInsufficientDiskSpace, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 2:
                        helper.Show(nsync.Properties.Resources.leftFolderInsufficientDiskSpace, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 3:
                        string extraMessage = null;
                        if (errorDirectory != null)
                        {
                            extraMessage = "\nError in " + errorDirectory;
                        }
                        helper.Show(nsync.Properties.Resources.accessRightsInsufficient + extraMessage, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    case 4:
                        helper.Show(nsync.Properties.Resources.fileNotAccessible, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                    default:
                        helper.Show(nsync.Properties.Resources.defaultErrorMessage, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        break;
                }

                // Unsuccessful sync jobs folders will be saved in MRU
                // This is to give users convenience so that
                // They can just click on MRU to retrieve the folder pairs again
                SaveFolderPaths();
                ReloadFolderPaths();

                LabelProgress.Visibility = Visibility.Visible;
                LabelProgress.Content = MESSAGE_ERROR_DETECTED;
                LabelProgressPercent.Visibility = Visibility.Hidden;
                // Users should be able click the sync button again,
                // in case they've freed some disk space
                ButtonSync.Visibility = Visibility.Visible;
                ButtonPreview.Visibility = Visibility.Visible;
                //FolderCheck();
                return;
            }
            if ((!synchronizer.IsFoldersSync()) && (settingsManager.GetTrackBackStatus() == 1))
            {
                trackback.LeftFolderPath = actualLeftPath;
                trackback.RightFolderPath = actualRightPath;

                EnableInterface(false);

                LabelProgress.Content = MESSAGE_BACKING_UP_FOLDERS;

                try
                {
                    if (trackback.hasEnoughDiskSpaceInLeftFolder() && trackback.hasEnoughDiskSpaceInRightFolder() &&
                                        synchronizer.hasEnoughSpaceInLeftFolder() && synchronizer.hasEnoughSpaceInRightFolder())
                        trackback.StartBackup();
                    else
                    {
                        EnableInterface(true);
                        LabelProgress.Visibility = Visibility.Visible;
                        LabelProgress.Content = MESSAGE_ERROR_DETECTED;
                        helper.Show((!(trackback.hasEnoughDiskSpaceInLeftFolder() && synchronizer.hasEnoughSpaceInLeftFolder())) ? nsync.Properties.Resources.leftFolderInsufficientDiskSpace :
                            nsync.Properties.Resources.rightFolderInsufficientDiskSpace, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                        return;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    EnableInterface(true);
                    LabelProgress.Visibility = Visibility.Visible;
                    LabelProgress.Content = MESSAGE_ERROR_DETECTED;
                    LabelProgressPercent.Visibility = Visibility.Hidden;
                    helper.Show(nsync.Properties.Resources.accessRightsInsufficient, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                    return;
                }
            }
            else
            {
                EnableInterface(false);
                LabelProgress.Content = MESSAGE_SYNCING_FOLDERS;
                LabelProgressPercent.Visibility = Visibility.Visible;
                LabelProgressPercent.Content = "0 %";

                synchronizer.StartSync();
            }
        }

        /// <summary>
        /// This method is called when the preview button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!ShowSync())
                return;

            if (!settingsManager.IsFoldersLocked())
            {
                EnableInterface(false);
                previewSync = new Preview();
                previewSync.LeftPath = actualLeftPath;
                previewSync.RightPath = actualRightPath;
                previewSync.ExcludeData = settingsManager.LoadExcludeData(actualLeftPath, actualRightPath);
                previewSync.backgroundWorkerForPreview.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerForPreview_RunWorkerCompleted);
                previewSync.PreviewSync();

                // Updates UI
                LabelProgress.Visibility = Visibility.Visible;
                LabelProgress.Content = MESSAGE_PREPARING_FOLDERS;
            }
        }

        /// <summary>
        /// This method is called when when preview sychronization is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerForPreview_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string message = e.Error.Message.Remove(e.Error.Message.IndexOf("("));

                LabelProgress.Content = MESSAGE_ERROR_DETECTED;
                EnableInterface(true);

                if (message == MESSAGE_ACCESS_DENIED_ERROR)
                {
                    helper.Show(nsync.Properties.Resources.accessRightsInsufficient, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                }
                else
                {
                    helper.Show(nsync.Properties.Resources.defaultErrorMessage, HELPER_WINDOW_HIGH_PRIORITY, HelperWindow.windowStartPosition.windowTop);
                }
            }
            else
            {
                LabelProgress.Visibility = Visibility.Hidden;
                fileData = new List<FileData>();
                fileData = previewSync.GetData();

                VisualPreviewWindow WindowVisualPreview = new VisualPreviewWindow();
                WindowVisualPreview.Closing += new CancelEventHandler(WindowVisualPreview_Closing);
                WindowVisualPreview.Owner = mainWindow;
                WindowVisualPreview.LeftPath = actualLeftPath;
                WindowVisualPreview.RightPath = actualRightPath;
                WindowVisualPreview.PreviewFileData = fileData;
                mainWindow.Opacity = 0.2;
                WindowVisualPreview.ShowDialog();
                mainWindow.Opacity = 1;
            }
        }

        /// <summary>
        /// This method is called when preview window closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowVisualPreview_Closing(object sender, CancelEventArgs e)
        {
            EnableInterface(true);
        }

        /// <summary>
        /// This method is called when user points the mouse pointer on the rotating sync image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncingImage_MouseEnter(object sender, MouseEventArgs e)
        {
            // Only enable the Cancel function if nsync is syncing folders
            if (!isInterfaceEnabled && (string) LabelProgress.Content == MESSAGE_SYNCING_FOLDERS)
            {
                ButtonStop.Visibility = Visibility.Visible;
                SyncingImage.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// This method is called when user moves the mouse pointer outside of the rotating sync image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStop_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isInterfaceEnabled)
            {
                ButtonStop.Visibility = Visibility.Hidden;
                SyncingImage.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// This method is called when user clicks on the stop sync button during sync
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            synchronizer.backgroundWorkerForSync.CancelAsync();
        }
        #endregion
    }
}
