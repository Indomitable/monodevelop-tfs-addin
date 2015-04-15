using MonoDevelop.VersionControl.TFS.VersionControl.Structure;

namespace MonoDevelop.VersionControl.TFS.Infrastructure.Settings
{
    interface IConfigurationService
    {
        void Init(string configurationPath);

        void Save(Configuration configuration);

        Configuration Load();
    }
}