using Mono.Addins;

[assembly:Addin(
    "VersionControl.TFS", Namespace = "MonoDevelop", 
    Version = "1.7", Category = "Version Control")]

[assembly:AddinName("TFS support")]
[assembly:AddinDescription("TFS support for the Version Control Add-in")]
[assembly:AddinUrl("http://indomitable.github.io/monodevelop-tfs-addin")]
[assembly:AddinAuthor("Ventsislav Mladenov <ventsislav.mladenov@gmail.com>")]

[assembly:AddinDependency("Core", "5.7")]
[assembly:AddinDependency("Ide", "5.7")]
[assembly:AddinDependency("VersionControl", "5.7")]