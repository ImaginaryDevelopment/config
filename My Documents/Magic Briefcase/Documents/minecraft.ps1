function mineCraftAs {
    Param (
        [parameter(mandatory=$true, HelpMessage="Minecraft character name." ,ValueFromPipeline=$true)]
        [string] $name
    )
    if(!(test-path $env:appdata)) { $(throw "Appdata not found at $env:appdata")}
    $private:minecraftPath=Join-Path $env:appdata .minecraft
    if(!(test-path $minecraftPath)) { $(throw "Minecraft not found at $minecraftpath")}
    $private:minebinPath=join-path $minecraftPath "bin"
    if(!(test-path $minebinPath)) { $(throw "Minecraft bin not found at $minebinPath")}

    $minebinPath | write-debug
    gci $minebinpath | write-debug
    
    #java -Xms512m -Xmx1024m -cp "%APPDATA%/.minecraft\bin\*" -Djava.library.path="%APPDATA%\.minecraft\bin\natives" net.minecraft.client.Minecraft '"'%1'"'
    
    echo java -Xms512m -Xmx1024m  -cp ('"'+$minebinPath+'\*"') ('-Djava.library.path="'+$minebinPath+'\natives"') net.minecraft.client.Minecraft ($name)
    
    $minecraftJob=& 'C:\Program Files (x86)\Java\jre6\bin\java.exe' -Xms512m -Xmx1024m  -cp ('"'+$minebinPath+'\*"') ('-Djava.library.path="'+$minebinPath+'\natives"') net.minecraft.client.Minecraft ($name)
}
minecraftas newbie

