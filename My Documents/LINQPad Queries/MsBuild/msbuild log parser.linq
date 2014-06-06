<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Framework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.DataAnnotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Utilities.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.Protocols.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Tasks.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.RegularExpressions.dll</Reference>
  <Namespace>System.Web</Namespace>
</Query>

static bool delayLoad=false;
static bool firefoxUnc = true;
static Regex _fileReferenceRegex = new Regex(@"[a-e]:\\\w+(?:[a-z(0-9)\\.]|\s)+(?=\s|""|$|')", RegexOptions.IgnoreCase);
static string _lineEndings =  /*"<br />"+*/ Environment.NewLine;

void Main()
{
	
	//msbuild log file sectioning
	var servers=Environment.GetEnvironmentVariable("servers").Dump().Split(';');
	var buildServer=Util.ReadLine("buildServer?",servers.FirstOrDefault(s=>s.Contains("build", StringComparison.CurrentCultureIgnoreCase)),servers);
		
	var buildsPath = @"\\"+buildServer+"\\c$\\builds";
	var buildPaths = from agentFolder in System.IO.Directory.EnumerateDirectories(buildsPath)
					from teamFolder in System.IO.Directory.EnumerateDirectories(agentFolder)
					from buildFolder in System.IO.Directory.EnumerateDirectories(teamFolder) 
				select new{agentFolder,teamFolder,buildFolder,BuildName=System.IO.Path.GetFileName(buildFolder)};
	var targetBuild=Util.ReadLine("target build?", null, buildPaths.Select(bp=>bp.BuildName).ToArray().Dump()); 
	var buildDefinition =buildPaths.First(x=>x.BuildName == targetBuild);
	var buildSrc = System.IO.Path.Combine( buildDefinition.buildFolder,"src"); // our builds have the msbuild log files going into the src folder for the specific project
	var productFolder = System.IO.Path.Combine(buildSrc,"Products");
	var slnFolder = System.IO.Path.Combine(productFolder,targetBuild.Contains("CV")?"CVS":"MarketOnce");
	
	Debug.Assert(System.IO.Directory.Exists(slnFolder),"Could not locate application slnFolder at "+slnFolder);
	
	//var tempPath=System.IO.Path.GetTempPath();
	
	foreach(var logFile in System.IO.Directory.EnumerateFiles(slnFolder,"*.log", SearchOption.AllDirectories)){ //consider caching previous log file locations and blacklisting debug and bin folders?
		var info = new System.IO.FileInfo(logFile);
		if(info.Length==0){
			info.Dump("Empty logfile"+logFile);
		}
		var lines = System.IO.File.ReadLines(logFile) as IEnumerable<string>;
		IEnumerator<string> enumerator = lines.GetEnumerator();
		var cleaned=Htmlify(logFile,ReadBlock(enumerator,buildServer,null,string.Empty));
		
		var tempFile = System.IO.Path.GetTempFileName().Dump();
		var targetFileName= tempFile+".htm";
		System.IO.File.Move(tempFile,targetFileName);
		System.IO.File.WriteAllText(targetFileName,cleaned);
		var targetFilePath=new My.PathWrapper(targetFileName);
		object openLink;
		if(delayLoad){
		 openLink = Util.OnDemand("OpenFile",
			()=>{
				var p =Process.Start(targetFileName);
				return new{p.Id, p.MainModule.ModuleName}; 
				});
				
		} else {
			var p = Process.Start(targetFileName);
			openLink= new{p.Id, p.MainModule.ModuleName} ;
		}
		
		
		new{ FileLink = targetFilePath.ToAHref(), ExplorerLink =targetFilePath.AsExplorerSelectLink("ExplorerLink"),
		OpenLink= openLink}.Dump();
		
		
		//write out the file as html grouping based on indention?
		
		
	}
	
}

public static string Htmlify(string title,string content){
	return "<!DOCTYPE html>\r\n<html><head>"+Environment.NewLine
		+"<title>"
		+System.Net.WebUtility.HtmlEncode(title)
		+"</title>"
		+ IncludeCss("http://netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css")
		+ IncludeInlineStyleSheet(".warn{ border-bottom:1px solid yellow;}")
		+"</head><body>"
		+Environment.NewLine
		+content
		+IncludeScript("http://netdna.bootstrapcdn.com/bootstrap/3.1.1/js/bootstrap.min.js")
		+"</body></html>";
}

