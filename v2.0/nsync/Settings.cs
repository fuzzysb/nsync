// Jayden

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Windows;

namespace nsync
{
    /// <summary>
    /// Settings Class
    /// </summary>
    public sealed class Settings
    {
        #region Class Variables
        private string settingsFile = Environment.GetEnvironmentVariable("APPDATA") + nsync.Properties.Resources.settingsFilePath;
        private string settingsFolder = Environment.GetEnvironmentVariable("APPDATA") + "\\nsync\\";
        private string nsyncFolder = Environment.GetEnvironmentVariable("APPDATA") + "\\nsync\\";
        private string logFolder = Environment.GetEnvironmentVariable("APPDATA") + "\\nsync\\log\\";
        private string NULL_STRING = nsync.Properties.Resources.nullString;

        private const int NUMBER_OF_MOST_RECENT = 5;
        private const string PATH_SETTINGS = "/nsync/SETTINGS";
        private const string PATH_MRU = "/nsync/MRU";
        private const string PATH_REMOVEABLEDISK = "/nsync/REMOVEABLEDISK";
        private const int HELPER_WINDOW_FATAL_PRIORITY = -2;

        private ExcludeData excludeData;
        private Window mainWindow = Application.Current.MainWindow;
        private ExcludeWindow excludeWindow;
        private VisualPreviewWindow visualPreviewWindow;
        private HomePage homePage;
        #endregion

        #region Singleton Setup
        /// <summary>
        /// Create an instance of Settings class
        /// </summary>
        private static readonly Settings instance = new Settings();

        /// <summary>
        /// Constructor of Settings class
        /// </summary>
        private Settings() {}

        /// <summary>
        /// Gets the instance of the Settings object
        /// </summary>
        public static Settings Instance
        {
            get { return instance; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Change the status of the HelperWindow which determines the duration it appears in nsync
        /// </summary>
        /// <param name="timer">This parameter is an int to indicate how long the HelperWindow should appear</param>
        public void SetHelperWindowStatus(int timer)
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (!File.Exists(settingsFile))
                {
                    CreateNewSettingsXML();
                }

                XmlDocument doc = new XmlDocument();
                XmlNode helperWindowStatusNode = SelectNode(doc, PATH_SETTINGS + "/HelperWindowTimer");

                if (timer == 11)
                    timer = -1;

                helperWindowStatusNode.InnerText = "" + timer;

                doc.Save(settingsFile);
                ProtectFile(settingsFile);
            }
        }

        /// <summary>
        /// Gets the current status of the HelperWindow
        /// </summary>
        /// <returns>Returns an int which indicates how long the HelperWindow should appear</returns>
        public int GetHelperWindowStatus()
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (!File.Exists(settingsFile))
                {
                    CreateNewSettingsXML();
                }

                XmlDocument doc = new XmlDocument();
                XmlNode helperWindowStatusNode = SelectNode(doc, PATH_SETTINGS + "/HelperWindowTimer");

                ProtectFile(settingsFile);
                return int.Parse(helperWindowStatusNode.InnerText);
            }

            return 5; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Changes preview filter status
        /// </summary>
        public void SetPreviewFilterStatus(string filterType)
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (!File.Exists(settingsFile))
                {
                    CreateNewSettingsXML();
                }

                XmlDocument doc = new XmlDocument();
                XmlNode previewFilterStatusNode = SelectNode(doc, PATH_SETTINGS + "/PreviewFilterType");

                previewFilterStatusNode.InnerText = filterType;

