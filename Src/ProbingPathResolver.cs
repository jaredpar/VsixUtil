using System;
using System.IO;
using System.Reflection;

namespace VsixUtil
{
    internal class ProbingPathResolver : IDisposable
    {
        private readonly string Dir;
        private readonly string[] ProbePaths;

        internal ProbingPathResolver(string dir, string[] probePaths)
        {
            Dir = dir;
            ProbePaths = probePaths;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            foreach (var probePath in ProbePaths)
            {
                var path = Path.Combine(Dir, probePath, assemblyName.Name + ".dll");
                if (File.Exists(path))
                {
                    return Assembly.LoadFrom(path);
                }
            }

            return null;
        }
    }
}
