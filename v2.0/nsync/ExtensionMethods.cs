///////////////////////////////////////////////////////
//
//   Title:     ExtensionMethods.cs
//   Author:    Siow Boon Lin Eugene
//
///////////////////////////////////////////////////////

namespace nsync
{
    /// <summary>
    /// Provides additional methods to check for backend operations.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extension method to check if an item is in a string array
        /// </summary>
        /// <param name="stringList">the list which will have the contains method</param>
        /// <param name="item">the string to check for in the list</param>
        /// <returns>true if its in the list</returns>
        public static bool Contains(this string[] stringList, string item)
        {
            foreach (string s in stringList)
                if (item.Equals(s))
                    return true;
            return false;
        }
    }
}