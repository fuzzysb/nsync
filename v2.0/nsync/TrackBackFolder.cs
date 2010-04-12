﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Management;
using System.Xml;

namespace nsync
{
    class TrackBackFolder
    {
        #region Class Variables
        private string storageFolderPath, sourceFolderPath, destinationFolderPath;
        private string trackbackPath, dateTime, xmlPath;
        private DirectoryInfo trackbackFolder, storageFolder, sourceFolder, destinationFolder;

        private readonly string PATH_TRACKBACK = "/nsync/TrackBack";
        private readonly string PATH_SESSION = "/nsync/TrackBack/session";
        private readonly string TIMESTAMP_REGEX = "yyyy-MM-dd hh.mm.ss tt";
        private readonly string DATE_REGEX = "yyyy-MM-dd";
        private readonly string TIME_REGEX = "hh.mm.ss tt";

        private readonly string TRACKBACK_FOLDER_NAME = nsync.Properties.Resources.trackBackFolderName;
        private readonly string METADATA_FILE_NAME = nsync.Properties.Resources.metadataFileName;
        private readonly string TRACKBACK_XML_FILE_NAME = nsync.Properties.Resources.trackBackMetaDataFileName;
        #endregion

        #region Constructor
        ////////////////////
        // CONSTRUCTOR
        ////////////////////

        /// <summary>
        /// Creates TrackBackFolder object to allow querying of data
        /// </summary>
        /// <param name="sourcePath">The directory path of the folder to be queried</param>
        public TrackBackFolder(string sourcePath)
        {
            sourceFolderPath = sourcePath;
            sourceFolder = new DirectoryInfo(sourceFolderPath);

            trackbackPath = Path.Combine(sourcePath, TRACKBACK_FOLDER_NAME);
            if (Directory.Exists(trackbackPath)) trackbackFolder = new DirectoryInfo(trackbackPath);

            xmlPath = Path.Combine(trackbackPath, TRACKBACK_XML_FILE_NAME);
        }

        /// <summary>
        /// Creates TrackBackFolder object to prepare for back up
        /// </summary>
        /// <param name="sourcePath">The source directory path of the folder to be synchronized</param>
        /// <param name="destinationPath">The destination directory path of the folder to be synchronized</param>
        /// <param name="timeStamp">The time and date of the synchronization process</param>
        public TrackBackFolder(string sourcePath, string destinationPath, string timeStamp)
        {
            dateTime = timeStamp;

            sourceFolderPath = sourcePath;
            sourceFolder = new DirectoryInfo(sourceFolderPath);

            destinationFolderPath = destinationPath;
            destinationFolder = new DirectoryInfo(destinationFolderPath);

            trackbackPath = Path.Combine(sourcePath, TRACKBACK_FOLDER_NAME);
            if (!Directory.Exists(trackbackPath))
                trackbackFolder = CreateFolder(trackbackPath, true);
            else 
                trackbackFolder = new DirectoryInfo(trackbackPath);

            storageFolderPath = Path.Combine(trackbackPath, timeStamp);
            storageFolder = CreateFolder(storageFolderPath, true);

            xmlPath = Path.Combine(trackbackPath, TRACKBACK_XML_FILE_NAME);
            if (!File.Exists(xmlPath)) CreateTrackBackXml();
        }
        #endregion

        #region Public Methods
        ////////////////////
        // PUBLIC METHODS
        ////////////////////

        /// <summary>
        /// Retrieves a list of folder names of the different versions stored in TrackBack.
        /// </summary>
        /// <returns>An array of folder names</returns>
        public string[] GetFolderVersions()
        {
            XmlDocument document = new XmlDocument();
            document.Load(xmlPath);

            XmlNodeList nodeList = document.SelectNodes(PATH_SESSION);

            string[] folderList = new string[5];

            for (int i = 0; i < nodeList.Count; i++)
            {
                folderList[i] = nodeList[i].FirstChild.InnerText.Substring(nodeList[i].FirstChild.InnerText.LastIndexOf("\\")+1);
            }

            return folderList;
        }

