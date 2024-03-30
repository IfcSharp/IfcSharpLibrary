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
public static bool HtmlDisplayGlobalId=false;

public override void Initialise(){Highlighted=Highlighting;}
public bool Highlighted=false;
public static bool Highlighting=false;

public static string HtmlRefOut(object o)
{
                                                                 
string expr=o.ToString();
List<int> SharpList=new List<int>();
if (expr.Contains("#")) 
   {string SavedExpr=expr;
    expr="(";
    int pos=0;   
    foreach (object item in (IEnumerable)o) if (item!=null) if (item is ENTITY || item is SELECT)
            {pos++;if (pos>1) expr+=","; 
             ENTITY e=null;

             if  ((item is SELECT) && ( typeof(ENTITY).IsAssignableFrom( ((SELECT)item).SelectType())) ) e= (ENTITY) ((SELECT)item).SelectValue();
             else if (item is ENTITY) e=(ENTITY)item;

             if (e!=null)
                {string RefClassName="ref";if (e.Highlighted) RefClassName="refX";
                 expr+="<a href=\"#"+e.LocalId+"\" class=\""+RefClassName+"\">#"+e.LocalId+"</a>";
                }
           // if (item is ENTITY) s+=EntityVarName(((ENTITY)item).LocalId,CurrentModel);
            }
    expr+=")";
if (expr.Length==2) expr=SavedExpr;
   }
return expr;
}

public static string HtmlRefOut(FieldInfo field,string IfcId,ENTITY e){string RefClassName="ref";if (e.Highlighted) RefClassName="refX"; if (NameKeywDict.ContainsKey(e.ShortTypeName())) RefClassName+=" keyw"+NameKeywDict[e.ShortTypeName()];return "<a href=\""+IfcId+"\" class=\""+RefClassName+"\" title=\""+field.FieldType.Name+" "+field.Name+"="+IfcId+"\">"+IfcId+"</a>";}
public static string HtmlOut(FieldInfo field,string ClassName, string value){return "<span class=\""+ClassName+"\" title=\""+field.FieldType.Name+" "+field.Name /*+"="+value*/ +"\" >"+value+"</span>";}
public static string HtmlNullOut(FieldInfo field,bool IsDerived){return "<span class=\"dollar\" title=\""+field.FieldType.Name+" "+field.Name+"="+((IsDerived)?"* (is derived) ":"$ (null)")+"\" >"+((IsDerived)?"*":"$")+"</span>";}
public static string HtmlEnumOut(FieldInfo field,string value){return "<span class=\"enum\" title=\""+field.FieldType.Name+" "+field.Name+"="+value+"\" > ."+value+".</span>";}
public static string HtmlTextOut(FieldInfo field,string value){return "<span class=\"text\" title=\""+field.FieldType.Name+" "+field.Name+"="+value+"\" >"+value+"</span>";}

public static string HtmlOut(FieldInfo field,object o,bool IsDerived){
string s=""; 
if (!HtmlDisplayGlobalId) if (field.Name=="GlobalId") return HtmlNullOut(field:field,IsDerived:false);

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
     else if( typeof(IEnumerable).IsAssignableFrom(o.GetType())) s=HtmlOut(field,"list",HtmlRefOut(o));
     else                     {if (o.ToString()=="null") s=HtmlNullOut(field,IsDerived); else s=HtmlOut(field,"int",o.ToString());} 
                             
return s;
}

public static Dictionary<string, int>  NameKeywDict=new Dictionary<string,int>();

