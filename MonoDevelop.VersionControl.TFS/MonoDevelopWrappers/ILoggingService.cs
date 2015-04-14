namespace MonoDevelop.VersionControl.TFS.MonoDevelopWrappers
{
    interface ILoggingService
    {
        bool IsDebugMode { get; }
        void LogToDebug(string message);
        void LogToInfo(string message);
    }
}
