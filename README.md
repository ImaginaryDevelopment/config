config
======

my configurations!
For instance this folder may be 
c:\projects\config
with links made to
```Batchfile
c:\projects\config>mkdir userprofile
c:\projects\config>mklink /j "My Documents" "%USERPROFILE%\documents"
```

Visual Studio External Tools Snippets
========
|Name | Purpose | Cmd | Args | Image |
| --- | --------| ----| ---- | ----- |
|VS Snippets | Open the VS snippet (built-in) storage folder | C:\Windows\explorer.exe | "C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC#\Snippets\1033\Visual C#" | |
| ExplorerCurrentOpenFile | Open explorer with the current file already selected | explorer.exe| /select, "$(ItemPath)"
|MSBuild| MsBuild the solution | %windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe | /m $(SolutionFileName) /v:m |
|MsBuild2| MsBuild the solution with perf summary | %windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe | /m:2 $(SolutionFileName) /clp:PerformanceSummary=true |
| MsBuildProj | MsBuild the current project | %windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe | /m:2 $(ProjectFileName) /p:MvcBuildViews=true | 
|WCFTestClient | open the VS built-in WCF test client | C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\WcfTestClient.exe
|MySnippets| open the VS snippet (my snippets) folder | explorer.exe | "%USERPROFILE%\Documents\Visual Studio 2012\Code Snippets\Visual C#\My Code Snippets"
| IISReset | to reset your local IIS if you are using it instead of IisExpress | C:\Windows\System32\iisreset.exe| /restart
| VS Cmd | open a visual studio command prompt | c:\Windows\System32\cmd.exe |  /k "%programfiles(x86)%\Microsoft Visual Studio 11.0\Common7\Tools\VsDevCmd.bat"
