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
    //add a call to set-consoleicon as seen below...hm...!
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