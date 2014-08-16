# to enable better debugging:
# set-psdebug -strict
# to reload profile . $profile

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
  
function VsVars32($version = "12.0")
{
    if(test-path HKLM:\SOFTWARE\WOW6432NODE){
        $key = "Registry::HKLM\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\" + $version
    } else {
        $key = "Registry::HKLM\SOFTWARE\Microsoft\VisualStudio\" + $version
    }
    $VsKey = get-ItemProperty $key
    $VsInstallPath = [System.IO.Path]::GetDirectoryName($VsKey.InstallDir)
    $VsToolsDir = [System.IO.Path]::GetDirectoryName($VsInstallPath)
    $VsToolsDir = [System.IO.Path]::Combine($VsToolsDir, "Tools")
    $BatchFile = [System.IO.Path]::Combine($VsToolsDir, "vsvars32.bat")
    Get-Batchfile $BatchFile
    [System.Console]::Title = "Visual Studio " + $version + " Windows Powershell"
    #add a call to set-consoleicon as seen below...hm...!
}
function devenv($rootsuffix="Roslyn"){
    VsVars32
    # http://stackoverflow.com/a/23171639/57883
    &"devenv /rootsuffix $rootsuffix"
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

function RecurseSolutionFolderProjects(){
    param(
        [parameter(Position=0,ValueFromPipeline=$True)]
        $solutionFolder = $(throw "Please specify a solutionFolder")
        )
    $projectList = @()
    for($i = 1; $i -le $solutionFolder.ProjectItems.Count; $i++){
        $subProject = $solutionFolder.ProjectItems.Item($i).subProject
        if($subProject -eq $null){
            continue;
        }
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

    $SourceControl = get-interface $dte.SourceControl ([EnvDTE.SourceControl])
    #$sourceControl.IsItemUnderSCC((get-interface $dte.Solution ([EnvDTE80.Solution2])).Projects[0].FullName)

        if($subProject.Kind -eq [EnvDTE80.ProjectKinds]::vsProjectKindSolutionFolder)
        {
            $projectList += RecurseSolutionFolderProjects($subProject)
        } else {
            $projectList += $subProject
        }
    }
    return $projectList
}

function GetProjectFileTypes(){
    param(
        [parameter(Position=0,ValueFromPipeline=$True)]
        $project = $(throw "Please specify a project")
        )
    $files= GetProjectFiles $project
    # http://stackoverflow.com/a/6000217/57883
    write-debug ("item count = " + $files.Count)
    $fileNames = $files | foreach-object { 
        new-object psobject -property @{
                FileName= $_.FileNames(0)
                Extension =[System.IO.Path]::GetExtension($_.FileNames(0))
        }}
    #write-host $fileNames    
    write-debug ("item count = " + $files.Count)
    return $fileNames | Group-Object Extension | %{
        New-Object psobject -Property @{Extension = $_.Name ;Count= $_.Count}
    }
}

function GetSolutionProjects(){
    $projects = get-interface $dte.Solution.Projects ([EnvDTE.Projects])
    write-debug "projects=$projects"
        #$result = new-object "System.Collections.Generic.List[System.Object]"
    $result = new-object "System.Collections.Generic.List[EnvDTE.Project]"
    foreach($project in $projects.GetEnumerator()) {
            if($project -eq $null){
                continue;
            }

            write-debug "yay project or solution folder! $project $project.Kind"
            if($project.Kind -eq [EnvDTE80.ProjectKinds]::vsProjectKindSolutionFolder){
                write-debug -message "Solution folder! '$project.Name'"

                foreach($solutionFolderProject in RecurseSolutionFolderProjects($project)){
                    $result+=$solutionFolderProject
                }

            } else {
                write-debug "else! '$project.Name' '$project.Kind'"
                $result+=$project
            }
    }
    return $result
}

function RecurseDescendants(){
    param(
        [parameter(Position=0,ValueFromPipeline=$True)]
        $source  = $(throw "Please specify a source")
        )
    write-debug "starting RecurseDescendants"
    $result = new-object "System.Collections.Generic.List[EnvDTE.ProjectItem]"
    foreach($s in $source){
        #write-host "working on " $s.Kind $s.Name $s.FileNames(0)

        $pi = $s.Object -as [EnvDTE.ProjectItem]
        $result+=$s
        $children=$s.ProjectItems
        foreach($child in RecurseDescendants($children)){
            $result+=$child
        }
        write-debug "inner for each stopped"
    }
    write-debug "outer for each finished"
    return $result
}

function GetProjectItems(){ 
    param(
        [parameter(Position=0,ValueFromPipeline=$True)]
        $project = $(throw "Please specify a project")
        )
    if($project.ProjectItems.count -gt 0){
        write-debug "getting project items for '$project.Name' '$project.ProjectName'"
    }
    #example: GetProjectItems((GetSolutionProjects).get_Item(1))
    $result =RecurseDescendants($project.ProjectItems)
    return $result
}

function GetProjectFiles(){
    param(
        [parameter(Position=0,ValueFromPipeline=$True)]
        $project = $(throw "Please specify a project")
        )

    write-debug ("getting project files for " + $project.Name + " "+ $project.ProjectName)

    $projectItems = RecurseDescendants($project.ProjectItems)
    return $projectItems | Where-Object {$_.Kind -ne [EnvDTE.Constants]::vsProjectItemKindPhysicalFolder}
}

function GetUnversionedFiles(){
    param(
        [parameter(Position=0,ValueFromPipeline=$True)]
        $items = $(throw "Please specify items")
        )
    #GetUnversionedFiles(getprojectfiles((GetSolutionProjects).get_Item(0))) | format-table
    write-host "checking for unversioned files"

    $SourceControl = get-interface $dte.SourceControl ([EnvDTE.SourceControl])
    return $items | Where-Object {$SourceControl.IsItemUnderSCC($_.FileNames(0)) -eq $FALSE }

}

function CheckProjectForMissingOrUnversioned(){
    param(
        [parameter(Position=0,ValueFromPipeline=$True)]
        $project = $(throw "Please specify a project")
        )

    $SourceControl = get-interface $dte.SourceControl ([EnvDTE.SourceControl])
    $projectFiles = GetProjectFiles($project)
    return $projectFiles | Where-object {$SourceControl.IsItemUnderSCC($_.FileNames(0)) -eq $FALSE -Or ((test-path -path $_.FileNames(0)) -ne $TRUE)}
}
# http://stackoverflow.com/questions/6460854/adding-solution-level-items-in-a-nuget-package
# http://blogs.interfacett.com/working-hierarchical-objects-powershell

# set-alias svc 'sc.exe' # doesn't work as I'd hoped
 function GetServiceRunAs(){
    param($computername = $(throw "please specify a computer name"))
    return Get-WmiObject win32_service -filter "(StartName Like '%$runasuser%')" -ComputerName $computername -ErrorAction Stop | select name,startname
    }