        /// <summary>
        /// Retrieves a list of folder paths of the destination the folder was synced to.
        /// </summary>
        /// <returns>A string array containing the folder paths</returns>
        public string[] GetFolderDestinations()
        {
            XmlDocument document = new XmlDocument();
            document.Load(xmlPath);

            XmlNodeList nodeList = document.SelectNodes(PATH_SESSION);

            string[] folderList = new string[5];

            for (int i = 0; i < nodeList.Count; i++)
            {
                folderList[i] = nodeList[i].LastChild.InnerText;
            }

            return folderList;
        }

        /// <summary>
        /// Retrieves a list of the dates and times of when the sync took place.
        /// </summary>
        /// <returns>A string array of dates and times</returns>
        public string[] GetFolderTimeStamp()
        {
            XmlDocument document = new XmlDocument();
            document.Load(xmlPath);

            XmlNodeList nodeList = document.SelectNodes(PATH_SESSION);

            string[] folderList = new string[5];

            for (int i = 0; i < nodeList.Count; i++)
            {
                folderList[i] = nodeList[i].Attributes[0].Value + " " + nodeList[i].Attributes[1].Value;
            }

            return folderList;
        }

        /// <summary>
        /// Backups the folder and create the necessary TrackBack files and folders
        /// </summary>
        public void BackupFolder()
        {
            CopyFolder(sourceFolder, storageFolder);
            SaveTrackBackSession(sourceFolderPath, destinationFolderPath, dateTime);
        }

        /// <summary>
        /// Restores the folder back to the selected version
        /// </summary>
        public void RestoreFolder(string timeStamp)
        {
            string folderVersionPath = Path.Combine(trackbackPath, timeStamp);
            DirectoryInfo folderVersion = new DirectoryInfo(folderVersionPath);

            // Takes care of the scenario where no folder was backed up (probably due to previous errors)
            if (folderVersion.GetDirectories().Length == 0)
                return;

            DeleteSubfolders(sourceFolder);
            DeleteFiles(sourceFolder);

            CopyFolder(folderVersion.GetDirectories()[0], sourceFolder.Parent);
        }

