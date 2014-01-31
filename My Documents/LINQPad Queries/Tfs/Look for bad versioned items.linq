<Query Kind="Statements">
  <Connection>
    <ID>3eb60221-3a72-4eab-b9dd-348fb1dccb31</ID>
    <Server>svrrbidevdb03</Server>
    <Database>Tfs_Configuration</Database>
  </Connection>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft Visual Studio 12.0\Common7\IDE\ReferenceAssemblies\v2.0\Microsoft.TeamFoundation.Client.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft Visual Studio 12.0\Common7\IDE\ReferenceAssemblies\v2.0\Microsoft.TeamFoundation.VersionControl.Client.dll</Reference>
  <Namespace>Microsoft.TeamFoundation.Client</Namespace>
  <Namespace>Microsoft.TeamFoundation.VersionControl.Client</Namespace>
</Query>

var tfs=new Microsoft.TeamFoundation.Client.TfsTeamProjectCollection(new Uri("https://tfs.oceansideten.com"));
var vcs=tfs.GetService<VersionControlServer>();
//vcs.GetItems("*.user", RecursionType.Full).Dump();
var bads= new[]{"**.user","**.suo","**resharper**","**.DS_Store"};
var tp=vcs.GetTeamProject("Development");
//var dev=vcs.GetItem("$/Development");
var badItems=new List<Item>();
foreach(var item in bads){
	var items = vcs.GetItems("$/Development/"+item, RecursionType.Full);
	badItems.AddRange(items.Items);
	items.Items.Select (i => i.ServerItem).Dump();	
}

//http://en.wikipedia.org/wiki/.DS_Store

var myPending= vcs.QueryWorkspaces(null,Environment.UserName,Environment.MachineName).FirstOrDefault ();
if(myPending==null)
return;

myPending.GetPendingChanges().Select (p => new{p.ChangeType, p.ServerItem}).Dump(1);
foreach(var i in badItems){
	myPending.PendDelete(i.ServerItem).DumpIf(a=>a!=0, "Pend delete error code for "+i.ServerItem);
}

//http://msdn.microsoft.com/en-us/magazine/jj553516.aspx