public virtual string ToHtml(){
Threading.Thread.CurrentThread.CurrentCulture=CultureInfo.InvariantCulture;
string ElementName=this.GetType().ToString().Replace("ifc.","");
string EntityClassName="entity";if (NameKeywDict.ContainsKey(ElementName)) EntityClassName+=" keyw"+NameKeywDict[ElementName];
string     IdClassName="id";    if (NameKeywDict.ContainsKey(ElementName))     IdClassName+=" keyw"+NameKeywDict[ElementName];

string Args="(";
AttribListType AttribList=TypeDictionary.GetComponents(this.GetType()).AttribList;
int sep=0;foreach (AttribInfo attrib in AttribList) Args+=((++sep>1)?",":"")+"&#013;&#010;"+sep+"&#009;"+attrib.field.FieldType.Name+"&#009;"+attrib.field.Name+"&#009;[of class "+attrib.field.DeclaringType.FullName+"]" ;//+"&#009;"+"="+attrib.field.GetValue(this);
Args+="&#013;&#010;"+")";

List<string> InheritanceList=new List<string>();  

Args+="&#013;&#010;"+"Inheritance of "+this.GetType().ToString().Replace("ifc.","ifc")+":";
Type t=this.GetType();       InheritanceList.Add(t.ToString().Replace("ifc.","ifc"));  
while ((t=t.BaseType)!=null) InheritanceList.Add(t.ToString().Replace("ifc.","ifc"));
InheritanceList.Reverse();
bool Display=false;foreach(string sx in InheritanceList) {if (Display) Args+="&#013;&#010;"+sx;if (sx=="ifcENTITY") Display=true;}


string s="\r\n<div class=\"line"+(Highlighted?"X":(ifc.EntityComment.HtmlCnt%4).ToString()) +"\"><a name=\""+this.LocalId.ToString()+"\"/><span class=\""+IdClassName+"\">"+"<a href=\"#"+this.LocalId.ToString()+"\">#"+ this.LocalId.ToString()+"</a></span><span class=\"equal\">=</span>"
+"<a class=\""+EntityClassName+"\" href=\"javascript:EntityHelp('"+Layer.Name(this.LayerId())+"','"+ElementName+"')\" ><span class=\"ifc\">ifc</span><span class=\""
+EntityClassName+"\" "
+"title=\"ifc"+ElementName
+Args
+"\">"+ElementName+"</span></a>(";

 sep=0;
     if (this is CartesianPoint) {CartesianPoint cp=(CartesianPoint)this;string coords="";foreach (LengthMeasure lm in cp.Coordinates    ) coords+=((++sep>1)?",":"")+((double)lm).ToString("#0.0000"); s+=HtmlOut(AttribList[0].field,"list",coords);}
else if (this is Direction     ) {Direction      cp=(Direction)     this;string coords="";foreach (Real          lm in cp.DirectionRatios) coords+=((++sep>1)?",":"")+((double)lm).ToString("#0.0000"); s+=HtmlOut(AttribList[0].field,"list",coords);}
else foreach (AttribInfo attrib in AttribList) s+=((++sep>1)?",":"")+HtmlOut(attrib.field,attrib.field.GetValue(this),attrib.IsDerived); 

s+=")<span class=\"semik\">;</span>";

string HtmlCommentClass="EndOfLineComment";if (Highlighted) HtmlCommentClass="lineXC";

if (EndOfLineComment!=null) s+="<span class=\""+HtmlCommentClass+"\">/* "+EndOfLineComment+" */</span>";
s+="<br/></div>";

return s;
}


}//of ENTITY ==========================================================================================================



public partial class EntityComment:ENTITY{//==========================================================================================
public override string ToHtml(){HtmlCnt++;string HtmlClass="Commentline";if (Highlighted) HtmlClass="lineXC";if (CommentLine.TrimStart(' ').Length==0) return ""; else return "\r\n<span class=\""+HtmlClass+"\">/* "+CommentLine+" */</span><br/>";}

public               EntityComment(string CommentLine,bool Highlighting){this.Highlighted=ENTITY.Highlighting=Highlighting; AddNextCommentLine();this.CommentLine=CommentLine;if (this.CommentLine.Length<CommentWidth) this.CommentLine+=new string(' ',CommentWidth-this.CommentLine.Length);}
public               EntityComment(bool Highlighting){this.Highlighted=ENTITY.Highlighting=Highlighting; AddNextCommentLine();this.CommentLine="";}

}//=====================================================================================================================


