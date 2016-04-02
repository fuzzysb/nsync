

# Synchronization #
  * 2-way [synchronization](Synchronization.md) (Sync two folders)
  * Update functionality (update both folders with the latest versions of documents and files)
  * Diskspace check (checks if there is available space for synchronization)

# Stop Sync #
![https://nsync.googlecode.com/svn/wiki/images/cancelsync.png](https://nsync.googlecode.com/svn/wiki/images/cancelsync.png)
  * Allows users to stop a sync process midway. It leads to an incomplete sync since only some files are synchronized.
  * The cancel sync appears only when the sync is in progress. It’s executed using the same “Sync” button.

# Easy to use, Intuitive, fast and attractive interface #

  * Regular folder browsing where you click and select the source/destination folder.
  * Apart from the regular folder browsing, nsync also comes with a Drag and Drop feature (Folders can be dragged from anywhere in Windows into nsync and it is capable of recognizing them)
  * To minimise the untidiness and make the interface look smooth and clean, nsync automatically displays the sync button only when both source and destination folders have been chosen.
  * Folder icon changes accordingly if the selected folders are missing/do not exist. Even on navigating with a removable drive or selecting a folder within a removable drive using the folder browsing technique, the folder icon changes to that of a removable drive.
  * To keep track of the sync operation, we have 3 indication bars at the bottom of the interface – status bar (displays status messages like preparing folders/syncing folders/sync completed), progress bar and percentage bar (shows sync progress in percentage)
  * The progress bar is so designed in such a way that the team14 logo will fill up with  orange colour as the sync proceeds. It makes it more appealing to the eye thereby conveying the information to the user about a sync operation’s progress.
  * A slider window to access different pages mainly Settings, Main Window and Track Back (to be included in the next release). When we move the mouse over the left and right side of the interface, a small button appears. Clicking on it allows you to slide to the next page in the line up. On mouse click and hold, the image of the button becomes depressed.
  * When the user reaches the leftmost/rightmost page of nsync, the left/right slider bar won’t appear at all.
  * Another way to navigate through the interface pages is by the use of dots (Navidots). These appear at the top part of the interface and clicking on them, you can directly jump to any page out of Settings, Main Window and Track Back (to be included in the next release).
  * Last 5 successfully synced folders are saved in a MRU (Most Recently Used) list which can be accessed by moving the mouse over the folder space. The MRU list is intelligent enough to save the synced folder pairs in the Settings.xml file so that in the case of multiple syncs at one go (to be implemented in the next release), all folder pairs would be saved. Currently, multiple sync is not included, hence the software doesn’t allow the user to add another set of folders in the queue while the current sync is going on.
  * The MRU list also takes care of preventing any duplicate folder pairs in the last 5 synced pairs.
  * The interface disables the scrolling buttons, the Navidots and the MRU list box while syncing to avoid any hang up or crashing and to prevent the user from committing any mistake unknowingly. Also, you can minimize or move the main window/interface while syncing but cannot close the application.
  * Tool Tip feature - It displays the functionality of each and every button as you scroll the mouse over it.
  * Long folder path names are truncated automatically to avoid giving a messy look in the MRU, instead all folder paths would have a tooltip displaying their full folder path.
  * The helper/notification window resets its position according to how we drag the main window. It either fits itself at the top or the bottom of the interface window.
  * Use of tooltip makes it more intuitive and self explanatory for the user. The tooltip in Visual Preview displays the full path for each file that is waiting to be synced in the left/right folder.
  * We believe in a minimalistic design with less cluttered features/buttons, hence we decided to have the “Cancel sync” button in the same place as “Sync button”.
  * Use of colour coding makes it more readable and clearer to the users. Example:
    1. In Visual Preview, folders are differentiated from files using colour coding.
    1. In Exclude, colour coding is used to differentiate specific file types (based on their extension); individual files and folders dragged in; and other folders dragged in that are not present in any of the left/right folder.


# nsync remembers what you sync #
https://nsync.googlecode.com/svn/wiki/images/MRU.JPG
  * Even after exiting nsync and executing it again, nsync remembers your last synced folders (the source and the destination folder) and up to 5 of the most recently synced folders.

# sync using a removable drive #

  * nsync is capable of syncing a removable drive with other folders on the computer.

# Intelligence #

  * nsync’s differentiating factor among the other synchronization tools is its intelligence. There exists an in built powerful help function that provides help and timely information throughout the software. Beats reading a 1000 page help file!
  * Makes intelligent decisions about folder paths as they are dragged in. E.g. In the case where a folder to be synced appears in the source directory but not in the destination directory and the destination directory is a drive, nsync automatically creates the same folder name in the destination directory.
  * When left folder is a subfolder of right folder or vice versa, it does not allow the sync process to continue so as to prevent an infinite loop.
  * It also alerts the user in the case of an operation being performed incorrectly. Eg: If a user is trying to sync the same folders, the help guide pops up an unobtrusive message window saying “The source and destination folders are the same”
  * It allows you to continue with your other work while the sync process is being carried out and a notification saying “sync done” pops up in front of your active window as soon as the sync is done.
  * It saves you time in identifying the already synchronized folders and alerts you the same with a notification.
  * It checks and reports if one or more of the selected folders have been moved/do not exist by displaying an appropriate message.
  * nsync is capable of identifying shortcuts and thanks to its intelligence, it can link to the actual folder path (to which the shortcuts points). So whenever a user drags in a shortcut, the folder that the shortcut points to appears instead.
  * If the user chooses to set the duration for the helper window to 0 sec (does not show at all) in the settings page, nsync still shows few notifications which it considers to be really crucial and important for alerting the user like “Sync Done”, “Folder restored”, “Insufficient Disk Space”, “Folder do not exist”, “Similar folder”, “Subfolder of the other”.
  * nsync syncs the file on the opposite side in the correct folder hierarchy without bothering the user to think or worry about it.
Example:
<br>
- User syncs 2 folders, namely Left Folder and Right Folder<br>
<br>
- User updates a subfolder named A inside Left Folder  and wants to propagate these changes in the subfolder A of Right Folder<br>
<br />
- User drags in Left Folder/A in the left, the right side automatically changes to Right Folder/A, hence only the required folders would be synced again and not the full parent folders<br>
<br />
<ul><li>nsync remembers the removable drive sync – If it’s a thumb drive or any other removable drive, nsync remembers the last synced folder in the removable drive and restores back the folder once the drive is plugged in again under certain conditions (there are no files in the removable drive, only folders exist).<br>
Example:<br>
<br>
- User syncs G:/test1 and C:/Program files<br>
<br>
- User now syncs G:/test2 and C:/Program files<br>
<br>
- Now the user syncs some other folder pairs excluding the removable drive G:<br>
<br>
- At this point, if user drags in G: again, the last synced folder would appear, that is, G:/test2 would appear.<br>
This only happens under certain conditions (there aren’t any files but just folders in the removable drive)<br>
<br>
Reason: We don’t want to outsmart the user because there could be a case where he actually wants to sync the files present in G:/, so it isn’t appropriate to make G:/test2 appear when actually just G:/ is required.<br>
</li><li>nsync recognises a removable drive by its serial number, so even if we change the drive name, nsync recognises it and remembers its last synced folder.<br>
</li><li>Visual Preview remembers the last filter option (Show changes for Both / Left / Right folder) for each folder pair that was synced.<br>
</li><li>nsync detects folders/files that are locked, shows a helper window message saying which file/folder is locked, and then closes elegantly, instead of crashing like other software. Hence nsync doesn’t crash if %APPDATA%\nsync or any of its subfolder/files are locked.</li></ul>

<h1>Track Back</h1>
<ul><li>Before every sync session, a backup file of the folder pair is created for restoration purposes. By Track Back we propose to have a time machine kind of feature that allows you to restore your files/folders of a particular day, e.g.: Restore the folder version as on last Monday before sync was carried out on it.<br>
</li><li>Track Back enables the user to restore the pre sync state of the last 5 saved folder pair versions. It all happens in just 1 click. Track Back is disabled by default. A change in the Settings is required to enable it.<br>
</li><li>The user has the option to either restore only the left folder or the right folder or both</li></ul>

<h1>Visual Preview</h1>
<ul><li>The users can click a preview button on the home page (next to Sync button) which shows them the post sync state of either both the folders or just the right folder or just the left folder. The possible actions could be create / delete / rename / change /no change (denoted by blank)</li></ul>

<h1>Exclude</h1>
<ul><li>The users can drag and drop files that are to be excluded from the sync operation.<br>
</li><li>The users can also select files of specific types to be excluded based on their extension.<br>
</li><li>Clicking the close button directs you to the home page where you have to click sync again if you wish to resynchronize. Clicking on the continue button resumes the sync operation while excluding the selected files.<br>
</li><li>By default, this feature is disabled. Once the user enables it in the Settings page, this window automatically appears whenever the user clicks the sync button.</li></ul>

<h1>Settings Page</h1>
<ul><li>Allows the users to enable/disable features and change settings according to their wish. For example, users can choose to have the helper window (notifications) on/off.<br>
</li><li>Future options would be to allow users to toggle features like Track Back (v2.0), intelligence (v2.0) etc<br>
</li><li>Users can choose to enable/disable the Track Back feature<br>
</li><li>Users can choose to enable/disable the Exclude window<br>
</li><li>Users can choose the duration for helper/notification window to appear ranging from 0 sec (not at all) to infinity (doesn’t go away on its own – need to be manually closed)<br>
</li><li>Other options available are :<br>
<ol><li>Clear Meta Data – It clears the metadata file from both the folders (if it exists, that is, if the folders have been synced before)<br>
</li><li>Reset Settings – It forgets everything (track back versions, left and right folder etc), resets the settings, and brings everything back to the state in which nsync was right after installation.<br>
</li><li>Open Log Folder – Opens the folder where you can see the log/summary report<br>
</li><li>Clear Log Folder – Clears the log/summary report from the log folder.<br>
</li></ol></li><li>The default settings include – Disabled Track Back, Disabled Exclude, and timer for notification window set to 5 sec.</li></ul>

<h1>Log/summary report file</h1>
<ul><li>Produces a summary report at the end of the sync operation. nsync itself prompts the user to see the report if there is a renaming conflict else the user has to manually go to the log folder to see the report. The report displays the outcome of the sync operation (successful/unsuccessful) and states the cause of the conflict (if any).</li></ul>

<h1>Debug Logger</h1>
<ul><li>We have added a debug logger class where we record all the user actions and the notifications/messages so that in case of an unexpected error/crash, users can send the debugger file to us. Users can find it in the debug folder.