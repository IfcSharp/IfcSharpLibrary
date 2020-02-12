// ifc_out_xml.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Reflection;
using Threading=System.Threading;
using System.IO;
using System.Xml;
using System.Text;

namespace ifc{//==============================


public partial interface ifcListInterface{
void ToÍfcXml(XmlWriter xml);
}

public partial class LIST<T> : List<T>,ifcListInterface,ifcSqlTypeInterface{
//public partial class LIST<T> {
public void ToÍfcXml(XmlWriter xml){foreach (T t in this)  ENTITY.IfcXmlOut(t,t.GetType().ToString(),xml); //hier noch Exluding Attribute
                                   }
}


public partial class ENTITY{//==========================================================================================



public static void IfcXmlOut(object o,string Name,XmlWriter xml){
/*
Entityausgabe
1. Typen als Attribute (ausser SELECT und Listen) 
2. Selects (Name des Attributes, danach der Klasse mit "-wrapper")
3. Listentypen (Auflistng der Instanztpnamen, zumindest bei EntityRef-Listen)
4. EntityRefs (Name des Attributes, nicht der Klasse)
5. InversElemente nach gleichem Schema wie Entityausgabe

  <IfcExtrudedAreaSolid Depth="40">
	  <SweptArea xsi:type="IfcRectangleProfileDef" ProfileType="area" ProfileName="Panel" XDim="800" YDim="2040">
	  </SweptArea>
	  <Position xsi:nil="true" ref="i1"/>
	  <ExtrudedDirection xsi:nil="true" ref="i6"/>
  </IfcExtrudedAreaSolid>

*/
//Console.WriteLine("EEEa "+Name );

          if (o==null)       {}  // hier OptionalFlag abfragen !!!
     else if (o is Enum)     {xmlAttrib(xml,Name,o.ToString());}
     else if (o is SELECT)   {if (!((SELECT)o).IsNull)
                                 {xmlStart(xml,Name);//xml.WriteAttributeString("nil","xsi","true");xml.WriteAttributeString("ref","i"+((ENTITY)o).Id);
                                     xmlElement(xml,((SELECT)o).SelectType().Name+"-wrapper",((SELECT)o).SelectValue().ToString());
                    // hier noch ENTITY-Fallunterscheidung
                                  xmlEnd(xml);} 
                                    //if (((SELECT)o).SelectValue() is ENTITY) s=((SELECT)o).SelectValue().ToString(); 
                                    // else  s="IFC"+((SELECT)o).SelectType().Name.ToUpper()+"("+((SELECT)o).SelectValue().ToString()+")";
                                  // }
                             }
     else if (o is ENTITY)   {if (((ENTITY)o).IsAssigned) {xmlStart(xml,Name);xmlAttrib(xml,"xsi","nil",null,"true");xmlAttrib(xml,"ref","i"+((ENTITY)o).Id);xmlEnd(xml);}
                              else                        {((ENTITY)o).ToIfcXml(xml);((ENTITY)o).IsAssigned=true;}
                             }
     else if (o is TypeBase) {if (!((TypeBase)o).IsNull) xmlAttrib(xml,Name,o.ToString());}
     else if (o is ifcListInterface)  {((ifcListInterface)o).ToÍfcXml(xml);} // noch anders lösen 
     else                    {}// error

//var x=new ifc.SimplePropertyTemplate(); new ifc.PropertyEnumeration()

}

public static bool IsAttribute(Type FieldType)
{
if (FieldType.IsSubclassOf(typeof(TypeBase))) return true;
if (FieldType.IsSubclassOf(typeof(Enum))) return true;
if ( (Nullable.GetUnderlyingType(FieldType)!=null) && (Nullable.GetUnderlyingType(FieldType).IsSubclassOf(typeof(Enum))) )  return true;
return false;
}

// virtual for comment-Lines
public virtual void ToIfcXml(XmlWriter xml){
if (EndOfLineComment!=null) xmlComment(xml,EndOfLineComment);
xmlStart(xml,"ifc"+this.GetType().Name);


// hier noch austauschen ##############################
Dictionary<int,FieldInfo> VarDict=new Dictionary<int,FieldInfo>();
List<FieldInfo> InverseList=new List<FieldInfo>();
int VarCount=0;
foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) 
foreach (Attribute attr in field.GetCustomAttributes(true)) 
        {if (attr is ifcAttribute) {VarDict.Add(((ifcAttribute)attr).OrdinalPosition,field);VarCount++;} //  if (attr is ifcAttribute)
         if (attr is ifcInverseAttribute) InverseList.Add(field);
        }
//Console.WriteLine("CCC "+this.GetType().Name);
// hier noch austauschen ##############################


for (int i=1;i<=VarCount;i++) if ( IsAttribute(VarDict[i].FieldType)) IfcXmlOut(VarDict[i].GetValue(this),VarDict[i].Name,xml);
for (int i=1;i<=VarCount;i++) if (!IsAttribute(VarDict[i].FieldType)) IfcXmlOut(VarDict[i].GetValue(this),VarDict[i].Name,xml);
//Console.WriteLine("DDD");

/*
if(1==2)
foreach (FieldInfo f in InverseList) {// über Listenelemnete iterieren
                                      // Prüfung ob Listenelemente bereits verwendet 
                                      xml.WriteStartElement(f.Name);
             
                                      xml.WriteEndElement();
                                     } //IfcXmlOut(f.GetValue(this),f.Name,xml);
// Invers-Elemente auswerten, sofern diese auf noch nicht verwendete Elemnet verweisen
//new ifc.Project
*/
xmlEnd(xml);
}


static private bool XmlConsole=true;
static private bool XmlFile=true;
static private int  XmlNestLevel=1;
private static void xmlStart(XmlWriter xml,string Name){XmlNestLevel++; if (XmlConsole) Console.WriteLine(XmlNestLevel+new string(' ',XmlNestLevel*2)+Name);if (XmlFile) xml.WriteStartElement(Name);}
private static void xmlAttrib(XmlWriter xml,string Name, string value){ if (XmlConsole) Console.WriteLine(XmlNestLevel+new string(' ',XmlNestLevel*2+2)+Name+"="+value);if (XmlFile) xml.WriteAttributeString(Name,value);}
private static void xmlAttrib(XmlWriter xml,string pf,string Name,string ns, string value){ if (XmlConsole) Console.WriteLine(XmlNestLevel+new string(' ',XmlNestLevel*2+2)+pf+":"+Name+"="+value);if (XmlFile) xml.WriteAttributeString(pf,Name,ns,value);}
private static void xmlElement(XmlWriter xml,string Name, string value){ if (XmlConsole) Console.WriteLine(XmlNestLevel+new string(' ',XmlNestLevel*2+2)+Name+"#"+value);if (XmlFile) xml.WriteElementString(Name,value);}
private static void xmlComment(XmlWriter xml,string cmt){ if (XmlConsole) Console.WriteLine(XmlNestLevel+new string(' ',XmlNestLevel*2+2)+"<!-- "+cmt+" -->");if (XmlFile) xml.WriteComment(cmt);}
private static void xmlEnd  (XmlWriter xml){if (XmlNestLevel<1) XmlNestLevel=0;if (XmlConsole) Console.WriteLine(XmlNestLevel+new string(' ',XmlNestLevel*2)+"End of Node");if (XmlFile) xml.WriteEndElement();XmlNestLevel--;}

}//=====================================================================================================================

