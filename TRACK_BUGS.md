How to track bugs
=================
[MonoDevelop]: http://monodevelop.com/
[XamarinStudio]: http://xamarin.com/studio 'Xamarin Studio'
### The best way to track bugs is using debugging. 
1. Go to [monodevelop-tfs-addin](https://github.com/Indomitable/monodevelop-tfs-addin) and click on **Fork** button.
2. If you use Mac OS open the terminal and type `git clone url_to_you_repository`
3. `cd monodevelop-tfs-addin`
4. Download monodevelop and its submodules `git submodule update --init --recursive` - this will take some time.
5. Create new branch where to debug and fix the problem: `git checkout -b BranchName`
6. Open **MonoDevelop.VersionControl.TFS.sln** file using [XamarinStudio] or [MonoDevelop]. 
7. For **Mac** and **Windows** select _Project -> Solution Options -> Build -> Configurations -> Configuration Mappings_ then for MonoDevelop.VersionControl.TFS project select configuration **Debug No-DBus**. I use DBus under **Linux/KDE** for talking with KWallet, but for Mac and Windows I use stardard [MonoDevelop] function for storing password securely.
8. Build Solutuion.
9. Before starting debug you have to build MonoDevelop: go to [MonoDevelop] folder `cd External/monodevelop/` and type `./configure && make` this will create [MonoDevelop] binaries.
10. Finally from the menu select `Run -> Debug Application...` choose _External/monodevelop/main/build/bin/_ folder and select **MonoDevelop.exe**

Happy debugging... :)