        /// <summary>
        /// Checks if there is sufficient disk space for TrackBack to run
        /// </summary>
        /// <returns>If there is enough space for the folder to be copied, return true, false otherwise.</returns>
        public bool hasEnoughDiskSpace()
        {
            try
            {
                return GetDirectorySpaceInBytes(sourceFolder) < GetFreeDiskSpaceInBytes(sourceFolder.Root.Name.Substring(0, 1));
            }
            catch (UnauthorizedAccessException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Checks if the folder has any previous stored versions in TrackBack. An additional
        /// <para>check is done to ensure that the data is intact and not corrupted.</para>
        /// </summary>
        /// <returns>If the folder has stored previous folder versions and the data is valid,
        /// <para>return true, false otherwise.</para></returns>
        public bool hasTrackBackData()
        {
            return File.Exists(xmlPath) && isTrackBackXMLValid();
        }

        /// <summary>
        /// Checks to ensure that the XML document is valid.
        /// </summary>
        /// <returns>If valid, return true. Return false otherwise.</returns>
        public bool isTrackBackXMLValid()
        {
            if (!File.Exists(xmlPath)) return false;

            XmlDocument document = new XmlDocument();
            document.Load(xmlPath);

            if (document.SelectSingleNode(PATH_TRACKBACK) == null) return false;

            DirectoryInfo[] folderList = new DirectoryInfo(xmlPath.Substring(0, xmlPath.LastIndexOf("\\"))).GetDirectories();
            XmlNodeList nodeList = document.SelectNodes(PATH_SESSION);

            if (folderList.Length != nodeList.Count) return false;

            for (int i = 0; i < folderList.Length; i++)
            {
                string folderName = nodeList.Item(i).Attributes.Item(0).Value + " " + nodeList.Item(i).Attributes.Item(1).Value;
                if (!ContainsFolder(folderList, folderName))
                    return false;
            }
            return true;            
        }
        #endregion

        #region Private Methods
        ////////////////////
        // PRIVATE METHODS
        ////////////////////

        /// <summary>
        /// Copies a folder to the destination directory
        /// </summary>
        /// <param name="sourceDirectory">The folder to be copied</param>
        /// <param name="destinationDirectory">The directory to be copied into</param>
        private void CopyFolder(DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
        {
            // queue to store subfolders
            Queue<DirectoryInfo> directoryQueue = new Queue<DirectoryInfo>(20);
            directoryQueue.Enqueue(sourceDirectory);

            bool isParentRootDrive = (sourceDirectory.Parent.Name == sourceDirectory.Root.Name) ? true : false;

            while (directoryQueue.Count > 0)
            {
                DirectoryInfo currentDirectory = directoryQueue.Dequeue();

                DirectoryInfo[] folders = null;

                // stores all subfolders in the current directory
                folders = currentDirectory.GetDirectories();

                string folderPath = isParentRootDrive ?
                    Path.Combine(destinationDirectory.FullName, currentDirectory.FullName.Remove(0, sourceDirectory.Parent.FullName.Length)) :
                     Path.Combine(destinationDirectory.FullName, currentDirectory.FullName.Remove(0, sourceDirectory.Parent.FullName.Length + 1));

                if (!Directory.Exists(folderPath)) CreateFolder(folderPath, false);

                foreach (DirectoryInfo folder in folders)
                {
                    if (folder.Name != TRACKBACK_FOLDER_NAME)
                        directoryQueue.Enqueue(folder);
                }

                FileInfo[] files = currentDirectory.GetFiles();

                // copies the files found in the current directory to the destination folders
                foreach (FileInfo file in files)
                {
                    string path = isParentRootDrive ?
                        Path.Combine(destinationDirectory.FullName, file.FullName.Substring(sourceDirectory.Parent.FullName.Length)) :
                        Path.Combine(destinationDirectory.FullName, file.FullName.Substring(sourceDirectory.Parent.FullName.Length + 1));

                    if (file.Name != METADATA_FILE_NAME)
                        file.CopyTo(path);
                }  
             }
        }

        /// <summary>
        /// Creates a folder and sets its attributes as hidden
        /// </summary>
        /// <param name="path">The path of the folder to be created</param>
        /// <param name="isHidden">If true, set the folder as hidden. Else, do nothing.</param>
        /// <returns>The resultant folder</returns>
        private DirectoryInfo CreateFolder(string path, bool isHidden)
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            folder.Create();
            folder.Attributes = isHidden? FileAttributes.Directory | FileAttributes.Hidden : FileAttributes.Directory;

            return folder;
        }

        /// <summary>
        /// Deletes all subfolders in a given folder
        /// </summary>
        /// <param name="folder">The folder that contains the subfolders to be deleted</param>
        private void DeleteSubfolders(DirectoryInfo folder)
        {
            DirectoryInfo[] subfolders = folder.GetDirectories();

            foreach (DirectoryInfo subfolder in subfolders)
            {
                if (subfolder.Name != TRACKBACK_FOLDER_NAME)
                    DeleteDirectory(subfolder.FullName);
            }
        }

        /// <summary>
        /// Deletes all files in a given folder
        /// </summary>
        /// <param name="folder">The folder that contains the files to be deleted</param>
        private void DeleteFiles(DirectoryInfo folder)
        {
            FileInfo[] files = folder.GetFiles();

            foreach (FileInfo file in files)
            {
                File.SetAttributes(file.FullName, FileAttributes.Normal);
                File.Delete(file.FullName);
            }
        }

        /// <summary>
        /// Deletes the directory recursively. This method differs from Directory Delete method in that it
        /// is able to remove read-only files as well.
        /// </summary>
        /// <param name="directory">The directory path that is to be deleted</param>
        private void DeleteDirectory(string directory)
        {
            string[] files = Directory.GetFiles(directory);
            string[] folders = Directory.GetDirectories(directory);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string folder in folders)
            {
                DeleteDirectory(folder);
            }

            Directory.Delete(directory, false);
        }

        /// <summary>
        /// Computes the amount of free disk space
        /// <para>Units is in bytes</para>
        /// </summary>
        /// <param name="drive">This parameter is the drive volume to be checked</param>
        /// <returns>Amount of disk space in bytes, represented by 64-bit unsigned integer</returns>
        private ulong GetFreeDiskSpaceInBytes(string drive)
        {
            ManagementObject disk = new ManagementObject(
            "win32_logicaldisk.deviceid=\"" + drive + ":\"");
            disk.Get();
            return (ulong)disk["FreeSpace"];
        }

        /// <summary>
        /// Calculates the directory spaced needed in bytes.
        /// </summary>
        /// <param name="directory">The directory to be checked</param>
        /// <returns>Amount of disk space in bytes, represented by 64-bit unsigned integer</returns>
        private ulong GetDirectorySpaceInBytes(DirectoryInfo directory)
        {
            ulong size = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                size += (ulong)file.Length;
            }

            DirectoryInfo[] folders = directory.GetDirectories();
            foreach (DirectoryInfo folder in folders)
            {
                size += GetDirectorySpaceInBytes(folder);
            }
            return (size);
        }

