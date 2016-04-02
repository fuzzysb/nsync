

# Introduction #

Data synchronization is the process of establishing consistency among data from a source to a target data storage and vice versa. In other words it is the continuous harmonization of the data between workstations over time. This includes file and folder synchronization which [nsync](nsync.md) provides.

Folder synchronization helps keep 2 folders, usually across media, updated and current. If you change files in one folder, synchronization helps replicate this change at the other folder.

# Why would you need it? #

## Portability of work across multiple devices ##

It is particularly useful for workers who are constantly on the go (mobile users), or for those who work on multiple computers. It is possible to synchronize multiple locations by synchronizing them a pair at a time.

Let's say i have a 'School' or 'Work' folder on my desktop and another on my laptop. Sometimes I work on my laptop when I'm on the go and sometimes I work on my desktop, when i'm in the comfort of my home (I like the larger monitor). How do I keep both folders current and updated with my latest changes? Synchronization is the way to go!

## Keeping backups ##

File synchronization is also commonly used for home backups on external hard drives.

When I want to backup my Documents folder on my hard drive and don't want to have to copy and paste the whole folder, and be asked everytime there is an overwrite operation, I'd use [nsync](nsync.md) to keep them mirrored and current!

# What kind of synchronization does nsync provide? #

nsync provides something called 2-way synchronization. This means that updated files are copied in both directions, usually with the purpose of keeping the two locations identical to each other.

## Examples ##

  * When i delete a file in folder A it is deleted in folder B
  * When i create a new file in folder A, it is copied to folder B too
  * When i update a file in folder A, it is updated in folder B (replaced)

## Conflicts ##

What happens when I update files in both my folders?

  * We take the latest updated version of your file based on the time and update it to both folders.
  * However, don't worry, you can still retrieve the other modified file using our [TrackBack](TrackBack.md) feature.