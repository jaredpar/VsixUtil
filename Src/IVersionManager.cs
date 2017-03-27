namespace VsixUtil
{
    internal interface IVersionManager
    {
        void Run(string appPath, Version version, string rootSuffix, ToolAction toolAction, string arg);
    }
}