public partial class Model{//==========================================================================================
public static bool BackgroundDisplay=false;

private static string FormattedHeaderLine(string line,string attribs="",bool a=false){string tag="span";if (a) tag="a"; return "<"+tag+" class=\"header\""+attribs+">"+line+"</"+tag+">"+"<br/>";}

public void ToHtmlFile(string filePath="",bool JavaScript=true)
{
if (string.IsNullOrEmpty(filePath)) filePath = Header.Name + ".ifc.html";

StreamWriter sw=new StreamWriter(filePath);
//Console.WriteLine("Start ToHtmlFile");
sw.WriteLine("<!-- "+Header.Name+".ifc.html"+" was created using IfcSharp (see https://github.com/IfcSharp) -->");
sw.WriteLine("<html>");
sw.WriteLine("<head>");
sw.WriteLine("<title>ifc</title>");
//sw.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"ifc.css\"/>");
sw.WriteLine("<style>");
sw.WriteLine(".global{");
if (BackgroundDisplay) sw.WriteLine("  background-color: #FFFFEE;"); else sw.WriteLine("  background-color: #FFFFFF;");
sw.WriteLine("  font-size: 10pt;");
sw.WriteLine("  font-family: Courier New;");
sw.WriteLine("  margin: 1em; padding: 0.5em;");
sw.WriteLine("}");
sw.WriteLine("  .id {color: red;}");
sw.WriteLine("  .ifc {color: gray;}");
sw.WriteLine("  .commentline {color: black; background-color:white; font-weight: bold;white-space: pre;text-decoration: underline;}");
sw.WriteLine("  .EndOfLineComment {color: black; background-color:white; font-weight: bold;white-space: pre;}");
sw.WriteLine("  .ref {color: blue;text-decoration: underline;}");
sw.WriteLine("  .refX {color: blue;text-decoration: underline;background-color:#FFFF00;}");
sw.WriteLine("  .entity {color: navy;font-weight:bold;text-decoration: none;}");
sw.WriteLine("  .text {color: maroon; font-weight:bold;}");
sw.WriteLine("  .dollar {color: gray; }");
sw.WriteLine("  .float {color: purple; font-weight:bold;}");
sw.WriteLine("  .int {color: teal;}");
sw.WriteLine("  .list {color: black;}");
sw.WriteLine("  .guid {color: orange;}");
sw.WriteLine("  .enum {color: green;font-weight:bold;}");
sw.WriteLine("  .header {color: darkcyan;text-decoration: none;}");
sw.WriteLine("  .equal {color: darkgray;}");
sw.WriteLine("  .semik {color: olive;}");
sw.WriteLine("  .keyw0 {background-color:#A0FFFF;text-decoration: none;}");
sw.WriteLine("  .keyw1 {background-color:#FFA0FF;text-decoration: none;}");
sw.WriteLine("  .keyw2 {background-color:#FFFFA0;text-decoration: none;}");
sw.WriteLine("  .keyw3 {background-color:#A0FFA0;text-decoration: none;}");
if (BackgroundDisplay) sw.WriteLine("  .line0 {background-color:#F0FFFF;}"); else sw.WriteLine("  .line0 {background-color:#FFFFFF;}");
if (BackgroundDisplay) sw.WriteLine("  .line1 {background-color:#FAFAFA;}"); else sw.WriteLine("  .line0 {background-color:#FFFFFF;}");
if (BackgroundDisplay) sw.WriteLine("  .line2 {background-color:#FFFFF8;}"); else sw.WriteLine("  .line0 {background-color:#FFFFFF;}");
if (BackgroundDisplay) sw.WriteLine("  .line3 {background-color:#F0FFF0;}"); else sw.WriteLine("  .line0 {background-color:#FFFFFF;}");
if (BackgroundDisplay) sw.WriteLine("  .lineX {background-color:#FFFF00;}"); else sw.WriteLine("  .line0 {background-color:#FFFFFF;}");
if (BackgroundDisplay) sw.WriteLine("  .lineXC{background-color:#FFFF00;font-weight:bold;}"); sw.WriteLine("  .lineXC{background-color:#FFFFFF;font-weight:bold;}");
sw.WriteLine("</style>");

sw.WriteLine("</head>");
sw.WriteLine("<body>");
//sw.WriteLine("<script src=\"help.js\"></script>");
if (JavaScript)
   {sw.WriteLine("<script>");
    sw.WriteLine("function ShowHelp() {window.open(document.getElementById(\"SpecificationBaseUrl\").getAttribute(\"href\"));}");
    sw.WriteLine("function EntityHelp(layer, entity) {window.open(document.getElementById(\"SpecificationBaseUrl\").getAttribute(\"href\") + '/schema/ifc' + layer.toLowerCase() + '/lexical/ifc' + entity.toLowerCase() + '.htm');}");
    sw.WriteLine("</script>");
   }

sw.WriteLine("<div class=\"global\">");
sw.WriteLine(FormattedHeaderLine("ISO-10303-21;"));
sw.WriteLine(FormattedHeaderLine("HEADER;","  id=\"SpecificationBaseUrl\" href=\""+ifc.Specification.SpecificationBaseUrl+"/HTML\""));
sw.WriteLine(FormattedHeaderLine("FILE_DESCRIPTION (('"+Header.ViewDefinition+"'), '2;1');"));
sw.WriteLine(FormattedHeaderLine("FILE_NAME ('"+Header.Name+"', '"+NetSystem.String.Format("{0:s}",NetSystem.DateTime.Now)+"', ('"+Header.Author+"'), ('"+Header.Organization+"'), '"+ Header.PreprocessorVersion+"', '"+Header.OriginatingSystem+"', '"+Header.Authorization+"');"));
sw.WriteLine(FormattedHeaderLine("FILE_SCHEMA (('"+ifc.Specification.SchemaName+"'));"," title=\"Click to show schema documentation\" href=\"javascript:ShowHelp()\"));",true));
sw.WriteLine(FormattedHeaderLine("ENDSEC;"));
sw.WriteLine(FormattedHeaderLine("DATA;"));

//foreach (KeyValuePair<int,ENTITY> kvp in AssignedEntityDict) sw.Write(kvp.Value.ToHtml()+"<br/>");  
foreach (ENTITY e in EntityList) sw.Write(e.ToHtml()); 
sw.WriteLine(FormattedHeaderLine("ENDSEC;"));
sw.WriteLine(FormattedHeaderLine("END-ISO-10303-21;"));

sw.WriteLine("</div>"); // class globl (pre)
sw.WriteLine("</body>");
sw.WriteLine("</html>");
sw.Close();
}// of ToHmlFile
}// of Model ==========================================================================================================



}//ifc ==============================
