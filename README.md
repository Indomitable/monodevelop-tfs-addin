Monodevelop TFS Addin
=====================
# This project is discontinued. 
I switched from TFS to GIT and I don't have anytime to support this project. If somebody wants to continue working on it, he can fork it. I suggest to start from tfs-addin-2.0 branch It is completely refactored. 


MonoDevelop Team Foundation Version Control Addin.

This addin works on MonoDevelop version 4.1.11+

How to install:
---------------

Use Add-in Manager in MonoDevelop, addin could be found in Alpha repository.

Work Items:
-----------
  This addin gives you access to your work items but currently you could only associate work items on commit.

Note for Mac and Linux users:
-----------------------------
  If you have used a TeamAddins addin you shoud fix your Workspace mappings.  
  Team Foundation Server requires a drive letter for local path.  
  Team Addins uses drive letter "C:" but this addin uses "U:" like git-tf.  
  You have to change your mappings from C: to U: this could be done in several ways:  
  * Using Visual Studio.
  * Using Eclipse TFS Addin.
  * Drop and create the workspaces using this addin (Note all pending changes will be lost).

More Info
---------
Visit: [Addin Home Page](http://indomitable.github.io/monodevelop-tfs-addin) 
