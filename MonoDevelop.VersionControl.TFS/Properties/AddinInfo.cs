using Mono.Addins;

[assembly:Addin (
    "VersionControl.TFS", Namespace = "MonoDevelop", 
    Version = "1.7", Category = "Version Control")]

[assembly:AddinName ("TFS support")]
[assembly:AddinDescription ("TFS support for the Version Control Add-in")]
[assembly:AddinUrl ("http://indomitable.github.io/monodevelop-tfs-addin")]
[assembly:AddinAuthor ("Ventsislav Mladenov <ventsislav.mladenov@gmail.com>")]

[assembly:AddinDependency ("Core", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency ("Ide", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency ("VersionControl", MonoDevelop.BuildInfo.Version)]