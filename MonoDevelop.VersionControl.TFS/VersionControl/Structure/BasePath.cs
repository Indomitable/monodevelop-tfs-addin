namespace MonoDevelop.VersionControl.TFS.VersionControl.Structure
{
    abstract class BasePath
    {
        protected string Path;
        
        public abstract bool IsDirectory { get; }

        public static implicit operator string(BasePath path)
        {
            return path.Path;
        }
    }
}
