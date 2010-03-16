using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Management;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Xml;

namespace nsync
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private string previousTextLeft;
        private ImageSource previousImageLeft;
        private string previousTextRight;
        private ImageSource previousImageRight;
        private bool hasLeftPath = false;
        private bool hasRightPath = false;
        private string settingsFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + nsync.Properties.Resources.settingsFilePath;
        private SyncEngine synchronizer;
        //private TrackBackEngine trackBack;
        private LinearGradientBrush blankOpacityMask;
        private HelperManager helper;
        private Window mainWindow = Application.Current.MainWindow;
        private Settings settingsManager;
        private string actualLeftPath;
        private string actualRightPath;
        
        private string[] originalFolderPaths;

        private string NULL_STRING = nsync.Properties.Resources.nullString;
        private string ICON_LINK_REMOVABLE_DRIVE = nsync.Properties.Resources.thumbdriveIconPath;
        private string ICON_LINK_FOLDER = nsync.Properties.Resources.folderIconPath;
        private string ICON_LINK_FOLDER_MISSING = nsync.Properties.Resources.folderMissingIconPath;

        /// <summary>
        /// Constructor for HomePage class
        /// </summary>
        public HomePage()
        {
            InitializeComponent();

            //Initialise Helper
            helper = new HelperManager(mainWindow);

            settingsManager = Settings.Instance;
            
            mainWindow.Closing += new CancelEventHandler(mainWindow_Closing);
        }

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

            // Initialize folder path array
            originalFolderPaths = new string[10];
            for (int i = 0; i < 10; i++)
            {
                originalFolderPaths[i] = "";
            }

            //trackBack = new TrackBackEngine();

            // Create event handlers for backgroundWorker
            synchronizer.backgroundWorkerForSync.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            synchronizer.backgroundWorkerForSync.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);

            synchronizer.backgroundWorkerForPreSync.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker2_RunWorkerCompleted);

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
        void mainWindow_LocationChanged(object sender, EventArgs e)
        {
            helper.UpdateMove(); 
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
        void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveFolderPaths();
        }

        /// <summary>
        /// This method is called when user drag and drop something into the left box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxLeft_Drop(object sender, DragEventArgs e)
        {
            hasLeftPath = true;
            e.Handled = true;
            ShowSync();
        }

        /// <summary>
        /// This method is called when user drag and drop something into the right box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxRight_Drop(object sender, DragEventArgs e)
        {
            hasRightPath = true;
            e.Handled = true;
            ShowSync();
        }

        /// <summary>
        /// This method is called when user drag, but did not drop, something into the left box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxLeft_DragEnter(object sender, DragEventArgs e)
        {
            previousImageLeft = LeftIcon.Source;
            //SQ previousTextLeft = LeftText.Text;
            previousTextLeft = actualLeftPath; //SQ
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (string i in fileNames)
                {
                    DirectoryInfo dirTemp = new DirectoryInfo(i);
                    FileInfo fileTemp = new FileInfo(i);
                    if (dirTemp.Exists)
                    {
                        //SQ LeftText.Text = i;
                        actualLeftPath = i; //SQ
                        LeftText.Text = ShortenText(actualLeftPath); //SQ
                    }
                    else
                    {
                        actualLeftPath = fileTemp.DirectoryName; //SQ
                        //SQ LeftText.Text = fileTemp.DirectoryName;
                        LeftText.Text = ShortenText(actualLeftPath); //SQ
                    }
                }
            }
            //SQ synchronizer.LeftPath = LeftText.Text;
            synchronizer.LeftPath = actualLeftPath; //SQ

            LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
            //SQ ShowRemovableDrives(LeftText.Text, "left");
            ShowRemovableDrives(actualLeftPath, "left"); //SQ
        }

        /// <summary>
        /// This method is called when user drag, but did not drop, something into the right box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxRight_DragEnter(object sender, DragEventArgs e)
        {
            previousImageRight = RightIcon.Source;
            //SQ previousTextRight = RightText.Text;
            previousTextRight = actualRightPath; //SQ

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                foreach (string i in fileNames)
                {
                    DirectoryInfo dirTemp = new DirectoryInfo(i);
                    FileInfo fileTemp = new FileInfo(i);
                    if (dirTemp.Exists)
                    {
                        //SQ RightText.Text = i;
                        actualRightPath = i; //SQ
                        RightText.Text = ShortenText(actualRightPath); //SQ
                    }
                    else
                    {
                        //SQ RightText.Text = fileTemp.DirectoryName;
                        actualRightPath = fileTemp.DirectoryName; //SQ
                        RightText.Text = ShortenText(actualRightPath); //SQ
                    }
                }
            }
            //SQ synchronizer.RightPath = RightText.Text;
            synchronizer.RightPath = actualRightPath; //SQ

            RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
            //SQ ShowRemovableDrives(RightText.Text, "right");
            ShowRemovableDrives(actualRightPath, "right");
        }

        /// <summary>
        /// This method is called when user drag, but did not drop, and instead drag out of the right box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxRight_DragLeave(object sender, DragEventArgs e)
        {
            //SQ RightText.Text = previousTextRight;
            actualRightPath = previousTextRight; //SQ
            RightText.Text = ShortenText(actualRightPath); //SQ

            //SQ synchronizer.RightPath = RightText.Text;
            synchronizer.RightPath = actualRightPath; //SQ
            RightIcon.Source = previousImageRight;
        }

        /// <summary>
        /// This method is called when user drag, but did not drop, and instead drag out of the left box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxLeft_DragLeave(object sender, DragEventArgs e)
        {
            //SQ LeftText.Text = previousTextLeft;
            actualLeftPath = previousTextLeft; //SQ
            LeftText.Text = ShortenText(actualLeftPath); //SQ
            //SQ synchronizer.LeftPath = LeftText.Text;
            synchronizer.LeftPath = actualLeftPath; //SQ
            LeftIcon.Source = previousImageLeft;
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
                //SQ currentPath = LeftText.Text;
                currentPath = actualLeftPath; //SQ
            }
            string directoryPath = FolderSelect(currentPath);
            if (directoryPath != NULL_STRING)
            {
                //SQ LeftText.Text = directoryPath;
                actualLeftPath = directoryPath; //SQ
                LeftText.Text = ShortenText(actualLeftPath); //SQ
                //SQ ShowRemovableDrives(LeftText.Text, "left");
                ShowRemovableDrives(actualLeftPath, "left"); //SQ
                hasLeftPath = true;
            }
            //SQ synchronizer.LeftPath = LeftText.Text;
            synchronizer.LeftPath = actualLeftPath; //SQ
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
                //SQ currentPath = RightText.Text;
                currentPath = actualRightPath; //SQ
            }
            string directoryPath = FolderSelect(currentPath);
            if (directoryPath != NULL_STRING)
            {
                //SQ RightText.Text = directoryPath;
                actualRightPath = directoryPath; //SQ
                RightText.Text = ShortenText(actualRightPath); //SQ
                //SQ ShowRemovableDrives(RightText.Text, "right");
                ShowRemovableDrives(actualRightPath, "right"); //SQ
                hasRightPath = true;
            }
            //SQ synchronizer.RightPath = RightText.Text;
            synchronizer.RightPath = actualRightPath; //SQ
            ShowSync();
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
                LeftBarScrollLeft.Visibility = Visibility.Visible;
                LeftBarScrollRight.Visibility = Visibility.Visible;
            
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
                LeftBarScrollLeft.Visibility = Visibility.Hidden;
                LeftBarScrollRight.Visibility = Visibility.Hidden;
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
        /// This method is called when the mouse pointer enters right MRU bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BarMRURight_MouseEnter(object sender, MouseEventArgs e)
        {
                BarMRURight.Opacity = 0.5;
                BarMRURight.Cursor = Cursors.Hand;
                RightBarScrollLeft.Visibility = Visibility.Visible;
                RightBarScrollRight.Visibility = Visibility.Visible;
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
                RightBarScrollLeft.Visibility = Visibility.Hidden;
                RightBarScrollRight.Visibility = Visibility.Hidden;
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

        #region User Defined Functions

        /// <summary>
        /// Change the leftbox/rightbox icon if folder path is a removeable drive
        /// </summary>
        /// <param name="path"></param>
        /// <param name="leftOrRight"></param>
        private void ShowRemovableDrives(string path, string leftOrRight)
        {
            if (synchronizer.CheckRemovableDrive(path))
            {
                if (leftOrRight == "left" || leftOrRight == "Left")
                {
                    LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_REMOVABLE_DRIVE));
                }
                else if (leftOrRight == "right" || leftOrRight == "Right")
                {
                    RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_REMOVABLE_DRIVE));
                }
            }
        }

        /// <summary>
        /// Checks if folder paths exist
        /// </summary>
        /// <returns>Return a boolean to determine if folder paths exist</returns>
        private bool FolderCheck()
        {
            bool rightFolderExists = synchronizer.CheckFolderExists("right");
            bool leftFolderExists = synchronizer.CheckFolderExists("left");

            if (!rightFolderExists && !leftFolderExists)
            {
                helper.Show(nsync.Properties.Resources.bothFoldersNotExist, 5, HelperWindow.windowStartPosition.windowTop);
                LeftIcon.Source = RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER_MISSING));
                return false;
            }
            else if (!rightFolderExists)
            {
                helper.Show(nsync.Properties.Resources.rightFolderNotExist, 5, HelperWindow.windowStartPosition.windowTop);
                RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER_MISSING));
                LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
                return false;
            }
            else if (!leftFolderExists)
            {
                helper.Show(nsync.Properties.Resources.leftFolderNotExist, 5, HelperWindow.windowStartPosition.windowTop);
                RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
                LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER_MISSING));
                return false;
            }
            else
            {
                RightIcon.Source = LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
                return true;
            }
        }

        /// <summary>
        /// Checks if folders are similar
        /// </summary>
        /// <returns>Return a boolean to determine if folder paths are similar</returns>
        private bool SimilarFolderCheck()
        {
            if (synchronizer.CheckSimilarFolder())
            {
                helper.Show(nsync.Properties.Resources.similarFolders, 5, HelperWindow.windowStartPosition.windowTop);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if one folder is a subfolder of another
        /// </summary>
        /// <returns>Return a boolean to determine if one folder is subfolder of another</returns>
        private bool SubFolderCheck()
        {
            if (!synchronizer.CheckSubFolder())
            {
                helper.Show(nsync.Properties.Resources.subfolderOfFolder, 5, HelperWindow.windowStartPosition.windowTop);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the sync button should appear
        /// </summary>
        /// <returns>Return a boolean to determine if sync button should appear</returns>
        private bool ShowSync()
        {
            helper.HideWindow();

            LabelProgress.Visibility = Visibility.Hidden;
            LabelProgressPercent.Visibility = Visibility.Hidden;

            // Updates the folder icons accordingly first, if the folder path exists in the first place
            if (hasLeftPath)
            {
                LeftIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
                //SQ ShowRemovableDrives(LeftText.Text, "left");
                ShowRemovableDrives(actualLeftPath, "left"); //SQ
            }
            if (hasRightPath)
            {
                RightIcon.Source = new BitmapImage(new Uri(ICON_LINK_FOLDER));
                //SQ ShowRemovableDrives(RightText.Text, "right");
                ShowRemovableDrives(actualRightPath, "right"); //SQ
            }

            // Only if both boxes are filled with folder paths, then we need to check validity
            if (!hasLeftPath || !hasRightPath)
                return false;

            if (!FolderCheck() || SimilarFolderCheck() || !SubFolderCheck())
            {
                //SQ ShowRemovableDrives(LeftText.Text, "left");
                //SQ ShowRemovableDrives(RightText.Text, "right");
                ShowRemovableDrives(actualLeftPath, "left"); //SQ
                ShowRemovableDrives(actualRightPath, "right"); //SQ
                ButtonSync.Visibility = Visibility.Hidden;
                return false;
            }

            //SQ ShowRemovableDrives(LeftText.Text, "left");
            //SQ ShowRemovableDrives(RightText.Text, "right");
            ShowRemovableDrives(actualLeftPath, "left"); //SQ
            ShowRemovableDrives(actualRightPath, "right"); //SQ
            ButtonSync.Visibility = Visibility.Visible;
            return true;
        }

        /// <summary>
        /// Saves folder paths to settings.xml
        /// </summary>
        private void SaveFolderPaths()
        {
            // pass to settingsmanager the 2 current folderpaths, if any
            if (hasLeftPath && hasRightPath)
            {
                //SQ settingsManager.SaveFolderPaths(LeftText.Text, RightText.Text);
                settingsManager.SaveFolderPaths(actualLeftPath, actualRightPath); //SQ
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

            if (folderPaths.Count == 0)
                return;

            //SQ LeftText.Text = folderPaths[0];
            actualLeftPath = folderPaths[0]; //SQ
            LeftText.Text = ShortenText(actualLeftPath); //SQ
            //SQ synchronizer.LeftPath = LeftText.Text;
            synchronizer.LeftPath = actualLeftPath; //SQ

            //SQ RightText.Text = folderPaths[1];
            actualRightPath = folderPaths[1]; //SQ
            RightText.Text = ShortenText(actualRightPath); //SQ
            //SQ synchronizer.RightPath = RightText.Text;
            synchronizer.RightPath = actualRightPath; //SQ

            if (actualLeftPath == NULL_STRING) //SQ (LeftText.Text == NULL_STRING)
            {
                //SQ LeftText.Text = nsync.Properties.Resources.panelText;
                actualLeftPath = nsync.Properties.Resources.panelText; //SQ
                LeftText.Text = nsync.Properties.Resources.panelText; //SQ
                hasLeftPath = false;
            }
            else
                hasLeftPath = true;

            if (actualRightPath == NULL_STRING) //SQ (RightText.Text == NULL_STRING)
            {
                //SQ RightText.Text = nsync.Properties.Resources.panelText;
                actualRightPath = nsync.Properties.Resources.panelText; //SQ
                RightText.Text = nsync.Properties.Resources.panelText; //SQ
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

                listBoxLeft.Content = ShortenPath(folderPaths[i + (i-2)]);
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

                listBoxRight.Content = ShortenPath(folderPaths[i + (i-1)]);
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
        /// Shortens folder path for MRU list
        /// </summary>
        /// <param name="oldPath">The path that is to be shortened is passed in</param>
        /// <returns>A string containing the new folder path is returned</returns>
        private string ShortenPath(string oldPath)
        {
            if (oldPath.Length > 40)
            {
                return oldPath.Substring(0, 28) + "..." + oldPath.Substring(oldPath.Length - 10, 10);
            }
            else
            {
                return oldPath;
            }
        }

        /// <summary>
        /// Shortens folder path for left/right box text
        /// </summary>
        /// <param name="oldText">The path that is to be shortened is passed in</param>
        /// <returns>A string containing the new folder path is returned</returns>
        private string ShortenText(string oldText)
        {
            if (oldText.Length > 90)
            {
                return oldText.Substring(0, 60) + "..." + oldText.Substring(oldText.Length - 30, 30);
            }
            else
            {
                return oldText;
            }
        }

        /// <summary>
        /// This method is called when mouse pointer leaves right listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void listBoxRight_MouseLeave(object sender, MouseEventArgs e)
        {
            LeftListBox.SelectedIndex = -1;
        }

        /// <summary>
        /// This method is called when mouse pointer leaves left listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void listBoxLeft_MouseLeave(object sender, MouseEventArgs e)
        {
            RightListBox.SelectedIndex = -1;
        }

        /// <summary>
        /// This method is called when mouse pointer enters left listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void listBoxLeft_MouseEnter(object sender, MouseEventArgs e)
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
        void listBoxRight_MouseEnter(object sender, MouseEventArgs e)
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
        void ListBoxLeft_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem lb = new ListBoxItem();
            lb = (ListBoxItem)e.Source;
            //SQ LeftText.Text = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 2)];
            actualLeftPath = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 2)]; //SQ
            LeftText.Text = ShortenText(actualLeftPath); //SQ

            int index = Convert.ToInt32(lb.Tag);
            RightListBox.SelectedIndex = index - 1;
            lb = (ListBoxItem)RightListBox.SelectedItem;
            //SQ RightText.Text = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 1)];
            actualRightPath = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 1)]; //SQ
            RightText.Text = ShortenText(actualRightPath); //SQ

            //SQ synchronizer.LeftPath = LeftText.Text;
            //SQ synchronizer.RightPath = RightText.Text;
            synchronizer.LeftPath = actualLeftPath; //SQ
            synchronizer.RightPath = actualRightPath; //SQ

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
        void ListBoxRight_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem lb = new ListBoxItem();
            lb = (ListBoxItem)e.Source;
            // Change path label to this one and update synchronizer
            //SQ RightText.Text = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 1)];
            actualRightPath = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 1)]; //SQ
            RightText.Text = ShortenText(actualRightPath); //SQ

            int index = Convert.ToInt32(lb.Tag);
            LeftListBox.SelectedIndex = index - 1;
            lb = (ListBoxItem)LeftListBox.SelectedItem;
            //SQ LeftText.Text = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 2)];
            actualLeftPath = originalFolderPaths[Convert.ToInt32(lb.Tag) + (Convert.ToInt32(lb.Tag) - 2)]; //SQ
            LeftText.Text = ShortenText(actualLeftPath); //SQ

            //SQ synchronizer.LeftPath = LeftText.Text;
            //SQ synchronizer.RightPath = RightText.Text;
            synchronizer.LeftPath = actualLeftPath; //SQ
            synchronizer.RightPath = actualRightPath; //SQ

            RightListBox.SelectedIndex = -1;
            LeftListBox.Visibility = Visibility.Hidden;
            RightListBox.Visibility = Visibility.Hidden;

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
        /// This method is called when user click on the sync button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
            // check one more time
            // handle the situation when after a sync job is done,
            // user deletes the 2 folders n click sync again
            if (!ShowSync())
                return;

            LeftListBox.Visibility = Visibility.Hidden;
            RightListBox.Visibility = Visibility.Hidden;

            LabelProgress.Visibility = Visibility.Visible;
            LabelProgress.Content = "Preparing folders...";

            EnableInterface(false);

            // Do PreSync Calculations: count how many changes need to be done
            // If not enough disk space, return
            // If enough, continue to start the real sync
            synchronizer.PreSync();
        }

        /// <summary>
        /// This method is called when progress percentage has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
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
                SyncingImage.Visibility = Visibility.Hidden;
            }
            else
            {
                enableButtons = false;
                opacityValue = 0.5;
                ButtonSync.Visibility = Visibility.Hidden;
                SyncingImage.Visibility = Visibility.Visible;
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
            Button ButtonPageBackTrack = (Button)mainWindow.FindName("ButtonPageBackTrack");
            ButtonPageBackTrack.IsEnabled = enableButtons;

            //Set Opacity
            BoxLeft.Opacity = BoxRight.Opacity = opacityValue;
            ButtonSideTabLeft.Opacity = ButtonSideTabRight.Opacity = opacityValue;
            ButtonPageSettings.Opacity = ButtonPageHome.Opacity = ButtonPageBackTrack.Opacity = opacityValue;
        }

        /// <summary>
        /// This method is called when when synchronization is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableInterface(true);

            if (!(bool)e.Result)
            {
                LabelProgress.Content = "Error detected";
                LabelProgressPercent.Visibility = Visibility.Hidden;
                return;
            }

            if (synchronizer.AreFoldersSync())
            {
                ImageTeam14Over.OpacityMask = blankOpacityMask;

                LabelProgress.Content = "Sync completed";
                LabelProgressPercent.Content = "100 %";
                helper.Show(nsync.Properties.Resources.synchronizedFolders, 5, HelperWindow.windowStartPosition.windowTop);
                return;
            }

            ImageTeam14Over.OpacityMask = blankOpacityMask;

            // When all sync job done, save the folder pairs to MR and settings.xml
            SaveFolderPaths();
            ReloadFolderPaths();

            helper.Show(nsync.Properties.Resources.syncComplete, 5, HelperWindow.windowStartPosition.windowTop);
            LabelProgress.Visibility = Visibility.Visible;
            LabelProgressPercent.Visibility = Visibility.Visible;

            LabelProgress.Content = "Sync completed";
            LabelProgressPercent.Content = "100 %";
        }

        /// <summary>
        /// This method is called when presync calculations are completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!(bool) e.Result)
            {
                EnableInterface(true);
                helper.Show(nsync.Properties.Resources.insufficientDiskSpace, 5, HelperWindow.windowStartPosition.windowTop);
                LabelProgress.Visibility = Visibility.Hidden;
                LabelProgressPercent.Visibility = Visibility.Hidden;
                ButtonSync.Visibility = Visibility.Hidden;
                return;
            }

            EnableInterface(false);

            LabelProgress.Content = "Syncing folders...";
            LabelProgressPercent.Visibility = Visibility.Visible;
            LabelProgressPercent.Content = "0 %";
            //if (!synchronizer.AreFoldersSync()) trackBack.BackupFolders(LeftText.Text, RightText.Text);
            synchronizer.StartSync();
        }

        #endregion

    }
}
