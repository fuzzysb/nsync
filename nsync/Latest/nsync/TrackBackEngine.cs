using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;

namespace nsync
{
    class TrackBackEngine
    {
        ////////////////////
        // CLASS VARIABLES
        ////////////////////
        
        private DirectoryInfo storageFolder, destinationFolder;
        private string destinationPath, storagePath;
        private Stack<TrackBackData> trackBackStack;

        ////////////////////
        // CONSTRUCTOR
        ////////////////////

        public TrackBackEngine()
        {
            storagePath = "trackback";
            storageFolder = CreateFolder(storagePath);

            // creates a stack to store the folder pairs synced
            trackBackStack = new Stack<TrackBackData>(5);
        }

        ////////////////////
        // PRIVATE METHODS
        ////////////////////

        // copies a folder to the storage directory
        private void CopyFolder(DirectoryInfo srcDir)
        {
            string srcPath = srcDir.FullName;

            // queue to store subfolders
            Queue<DirectoryInfo> dirQueue = new Queue<DirectoryInfo>(20);
            dirQueue.Enqueue(srcDir);

            while (dirQueue.Count > 0)
            {
                DirectoryInfo currDir = dirQueue.Dequeue();

                DirectoryInfo[] dirs = null;

                // stores all subfolders in the current directory
                dirs = currDir.GetDirectories();

                string desFolderPath = Path.Combine(destinationPath, currDir.FullName.Remove(0, srcDir.Parent.FullName.Length + 1));

                if (!Directory.Exists(desFolderPath)) CreateFolder(desFolderPath);

                foreach (DirectoryInfo dir in dirs) dirQueue.Enqueue(dir);

                FileInfo[] files = currDir.GetFiles();

                // copies the files found in the current directory to the destination folders
                foreach (FileInfo file in files)
                {
                    string path = destinationPath + file.FullName.Remove(0, srcPath.LastIndexOf("\\" + srcDir.Name));
                    if (file.Name != "filesync.metadata")
                    {
                        file.CopyTo(path);
                        File.SetAttributes(path, FileAttributes.Hidden);
                    }
                }
            }
        }

        // creates a folder and sets its attributes as hidden
        private DirectoryInfo CreateFolder(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            dir.Create();
            dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            return dir;
        }

        private void CreateXmlNode()
        {
            XmlDocument document = new XmlDocument();
            document.Load("settings.xml");
            XmlNode node = document.CreateNode(XmlNodeType.Element, "TrackBack", "");

            if (document.GetElementsByTagName("TrackBack").Count == 0)
            {
                document.GetElementsByTagName("nsync")[0].InsertAfter(node,
                                                                      document.GetElementsByTagName("nsync")[0].
                                                                          LastChild);
                document.Save("settings.xml");
            }
        }

        private void SaveData()
        {
            
        }

        ////////////////////
        // PUBLIC METHODS
        ////////////////////

        // stores a copy of the sync folder pair in a subfolder located inside the "trackback" folder
        public void BackupFolders(string dirPathA, string dirPathB)
        {
            trackBackStack.Push(new TrackBackData(dirPathA, dirPathB));

            // creates a folder with its timestamp
            destinationPath = Path.Combine(storagePath, trackBackStack.Peek().TimeStamp);
            destinationFolder = CreateFolder(destinationPath);

            CopyFolder(trackBackStack.Peek().SourceDirectory);
            CopyFolder(trackBackStack.Peek().DestinationDirectory);
        }
    }
}
