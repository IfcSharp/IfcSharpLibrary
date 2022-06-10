// ifc_out_html.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using Threading=System.Threading;
using System.Reflection;
using NetSystem=System;


namespace ifc{//==============================

public partial class ENTITY{//==========================================================================================

public static string HtmlRefOut(string expr)
{
List<int> SharpList=new List<int>();
if (expr.Contains("#")) 
   {for (int pos=0;pos<expr.Length;pos++) if (expr[pos]=='#') SharpList.Add(pos);
    string NewExpr=expr;
    foreach (int SharpPos in SharpList) {int pos=SharpPos;while ( (pos<expr.Length) && (expr[pos]!=',') && (expr[pos]!=')') ) pos++;
                                         string link="-"; if (pos>SharpPos) link=expr.Substring(SharpPos,pos-SharpPos);
                                         string NewLink="<a href=\""+link+"\" class=\"ref\">"+link+"</a>";
                                         NewExpr=NewExpr.Replace(link, NewLink);
                                        }  
    expr=NewExpr;
   }
return expr;
}

public static string HtmlRefOut(FieldInfo field,string IfcId,ENTITY e){string RefClassName="ref";if (NameKeywDict.ContainsKey(e.ShortTypeName())) RefClassName+=" keyw"+NameKeywDict[e.ShortTypeName()];return "<a href=\""+IfcId+"\" class=\""+RefClassName+"\">"+IfcId+"</a>";}
public static string HtmlOut(FieldInfo field,string ClassName, string value){return "<span class=\""+ClassName+"\">"+value+"</span>";}
public static string HtmlNullOut(FieldInfo field,bool IsDerived){return "<span class=\"dollar\">"+((IsDerived)?"*":"$")+"</span>";}
public static string HtmlEnumOut(FieldInfo field,string value){return "<span class=\"enum\" title=\""+field.FieldType.Name+" "+field.Name+"\" >."+value+".</span>";}
public static string HtmlTextOut(FieldInfo field,string value){return "<span class=\"text\">"+value+"</span>";}

public static string HtmlOut(FieldInfo field,object o,bool IsDerived){
string s=""; 
          if (o==null)       { s=HtmlNullOut(field,IsDerived);}
     else if (o is Enum)     {/*if (o.ToString()=="_NULL") s=HtmlNullOut(field,IsDerived); else */ s=HtmlEnumOut(field,o.ToString());}
     else if (o is SELECT)   {if ( ((SELECT)o).IsNull) s=HtmlNullOut(field,IsDerived);
                              else { if (((SELECT)o).Id==0) if ( ((SELECT)o).SelectType().IsSubclassOf(typeof(ENTITY)) ) { ((SELECT)o).Id=((ENTITY)((SELECT)o).SelectValue()).LocalId; } 
                                     if (((SELECT)o).Id>0) s=HtmlRefOut(field,"#"+((SELECT)o).Id.ToString(),(ENTITY)(((SELECT)o).SelectValue())); 
                                     else                  s="IFC"+((SELECT)o).SelectType().Name.ToUpper()+"("+HtmlOut(field,((SELECT)o).SelectValue().ToString(),IsDerived)+")";
                                   }
                             }
     else if (o is ENTITY)    if ( ((ENTITY)o).LocalId==0 ) s=HtmlNullOut(field,IsDerived); else  s=HtmlRefOut(field,((ENTITY)o).IfcId(),(ENTITY)o); 
     else if (o is TypeBase) {TypeBase tb=(TypeBase)o;if (tb.GetBaseType()==typeof(String)) {if (o.ToString()=="" || o.ToString()=="null") s=HtmlNullOut(field,IsDerived);else  s=HtmlTextOut(field,o.ToString()); } else  {if (o.ToString()=="null") s=HtmlNullOut(field,IsDerived);else s=HtmlOut(field,"float",o.ToString());} }
     else if (o is String)   {if (o.ToString()=="") s=HtmlNullOut(field,IsDerived);else s=HtmlOut(field,"text",o.ToString());}
     else if( typeof(IEnumerable).IsAssignableFrom(o.GetType())) {s=HtmlOut(field,"list",HtmlRefOut(o.ToString()));}
     else                     {if (o.ToString()=="null") s=HtmlNullOut(field,IsDerived); else s=HtmlOut(field,"int",o.ToString());} 
                             
return s;
}

public static Dictionary<string, int>  NameKeywDict=new Dictionary<string,int>();

public virtual string ToHtml(){
Threading.Thread.CurrentThread.CurrentCulture=CultureInfo.InvariantCulture;
string ElementName=this.GetType().ToString().Replace("IFC4","ifc").Replace("ifc.","");
string EntityClassName="entity";if (NameKeywDict.ContainsKey(ElementName)) EntityClassName+=" keyw"+NameKeywDict[ElementName];
string     IdClassName="id";    if (NameKeywDict.ContainsKey(ElementName))     IdClassName+=" keyw"+NameKeywDict[ElementName];

string Args="(";
AttribListType AttribList=TypeDictionary.GetComponents(this.GetType()).AttribList;
int sep=0;foreach (AttribInfo attrib in AttribList) Args+=((++sep>1)?",":"")+attrib.field.FieldType.Name +attrib.field.Name;
Args+=")";

string s="\r\n<div class=\"line"+(ifc.EntityComment.HtmlCnt%4) +"\"><a name=\""+this.LocalId.ToString()+"\"/><span class=\""+IdClassName+"\">"+"<a href=\"#"+this.LocalId.ToString()+"\">#"+ this.LocalId.ToString()+"</a></span><span class=\"equal\">=</span><span class=\"ifc\">ifc</span><span class=\""
+EntityClassName+"\" title=\"ifc"+ElementName
+Args
+"\">"+ElementName+"</span>(";

 sep=0;
     if (this is CartesianPoint) {CartesianPoint cp=(CartesianPoint)this;string coords="";foreach (LengthMeasure lm in cp.Coordinates    ) coords+=((++sep>1)?",":"")+((double)lm).ToString("#0.0000"); s+=HtmlOut(AttribList[0].field,"list",coords);}
else if (this is Direction     ) {Direction      cp=(Direction)     this;string coords="";foreach (Real          lm in cp.DirectionRatios) coords+=((++sep>1)?",":"")+((double)lm).ToString("#0.0000"); s+=HtmlOut(AttribList[0].field,"list",coords);}
else foreach (AttribInfo attrib in AttribList) s+=((++sep>1)?",":"")+HtmlOut(attrib.field,attrib.field.GetValue(this),attrib.IsDerived); 

s+=")<span class=\"semik\">;</span>";
if (EndOfLineComment!=null) s+="<span class=\"EndOfLineComment\">/* "+EndOfLineComment+" */</span>";
s+="<br/></div>";

return s;
}

}//of ENTITY ==========================================================================================================



public partial class EntityComment:ENTITY{//==========================================================================================
public override string ToHtml(){HtmlCnt++;return "\r\n<span class=\"Commentline\">/* "+CommentLine+" */</span><br/>";}
}//=====================================================================================================================


public partial class Model{//==========================================================================================

private static string FormattedHeaderLine(string line){return "<span class=\"header\">"+line+"</span>"+"<br/>";}

public void ToHtmlFile()
{
// 2022-06-10 (ef): the filename from the Header can contain the whole path to the file
//                  therefore we only use the filename without extension as the output path.
string filename = NetSystem.IO.Path.GetFileNameWithoutExtension(Header.name);
StreamWriter sw=new StreamWriter(filename + ".html");
//Console.WriteLine("Start ToHtmlFile");
sw.WriteLine("<html>");
sw.WriteLine("<head>");
sw.WriteLine("<title>ifc</title>");
//sw.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"ifc.css\"/>");
sw.WriteLine("<style>");
sw.WriteLine(".global{");
sw.WriteLine("  background-color: #FFFFEE;");
sw.WriteLine("  font-size: 10pt;");
sw.WriteLine("  font-family: Courier New;");
sw.WriteLine("  margin: 1em; padding: 0.5em;");
sw.WriteLine("}");
sw.WriteLine("  .id {color: red;}");
sw.WriteLine("  .ifc {color: gray;}");
sw.WriteLine("  .commentline {color: black; background-color:white; font-weight: bold;white-space: pre;text-decoration: underline;}");
sw.WriteLine("  .EndOfLineComment {color: black; background-color:white; font-weight: bold;white-space: pre;}");
sw.WriteLine("  .ref {color: blue;text-decoration: underline;}");
sw.WriteLine("  .entity {color: navy;font-weight:bold;}");
sw.WriteLine("  .text {color: maroon; font-weight:bold;}");
sw.WriteLine("  .dollar {color: gray; }");
sw.WriteLine("  .float {color: purple; font-weight:bold;}");
sw.WriteLine("  .int {color: teal;}");
sw.WriteLine("  .list {color: black;}");
sw.WriteLine("  .guid {color: orange;}");
sw.WriteLine("  .enum {color: green;font-weight:bold;}");
sw.WriteLine("  .header {color: darkcyan;}");
sw.WriteLine("  .equal {color: darkgray;}");
sw.WriteLine("  .semik {color: olive;}");
sw.WriteLine("  .keyw0 {background-color:#A0FFFF;}");
sw.WriteLine("  .keyw1 {background-color:#FFA0FF;}");
sw.WriteLine("  .keyw2 {background-color:#FFFFA0;}");
sw.WriteLine("  .keyw3 {background-color:#A0FFA0;}");
sw.WriteLine("  .line0 {background-color:#F0FFFF;}");
sw.WriteLine("  .line1 {background-color:#FAFAFA;}");
sw.WriteLine("  .line2 {background-color:#FFFFF8;}");
sw.WriteLine("  .line3 {background-color:#F0FFF0;}");
sw.WriteLine("</style>");

sw.WriteLine("</head>");
sw.WriteLine("<body>");
sw.WriteLine("<div class=\"global\">");


//foreach (string s in Header) sw.WriteLine("<span class=\"header\">"+s+"</span>"+"<br/>");

sw.WriteLine(FormattedHeaderLine("ISO-10303-21;"));
sw.WriteLine(FormattedHeaderLine("HEADER;"));
sw.WriteLine(FormattedHeaderLine("FILE_DESCRIPTION (('"+Header.description+"'), '2;1');"));
sw.WriteLine(FormattedHeaderLine("FILE_NAME ('"+Header.name+"', '"+NetSystem.String.Format("{0:s}",NetSystem.DateTime.Now)+"', ('"+Header.author+"'), ('"+Header.organization+"'), '"+ Header.preprocessor_version+"', '"+Header.originating_system+"', '"+Header.authorization+"');"));
sw.WriteLine(FormattedHeaderLine("FILE_SCHEMA (('"+ifc.Specification.SchemaName+"'));"));
sw.WriteLine(FormattedHeaderLine("ENDSEC;"));
sw.WriteLine(FormattedHeaderLine("DATA;"));

//foreach (KeyValuePair<int,ENTITY> kvp in AssignedEntityDict) sw.Write(kvp.Value.ToHtml()+"<br/>");  
foreach (ENTITY e in EntityList) sw.Write(e.ToHtml()); 
sw.WriteLine(FormattedHeaderLine("ENDSEC;"));
sw.WriteLine(FormattedHeaderLine("END-ISO-10303-21;"));

sw.WriteLine("</div>"); // pre
sw.WriteLine("</body>");
sw.WriteLine("</html>");
sw.Close();
}// of ToHmlFile
}// of Model ==========================================================================================================



}//ifc ==============================
