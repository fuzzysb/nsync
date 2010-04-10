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
        private string settingsFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + nsync.Properties.Resources.settingsFilePath;
        private string NULL_STRING = nsync.Properties.Resources.nullString;
        private const int NUMBER_OF_MOST_RECENT = 5;
        private const string PATH_SETTINGS = "/nsync/SETTINGS";
        private const string PATH_MRU = "/nsync/MRU";
        private const string PATH_REMOVEABLEDISK = "/nsync/REMOVEABLEDISK";
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
            protectFile(settingsFile);
        }

        /// <summary>
        /// Gets the current status of the HelperWindow
        /// </summary>
        /// <returns>Returns an int which indicates how long the HelperWindow should appear</returns>
        public int GetHelperWindowStatus()
        {
            if (!File.Exists(settingsFile))
            {
                CreateNewSettingsXML();
            }

            XmlDocument doc = new XmlDocument();
            XmlNode helperWindowStatusNode = SelectNode(doc, PATH_SETTINGS + "/HelperWindowTimer");
            
            protectFile(settingsFile);
            return int.Parse(helperWindowStatusNode.InnerText);
        }

        /// <summary>
        /// Change the status of the exclude window
        /// </summary>
        /// <param name="status">This parameter indicates if the exclude window is enabled or disabled</param>
        public void SetExcludeWindowStatus(bool status)
        {
            if (!File.Exists(settingsFile))
            {
                CreateNewSettingsXML();
            }

            XmlDocument doc = new XmlDocument();
            XmlNode excludeWindowStatusNode = SelectNode(doc, PATH_SETTINGS + "/ExcludeWindowStatus");

            excludeWindowStatusNode.InnerText = "" + status.ToString();

            doc.Save(settingsFile);
            protectFile(settingsFile);
        }

        /// <summary>
        /// Gets the current status of the Exclude Window
        /// </summary>
        /// <returns>Returns a boolean which indicates whether the exclude window is enabled or disabled</returns>
        public bool GetExcludeWindowStatus()
        {
            if (!File.Exists(settingsFile))
            {
                CreateNewSettingsXML();
            }

            XmlDocument doc = new XmlDocument();
            XmlNode excludeWindowStatusNode = SelectNode(doc, PATH_SETTINGS + "/ExcludeWindowStatus");

            protectFile(settingsFile);
            return bool.Parse(excludeWindowStatusNode.InnerText);
        }

        /// <summary>
        /// Change the status of the TrackBack
        /// </summary>
        /// <param name="status">This parameter indicates if the trackback is enabled or disabled</param>
        public void SetTrackBackStatus(bool status)
        {
            if (!File.Exists(settingsFile))
            {
                CreateNewSettingsXML();
            }

            XmlDocument doc = new XmlDocument();
            XmlNode trackBackStatusNode = SelectNode(doc, PATH_SETTINGS + "/TrackBackStatus");

            trackBackStatusNode.InnerText = "" + status.ToString();

            doc.Save(settingsFile);
            protectFile(settingsFile);
        }

        /// <summary>
        /// Gets the current status of the TrackBack
        /// </summary>
        /// <returns>Returns a boolean which indicates whether the trackback is enabled or disabled</returns>
        public bool GetTrackBackStatus()
        {
            if (!File.Exists(settingsFile))
            {
                CreateNewSettingsXML();
            }

            XmlDocument doc = new XmlDocument();
            XmlNode trackBackStatusNode = SelectNode(doc, PATH_SETTINGS + "/TrackBackStatus");

            protectFile(settingsFile);
            return bool.Parse(trackBackStatusNode.InnerText);
        }

        /// <summary>
        /// Loads the saved folder paths into a list
        /// </summary>
        /// <returns>Returns a list of strings which contains the saved folder paths</returns>
        public List<string> LoadFolderPaths()
        {
            List<string> results = new List<string>();
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

                protectFile(settingsFile);
            }
            return results;
        }

        /// <summary>
        /// Saves the current folder paths into settings.xml
        /// </summary>
        /// <param name="leftPath">This parameter will be saved into settings.xml</param>
        /// <param name="rightPath">This parameter will be saved into settings.xml</param>
        public void SaveFolderPaths(string leftPath, string rightPath)
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

            for (int i = 1; i <= NUMBER_OF_MOST_RECENT; i++)
            {
                tempStorage[counter++] = mruNode["left" + i.ToString()].InnerText;
                tempStorage[counter++] = mruNode["right" + i.ToString()].InnerText;
            }

            mruNode["left1"].InnerText = leftPath;
            mruNode["right1"].InnerText = rightPath;

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

                mruNode["left" + i.ToString()].InnerText = tempStorage[counter];
                mruNode["right" + i.ToString()].InnerText = tempStorage[counter + 1];

                counter += 2;
            }

            doc.Save(settingsFile);
            protectFile(settingsFile);
        }

        /// <summary>
        /// Save folder path for removeable disk into settings.xml
        /// </summary>
        /// <param name="serialNumber">This parameter indicates the serial number of the removeable disk</param>
        /// <param name="leftPath">This parameter indicates the leftPath of the sync job</param>
        /// <param name="rightPath">This parameter indicates the rightPath of the sync job</param>
        public void SaveFolderPathForRemoveableDisk(string serialNumber, string leftPath, string rightPath)
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
            protectFile(settingsFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public string[] GetLastRemoveableDiskSync(string serialNumber)
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

                return results;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="excludeList"></param>
        /// <returns></returns>
        public void StoreFilters(List<string> filterList)
        {
            int filterSize = filterList.Count;

            IsSettingsFileExists();

            XmlDocument doc = new XmlDocument();
            XmlNode mruNode = SelectNode(doc, PATH_MRU);
            XmlElement tempFilterElement;

            XmlNode filterNode = mruNode.SelectSingleNode("filter1");
            filterNode["size"].InnerText = filterSize.ToString();

            for (int i = 0; i < filterSize; i++)
            {
                tempFilterElement = doc.CreateElement("exclude" + i.ToString());
                tempFilterElement.InnerText = filterList[i];
                filterNode.AppendChild(tempFilterElement);
            }
        }

        /// <summary>
        /// Clears logs in log folder
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public void ClearLogFolder(SettingsPage settingsPage)
        {
            settingsPage.LabelProgress.Content = "Clearing Logs...";
            settingsPage.LabelProgress.Visibility = Visibility.Visible;
            string logPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
            "\\log\\";

            foreach (string fileName in Directory.GetFiles(logPath))
            {
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.Delete(fileName);
            }
            settingsPage.LabelProgress.Content = "Logs Cleared.";
        }
        #endregion

        #region Private Methods
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

            unprotectFile(settingsFile);

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
                textWriter.WriteStartElement("size");
                textWriter.WriteString("0");
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
            textWriter.WriteString("false");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("TrackBackStatus");
            textWriter.WriteString("false");
            textWriter.WriteEndElement();

            textWriter.WriteEndElement();

            //Removeable Disk information
            textWriter.WriteStartElement("REMOVEABLEDISK");
            textWriter.WriteEndElement();

            //End Root
            textWriter.WriteEndElement();
            textWriter.WriteEndDocument();

            textWriter.Close();
            protectFile(settingsFile);
        }

        /// <summary>
        /// Makes the file normal for editing
        /// </summary>
        private void unprotectFile(string file)
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        /// <summary>
        /// Makes the file hidden and readOnly
        /// </summary>
        private void protectFile(string file)
        {
            File.SetAttributes(file, FileAttributes.Hidden | FileAttributes.ReadOnly);
        }
        #endregion
    }
}
