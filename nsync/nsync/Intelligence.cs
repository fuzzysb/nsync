using System.IO;
using System.Management;

namespace nsync
{
    class Intelligence
    {
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
            DirectoryInfo leftPathDir = new DirectoryInfo(leftFolderPath);
            DirectoryInfo rightPathDir = new DirectoryInfo(rightFolderPath);
            string leftPathDirParent;
            string rightPathDirParent;

            if (leftPathDir.Parent != null)
                leftPathDirParent = leftPathDir.Parent.FullName.ToString();
            else
                leftPathDirParent = leftPathDir.FullName.ToString();

            if (rightPathDir.Parent != null)
                rightPathDirParent = rightPathDir.Parent.FullName.ToString();
            else
                rightPathDirParent = rightPathDir.FullName.ToString();

            if (leftPathDirParent.Contains(rightFolderPath) || rightPathDirParent.Contains(leftFolderPath))
            {
                return true;
            }
            else
                return false;
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
    }
}
