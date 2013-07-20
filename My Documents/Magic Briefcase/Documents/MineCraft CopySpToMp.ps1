param([string] $serverPath= "f:\minecraft")

$private:propPath=join-path $serverPath "server.properties"
if(!(test-path $propPath)) 
    {$(throw "unable to locate server properties file at "+$propPath)}
    
$private:sourceFolder=(get-newestdirectory $env:AppData\.minecraft\saves)  
$folderName=  [System.IO.Path]::GetFileName($sourceFolder.Name)

$targetPath=join-path $serverPath $foldername
echo $targetpath
$exists=test-path ($serverPath + $folderName)



