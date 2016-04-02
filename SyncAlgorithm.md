The replica where synchronization initiated from is called the source and the replica it connects to is called the destination. The following diagram outlines the flow of synchronization:

![https://nsync.googlecode.com/svn/wiki/images/v2.0%20sync%20algorithm.png](https://nsync.googlecode.com/svn/wiki/images/v2.0%20sync%20algorithm.png)

_Image credits: [Microsoft Sync Framework Developer Center](http://msdn.microsoft.com/en-us/sync/bb821992.aspx)_

  1. The source initiates communication with the destination. The link formed between them is called a synchronization session.
  1. The knowledge (state and change information) stored in the destination’s metadata is passed on to the source.
  1. The source compares the information received to the versions of its local items in order to determine the items that the destination does not have information on.
  1. After preparing the list of changes required, the source transfers it to the destination.
  1. The destination uses this information to prepare a list of items that the source needs to send. The destination also uses this information to detect if there are any constraint conflicts. (A conflict is detected when the change version in one replica does NOT contain the knowledge of the other. Fundamentally, a conflict occurs if a change is made to the same item on two replicas between synchronization sessions)
  1. Conflicts are resolved by the destination. Our software uses the following resolution policy:
    * Last-Writer Wins: Based on the assumption that all replicas are trusted to make changes and wall clocks are synchronized, we allow the last writer to win (details below)
  1. After determining which items in the source need to be retrieved, the destination communicates back to the source.
  1. In response to the request, the source prepares the actual data to be transferred to the destination.
  1. The items received are taken and applied at the destination. The knowledge received from the source is added to the destination knowledge.

For bidirectional synchronization, this process will be executed twice; source and destination swapped on the second iteration.

# A rough idea of what we mean by “Last Writer Wins” #
  1. The user syncs the left and right folders.
  1. The user modifies a file in the left folder.
  1. The user modifies the same file in the right folder and sync again
  1. Only the latest changes would be replicated in both the folders (i.e. in this case, only the modified file in the right folder would be propagated to the left folder)

# Additional Requirements, Assumptions, Constraints and Dependencies #
The synchronization system in our product will be implemented with the help of the Microsoft Sync Framework. This framework is a data synchronization platform from Microsoft that can be used to synchronize data across multiple data stores or devices.
  * The framework will also automatically create metadata files to enable the smooth synchronization of files and folders.
  * The framework is also very extensible with lots of other capabilities which developers can use.
  * File conflicts will be handled by the framework and our code. To give a rough idea of what file conflicts are, we will give a simple explanation here – File conflict happens when the same files that exist on different computers are modified by the user and later synchronized. There are a few solutions for such cases such as skipping synchronization on that particular file, synchronizing the latest copy of the file or propagating both files to both locations by renaming either one of them. We will not go into further details at the moment. Additional resources will be included in the appendix for a full explanation of the different conflicts and its solutions to each problem.

Our reason for using this framework is so that it will reduce our workload on creating the synchronization system. Instead, we can tap on this framework, and improve on it further. On the other hand, we will be able to spend more time on the user interface and user controls, which our team strongly feel are equally as important. Lastly, we will also be able to have ample time to fine-tune the tiny details, which will improve the overall user experience of our product.