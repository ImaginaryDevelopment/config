$private:JavaPath='C:\Program Files (x86)\Java\jre6\bin\java.exe'
if(test-path $javaPath){set-alias java $javaPath}

function Get-NewestDirectory{
param (
    [string] $path = $(throw "Path is required")
)
    gci $path `
    | ? {$_.PSIsContainer} `
    | select Name,LastWriteTime `
    | sort-object lastwriteTime -descending `
    | select-object -first 1
}
function CreateNewZip{
#http://blogs.msdn.com/b/stuartleeks/archive/2009/08/13/powershell-script-to-clean-and-zip-a-directory.aspx
param (
     [Parameter(Mandatory=$true)]
     [string] $zipPath
    )
    if(test-path $zipPath) {$(throw "Zip file already exists at "+$zipPath); return}
    set-content $zipPath ("PK" + [char]5 + [char]6 + ("$([char]0)" * 18))
    (dir $Zippath).IsReadOnly= $false
    #if((dir $zipfi
    return (new-object -com shell.application).NameSpace($zipPath)
    }