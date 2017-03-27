namespace VsixUtil
{
    internal struct CommandLine
    {
        internal static readonly CommandLine Help = new CommandLine(ToolAction.Help, null, null, "", "");

        internal readonly ToolAction ToolAction;
        internal readonly string RootSuffix;
        internal readonly string Version;
        internal readonly string[] Skus;
        internal readonly string Arg;

        internal CommandLine(ToolAction toolAction, string version, string[] skus, string rootSuffix, string arg)
        {
            ToolAction = toolAction;
            Version = version;
            Skus = skus;
            RootSuffix = rootSuffix;
            Arg = arg;
        }
    }
}
