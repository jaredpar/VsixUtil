namespace VsixUtil
{
    public struct CommandLine
    {
        internal static readonly CommandLine Help = new CommandLine(ToolAction.Help, null, "", "", "");

        public readonly ToolAction ToolAction;
        public readonly VsVersion? VsVersion;
        public readonly string Product;
        public readonly string RootSuffix;
        public readonly bool DefaultDomain;
        public readonly string Arg;

        public CommandLine(ToolAction toolAction = ToolAction.Help, VsVersion? vsVersion = null, string product = null,
            string rootSuffix = null, string arg = null, bool defaultDomain = false)
        {
            ToolAction = toolAction;
            VsVersion = vsVersion;
            Product = product;
            RootSuffix = rootSuffix;
            DefaultDomain = defaultDomain;
            Arg = arg;
        }
    }
}
