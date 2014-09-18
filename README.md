VsixUtil
====

This is a command line tool for managing VSIX installations.  It allows for install, uninstall and listing of extensions.  It supports 2010 - 2013.  

- Install: `vsixutil [/install[+]] {vsixFilePath}`

This is a replacement for the command line shipped with Visual Studio vsixInstaller.  That tool is asynchronous and hence cannot be used reliably in scripting.  
