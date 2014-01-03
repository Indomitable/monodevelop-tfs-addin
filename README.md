Monodevelop TFS Addin
=====================

MonoDevelop Team Foundation Version Control Addin.

This addin works on MonoDevelop version 4.1.11+

<h5>How to install:</h5>

To use this addin add following url using Add-in Manager in MonoDevelop

For MonoDevelop/Xamarin Studio 4.1 use:
http://indomitable.github.io/monodevelop-tfs-addin/4.1/

For MonoDevelop/Xamarin Studio 4.2 use:
http://indomitable.github.io/monodevelop-tfs-addin/4.2/

<h5>Work Items:</h5>
  This addin gives you access to your work items but currently you could only associate work items on commit.

<h5>Note for Mac and Linux users:</h5>
  If you have used a TeamAddins addin you shoud fix your Workspace mappings.<br/>
  Team Foundation Server requires a drive letter for local path. <br/>
  Team Addins uses drive letter "C:" but this addin uses "U:" like git-tf. <br/>
  You have to change your mappings from C: to U: this could be done in several ways:<br/>
  <ol>
    <li>
      Using Visual Studio.
    </li>
    <li>
      Using Eclipse TFS Addin.
    </li>
    <li>
      Drop and create the workspaces using this addin (Note all pending changes will be lost).
    </li>
 </ol>    

<h5>More Info</h5>
Visit: <a href="http://indomitable.github.io/monodevelop-tfs-addin/">Addin Home Page</a>
