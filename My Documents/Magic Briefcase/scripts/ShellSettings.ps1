
#map the hkcr drive
if(!(Test-Path "hkcr:")){
    New-PSDrive -Name HKCR -PSProvider Registry -Root HKEY_CLASSES_ROOT
    get-psdrive
}
#make powershell open with powershell instead of notepad

{    
    gp "hkcr:\Microsoft.powershellscript.1\shell\open\command"

    #remove existing bad key if needed

    if(!((gp "hkcr:\Microsoft.powershellscript.1\shell\open\command" | select "(default)") -contains "powershell.exe")){
        Remove-Item Registry::hkey_classes_root\microsoft.powershellscript.1\shell\open\command
    }

    new-item Registry::hkey_classes_root\microsoft.powershellscript.1\shell\open\command -value ('"' + $PSHOME + '\powershell.exe" -command "& ''%1''"')

    gp "hkcr:\Microsoft.powershellscript.1\shell\open\command"
}

#get sln files to have a run as admin option
gci "hkcr:\VisualStudio.Launcher.sln\Shell\"

gp Registry::hkey_classes_root\VisualStudio.Launcher.sln\shell\runas\command
new-item Registry::hkey_classes_root\VisualStudio.Launcher.sln\shell\runas
new-item Registry::hkey_classes_root\VisualStudio.Launcher.sln\shell\runas\command -value ('"C:\Program Files (x86)\Common Files\microsoft shared\MSEnv\VSLauncher.exe" "''%1''"')
gp Registry::hkey_classes_root\VisualStudio.Launcher.sln\shell\runas\command
