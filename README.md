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
|VS Snippets | Open the VS snippet storage folder | C:\Windows\explorer.exe | "C:\Program Files (x86)\Microsoft Visual Studio 11.0\VC#\Snippets\1033\Visual C#" | |
| ExplorerCurrentOpenFile | Open explorer with the current file already selected | explorer.exe| /select, "$(ItemPath)"


