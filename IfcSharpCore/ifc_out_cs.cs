// ifc_out_cs.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using NetSystem=System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using Threading=System.Threading;
using System.Reflection;
using System.Text;
using System.Xml;


namespace ifc{//==============================

public partial class ENTITY{//==========================================================================================

protected static int cnt=0;

public  string EntityVarName(int Id){if (Id>0) return ifc.Repository.CurrentModel.EntityDict[Id].ShortTypeName()+Id.ToString(); else return "comment"+ NetSystem.Math.Abs(Id);}
                              

public  string CsOut(object o){
string s=""; 
cnt++;
          if (o==null)         s="null";
     else if (o is NetSystem.Enum)     {if (o.ToString()=="_NULL") s= "ifc."+o.GetType().Name+"._NULL"; else s="ifc."+o.GetType().Name+"."+o.ToString();}
     else if (o is SELECT)   {     if (((SELECT)o).Id>0) s="new ifc."+o.GetType().Name+"("+EntityVarName(((SELECT)o).Id)+")"; 
                              else if( ((SELECT)o).SelectValue()==null  ) s="null"; 
                              else s="new ifc."+o.GetType().Name+"(("+((SELECT)o).SelectType()+")"+((SELECT)o).SelectValue().ToString().Replace('\'','"')+")";
                             }
     else if (o is ENTITY)   {if (((ENTITY)o).Id>0) s=EntityVarName(((ENTITY)o).Id); else s="null";}
     else if (o is TypeBase) {TypeBase tb=(TypeBase)o;
                              if (o is ifc.Logical)  {s="(ifc.Logical)";if (o.ToString()=="False)") s+="false"; else s+="true";} else
                              if (o is ifc.Boolean)  {s="(ifc.Boolean)";if (o.ToString()=="False)") s+="false"; else s+="true";} else
                              if (o is ifc.GloballyUniqueId) s="ifc.GloballyUniqueId.NewId() /*\""+o.ToString()+"\"*/"; else 
                              if (tb.IsNull) s="null"; else
                                 {if (tb.GetBaseType()==typeof(NetSystem.String)) {if ( ((TypeBase)o).IsNull /*.ToString()==""*/) s="null";else s="new ifc."+o.GetType().Name+"(\""+o.ToString()+"\")"; } 
                                  else if( typeof(IEnumerable).IsAssignableFrom(tb.GetBaseType())) 
                                         {ifcListInterface li=(ifcListInterface)tb.GetBaseType(); 
                                          s="new ifc."+o.GetType().Name+o.ToString();
                                         } 
                                  else s="(ifc."+o.GetType().Name+")("+o.ToString()+")";
                                 }
                             }
     else if (o is NetSystem.String)   {if (o.ToString()=="") s="null";else s="\""+o.ToString()+"\"";}

     else if( typeof(IEnumerable).IsAssignableFrom(o.GetType()))
                         {bool TypeDisplay=(this.GetType()!=typeof(ifc.CartesianPoint) ) && (this.GetType()!=typeof(ifc.Direction) );
                          if (TypeDisplay) s+="new "+o.GetType().ToString().Replace("`1[","<").Replace("]",">")+"(";
                          int pos=0; 
                          foreach (object item in (IEnumerable)o) if (item!=null) 
                                 {pos++;if (pos>1) s+=","; 
                                       if (item is ENTITY) s+=EntityVarName(((ENTITY)item).Id);
                                  else if (item is SELECT) {if (((SELECT)item).Id>0) s+="new ifc."+item.GetType().Name+"("+EntityVarName(((SELECT)item).Id)+")"; else    CsOut(((SELECT)item).SelectValue());}
                                  else if (item is TypeBase) {TypeBase tb=(TypeBase)item;
                                                                   if (tb.GetBaseType()==typeof(NetSystem.String)) {if (item.ToString()=="") s+=""; /* null */else s+="(ifc."+item.GetType().Name+")\""+item.ToString()+"\""; } 
                                                              else if( typeof(IEnumerable).IsAssignableFrom(tb.GetBaseType())) {s+="new ifc."+item.GetType().Name+item.ToString();} 
                                                              else {if (TypeDisplay)  s+="(ifc."+item.GetType().Name+")("+item.ToString()+")"; else s+=item.ToString(); }
                                                             }
                                  else if (item is double  ) {s+=item.ToString(); }
                                  else throw new ifc.Exception("CsOut: unknown enumerable-type"); 
                                 }
                          if (TypeDisplay) s+=")";
                         }
     else                     s=o.ToString();

                             
return s;
}

public virtual string ToCs(){
Threading.Thread.CurrentThread.CurrentCulture=CultureInfo.InvariantCulture;
string s="";

string ElementName=this.GetType().ToString().Replace("IFC4","ifc");
int ElementNameMaxSize=35;
if  (ElementName.Length<ElementNameMaxSize) ElementName+=new string(' ',ElementNameMaxSize-ElementName.Length);
int IdStringMaxSize=ElementNameMaxSize+4;

string IdString=EntityVarName(this.Id);
if  (IdString.Length<IdStringMaxSize) IdString+=new string(' ',IdStringMaxSize-IdString.Length);

if (this is ifc.EntityComment) s=new string(' ',IdStringMaxSize)+"     new "+ElementName+"(";
else                           s="var "+IdString+"=new "+ElementName+"(";

AttribListType AttribList=TypeDictionary.GetComponents(this.GetType()).AttribList;
int sep=0;foreach (FieldInfo field in AttribList) s+=((++sep>1)?",":"")+field.Name+":"+CsOut(field.GetValue(this));
if (this.EndOfLineComment!=null) if (this.EndOfLineComment.Length>0) s+=",\""+this.EndOfLineComment+'"';
if (this is ifc.EntityComment) s+="\""+((ifc.EntityComment)this).CommentLine.TrimEnd(' ')+'"';
return s+");";// //#"+(this.SortPos);
}


public static Dictionary<int,string> IdVarNameDict=new Dictionary<int,string>(); // contains the assognment of Entity-Ids to variable-names

public static void EvalPdbXml(string PdbFileName,string MethodName,int IdOffset=0)
{
XmlDocument doc=new XmlDocument();
            doc.Load(PdbFileName);

foreach (XmlNode node1 in doc.ChildNodes) 
        {if (node1 is XmlElement) if (node1.Name=="SymbolData") 
            {foreach (XmlNode node2 in node1.ChildNodes) if (node2 is XmlElement) if (node2.Name=="method") if (node2.Attributes["name"].Value==MethodName)
                    {foreach (XmlNode node3 in node2.ChildNodes) if (node3 is XmlElement) if (node3.Name=="rootScope") 
                             {foreach (XmlNode node4 in node3.ChildNodes) if (node4 is XmlElement) if (node4.Name=="scope")  
                                      foreach (XmlNode node5 in node4.ChildNodes) if (node5 is XmlElement) if (node5.Name=="local") IdVarNameDict.Add(int.Parse(node5.Attributes["ilIndex"].Value),node5.Attributes["name"].Value); // Console.WriteLine(int.Parse(node5.Attributes["ilIndex"].Value)+": "+ node5.Attributes["name"].Value);

                             }
                    }
            }
        }
ifc.Repository.CurrentModel.AssignEntities();
}


}// of ENTITY =========================================================================================================

public partial class Model{//==========================================================================================
public void ToCsFile()
{
AssignEntities();
SortEntities();
StreamWriter sw=new StreamWriter(Header.name+".cs",false,Encoding.Default);
sw.WriteLine("");
sw.WriteLine("// CAUTION! THIS IS A GENERATED FILE! IT WILL BE OVERWRITTEN AT ANY TIME! ");
sw.WriteLine(@"// created with https://github.com/IfcSharp");
sw.WriteLine("");
sw.WriteLine("public class ifcOut{ public static void Generated(){ // ##########################################");
sw.WriteLine("");
sw.WriteLine("ifc.Repository.CurrentModel.ClearEntityList();");
sw.WriteLine("ifc.Repository.CurrentModel.Header.name=\"generated_from_IfcSharp_ifc_Model_ToCsFile()\"");
if (AssignedEntityDict==null) throw new ifc.Exception("AssignedEntityDict is not initialized");
foreach (KeyValuePair<int,ENTITY> kvp in AssignedEntityDict) sw.WriteLine(kvp.Value.ToCs());  
sw.WriteLine("");
sw.WriteLine("ifc.Repository.CurrentModel.ToStepFile();");
sw.WriteLine("}/* of void */ } // of class #####################################################################");
sw.WriteLine("");
sw.Close();
}// of ToCsFile
}// of Model ==========================================================================================================

}// ifc==============================
