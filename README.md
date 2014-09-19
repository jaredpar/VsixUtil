VsixUtil
====
[![Build status](https://ci.appveyor.com/api/projects/status/7xcrh26yq6lfqabw)](https://ci.appveyor.com/project/jaredpar/vsixutil)

This is a command line tool for managing VSIX installations.  It allows for install, uninstall and listing of extensions.  It supports 2010 - 2013.  

- Install VSIX: `vsixutil [/rootSuffix name] /install vsixFilePath`
- List installed: `vsixutil [/rootSuffix name] /list [filter]`

This is a replacement for the command line shipped with Visual Studio vsixInstaller.  That tool is asynchronous and hence cannot be used reliably in scripting.  
