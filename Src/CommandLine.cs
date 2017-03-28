namespace VsixUtil
{
    public struct CommandLine
    {
        internal static readonly CommandLine Help = new CommandLine(ToolAction.Help, "", "", "");

        public readonly ToolAction ToolAction;
        public readonly string RootSuffix;
        public readonly string Version;
        public readonly string Arg;

        internal CommandLine(ToolAction toolAction, string version, string rootSuffix, string arg)
        {
            ToolAction = toolAction;
            Version = version;
            RootSuffix = rootSuffix;
            Arg = arg;
        }
    }
}