public partial class Model{//==========================================================================================
public void ToXmlFile()
{


SortEntities();
foreach (ENTITY e /* abstract */ in EntityList) e.AssignInverseElements(); 
ENTITY root=null;
foreach (ENTITY e /* abstract */ in EntityList) if (e is ifc.Project) root=e;
foreach (ENTITY e in EntityList) e.IsAssigned=false;


XmlWriterSettings settings = new XmlWriterSettings();
                  settings.Indent = true;
                  settings.IndentChars ="  ";// "\t";
                  settings.Encoding=Encoding.Default;  

/*
StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());
sw.AutoFlush = true;
Console.SetOut(sw);
*/    
char SaveStringChar=TypeBase.StringChar;
                    TypeBase.StringChar='\0';  
bool SaveHasStringChar=TypeBase.HasStringChar;
                      TypeBase.HasStringChar=false;  
//XmlWriter       xml = XmlWriter.Create(sw /*Header.name+".ifcXml" */ , settings); // ifcXml
  XmlWriter       xml = XmlWriter.Create(Header.name+".ifcXml", settings);
                 
                xml.WriteProcessingInstruction("xml", "version='1.0' encoding='utf-8'");
                xml.WriteStartElement(prefix:"ifc", localName:"ifcXML",ns:@"http://www.buildingsmart-tech.org/ifc/IFC4x2/final");
                    xml.WriteAttributeString(prefix:"xsi",localName:"schemaLocation",ns:@"http://www.w3.org/2001/XMLSchema-instance",value:@"http://www.buildingsmart-tech.org/ifc/IFC4x2/final IFC4x2.xsd");
                    xml.WriteAttributeString(             localName:"xmlns" ,value:@"http://www.buildingsmart-tech.org/ifc/IFC4x2/final");

                    xml.WriteStartElement("header");
                      xml.WriteElementString("name",Header.name);
                      xml.WriteElementString("time_stamp",Header.time_stamp);
                      xml.WriteElementString("author",Header.author);
                      xml.WriteElementString("organization",Header.organization);
                      xml.WriteElementString("preprocessor_version",Header.preprocessor_version);
                      xml.WriteElementString("originating_system",Header.originating_system);
                      xml.WriteElementString("authorization",Header.authorization);
                      xml.WriteElementString("documentation",Header.documentation);
                    xml.WriteEndElement();  // header


root.ToIfcXml(xml);

// alle weiteren noch nicht verwendeten Entities

                xml.WriteEndElement();  // ifc.ifcXml

xml.Close();
TypeBase.StringChar=SaveStringChar;
TypeBase.HasStringChar=SaveHasStringChar;

/*
StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
standardOutput.AutoFlush = true;
Console.SetOut(standardOutput);
*/

}// of ToIfcXmlFile

}// of class ENTITY =====================================================================================================================
}// of namespace ifc=======================================