// Define other methods and classes here
public string ReadBlock (IEnumerator<string> lines,string buildServer,string parentLine, string prevIndent){
	var sb = new StringBuilder();
	const string openlist= "<ul>";
	const string closelist =" </ul>";
	var currentOpener = openlist;
	var currentCloser = closelist;
	if(parentLine!=null && parentLine.StartsWith("ForceNugetRestore:")){
		var @class="forceNugetRestore";
		currentOpener=BootstrapToggleButton("."+@class,"Collapse all "+@class)+"<div class='collapse "+@class+"'>"+openlist;
		currentCloser = "</div>"+closelist;
	}
	sb.Append(currentOpener);
	
	while(lines.MoveNext()){ 
		var l = lines.Current;
		var shallower =prevIndent!=string.Empty && l.StartsWith(prevIndent)==false;
		var deeper = !shallower && l.After(prevIndent).IsMatch(@"^\s",false);
		
		if(!deeper && ! shallower) //same indention level
		{	
			
			if(l.Trim().Length>0){
				var classes = new List<string>();
				var glyphs = new List<string>();
				if(l.IsMatch("\\): warning ",true)){
					glyphs.Add("glyphicon-warning-sign");
					classes.Add("warn");
				}
				if(l.IsMatch("Copying",true))
					classes.Add("copy");
				
				
				var lineClass=classes.Any()?" class='"+classes.Delimit(" ") +"'":string.Empty;
				var glyphText = glyphs.Any()?glyphs.Select(g=>"<span class='glyphicon "+g+"'></span>").Delimit(string.Empty):string.Empty;
				
				var reassembledLine = MarkupLine(l,buildServer);
				
				sb.AppendLine("<li"+lineClass+">"+glyphText+reassembledLine+"</li>"+_lineEndings);
				 
					
				
			}
			continue;
		}
		if(shallower){
			if(sb.Length==currentOpener.Length)
				return string.Empty;
			sb.AppendLine(currentCloser+_lineEndings);
			return sb.ToString();
		}
		// must be a deeper level
		var indentLevel = prevIndent==string.Empty? Regex.Match(l,@"\s+").Value: Regex.Match(l.After(prevIndent),@"\s+").Value; // get the whitespace after the current indent level if there is one
		//new {indentLevel, l}.Dump("going deeper!");
		sb.Append(ReadBlock(lines,buildServer,l,indentLevel)+_lineEndings);
	}
	sb.Append(currentCloser+_lineEndings);
	return sb.ToString();
}
public static string MarkupLine(string line,string server){
	//collect every mention of a .targets file then distinct by full path?
	return MarkupFileReferences(line,server);
}

public static string MarkupFileReferences(string line,string server){
	var fileReferences = _fileReferenceRegex.Matches(line);
	if(fileReferences.Count==0)
		return System.Net.WebUtility.HtmlEncode(line);
	var sb= new StringBuilder();
	
	var index=0;
	foreach(Match fr in fileReferences){
		if(index<fr.Index){
			sb.Append(System.Net.WebUtility.HtmlEncode( line.Before(fr.Value)));
			index=fr.Index;
		}
		var cleanedPath = System.IO.Path.GetFullPath(fr.Value); // eliminate c:\abc\..\def type stuff
		//firefox doesn't like this http://kb.mozillazine.org/Links_to_local_pages_don%27t_work
		var uncLinkStyle ="file://"+ (firefoxUnc? "///":string.Empty);
		
		var linkPath = System.IO.Directory.Exists(cleanedPath) || System.IO.File.Exists(cleanedPath) ? //let link point locally if file exists locally, otherwise point out to server filesystem
			cleanedPath: (server+"\\"+cleanedPath.Replace(":\\","$\\"));
		sb.Append("<a href='"+uncLinkStyle
			+HttpUtility.HtmlAttributeEncode(linkPath.Replace('\\','/')) +"'" 
			+" title='"+HttpUtility.HtmlAttributeEncode(fr.Value)+"'>"
			+System.Net.WebUtility.HtmlEncode(linkPath)
			+"</a>");
		index+=fr.Length;
	}
	if(index<line.Length){
		sb.AppendLine(System.Net.WebUtility.HtmlEncode(line.Substring(index))+_lineEndings);
	} else {
		sb.AppendLine(_lineEndings);
	}
	return sb.ToString();
}

public static string BootstrapToggleButton(string selector,string text){  //http://getbootstrap.com/javascript/#collapse
	return "<a data-toggle='collapse' data-target='"+selector+"'>"+HttpUtility.HtmlEncode(text)+"</a>";
}

public static string IncludeCss(string href){
	return Include("link",new Dictionary<string,string>{ {"rel","stylesheet"},{"href",href}},true);
}
public static string IncludeInlineStyleSheet(string content){
	return Include("style",null,content);
}
public static string IncludeScript(string src){
	return Include("script",new Dictionary<string,string>{ {"src",src}},false);
}

public static string Include(string tag, IDictionary<string,string> attributes, string content){
	var closing="</"+tag+">";
	var attributesText = attributes==null? string.Empty:(" "+ attributes.Select(x=>x.Key+"='"+HttpUtility.HtmlAttributeEncode(x.Value)+"'").Delimit(" "));
	return "<"+tag+attributesText +">"+content+ closing;
}
/// <summary>
/// for self-closing links or links with no content
/// </summary>
public static string Include(string tag, IDictionary<string,string> attributes, bool selfClose){
	
	var closing = selfClose?" />":("></"+tag+">");
	var attributesText = attributes==null?string.Empty: attributes.Select(x=>x.Key+"='"+HttpUtility.HtmlAttributeEncode(x.Value)+"'").Delimit(" ");
	return "<"+tag+" " +attributesText+ closing;
}