        /// <summary>
        /// Creates the metadata file for TrackBack
        /// </summary>
        private void CreateTrackBackXml()
        {
            XmlTextWriter textWriter = new XmlTextWriter(xmlPath, null);
            textWriter.Formatting = Formatting.Indented;
            textWriter.WriteStartDocument();

            textWriter.WriteStartElement("nsync");
            textWriter.WriteStartElement("TrackBack");

            textWriter.WriteEndDocument();
            textWriter.Close();
            File.SetAttributes(xmlPath, FileAttributes.Hidden);
        }

        /// <summary>
        /// Saves the latest sync session to the metadata file
        /// </summary>
        /// <param name="sourcePath">The file path of the source directory</param>
        /// <param name="destinationPath">The file path of the destination directory</param>
        /// <param name="timeStamp">The time and date when the sync is carried out</param>
        private void SaveTrackBackSession(string sourcePath, string destinationPath, string timeStamp)
        {
            if (!File.Exists(xmlPath)) CreateTrackBackXml();

            File.SetAttributes(xmlPath, FileAttributes.Normal);
            XmlDocument document = new XmlDocument();
            document.Load(xmlPath);

            XmlNode node = document.CreateNode(XmlNodeType.Element, "session", "");
            XmlNode dateNode = document.CreateNode(XmlNodeType.Attribute, "date", "");
            dateNode.Value = timeStamp.Substring(0, timeStamp.IndexOf(' '));
            node.Attributes.SetNamedItem(dateNode);

            XmlNode timeNode = document.CreateNode(XmlNodeType.Attribute, "time", "");
            timeNode.Value = timeStamp.Substring(timeStamp.IndexOf(' ') + 1);
            node.Attributes.SetNamedItem(timeNode);

            // limits the number of TrackBack sessions to 5
            if (document.GetElementsByTagName("session").Count < 5)
            {
                document.GetElementsByTagName("TrackBack")[0].InsertAfter(node, document.GetElementsByTagName("TrackBack")[0].LastChild);
            }
            else
            {
                DateTime result = GetEarliestSession(GetDateTimeList(document));
                DeleteDirectory(Path.Combine(trackbackPath, result.ToString(TIMESTAMP_REGEX)));

                string date = result.ToString(DATE_REGEX);
                string time = result.ToString(TIME_REGEX);

                XmlNode oldNode = document.SelectSingleNode("//session[@date=\'" + date + "\'][@time=\'" + time + "\']");
                XmlNode trackback = document.SelectSingleNode(PATH_TRACKBACK);
                trackback.ReplaceChild(node, oldNode);
            }
            XmlNode source = document.CreateNode(XmlNodeType.Element, "source", "");
            source.InnerText = sourcePath;
            node.AppendChild(source);

            XmlNode destination = document.CreateNode(XmlNodeType.Element, "destination", "");
            destination.InnerText = destinationPath;
            node.AppendChild(destination);

            document.Save(xmlPath);
            File.SetAttributes(xmlPath, FileAttributes.Hidden);
        }

