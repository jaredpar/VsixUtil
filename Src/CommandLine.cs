namespace VsixUtil
{
    public struct CommandLine
    {
        internal static readonly CommandLine Help = new CommandLine(ToolAction.Help, null, null, "", "");

        public readonly ToolAction ToolAction;
        public readonly string RootSuffix;
        public readonly string Version;
        public readonly string[] Skus;
        public readonly string Arg;

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
