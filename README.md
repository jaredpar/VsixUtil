VsixUtil
====
[![Build status](https://ci.appveyor.com/api/projects/status/7xcrh26yq6lfqabw)](https://ci.appveyor.com/project/jaredpar/vsixutil)

This is a command line tool for managing VSIX installations.  It allows for install, uninstall and listing of extensions.  It supports 2010 - 2013 (and Dev14).  

- Install: `vsixutil [/rootSuffix name] /install vsixFilePath`
- Uninstall: `vsixutil [/rootSuffix name] /uninstall identifier`
- List installed: `vsixutil [/rootSuffix name] /list [filter]`

This is a replacement for vsixInstaller which is shipped with Visual Studio.  That tool is asynchronous which makes in unsuitable for scripting and fast turn around dogfooding
