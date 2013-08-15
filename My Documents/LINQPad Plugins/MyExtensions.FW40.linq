<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.InteropServices.dll</Reference>
  <Namespace>Microsoft.Win32.SafeHandles</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

void Main()
{
	// Write code to test your extensions here. Press F5 to compile and run.
	
	
	Debug.Assert( @"C:\program files\".AsFilePath().GetSegments().Count()==2);
	Debug.Assert( @"C:\program files".AsFilePath().GetSegments().Count()==2);
	Debug.Assert( @"\\vbcdapp1\c$".AsFilePath().GetSegments().Count()==2,"GetSegments on a network path");
	Debug.Assert( @"\\vbcdapp1\c$\".AsFilePath().GetSegments().Count()==2,"GetSegments on a network path");
	Debug.Assert( 1.To(10).Aggregate((x,y)=>x+y)==1+2+3+4+5+6+7+8+9);
	Debug.Assert(2.To(10).Aggregate((x,y)=>x+y)== 2+3+4+5+6+7+8+9);
}
///http://codebetter.com/patricksmacchia/2010/06/28/elegant-infoof-operators-in-c-read-info-of/
public static class LinqOp{
	public static MethodInfo MethodOf<T>(Expression<Func<T>> expression) {
 
      var body = (MethodCallExpression)expression.Body;
 
      return body.Method;
 
   }
	public static MethodInfo MethodOf(Expression<Action> expression) {
 
      var body = (MethodCallExpression)expression.Body;
 
      return body.Method;
 
   }
 
   public static ConstructorInfo ConstructorOf<T>(Expression<Func<T>> expression) {
 
      var body = (NewExpression)expression.Body;
 
      return body.Constructor;
 
   }
 
   public static PropertyInfo PropertyOf<T>(Expression<Func<T>> expression) {
 
      var body = (MemberExpression)expression.Body;
 
      return (PropertyInfo)body.Member;
 
   }
 
   public static FieldInfo FieldOf<T>(Expression<Func<T>> expression) {
 
      var body = (MemberExpression)expression.Body;
 
      return (FieldInfo)body.Member;
 
   }

}

public static class MyExtensions
{
// Write custom extension methods here. They will be available to all queries.
	public static TimeSpan Minutes(this int m){
		return TimeSpan.FromMinutes(m);
	}
	public static string ToTime(this DateTime dt){
		return dt.ToString().After(" ");
	}
	

	public static bool NextBool(this Random rnd)
	{
		return rnd.NextDouble()>0.5;
	}

	public static StreamOuts RunProcessRedirected(this Process ps, string arguments)
	{
		ps.StartInfo.Arguments=arguments;
		ps.Start();
		var output=ps.StandardOutput.ReadtoEndAndDispose();
		var errors=ps.StandardError.ReadtoEndAndDispose();
	
		ps.WaitForExit(2000);
		if(errors.Length>0) 	Util.Highlight(errors).Dump("errors");
		return new StreamOuts(){ Errors=errors, Output=output };
	}
	
	#region xml related
	
	public static string GetAttribValOrNull(this XElement node, XName name){
		var xa= node.Attribute(name);
		if(xa==null)
		return null;
		return xa.Value;
	}
	
	 public static XElement GetXElement(this XmlNode node)
	{
		XDocument xDoc = new XDocument();
		using (XmlWriter xmlWriter = xDoc.CreateWriter())
			node.WriteTo(xmlWriter);
		return xDoc.Root;
	}

	public static XmlNode GetXmlNode(this XElement element)
	{
		using (XmlReader xmlReader = element.CreateReader())
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(xmlReader);
			return xmlDoc;
		}
	}
	 /// <summary>
	/// Get the absolute XPath to a given XElement
	/// (e.g. "/people/person[6]/name[1]/last[1]").
	/// </summary>
	public static string GetAbsoluteXPath(this XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}

		Func<XElement, string> relativeXPath = e =>
		{
			int index = e.IndexPosition();
			string name = e.Name.LocalName;

			// If the element is the root, no index is required

			return (index == -1) ? "/" + name : string.Format
			(
				"/{0}[{1}]",
				name, 
				index.ToString()
			);
		};

		var ancestors = from e in element.Ancestors()
						select relativeXPath(e);

		return string.Concat(ancestors.Reverse().ToArray()) + 
			   relativeXPath(element);
	}

	/// <summary>
	/// Get the index of the given XElement relative to its
	/// siblings with identical names. If the given element is
	/// the root, -1 is returned.
	/// </summary>
	/// <param name="element">
	/// The element to get the index of.
	/// </param>
	public static int IndexPosition(this XElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}

		if (element.Parent == null)
		{
			return -1;
		}

		int i = 1; // Indexes for nodes start at 1, not 0

		foreach (var sibling in element.Parent.Elements(element.Name))
		{
			if (sibling == element)
			{
				return i;
			}

			i++;
		}

		throw new InvalidOperationException
			("element has been removed from its parent.");
	}
	
	#endregion
	
	public static string ReadtoEndAndDispose(this StreamReader reader)
	{
		using(System.IO.StreamReader r=reader)
		{
		return r.ReadToEnd();
		}
	}
	//public static class StringExtensions
	#region StringExtensions

	public static bool IsMatch(this string text, string pattern, bool ignoreCase)
	{
    	return ignoreCase ? Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase) :
        Regex.IsMatch(text, pattern);
	}
	public static byte[] ToByteArray(this string text)
	{
	    var encoding = new ASCIIEncoding();
		return encoding.GetBytes(text);
	}
	///<summary>
	///Assumes Ascii!
	///</summary>
	public static String ToStringFromBytes(this byte[] buffer)
	{
		var encoding = new ASCIIEncoding();
	    return encoding.GetString(buffer);
	}
	public static FilePathWrapper AsFilePath(this string path){
		return new FilePathWrapper(path);
	}
	public static DirectoryPathWrapper AsDirPath(this string path){
		return new DirectoryPathWrapper(path);
	}
	
	
	public static string WrapWith(this string txt, string delimiter){
		if(txt==null)
		return txt;
		return delimiter+txt+delimiter;
	}
	public static bool Contains(this string text,string value,StringComparison comparison){
		return text.IndexOf(value,comparison)>=0;
	}
	public static string[] SplitLines(this string text)
	{
		return text.Split(new string[] {"\r\n","\n"}, StringSplitOptions.None);
	}
	public static T? ParseEnum<T>(this String enumString) where T : struct
	{
		if (Enum.IsDefined(typeof(T), enumString))
			return (T)Enum.Parse(typeof(T), enumString);
		return new T?();
	}
	public static string RemoveMultipleWhitespaces(this string text)
	{
		return Regex.Replace(text,"\\s\\s+"," ");
	}
	/// <summary>
	/// Join a list of strings with a separator
	/// From BReusable
	/// </summary>
	/// <param name="l"></param>
	/// <param name="seperator"></param>
	/// <returns></returns>
	public static String DelimitLarge(this IEnumerable<string> l, string separator)
	{
		var counter = 0;
	
		var result = new StringBuilder();
		foreach (var item in l)
		{
			if (counter != 0) result.Append(separator);
			result.Append(item);
			counter++;
		}
		return result.ToString();
	}
	
	public static string Before(this string text, string delimiter,StringComparison? comparison=null)
	{
		
		return text.Substring(0,text.IndexOf(delimiter,comparison?? StringComparison.CurrentCulture));
	}
	
	public static string BeforeOrSelf(this string text, string delimiter)
	{
		if(text.Contains(delimiter)==false)
			return text;
		return text.Before(delimiter);
	}
	public static string AfterLast(this string text, string delimiter,StringComparison? comparison=null)
	{
		return text.Substring(text.LastIndexOf(delimiter, comparison?? StringComparison.CurrentCulture)+delimiter.Length);
	}
	public static string AfterLastOrSelf(this string text, string delimiter)
	{
		if(text.Contains(delimiter)==false)
		return text;
		return text.AfterLast(delimiter);
	}
	
	
	public static string AfterOrSelf(this string text, string delimiter)
	{
		if(text.Contains(delimiter)==false)
		return text;
		return text.After(delimiter);
	}
	
	// Write custom extension methods here. They will be available to all queries.
	public static bool IsNullOrEmpty(this string s)
	{
		return string.IsNullOrEmpty(s);
	}
	public static bool HasValue(this string s)
	{
		return !s.IsNullOrEmpty();
	}
	public static string After(this string text, string delimiter,StringComparison? comparison=null)
	{
		return text.Substring( text.IndexOf(delimiter,comparison?? StringComparison.CurrentCulture)+delimiter.Length);
	}
	public static int StrComp(this String str1, String str2, bool ignoreCase)
	{
		return string.Compare(str1, str2, ignoreCase);
	}
	public static bool IsIgnoreCaseMatch(this string s, string comparisonText)
    {
        return s.StrComp(comparisonText, true) == 0;
    }
	#endregion StringExtensions

	//public static class EnumerableExtensions
	#region EnumerableExtensions
	
	public static IEnumerable<int> CumulativeSum(this IEnumerable<int> source){
	//http://stackoverflow.com/a/4831908/57883
		int sum=0;
		foreach(var item in source){
			sum+=item;
			yield return sum;
		}
	}
	///<summary>
	/// MyExtensions! Delimit aggregate a list of strings
	///</summary>
	public static string Delimit(this IEnumerable<string> values, string delimiter)
	{
		return values.Aggregate ((s1,s2)=>s1+delimiter+s2);
	}
	
	public static IEnumerable<T> Materialize<T>(this IEnumerable<T> set)
	{
		return set.ToArray();
	}
	
	public static IEnumerable<T> Prepend<T>(this IEnumerable<T> values, T head){
		return new[]{head}.Concat(values);
	}
	
	public static IEnumerable<int> To(this int i, int exclusiveEnd){
		return Enumerable.Range(i,exclusiveEnd-i);
	}
	
