param ( [string]$version = $(throw ("Need a version number")))

$script:scriptPath = split-path -parent $MyInvocation.MyCommand.Definition 
pushd $scriptPath

$msbuild = resolve-path (join-path ${env:SystemRoot} "microsoft.net\framework\v4.0.30319\msbuild.exe")
$nuget = resolve-path ".nuget\NuGet.exe"

& $msbuild /nologo /verbosity:m /t:build Src\VsixUtil.csproj

$scratchPath = "Deploy\Scratch"
if (test-path $scratchPath) {
    rm -re -fo $scratchPath
}

$toolsPath = join-path $scratchPath "tools"
mkdir $toolsPath | out-null 

copy Src\bin\Debug\VsixUtil.exe $toolsPath
& $nuget pack Src\VsixUtil.nuspec -Symbols -Version $version -BasePath $scratchPath -OutputDirectory "Deploy" | out-null

popd