                doc.Save(settingsFile);
                ProtectFile(settingsFile);
            }
        }

        /// <summary>
        /// Gets preview filter status
        /// </summary>
        /// <returns>Status of Preview Filters</returns>
        public string GetPreviewFilterStatus()
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (!File.Exists(settingsFile))
                {
                    CreateNewSettingsXML();
                }

                XmlDocument doc = new XmlDocument();
                XmlNode previewFilterStatusNode = SelectNode(doc, PATH_SETTINGS + "/PreviewFilterType");

                ProtectFile(settingsFile);
                return previewFilterStatusNode.InnerText;
            }

            return "both"; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Change the status of the exclude window
        /// </summary>
        /// <param name="status">This parameter indicates if the exclude window is enabled or disabled</param>
        public void SetExcludeWindowStatus(bool status)
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (!File.Exists(settingsFile))
                {
                    CreateNewSettingsXML();
                }

                XmlDocument doc = new XmlDocument();
                XmlNode excludeWindowStatusNode = SelectNode(doc, PATH_SETTINGS + "/ExcludeWindowStatus");

                if (status == true)
                    excludeWindowStatusNode.InnerText = "1";
                else if (status == false)
                    excludeWindowStatusNode.InnerText = "0";

                doc.Save(settingsFile);
                ProtectFile(settingsFile);
            }
        }

        /// <summary>
        /// Gets the current status of the Exclude Window
        /// </summary>
        /// <returns>Returns a boolean which indicates whether the exclude window is enabled or disabled</returns>
        public int GetExcludeWindowStatus()
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (!File.Exists(settingsFile))
                {
                    CreateNewSettingsXML();
                }

                XmlDocument doc = new XmlDocument();
                XmlNode excludeWindowStatusNode = SelectNode(doc, PATH_SETTINGS + "/ExcludeWindowStatus");

                ProtectFile(settingsFile);
                return int.Parse(excludeWindowStatusNode.InnerText);
            }

            return -1; // return a default to nsync if it lock. This return is to tell the caller to stop doing anything else.
        }

        /// <summary>
        /// Change the status of the TrackBack
        /// </summary>
        /// <param name="status">This parameter indicates if the trackback is enabled or disabled</param>
        public void SetTrackBackStatus(bool status)
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (!File.Exists(settingsFile))
                {
                    CreateNewSettingsXML();
                }

                XmlDocument doc = new XmlDocument();
                XmlNode trackBackStatusNode = SelectNode(doc, PATH_SETTINGS + "/TrackBackStatus");

                if (status == true)
                    trackBackStatusNode.InnerText = "1";
                else if (status == false)
                    trackBackStatusNode.InnerText = "0";

                doc.Save(settingsFile);
                ProtectFile(settingsFile);
            }
        }

        /// <summary>
        /// Gets the current status of the TrackBack
        /// </summary>
        /// <returns>Returns an int which indicates whether the trackback is enabled or disabled. 0 means disabled. 1 means enabled. -1 means theres error</returns>
        public int GetTrackBackStatus()
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (!File.Exists(settingsFile))
                {
                    CreateNewSettingsXML();
                }

                XmlDocument doc = new XmlDocument();
                XmlNode trackBackStatusNode = SelectNode(doc, PATH_SETTINGS + "/TrackBackStatus");

                ProtectFile(settingsFile);
                return int.Parse(trackBackStatusNode.InnerText);
            }

            return -1; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Loads the saved folder paths into a list
        /// </summary>
        /// <returns>Returns a list of strings which contains the saved folder paths</returns>
        public List<string> LoadFolderPaths()
        {
            List<string> results = new List<string>();
            if (!IsFoldersLocked())
            {
                if (File.Exists(settingsFile))
                {
                    XmlDocument doc = new XmlDocument();
                    //Load MRU Information
                    XmlNode mruNode = SelectNode(doc, PATH_MRU);

                    for (int i = 1; i <= 5; i++)
                    {
                        results.Add(mruNode["left" + i.ToString()].InnerText);
                        results.Add(mruNode["right" + i.ToString()].InnerText);
                    }

                    ProtectFile(settingsFile);
                }
                return results;

            }

            return results; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Saves the current folder paths into settings.xml
        /// </summary>
        /// <param name="leftPath">This parameter will be saved into settings.xml</param>
        /// <param name="rightPath">This parameter will be saved into settings.xml</param>
        public void SaveFolderPaths(string leftPath, string rightPath)
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                string[] tempStorage = new string[10];

                int counter = 0;

                for (int i = 0; i < 10; i++)
                {
                    tempStorage[i] = NULL_STRING;
                }

                IsSettingsFileExists();

                XmlDocument doc = new XmlDocument();
                XmlNode mruNode = SelectNode(doc, PATH_MRU);

                XmlNode filterNode;
                XmlNode excludeFileTypeNode;
                XmlNode excludeFileNameNode;
                XmlNode excludeFolderNode;
                XmlNode newSizeNode;
                ExcludeData[] tempExcludeData = new ExcludeData[5];
                int[] fileTypeListSize = new int[5];
                int[] fileNameListSize = new int[5];
                int[] folderListSize = new int[5];

                for (int i = 0; i < NUMBER_OF_MOST_RECENT; i++)
                    tempExcludeData[i] = new ExcludeData();

                for (int i = 1; i <= NUMBER_OF_MOST_RECENT; i++)
                {
                    tempStorage[counter++] = mruNode["left" + i.ToString()].InnerText;
                    tempStorage[counter++] = mruNode["right" + i.ToString()].InnerText;

                    filterNode = mruNode.SelectSingleNode("filter" + i.ToString());

                    // Backup File Type from xml file
                    excludeFileTypeNode = filterNode.SelectSingleNode("excludeFileTypes");

                    fileTypeListSize[i - 1] = int.Parse(excludeFileTypeNode["size"].InnerText);
                    if (fileTypeListSize[i - 1] != 0)
                    {
                        for (int j = 0; j < fileTypeListSize[i - 1]; j++)
                        {
                            tempExcludeData[i - 1].AddExcludeFileType(excludeFileTypeNode["fileType" + j.ToString()].InnerText);
                        }
                    }

                    // Backup File Name from xml file
                    excludeFileNameNode = filterNode.SelectSingleNode("excludeFileNames");

                    fileNameListSize[i - 1] = int.Parse(excludeFileNameNode["size"].InnerText);
                    if (fileNameListSize[i - 1] != 0)
                    {
                        for (int j = 0; j < fileNameListSize[i - 1]; j++)
                        {
                            tempExcludeData[i - 1].AddExcludeFileName(excludeFileNameNode["fileName" + j.ToString()].InnerText);
                        }
                    }

                    // Backup Folder from xml file
                    excludeFolderNode = filterNode.SelectSingleNode("excludeFolders");

                    folderListSize[i - 1] = int.Parse(excludeFolderNode["size"].InnerText);
                    if (folderListSize[i - 1] != 0)
                    {
                        for (int j = 0; j < folderListSize[i - 1]; j++)
                        {
                            tempExcludeData[i - 1].AddExcludeFolder(excludeFolderNode["folder" + j.ToString()].InnerText);
                        }
                    }
                }

                mruNode["left1"].InnerText = leftPath;
                mruNode["right1"].InnerText = rightPath;

                // Change the nodes to the first filter
                filterNode = mruNode.SelectSingleNode("filter1");

                excludeFileTypeNode = filterNode.SelectSingleNode("excludeFileTypes");
                excludeFileNameNode = filterNode.SelectSingleNode("excludeFileNames");
                excludeFolderNode = filterNode.SelectSingleNode("excludeFolders");

                // storing exclude for File Types
                // Clearing the first node for space of new node
                excludeFileTypeNode.RemoveAll();
                newSizeNode = doc.CreateElement("size");
                newSizeNode.InnerText = excludeData.ExcludeFileTypeList.Count.ToString();
                excludeFileTypeNode.AppendChild(newSizeNode);

                if (excludeData.ExcludeFileTypeList.Count != 0)
                {
                    for (int i = 0; i < excludeData.ExcludeFileTypeList.Count; i++)
                    {
                        XmlNode newFileTypeNode = doc.CreateElement("fileType" + i.ToString());
                        newFileTypeNode.InnerText = excludeData.ExcludeFileTypeList[i];
                        excludeFileTypeNode.AppendChild(newFileTypeNode);
                    }
                }

                // storing exclude for File Names
                // Clearing the first node for space of new node
                excludeFileNameNode.RemoveAll();
                newSizeNode = doc.CreateElement("size");
                newSizeNode.InnerText = excludeData.ExcludeFileNameList.Count.ToString();
                excludeFileNameNode.AppendChild(newSizeNode);

                if (excludeData.ExcludeFileNameList.Count != 0)
                {
                    for (int i = 0; i < excludeData.ExcludeFileNameList.Count; i++)
                    {
                        XmlNode newFileNameNode = doc.CreateElement("fileName" + i.ToString());
                        newFileNameNode.InnerText = excludeData.ExcludeFileNameList[i];
                        excludeFileNameNode.AppendChild(newFileNameNode);
                    }
                }

                // storing exclude for Folder
                // Clearing the first node for space of new node
                excludeFolderNode.RemoveAll();
                newSizeNode = doc.CreateElement("size");
                newSizeNode.InnerText = excludeData.ExcludeFolderList.Count.ToString();
                excludeFolderNode.AppendChild(newSizeNode);

                if (excludeData.ExcludeFolderList.Count != 0)
                {
                    for (int i = 0; i < excludeData.ExcludeFolderList.Count; i++)
                    {
                        XmlNode newFolderNode = doc.CreateElement("folder" + i.ToString());
                        newFolderNode.InnerText = excludeData.ExcludeFolderList[i];
                        excludeFolderNode.AppendChild(newFolderNode);
                    }
                }

                for (int i = 0; i < 10; i += 2)
                {
                    if (tempStorage[i] == leftPath && tempStorage[i + 1] == rightPath)
                    {
                        tempStorage[i] = tempStorage[i + 1] = "REPLACED";
                        break;
                    }
                }

                counter = 0;
                for (int i = 2; i <= NUMBER_OF_MOST_RECENT; i++)
                {
                    while (tempStorage[counter] == "REPLACED" && tempStorage[counter + 1] == "REPLACED")
                        counter += 2;

                    // Stores the temp File Type into 2, 3, 4, 5th filters
                    if (fileTypeListSize[counter / 2] != 0)
                    {
                        filterNode = mruNode.SelectSingleNode("filter" + i.ToString());
                        excludeFileTypeNode = filterNode.SelectSingleNode("excludeFileTypes");

                        excludeFileTypeNode.RemoveAll();
                        newSizeNode = doc.CreateElement("size");
                        newSizeNode.InnerText = fileTypeListSize[counter / 2].ToString();
                        excludeFileTypeNode.AppendChild(newSizeNode);

                        for (int j = 0; j < fileTypeListSize[counter / 2]; j++)
                        {
                            XmlNode newFileTypeNode = doc.CreateElement("fileType" + j.ToString());
                            newFileTypeNode.InnerText = tempExcludeData[counter / 2].ExcludeFileTypeList[j];
                            excludeFileTypeNode.AppendChild(newFileTypeNode);
                        }
                    }

                    // Stores the temp File Name into 2, 3, 4, 5th filters
                    if (fileNameListSize[counter / 2] != 0)
                    {
                        filterNode = mruNode.SelectSingleNode("filter" + i.ToString());
                        excludeFileNameNode = filterNode.SelectSingleNode("excludeFileNames");

                        excludeFileNameNode.RemoveAll();
                        newSizeNode = doc.CreateElement("size");
                        newSizeNode.InnerText = fileNameListSize[counter / 2].ToString();
                        excludeFileNameNode.AppendChild(newSizeNode);

                        for (int j = 0; j < fileNameListSize[counter / 2]; j++)
                        {
                            XmlNode newFileNameNode = doc.CreateElement("fileName" + j.ToString());
                            newFileNameNode.InnerText = tempExcludeData[counter / 2].ExcludeFileNameList[j];
                            excludeFileNameNode.AppendChild(newFileNameNode);
                        }
                    }

                    // Stores the temp Folder into 2, 3, 4, 5th filters
                    if (folderListSize[counter / 2] != 0)
                    {
                        filterNode = mruNode.SelectSingleNode("filter" + i.ToString());
                        excludeFolderNode = filterNode.SelectSingleNode("excludeFolders");

                        excludeFolderNode.RemoveAll();
                        newSizeNode = doc.CreateElement("size");
                        newSizeNode.InnerText = folderListSize[counter / 2].ToString();
                        excludeFolderNode.AppendChild(newSizeNode);

                        for (int j = 0; j < folderListSize[counter / 2]; j++)
                        {
                            XmlNode newFolderNode = doc.CreateElement("folder" + j.ToString());
                            newFolderNode.InnerText = tempExcludeData[counter / 2].ExcludeFolderList[j];
                            excludeFolderNode.AppendChild(newFolderNode);
                        }
                    }

                    mruNode["left" + i.ToString()].InnerText = tempStorage[counter];
                    mruNode["right" + i.ToString()].InnerText = tempStorage[counter + 1];

                    counter += 2;
                }

                UnProtectFile(settingsFile);
                doc.Save(settingsFile);
                ProtectFile(settingsFile);
            }
        }

        /// <summary>
        /// Save folder path for removeable disk into settings.xml
        /// </summary>
        /// <param name="serialNumber">This parameter indicates the serial number of the removeable disk</param>
        /// <param name="leftPath">This parameter indicates the leftPath of the sync job</param>
        /// <param name="rightPath">This parameter indicates the rightPath of the sync job</param>
        public void SaveFolderPathForRemoveableDisk(string serialNumber, string leftPath, string rightPath)
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                IsSettingsFileExists();

                XmlDocument doc = new XmlDocument();
                doc.Load(settingsFile);

                XmlNode removeableDiskNode = SelectNode(doc, PATH_REMOVEABLEDISK);
                XmlNode diskNode = removeableDiskNode.SelectSingleNode("//Disk[@ID='" + serialNumber + "']");
                if (diskNode != null) // node exists, update paths
                {
                    if (diskNode["left"] == null)
                    {
                        diskNode.AppendChild(doc.CreateElement("left"));
                    }
                    if (diskNode["right"] == null)
                    {
                        diskNode.AppendChild(doc.CreateElement("right"));
                    }
                    diskNode["left"].InnerText = leftPath;
                    diskNode["right"].InnerText = rightPath;
                }
                else // node doesn't exists, create everything
                {
                    // Create serial number node
                    XmlNode newSerialNumberNode = doc.CreateNode(XmlNodeType.Element, "Disk", null);
                    XmlAttribute serialNumberAttribute = doc.CreateAttribute("ID");
                    serialNumberAttribute.Value = serialNumber;
                    newSerialNumberNode.Attributes.SetNamedItem(serialNumberAttribute);

                    // Create left and right node
                    XmlNode newLeftNode = doc.CreateElement("left");
                    newLeftNode.InnerText = leftPath;
                    XmlNode newRightNode = doc.CreateElement("right");
                    newRightNode.InnerText = rightPath;

                    // Add left and right node to parent node
                    newSerialNumberNode.AppendChild(newLeftNode);
                    newSerialNumberNode.AppendChild(newRightNode);

                    doc.DocumentElement["REMOVEABLEDISK"].AppendChild(newSerialNumberNode);
                }

                doc.Save(settingsFile);
                ProtectFile(settingsFile);
            }
        }

        /// <summary>
        /// Gets last removeable thumbdrive
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public string[] GetLastRemoveableDiskSync(string serialNumber)
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                if (File.Exists(settingsFile))
                {
                    string[] results = new string[2];
                    XmlDocument doc = new XmlDocument();
                    doc.Load(settingsFile);

                    XmlNode removeableDiskNode = SelectNode(doc, PATH_REMOVEABLEDISK);
                    if (removeableDiskNode == null)
                        return null;

                    XmlNode diskNode = removeableDiskNode.SelectSingleNode("//Disk[@ID='" + serialNumber + "']");
                    if (diskNode == null || diskNode["left"] == null || diskNode["right"] == null)
                        return null;

                    results[0] = diskNode["left"].InnerText;
                    results[1] = diskNode["right"].InnerText;

                    // check if the path in results array exists
                    // if any of them don't exists, no point restoring it back for the users
                    if (Directory.Exists(results[0]) && Directory.Exists(results[1]))
                        return results;
                    else
                        return null;
                }
                else
                {
                    return null;
                }
            }

            return null; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Load ExcludeData
        /// </summary>
        /// <param></param>
        /// <returns>saved exclude data</returns>
        public ExcludeData LoadExcludeData(string leftPath, string rightPath)
        {
            ExcludeData loadedExcludeData = new ExcludeData();

            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(settingsFile);

                XmlNode mruNode = SelectNode(doc, PATH_MRU);

                // finds the corresponding paths to which filters are stored.
                int filterIndex = 0;
                for (int i = 1; i <= NUMBER_OF_MOST_RECENT; i++)
                {
                    if (mruNode["left" + i.ToString()].InnerText.Equals(leftPath))
                        if (mruNode["right" + i.ToString()].InnerText.Equals(rightPath))
                            filterIndex = i;
                } 

                if (filterIndex != 0) // checks if there are any filters
                {
                    XmlNode filterNode = mruNode.SelectSingleNode("filter" + filterIndex.ToString());
                    XmlNode excludeFileTypeNode = filterNode.SelectSingleNode("excludeFileTypes");
                    XmlNode excludeFileNameNode = filterNode.SelectSingleNode("excludeFileNames");
                    XmlNode excludeFolderNode = filterNode.SelectSingleNode("excludeFolders");

                    // adds exclude File Types
                    for (int i = 0; i < int.Parse(excludeFileTypeNode["size"].InnerText); i++)
                    {
                        loadedExcludeData.AddExcludeFileType(excludeFileTypeNode["fileType" + i.ToString()].InnerText);
                    }

                    // adds exclude File Names
                    for (int i = 0; i < int.Parse(excludeFileNameNode["size"].InnerText); i++)
                    {
                        loadedExcludeData.AddExcludeFileName(excludeFileNameNode["fileName" + i.ToString()].InnerText);
                    }

                    // adds exclude Folders
                    for (int i = 0; i < int.Parse(excludeFolderNode["size"].InnerText); i++)
                    {
                        loadedExcludeData.AddExcludeFolder(excludeFolderNode["folder" + i.ToString()].InnerText);
                    } 
                }

                return loadedExcludeData; 
            }

            return loadedExcludeData; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Open log folder
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string OpenLogFolder()
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                string logPath = Environment.GetEnvironmentVariable("APPDATA") + nsync.Properties.Resources.logFolderPath;

                try
                {
                    if (Directory.Exists(logPath))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(logPath);
                        dirInfo.Attributes = FileAttributes.Normal;
                        System.Diagnostics.Process.Start(@logPath);

                        return null;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return "Not authorized to access log folder. Please check log folder permissions.";
                }

                return "Log folder does not exist.";
            }

            return null; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Clears logs in log folder
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string ClearLogFolder()
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                string logPath = Environment.GetEnvironmentVariable("APPDATA") + nsync.Properties.Resources.logFolderPath;

                try
                {
                    if (Directory.Exists(logPath))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(logPath);
                        dirInfo.Attributes = FileAttributes.Normal;

                        foreach (string fileName in Directory.GetFiles(logPath))
                        {
                            File.SetAttributes(fileName, FileAttributes.Normal);
                            File.Delete(fileName);
                        }

                        return "Logs Cleared.";
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return "Not authorized to access log folder. Please check log folder permissions.";
                }

                return "Log folder does not exist.";
            }

            return NULL_STRING; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Clears Meta Data in current selected left and right folder
        /// </summary>
        /// <param></param>
        /// <returns>string</returns>
        public string ClearMetaData()
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                string[] path = GetLeftAndRightFolderPath();
                string leftPath = path[0];
                string rightPath = path[1];
                int outcome = 0;
                string result = null;

                if ((leftPath != NULL_STRING) && (rightPath != NULL_STRING))
                {
                    if (!Directory.Exists(leftPath))
                        outcome = 2; // left path not exist
                    if (!Directory.Exists(rightPath))
                    {
                        if (outcome == 2)
                            outcome = 4; // both path not exist
                        else
                            outcome = 3; // right path not exist
                    }

                    if (outcome == 0)
                    {
                        DeleteAllMetaData(leftPath);
                        DeleteAllMetaData(rightPath);
                    } // both path exist

                } // end if for checking if folders not exist
                else
                    outcome = 1;


                // determines outcome
                switch (outcome)
                {
                    case 0: result = "Meta data cleared."; break;
                    case 1: result = "No Left and Right Folder selected."; break;
                    case 2: result = "Left Folder does not exist."; break;
                    case 3: result = "Right Folder does not exist."; break;
                    case 4: result = "Both Left and Right Folders do not exist."; break;
                }

                return result;
            }

            return null; // return a default to nsync if it lock. This return is to prevent error.
        }

        /// <summary>
        /// Clears saved settngs
        /// </summary>
        /// <param>settingsPage</param>
        /// <returns></returns>
        public void ClearSettings()
        {
            if (!IsFoldersLocked()) // checks if folders that nsync use are locked.
            {
                UnProtectFile(settingsFile);
                File.Delete(settingsFile);
                CreateNewSettingsXML();
            }
        }

        /// <summary>
        /// Setter and Getter method excludeData
        /// </summary>
        public ExcludeData ExcludedData
        {
            get { return excludeData; }
            set { excludeData = value; }
        }

        /// <summary>
        /// References the HomePage
        /// </summary>
        public void SetHomePage(HomePage homePage)
        {
            this.homePage = homePage;
        }

        /// <summary>
        /// References ExcludeWindow (Overloaded Method) 
        /// </summary>
        public void SetOwnerWindow(ExcludeWindow excludeWindow)
        {
            this.excludeWindow = excludeWindow;
        }

        /// <summary>
        /// References VisualPreviewWindow (Overloaded Method)
        /// </summary>
        public void SetOwnerWindow(VisualPreviewWindow visualPreviewWindow)
        {
            this.visualPreviewWindow = visualPreviewWindow;
        }

        /// <summary>
        /// Checks if log folder is locked.
        /// </summary>
        public bool IsFoldersLocked()
        {
            int outcome = 0;

            if (IsSettingsFileLocked())
                outcome = 3; // settings locked
            if (IsLogFolderLocked())
                outcome = 2; // log folder locked
            if (IsSettingsFileLocked())
                outcome = 1; // nsync folder locked

            if (outcome != 0)
            {
                homePage.IsErrorClosing();
                HelperManager emergencyHelper = new HelperManager(mainWindow);
                mainWindow.Visibility = Visibility.Hidden;
                if (visualPreviewWindow != null)
                    visualPreviewWindow.Visibility = Visibility.Hidden;
                if (excludeWindow != null)
                    excludeWindow.Visibility = Visibility.Hidden;

                if (outcome == 1)
                    emergencyHelper.Show("Nsync Folder is Locked.\n Path : " + nsyncFolder, HELPER_WINDOW_FATAL_PRIORITY, HelperWindow.windowStartPosition.center);

                if (outcome == 2)
                    emergencyHelper.Show("Log Folder is Locked.\n Path : " + logFolder, HELPER_WINDOW_FATAL_PRIORITY, HelperWindow.windowStartPosition.center);

                if (outcome == 3)
                    emergencyHelper.Show("Settings File is Locked.\n Path : " + settingsFile, HELPER_WINDOW_FATAL_PRIORITY, HelperWindow.windowStartPosition.center);

                return true;
            } //some folders has been locked

            return false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets a XMLNode from a XML document
        /// </summary>
        /// <param name="doc">This parameter indicates the XMLDocument to be read</param>
        /// <param name="path">This parameter indicates the tag of the XMLNode to search for</param>
        /// <returns>Returns a matching XMLNode</returns>
        private XmlNode SelectNode(XmlDocument doc, string path)
        {
            try
            {
                doc.Load(settingsFile);
            }
            catch
            {
                File.Delete(settingsFile); 
                CreateNewSettingsXML();
                doc.Load(settingsFile);
            }

            XmlElement root = doc.DocumentElement;
            
            if (!CheckSettingsXML(doc))
            {
                File.Delete(settingsFile);
                CreateNewSettingsXML();
                doc.Load(settingsFile);
                root = doc.DocumentElement;
            }

            UnProtectFile(settingsFile);

            XmlNode node = root.SelectSingleNode(path);
            return node;
        }

        /// <summary>
        /// Checks if a XML document is properly formatted
        /// </summary>
        /// <param name="doc">This parameter is the XML document to be checked</param>
        /// <returns>Returns a boolean to indicate if the XML document is valid</returns>
        private bool CheckSettingsXML(XmlDocument doc)
        {
            if (null == doc.SelectSingleNode(PATH_SETTINGS + "/HelperWindowTimer"))
                return false;

            if (null == doc.SelectSingleNode(PATH_REMOVEABLEDISK))
                return false;

            for (int i = 1; i <= NUMBER_OF_MOST_RECENT; i++)
            {
                if (null == doc.SelectSingleNode(PATH_MRU+"/left" + i.ToString()))
                    return false;
                if (null == doc.SelectSingleNode(PATH_MRU+"/right" + i.ToString()))
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Creates a new settings.xml
        /// </summary>
        private void CreateNewSettingsXML()
        {
            CheckFolderExist();

            XmlTextWriter textWriter = new XmlTextWriter(settingsFile, null);
            textWriter.Formatting = Formatting.Indented;
            textWriter.WriteStartDocument();

            //Start Root
            textWriter.WriteStartElement("nsync");

            //Write last opened information
            textWriter.WriteStartElement("MRU");

            for (int i = 1; i <= 5; i++)
            {
                textWriter.WriteStartElement("left" + i.ToString());
                textWriter.WriteString(NULL_STRING);
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("right" + i.ToString());
                textWriter.WriteString(NULL_STRING);
                textWriter.WriteEndElement();

                //Write Filter information
                textWriter.WriteStartElement("filter" + i.ToString());

                textWriter.WriteStartElement("excludeFileTypes");
                textWriter.WriteStartElement("size");
                textWriter.WriteString("0");
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("excludeFileNames");
                textWriter.WriteStartElement("size");
                textWriter.WriteString("0");
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();

                textWriter.WriteStartElement("excludeFolders");
                textWriter.WriteStartElement("size");
                textWriter.WriteString("0");
                textWriter.WriteEndElement();
                textWriter.WriteEndElement();

                textWriter.WriteEndElement();
            }

            //End last opened information
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("SETTINGS");

            textWriter.WriteStartElement("HelperWindowTimer");
            textWriter.WriteString("5");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("ExcludeWindowStatus");
            textWriter.WriteString("0");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("TrackBackStatus");
            textWriter.WriteString("0");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("PreviewFilterType");
            textWriter.WriteString("both");
            textWriter.WriteEndElement();

            textWriter.WriteEndElement();

            //Removeable Disk information
            textWriter.WriteStartElement("REMOVEABLEDISK");
            textWriter.WriteEndElement();

            //End Root
            textWriter.WriteEndElement();
            textWriter.WriteEndDocument();

            textWriter.Close();
            ProtectFile(settingsFile);
        }

        /// <summary>
        /// Makes the file normal for editing
        /// </summary>
        private void UnProtectFile(string file)
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        /// <summary>
        /// Makes the file hidden and readOnly
        /// </summary>
        private void ProtectFile(string file)
        {
            //File.SetAttributes(file, FileAttributes.Normal | FileAttributes.ReadOnly);
            File.SetAttributes(file, FileAttributes.Hidden | FileAttributes.ReadOnly);
        }
        
        /// <summary>
        /// Obtains the first 2 paths from settings.xml
        /// </summary>
        private string[] GetLeftAndRightFolderPath()
        {
            XmlDocument doc = new XmlDocument();
            //Load MRU Information
            XmlNode mruNode = SelectNode(doc, PATH_MRU);

            string[] path = new string[2];
            path[0] = mruNode["left1"].InnerText;
            path[1] = mruNode["right1"].InnerText;

            return path;
        }

        /// <summary>
        /// Recursive method to delete metaData
        /// </summary>
        private void DeleteAllMetaData(string path)
        {
            string[] filePaths = new string[0];
            List<string> directoriesToSearch = new List<string>();

            directoriesToSearch = GetSubFolder(path); // gets all accessible sub folders
            foreach (string directories in directoriesToSearch)
            {
                try
                {
                    filePaths = Directory.GetFiles(directories, "filesync.metadata", SearchOption.TopDirectoryOnly);
                }
                catch
                {
                    // do nothing and let the operation to continue
                }

                foreach (string foundPath in filePaths)
                {
                    if (File.Exists(foundPath))
                    {
                        try
                        {
                            File.SetAttributes(foundPath, FileAttributes.Normal);
                            File.Delete(foundPath);
                        }
                        catch
                        {
                            // do nothing and let the operation to continue
                        }
                    }
                }
            } // find filesync.metadata in the accessible folders

        }

        /// <summary>
        /// Recursive method to get all accessible Sub Folder
        /// </summary>
        private List<string> GetSubFolder(string path)
        {
            List<string> directories = new List<string>();
            string[] subDirectories;

            // child directory is not accessible
            try
            {
                subDirectories = Directory.GetDirectories(path);
                directories.Add(path); // adds the current directory to the list
            }
            catch (UnauthorizedAccessException)
            {
                return directories; // return nothing as a locked folder cannot be accessed.
            } 
            catch
            {
                return directories; // catch any other exception and allow the operation to complete
            }

            foreach (string subDirectory in subDirectories)
            {
                foreach (string allSubDirectory in GetSubFolder(subDirectory))
                    directories.Add(allSubDirectory); // recursively gets all sub directories and stores to caller.
            }

            return directories;
        }

        /// <summary>
        /// Check if the nsync folder exists in %APPDATA%
        /// </summary>
        private void CheckFolderExist()
        {
            if (!(Directory.Exists(settingsFolder)))
            {
                Directory.CreateDirectory(settingsFolder);
            }
        }

        /// <summary>
        /// Checks if nsync folder is locked.
        /// </summary>
        private bool IsNsyncFolderLocked()
        {
            try
            {
                Directory.GetFiles(nsyncFolder, "", SearchOption.TopDirectoryOnly);
                DirectoryInfo dirInfo = new DirectoryInfo(nsyncFolder);
                dirInfo.Attributes = FileAttributes.Normal;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            catch (ArgumentException)
            {
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                try // this try block checks if a parent folder is locked
                {
                    IsNsyncFolderExists();
                }
                catch (UnauthorizedAccessException)
                {
                    return true;
                }
            }
            catch (FileNotFoundException)
            {
                try // this try block checks if a parent folder is locked
                {
                    IsLogFolderExists();
                }
                catch (UnauthorizedAccessException)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if log folder is locked.
        /// </summary>
        private bool IsLogFolderLocked()
        {
            try
            {
                Directory.GetFiles(logFolder, "", SearchOption.TopDirectoryOnly);
                DirectoryInfo dirInfo = new DirectoryInfo(logFolder);
                dirInfo.Attributes = FileAttributes.Normal;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            catch (ArgumentException)
            {
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                try // this try block checks if a parent folder is locked
                {
                    IsLogFolderExists();
                }
                catch (UnauthorizedAccessException)
                {
                    return true;
                }
            }
            catch (FileNotFoundException)
            {
                try // this try block checks if a parent folder is locked
                {
                    IsLogFolderExists();
                }
                catch (UnauthorizedAccessException)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if settings file is locked.
        /// </summary>
        private bool IsSettingsFileLocked()
        {
            try
            {
                UnProtectFile(settingsFile);
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            catch (DirectoryNotFoundException)
            {
                try // this try block checks if a parent folder is locked
                {
                    IsSettingsFileExists();
                }
                catch (UnauthorizedAccessException)
                {
                    return true;
                }
            }
            catch (FileNotFoundException)
            {
                try // this try block checks if a parent folder is locked
                {
                    IsSettingsFileExists();
                }
                catch (UnauthorizedAccessException)
                {
                    return true;
                }
            }

            ProtectFile(settingsFile);

            return false;
        }

        /// <summary>
        /// Check if nsync folder exists, if not, create a new copy
        /// </summary>
        private bool IsNsyncFolderExists()
        {
            if (!File.Exists(nsyncFolder))
            {
                Directory.CreateDirectory(nsyncFolder);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if log folder exists, if not, create a new copy
        /// </summary>
        private bool IsLogFolderExists()
        {
            if (!File.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if settings.xml exists, if not, create a new copy
        /// </summary>
        private bool IsSettingsFileExists()
        {
            if (!File.Exists(settingsFile))
            {
                CreateNewSettingsXML();
                return false;
            }
            return true;
        }
        #endregion

    }
}