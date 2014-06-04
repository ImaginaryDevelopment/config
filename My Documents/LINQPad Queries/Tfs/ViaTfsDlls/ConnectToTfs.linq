<Query Kind="Statements">
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft Visual Studio 12.0\Common7\IDE\ReferenceAssemblies\v2.0\Microsoft.TeamFoundation.Client.dll</Reference>
  <Reference>&lt;ProgramFilesX86&gt;\Microsoft Visual Studio 12.0\Common7\IDE\ReferenceAssemblies\v2.0\Microsoft.TeamFoundation.VersionControl.Client.dll</Reference>
  <Namespace>Microsoft.TeamFoundation.Client</Namespace>
  <Namespace>Microsoft.TeamFoundation.VersionControl.Client</Namespace>
</Query>

var tfs=new Microsoft.TeamFoundation.Client.TfsTeamProjectCollection(new Uri("https://tfs.oceansideten.com"));
var vcs=tfs.GetService<VersionControlServer>();
vcs.Dump();
//vcs.GetItems("*.user", RecursionType.Full).Dump();
var tp=vcs.GetTeamProject("Development");
var dev=vcs.GetItem("$/Development");
var items = vcs.GetItems("$/Development/**.user", RecursionType.Full);
items.Items.Select (i => i.ServerItem).Dump();