#endregion
	
	#region Linqpad
	public static T DumpIf<T>(this T val, Func<T,bool> predicate, string header=null){
	if(predicate(val))
		val.Dump(header);
	return val;
	}
	#endregion
}

// You can also define non-static classes, enums, etc.


public struct StreamOuts
{
public string Errors{get;set;}
public string Output{get;set;}
}

public class PathWrapper{
	public string RawPath{get;private set;}
	
	public PathWrapper(string rawPath){
		RawPath=rawPath;
	}
	public IEnumerable<string> GetSegments(){
		var seperators=new[]{System.IO.Path.DirectorySeparatorChar,System.IO.Path.AltDirectorySeparatorChar};
		var first= string.Empty;
		var path=RawPath;
		if(RawPath.StartsWith(@"\\")){
			first=@"\\";
			path=path.Substring(2);
		}
		var splits=path.TrimEnd(seperators)
		.Split(seperators);
		if(first.IsNullOrEmpty())
		return splits;
		return new[]{ first+ splits.First()}.Concat(splits.Skip(1));
	}
	public  Hyperlinq AsExplorerSelectLink(string text){
		var arguments= string.Format("/select,{0}",this.RawPath);
		return new Hyperlinq( QueryLanguage.Expression, "Process.Start(\"Explorer.exe\",@\""+arguments+"\")",text);
	}
}

