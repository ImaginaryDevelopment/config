#r "System.Linq"
open System.Linq.Expressions
#r "System.Xml"
#r "System.Xml.Linq"
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open System
open System.Linq
open System.Xml
open System.Xml.Linq
let rec eval = function
    | Value(v,t) -> v
    | Coerce(e,t) -> eval e
    | NewObject(ci,args) -> ci.Invoke(evalAll args)
    | NewArray(t,args) -> 
        let array = Array.CreateInstance(t, args.Length) 
        args |> List.iteri (fun i arg -> array.SetValue(eval arg, i))
        box array
     | NewUnionCase(case,args) -> FSharpValue.MakeUnion(case, evalAll args)
     | NewRecord(t,args) -> FSharpValue.MakeRecord(t, evalAll args)
     | NewTuple(args) ->
         let t = FSharpType.MakeTupleType [|for arg in args -> arg.Type|]
         FSharpValue.MakeTuple(evalAll args, t)
     | FieldGet(Some(Value(v,_)),fi) -> fi.GetValue(v)
     | PropertyGet(None, pi, args) -> pi.GetValue(null, evalAll args)
     | PropertyGet(Some(x),pi,args) -> pi.GetValue(eval x, evalAll args)
     | Call(None,mi,args) -> mi.Invoke(null, evalAll args)
     | Call(Some(x),mi,args) -> mi.Invoke(eval x, evalAll args)
     | arg -> raise <| NotSupportedException(arg.ToString())
    and evalAll args = [|for arg in args -> eval arg|]

type String with 
    member x.Before(delimiter:string) : string = 
        x.Substring(0,x.IndexOf(delimiter))
    member x.After(delimiter:string) : string =
        x.Substring(x.IndexOf(delimiter)+delimiter.Length)
    member x.BeforeOrSelf(delimiter:string) : string =
        match x with 
        | x when x.Contains(delimiter) -> x.Before(delimiter)
        | _ -> x
    member x.AfterOrSelf(delimiter:string) : string =
        match x with
        | x when x.Contains(delimiter) -> x.After(delimiter)
        | _ -> x
// [<AbstractClass; Sealed>] // http://stackoverflow.com/questions/13101995/defining-static-classes-in-f
// [<Obsolete("not necessary in F#")>]
// type LambdaOp private () =
type [<Measure>]  minute
// type [<Measure>] second

// let seconds_per_minute = 60<second> / 1<minute>

type System.Int32 with
    member x.Minutes = 
        let y:double = double x // http://msdn.microsoft.com/en-us/library/dd233220.aspx
        TimeSpan.FromMinutes(y)
    member x.AsMinutes =
        let y:decimal = decimal x
        y * 1.0m<minute>

type System.DateTime with
    member dt.ToTimeString =
        dt.ToString().After(" ")
    member dt.UnixTicks =
        let d1 = new DateTime(1970,1,1)
        let d2 = dt.ToUniversalTime()
        let ts = new TimeSpan(d2.Ticks - d1.Ticks)
        ts.TotalMilliseconds
    member dt.StartOfWeek (weekStart : DayOfWeek) =
        let diff = int dt.DayOfWeek - int weekStart
        let absDiff =float ( if diff<0 then diff+7 else diff)
        dt.AddDays(-1.0*absDiff).Date

type System.Random with
    member rnd.NextBool = rnd.NextDouble() > 0.5
    
type System.Xml.Linq.XElement with
    member el.GetAttribValOrNull xname = // return value is (idiomatic c#) string or null
        let xa = el.Attribute(xname)
        if xa = null then null else xa.Value
    member el.GetAttribVal xname = // return value is  (idiomatic f#?)  option<string>
        match el.Attribute(xname) with
        | x when x <> null -> Some(x.Value)
        | _ -> None
    member el.GetXmlNode =
        let xmlDoc = XmlDocument()
        (
            use xmlReader = el.CreateReader()
            xmlDoc.Load(xmlReader)
        )
        xmlDoc
    member el.IndexPosition() = //assumes all siblings are the same element type
        if el = null then raise <| new ArgumentNullException("el") //guard clause
        match el.Parent with
        | p when p <> null -> Seq.findIndex (fun elem -> elem = el) [|for i in el.Parent.Elements(el.Name) do yield i|]
        | _ -> -1

    member el.GetAbsoluteXPath () =
        if el = null then raise <| new ArgumentNullException("el")
        let relativeXPath (e:XElement) =
            let index = e.IndexPosition()
            let name = e.Name.LocalName;
            
            if index = -1 then "/" + name else sprintf "/%s[%d]" name index
        let ancestors = el.Ancestors() |> Seq.map (relativeXPath)
        //let reversed =
        String.Concat(ancestors.Reverse()) + relativeXPath(el)
        
        
type System.Xml.XmlNode with
    member node.GetXElement =
        let xDoc = XDocument()
        (
            use xmlWriter = xDoc.CreateWriter()
            node.WriteTo(xmlWriter)
        )
        xDoc.Root


// [<EntryPoint>] // doesn't appear to work properly for sublime build, nor linqpad
let main args=
    assert("testing".Before( "ing") = "test")
    assert ("testing".After("test") = "ing")
    assert ("testing".BeforeOrSelf("ING") = "testing")
    assert ("testing".BeforeOrSelf("ng")="testi")
    let dtStart= DateTime(2014,5,22)
    
    assert ( dtStart.StartOfWeek(DayOfWeek.Monday) = DateTime(2014,5,19))
    assert ( dtStart.StartOfWeek(DayOfWeek.Tuesday) = DateTime(2014,5,20))
    assert ( dtStart.StartOfWeek(DayOfWeek.Wednesday) = DateTime(2014,5,21))
    assert ( dtStart.StartOfWeek(DayOfWeek.Thursday) = DateTime(2014,5,22))
    assert ( dtStart.StartOfWeek(DayOfWeek.Friday) = DateTime(2014,5,16))
    assert ( dtStart.StartOfWeek(DayOfWeek.Saturday) = DateTime(2014,5,17))
    assert ( dtStart.StartOfWeek(DayOfWeek.Sunday) = DateTime(2014,5,18))
    (
        let XName n = System.Xml.Linq.XNamespace.None+ n
        let child = XElement(XName "child")
        let child2 = XElement(XName "child")
        let attrib =System.Xml.Linq.XAttribute(XName "test","1")
        let xmlWithAttrib = System.Xml.Linq.XElement(XName "element",attrib,child,child2)
        assert( xmlWithAttrib.GetAttribVal(XName "test") = Some("1"))
        assert( xmlWithAttrib.GetAttribVal(XName "test1") = None)
        assert( xmlWithAttrib.GetAttribValOrNull(XName "test1") = null)
        assert( xmlWithAttrib.GetAttribValOrNull(XName "test") = "1")
        assert(xmlWithAttrib.GetAbsoluteXPath() = "/element")
        assert(child.GetAbsoluteXPath() = "/element/child[1]")
        assert(child2.GetAbsoluteXPath() = "/element/child[2]")
        // attrib.GetAbsoluteXPath().Dump() // not defined, XAttribute is not an XElement
    )
    printfn "%d" 0
    0
    
main([])