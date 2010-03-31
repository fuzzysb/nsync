using System.IO;
using System.Management;

namespace nsync
{
    class Intelligence
    {
        /// <summary>
        /// Check if there is any files inside the provided path
        /// </summary>
        /// <param name="path">This parameter is the folder path to be checked</param>
        /// <returns>Returns a boolean which indicates if the folder path has any files</returns>
        public bool IsThereFilesInRootPath(string path)
        {
            string[] fileNames = new string[100];
            fileNames = Directory.GetFiles(path);

            if (fileNames.Length > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if a path is the root path
        /// </summary>
        /// <param name="path">This string is the path to be checked</param>
        /// <returns>Returns a boolean which indicates if the folder path is a root path</returns>
        public bool IsPathRoot(string path)
        {
            // If cannot get parent, means path is parent
            if (Directory.GetParent(path) == null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if the folder path exists
        /// </summary>
        /// <param name="folderPath">This parameter is the folder path to be checked</param>
        /// <returns>Returns a boolean which indicates if the folder path exists</returns>
        public bool IsFolderExists(string folderPath)
        {
            if (Directory.Exists(folderPath))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Checks if two folder paths are similar
        /// </summary>
        /// <param name="leftFolderPath">This parameter is the folder path to be checked</param>
        /// <param name="rightFolderPath">This parameter is the folder path to be checked</param>
        /// <returns>Returns a boolean which indicates if both folder paths are similar</returns>
        public bool IsFoldersSimilar(string leftFolderPath, string rightFolderPath)
        {
            if (leftFolderPath == rightFolderPath)
                return true;
            return false;
        }

        /// <summary>
        /// Checks if a folder is a subfolder of another
        /// </summary>
        /// <param name="leftFolderPath">This parameter is the folder path to be checked</param>
        /// <param name="rightFolderPath">This parameter is the folder path to be checked</param>
        /// <returns>Returns a boolean which indicates if a folder is a subfolder of another</returns>
        public bool IsFolderSubFolder(string leftFolderPath, string rightFolderPath)
        {
            string[] leftFolderPathArray = leftFolderPath.Split(new char[] { '\\' });
            string[] rightFolderPathArray = rightFolderPath.Split(new char[] { '\\' });

            return isSamePath(leftFolderPathArray, rightFolderPathArray);
        }

        /// <summary>
        /// Compares the folder paths of the input arrays and determine if they
        /// have the same path from the root directory.
        /// </summary>
        /// <param name="sourceArray">Array of a folder path to be checked</param>
        /// <param name="destinationArray">Array of a folder path to be checked</param>
        /// <returns></returns>
        private bool isSamePath(string[] sourceArray, string[] destinationArray)
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

        /// <summary>
        /// Checks if the folder path belongs to a removable drive
        /// </summary>
        /// <param name="path">This parameter is the folder path to be checked</param>
        /// <returns>Returns a boolean which indicates if the folder path belongs to a removable drive</returns>
        public bool IsRemovableDrive(string path)
        {
            ManagementObjectSearcher mosDisks = new ManagementObjectSearcher(@"SELECT * FROM Win32_LogicalDisk WHERE DriveType=2"); //Finds all removable drives

            foreach (ManagementObject moDisk in mosDisks.Get())
            {
                if (moDisk["DeviceID"].ToString() == Directory.GetDirectoryRoot(path).Remove(2))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the unique serial number of the removeable disk
        /// </summary>
        /// <param name="path">This parameter is the directory to be checked</param>
        /// <returns>Returns a string which contains the serial number of the removeable disk
        /// <para>Returns null if the directory is not a removeable disk</para></returns>
        public string FindRemoveableDiskSerialNumber(string path)
        {
            ManagementObjectSearcher mosDisks = new ManagementObjectSearcher(@"SELECT * FROM Win32_LogicalDisk WHERE DriveType=2"); //Finds all removable drives

            foreach (ManagementObject moDisk in mosDisks.Get())
            {
                if (moDisk["DeviceID"].ToString() == Directory.GetDirectoryRoot(path).Remove(2))
                    return moDisk["VolumeSerialNumber"].ToString();
            }
            return null;
        }
    }
}
