namespace VsixUtil
{
    public struct CommandLine
    {
        internal static readonly CommandLine Help = new CommandLine(ToolAction.Help, null, "", "");

        public readonly ToolAction ToolAction;
        public readonly string RootSuffix;
        public readonly VsVersion? VsVersion;
        public readonly string Arg;

        internal CommandLine(ToolAction toolAction, VsVersion? vsVersion, string rootSuffix, string arg)
        {
            ToolAction = toolAction;
            VsVersion = vsVersion;
            RootSuffix = rootSuffix;
            Arg = arg;
        }
    }
}
