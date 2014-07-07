# to enable better debugging:
# set-psdebug -strict


function GetPSLoadedAssemblies{
    #also Get-PSSnapin
    [appdomain]::currentdomain.getassemblies() | sort -property fullname
}
function Get-Batchfile ($file) {
    $cmd = "`"$file`" & set"
    cmd /c $cmd | Foreach-Object {
        $p, $v = $_.split('=')
        Set-Item -path env:$p -value $v
    }
}
  
function VsVars32($version = "10.0")
{
    $key = "HKLM:SOFTWARE\Microsoft\VisualStudio\" + $version
    $VsKey = get-ItemProperty $key
    $VsInstallPath = [System.IO.Path]::GetDirectoryName($VsKey.InstallDir)
    $VsToolsDir = [System.IO.Path]::GetDirectoryName($VsInstallPath)
    $VsToolsDir = [System.IO.Path]::Combine($VsToolsDir, "Tools")
    $BatchFile = [System.IO.Path]::Combine($VsToolsDir, "vsvars32.bat")
    Get-Batchfile $BatchFile
    [System.Console]::Title = "Visual Studio " + $version + " Windows Powershell"
    #add a call to set-consoleicon as seen below...hm...!
}

function Invoke-SQL {
    param(
     [string] $dataSource = $(throw "Please specify a dataSource"),
        [string] $database = $(throw "Please specify a database."),
        [string] $sqlCommand = $(throw "Please specify a query.")
      )

    $connectionString = "Data Source=$dataSource; " +
            "Integrated Security=SSPI; " +
            "Initial Catalog=$database"

    $connection = new-object system.data.SqlClient.SQLConnection($connectionString)
    $command = new-object system.data.sqlclient.sqlcommand($sqlCommand,$connection)
    $connection.Open()

    $adapter = New-Object System.Data.sqlclient.sqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataSet) | Out-Null

    $connection.Close()
    $dataSet.Tables

}

function Invoke-Sql-SpHelpTextSelectedText{
    param(
           [string] $dataSource = $(throw "Please specify a dataSource"),
            [string] $database = $(throw "Please specify a database.")
        )
    $sqlCommand = "sp_helptext '"+$dte.ActiveDocument.Selection.Text+"'"
    Invoke-SQL $dataSource $database $sqlCommand
}

function Set-FontSize {
    param(
        [ValidateRange(6, 128)]
        [Parameter(position=0, mandatory=$true)]
        [int]$Size
    )
   $dte.Properties("FontsAndColors", "TextEditor").Item("FontSize").Value = $Size
}
function ReloadProfile{
    . $profile
}

$SolutionFolderGuid="{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
$ProjectGuid="{66A26722-8FB5-11D2-AA7E-00C04F688DDE}";
function GetSolutionProjects {
    param(
        $SourceControl = (get-interface $dte.SourceControl ([EnvDTE.SourceControl])),
        $sln2 = (get-interface $dte.Solution ([EnvDTE80.Solution2])),
        [Parameter(Mandatory=$false)]$items
    )
    #recursive
    if(!($items)){
        $items =[Array]$sln2.Projects
    }
    write-host "sln2 = $sln2" "SourceControl=$SourceControl" "items=$items" "projects = $sln2.Projects"
    #excludes solution folders and solution items
    $containers = New-Object System.Collections.Generic.List[EnvDTE.ProjectItems]
    foreach($item in $items){
        $project =get-interface $item ([EnvDTE.Project])
        $projectItem = get-interface $item ([EnvDTE.ProjectItem])

        if($project -ne $null){
            $projectItems =get-interface $project.ProjectItems ([EnvDTE.ProjectItems])
            $containers.Add($projectItems)
            if($project.SubProject -ne $null){
                write-host "sub project! " $project.SubProject
            }
        }
        if($projectItem -ne $null -and $projectItem.ProjectItems.count -gt 0){
            $containers.Add($projectItem.ProjectItems)
        }
    }
    write-host $containers
    
    for($i=1; $i -lt $containers.count; $i++){
        # $[EnvDTE.Constants] | get-member -static
        write-host "index is " $i
        $item = $containers[$i]
        switch($item.Kind.ToString()){ # http://geekswithblogs.net/michelotti/archive/2011/03/13/package-manager-console-for-more-than-managing-packages.aspx
            $SolutionFolderGuid {
                $sfold = get-interface $item.Object ([EnvDTE80.SolutionFolder])
                write-host "solution folder" $item.Name $item.Kind
                write-host $sFold
                $containers += $item.ProjectItems
                $subProject = $item.SubProject
                if($subProject -ne $null){
                    write-host "subproject!" $subProject.Name $subProject.Kind
                }
            }
            # http://www.mztools.com/articles/2006/mz2006004.aspx
            [EnvDTE.Constants]::vsProjectItemKindSubProject{
                write-host "subproject " $item.Name $item.Kind
            }
            [EnvDTE.Constants]::vsProjectItemKindPhysicalFile{
                write-host "physicalFile " $item.Name $item.Kind
            }
            $ProjectGuid {
                write-host "project " $item.Name $item.Kind
            }
            default {
                write-host "unknown" $item.Name $item.Kind
            }
        }
    }
    return $projects
}

# http://stackoverflow.com/questions/6460854/adding-solution-level-items-in-a-nuget-package
# http://blogs.interfacett.com/working-hierarchical-objects-powershell
function GetUnversionedItems {
    $sln2 = get-interface $dte.Solution ([EnvDTE80.Solution2])
    $SourceControl = get-interface $dte.SourceControl ([EnvDTE.SourceControl])
    #$sourceControl.IsItemUnderSCC((get-interface $dte.Solution ([EnvDTE80.Solution2])).Projects[0].FullName)
    $items = @()
    $items += $sln2.Projects
    write-host "starting with " $items.Length
    for($i =0; $i -lt $items.count; $i++)
    {

        $item=$items[$i]
        if($item.ProjectItems.count -gt 0)
        {

            $items += $item.ProjectItems
        }
        $kind = $item.Kind.ToString()
        switch($kind){
            $SolutionFolderGuid {write-host "Solution folder!" $item.Name $item.ProjectItems.Length }
            $ProjectGuid {write-host "Project" $item.Name}
            default {
                write-host $item.Name " kind " $item.Kind
            }
        }
        # write-host "checking " $item.Name " with kind " $kind.ToString() " against " $SolutionFolderGuid.ToString() " and " $ProjectGuid
        if($kind -eq $SolutionFolderGuid.ToString()){
            $slnFolder = get-interface $item ([EnvDTE80.SolutionFolder])
                write-host "SolutionFolder" $item.Name $slnFolder
                
        }
        if($item.Kind.ToString() -eq $ProjectGuid.ToString())
        {
                $project = get-interface $item ([EnvDTE.Project])
                write-host "ProjectName" $item.Name $project
                if($project -eq $null){
                    continue
                }
                $items += $project.ProjectItems
                write-host "ProjectName" $item.Name $project
                if($item.IsItemUnderScc -ne $TRUE ){
                    write-host $item.Properties
                    if($item.FileCount -gt 0){

                    write-host $item.Name $item.IsItemUnderScc $item.ProjectItems.count
                    write-host $project.ProjectItems([int16]0) "104 pass!"
                    } else {
                        write-host $item.Name $item.IsItemUnderScc $item.FileNames([byte]0) "105 pass!"
                    }
                }
        }
         
                # write-host "odd kind " $item.Name $item.Kind
                #if($item.IsItemUnderScc -ne $TRUE ){
                #    write-host $item.Name "unmatched guid" $item.Kind
                #}
            
        
        if($item.Name -ne "Miscellaneous Files"){
            Foreach($projectItem in $item.ProjectItems){ 
                # does not account for the possibility one of these items is a solution folder
                if($projectItem.Kind.ToString() -ne $SolutionFolderGuid){

                    if($projectItem.IsItemUnderScc -ne $TRUE ){
                        write-host "unversioned project item" $item.Name $projectItem.Name
                    }
                }
            }
        }    
    }
    write-host "Processed " $items.count
}