        /// <summary>
        /// Extracts and compiles the DateTime objects found in the XML document into an array
        /// </summary>
        /// <param name="document">The XML document that contains the DateTime objects</param>
        /// <returns>An array of DateTime objects</returns>
        private DateTime[] GetDateTimeList(XmlDocument document)
        {
            DateTime[] dateTimeList = new DateTime[5];

            for (int i = 0; i < 5; i++)
            {
                string dateValue = document.SelectNodes(PATH_SESSION)[i].Attributes[0].Value;
                string[] dateList = dateValue.Split('-');

                string timeValue = document.SelectNodes(PATH_SESSION)[i].Attributes[1].Value;
                string[] timeList = timeValue.Split('.');

                if (timeList[2].Contains("PM") && Int32.Parse(timeList[0]) < 12)
                    dateTimeList[i] = new DateTime(Int32.Parse(dateList[0]), Int32.Parse(dateList[1]), 
                        Int32.Parse(dateList[2]), Int32.Parse(timeList[0]) + 12, Int32.Parse(timeList[1]), 
                        Int32.Parse(timeList[2].Substring(0, timeList[2].IndexOf(" "))));
                else if (timeList[2].Contains("AM") && Int32.Parse(timeList[0]) == 12)
                    dateTimeList[i] = new DateTime(Int32.Parse(dateList[0]), Int32.Parse(dateList[1]), 
                        Int32.Parse(dateList[2]), 0, Int32.Parse(timeList[1]), 
                        Int32.Parse(timeList[2].Substring(0, timeList[2].IndexOf(" "))));
                else
                    dateTimeList[i] = new DateTime(Int32.Parse(dateList[0]), Int32.Parse(dateList[1]), 
                        Int32.Parse(dateList[2]), Int32.Parse(timeList[0]), Int32.Parse(timeList[1]), 
                        Int32.Parse(timeList[2].Substring(0, timeList[2].IndexOf(" "))));
            }
            return dateTimeList;
        }

        /// <summary>
        /// Retrieves the outdated session in the TrackBack system
        /// </summary>
        /// <param name="list">An array of DateTime objects to be compared</param>
        /// <returns>The outdated DateTime object</returns>
        private DateTime GetEarliestSession(DateTime[] list)
        {
            DateTime earliestSession = list[0];

            for (int i = 0; i < list.Length; i++)
            {
                if (DateTime.Compare(list[i], earliestSession) == -1)
                    earliestSession = list[i];
            }

            return earliestSession;
        }

        /// <summary>
        /// Checks if the folder list contains the specified search key
        /// </summary>
        /// <param name="list">The folder list to be searched</param>
        /// <param name="key">The search key</param>
        /// <returns></returns>
        private bool ContainsFolder(DirectoryInfo[] list, string key)
        {
            foreach (DirectoryInfo folder in list)
                if (folder.Name == key)
                    return true;
            return false;
        }

        /// <summary>
        /// Checks if the folder has restricted file permissions
        /// </summary>
        /// <param name="directory">The directory to be checked</param>
        /// <returns>If the folder can be accessed, return true. Return false otherwise.</returns>
        private bool IsDirectoryAccessible(DirectoryInfo directory)
        {
            try
            {
                directory.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        #endregion
    }
}
