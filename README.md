monodevelop-tfs-addin
=====================

MonoDevelop team foundation version control addin

This addin works on MonoDevelop version 4.1.11+

To use this addin add following url using Add-in Manager in MonoDevelop

For MonoDevelop/Xamarin Studio 4.1 use:
http://indomitable.github.io/monodevelop-tfs-addin/4.1/

For MonoDevelop/Xamarin Studio 4.2 use:
http://indomitable.github.io/monodevelop-tfs-addin/4.2/

Work Items:
  This addin gives you access to your work items but currently you could not resolve work items on commit.

Note:
  If you used a TeamAddins addin you shoud fix your Workspace mappings. 
  Team Foundation Server requires a drive letter for local path. 
  Team Addins uses drive letter "C:" but this addin uses "U:" like git-tf. 
  You have to change your mappings from C: to U: this could be done in several ways:
    1. Using Visual Studio.
    2. Using Eclipse TFS Addin.
    3. Drop and Create the workspaces using this addin (Note all pending changes will be lost).