public class FilePathWrapper:PathWrapper{

	public FilePathWrapper(string path) :base(path){}
	
	public string ReadAllShared(){
		using(var r=System.IO.File.Open(this.RawPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		using(var sr= new StreamReader(r,detectEncodingFromByteOrderMarks: true)){
			return sr.ReadToEnd();
		}
	
	}
	
	public DateTime GetLastWriteLocalTime(){
		return System.IO.File.GetLastWriteTime(this.RawPath);
	}
	public DateTime GetLastWriteUtcTime(){
		return System.IO.File.GetLastWriteTimeUtc(this.RawPath);
	}
	
	
}

public class DirectoryPathWrapper:PathWrapper{

public DirectoryPathWrapper(string path) :base(path){}

public IEnumerable<Tuple<string,string>> GetJunctions(){
		using(var ps= new Process()){
			ps.StartInfo.FileName="cmd.exe";
			ps.StartInfo.UseShellExecute=false;
			ps.StartInfo.RedirectStandardError=true;
			ps.StartInfo.RedirectStandardOutput=true;
			ps.StartInfo.WorkingDirectory=this.RawPath;
			var junctions=ps.RunProcessRedirected("/c dir").Output.SplitLines().Where(d=>d.Contains("<JUNCTION>"));
			foreach(var j in junctions){
				var junctionTargetPath=j.After("[").Before("]");
				yield return Tuple.Create(j.Before("[").After(">").TrimStart(),junctionTargetPath);
			}
		}
	}
}
#region PInvoke //http://www.codeproject.com/script/Articles/ViewDownloads.aspx?aid=15633
public static class PInvokeWrapper{


          [Flags]
          private enum EFileAccess : uint
          {
              GenericRead = 0x80000000,
              GenericWrite = 0x40000000,
              GenericExecute = 0x20000000,
              GenericAll = 0x10000000,
          }
		  
		  [Flags]
         private enum EFileAttributes : uint
         {
             Readonly = 0x00000001,
             Hidden = 0x00000002,
             System = 0x00000004,
             Directory = 0x00000010,
             Archive = 0x00000020,
             Device = 0x00000040,
             Normal = 0x00000080,
             Temporary = 0x00000100,
             SparseFile = 0x00000200,
             ReparsePoint = 0x00000400,
             Compressed = 0x00000800,
             Offline = 0x00001000,
             NotContentIndexed = 0x00002000,
             Encrypted = 0x00004000,
             Write_Through = 0x80000000,
             Overlapped = 0x40000000,
             NoBuffering = 0x20000000,
             RandomAccess = 0x10000000,
             SequentialScan = 0x08000000,
             DeleteOnClose = 0x04000000,
             BackupSemantics = 0x02000000,
             PosixSemantics = 0x01000000,
             OpenReparsePoint = 0x00200000,
             OpenNoRecall = 0x00100000,
             FirstPipeInstance = 0x00080000
         }
		[Flags]
          private enum EFileShare : uint
          {
              None = 0x00000000,
              Read = 0x00000001,
              Write = 0x00000002,
              Delete = 0x00000004,
          }  
		  private enum ECreationDisposition : uint
          {
              New = 1,
              CreateAlways = 2,
              OpenExisting = 3,
              OpenAlways = 4,
              TruncateExisting = 5,
          }
/// <summary>
/// Determines whether the specified path exists and refers to a junction point.
/// </summary>
/// <param name="path">The junction point path</param>
/// <returns>True if the specified path represents a junction point</returns>
/// <exception cref="IOException">Thrown if the specified path is invalid
/// or some other error occurs</exception>
public static bool JunctionExists(string path)
{
  if (! Directory.Exists(path))
      return false;

  using (SafeFileHandle handle = OpenReparsePoint(path, EFileAccess.GenericRead))
  {
      string target = InternalGetTarget(handle);
      return target != null;
  }
}
/// <summary>
/// The file or directory is not a reparse point.
/// </summary>
private const int ERROR_NOT_A_REPARSE_POINT = 4390;
/// <summary>
 /// Command to get the reparse point data block.
 /// </summary>
private const int FSCTL_GET_REPARSE_POINT = 0x000900A8;

 /// <summary>
 /// Reparse point tag used to identify mount points and junction points.
 /// </summary>
private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
/// This prefix indicates to NTFS that the path is to be treated as a non-interpreted
/// path in the virtual file system.
/// </summary>
private const string NonInterpretedPathPrefix = @"\??\";
 [StructLayout(LayoutKind.Sequential)]
private struct REPARSE_DATA_BUFFER
{
   /// <summary>
   /// Reparse point tag. Must be a Microsoft reparse point tag.
   /// </summary>
   public uint ReparseTag;

   /// <summary>
   /// Size, in bytes, of the data after the Reserved member. This can be calculated by:
   /// (4 * sizeof(ushort)) + SubstituteNameLength + PrintNameLength + 
   /// (namesAreNullTerminated ? 2 * sizeof(char) : 0);
   /// </summary>
   public ushort ReparseDataLength;

   /// <summary>
   /// Reserved; do not use. 
   /// </summary>
   public ushort Reserved;

   /// <summary>
   /// Offset, in bytes, of the substitute name string in the PathBuffer array.
   /// </summary>
   public ushort SubstituteNameOffset;

   /// <summary>
   /// Length, in bytes, of the substitute name string. If this string is null-terminated,
   /// SubstituteNameLength does not include space for the null character.
   /// </summary>
   public ushort SubstituteNameLength;

   /// <summary>
   /// Offset, in bytes, of the print name string in the PathBuffer array.
   /// </summary>
   public ushort PrintNameOffset;

   /// <summary>
   /// Length, in bytes, of the print name string. If this string is null-terminated,
   /// PrintNameLength does not include space for the null character. 
   /// </summary>
   public ushort PrintNameLength;

   /// <summary>
   /// A buffer containing the unicode-encoded path string. The path string contains
   /// the substitute name string and print name string.
   /// </summary>
   [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
   public byte[] PathBuffer;
}

 private static void ThrowLastWin32Error(string message)
{
   throw new IOException(message, Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
}
 private static string InternalGetTarget(SafeFileHandle handle)
{
    int outBufferSize = Marshal.SizeOf(typeof(REPARSE_DATA_BUFFER));
    IntPtr outBuffer = Marshal.AllocHGlobal(outBufferSize);

    try
    {
        int bytesReturned;
        bool result = DeviceIoControl(handle.DangerousGetHandle(), FSCTL_GET_REPARSE_POINT,
            IntPtr.Zero, 0, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);

        if (!result)
        {
            int error = Marshal.GetLastWin32Error();
            if (error == ERROR_NOT_A_REPARSE_POINT)
                return null;

            ThrowLastWin32Error("Unable to get information about junction point.");
        }

        REPARSE_DATA_BUFFER reparseDataBuffer = (REPARSE_DATA_BUFFER)
            Marshal.PtrToStructure(outBuffer, typeof(REPARSE_DATA_BUFFER));

        if (reparseDataBuffer.ReparseTag != IO_REPARSE_TAG_MOUNT_POINT)
            return null;

        string targetDir = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
            reparseDataBuffer.SubstituteNameOffset, reparseDataBuffer.SubstituteNameLength);

        if (targetDir.StartsWith(NonInterpretedPathPrefix))
            targetDir = targetDir.Substring(NonInterpretedPathPrefix.Length);

        return targetDir;
    }
    finally
    {
        Marshal.FreeHGlobal(outBuffer);
    }
}

private static SafeFileHandle OpenReparsePoint(string reparsePoint, EFileAccess accessMode)
          {
              SafeFileHandle reparsePointHandle = new SafeFileHandle(CreateFile(reparsePoint, accessMode,
                  EFileShare.Read | EFileShare.Write | EFileShare.Delete,
                  IntPtr.Zero, ECreationDisposition.OpenExisting,
                  EFileAttributes.BackupSemantics | EFileAttributes.OpenReparsePoint, IntPtr.Zero), true);
  
              if (Marshal.GetLastWin32Error() != 0)
                  ThrowLastWin32Error("Unable to open reparse point.");
  
              return reparsePointHandle;
          }
  
[DllImport("kernel32.dll", SetLastError = true)]
private static extern IntPtr CreateFile(
 string lpFileName,
 EFileAccess dwDesiredAccess,
 EFileShare dwShareMode,
 IntPtr lpSecurityAttributes,
 ECreationDisposition dwCreationDisposition,
 EFileAttributes dwFlagsAndAttributes,
 IntPtr hTemplateFile);  
       [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
 private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
   IntPtr InBuffer, int nInBufferSize,
   IntPtr OutBuffer, int nOutBufferSize,
    out int pBytesReturned, IntPtr lpOverlapped);
}
#endregion