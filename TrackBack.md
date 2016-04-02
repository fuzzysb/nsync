# Introduction #

**TrackBack** is a feature in nsync that ensures the recovery of your directories to the pre-sync state. It ensures that you never(unless there's a natural disaster of course) lose any files! If **reliability** is what you want, TrackBack seeks to provide this.

# How TrackBack works #

![https://nsync.googlecode.com/svn/wiki/images/trackback.png](https://nsync.googlecode.com/svn/wiki/images/trackback.png)

  1. Before every sync session, a back up file of the folder pair will be created for restoration purposes.
  1. It's works like a time machine whereby it allows you to restore your files/folders of a particular day, e.g. Restore the folder version as on last Monday before sync was carried out on it.
  1. TrackBack enables the user to restore the pre sync state of the last 5 saved folder pair versions. It all happens in just 1 click.
  1. The user has the option to either restore only the left folder or the right folder or both.
  1. TrackBack is disabled by default. A change in the Settings is required to enable it.