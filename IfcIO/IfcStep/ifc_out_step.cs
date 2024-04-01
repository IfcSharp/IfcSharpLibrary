// ifc_out_step.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Reflection;
using Threading=System.Threading;
using System.IO;
using NetSystem=System;

namespace ifc{//###################################################################################

public partial class ENTITY{//=====================================================================

public static string StepAttributeOut(object o,AttribInfo attrib=null){//-------------------------------------------------
string s;//=""; 
            if (o == null) { if (attrib == null) s = "$"; else s = ((attrib.IsDerived) ? "*" : "$"); }
            else if (o is Enum) { s = "." + o.ToString() + "."; }
            else if (o is SELECT) {
                if (o is ifc.LayeredItem li)
                {
                    bool stop = true;
                }
                
                if (((SELECT)o).IsNull) {
                    if (attrib.IsDerived == false) s = "$";
                    else s = "*";//2022-06-10 (ef): derived attributes which are NULL are written as '*'
                }
                else {
                    if (((SELECT)o).SelectValue() is ENTITY) s = ((SELECT)o).SelectValue().ToString();
                    else s = "IFC" + ((SELECT)o).SelectType().Name.ToUpper() + "(" + ((SELECT)o).SelectValue().ToString() + ")";
                }
            }
            else if (o is ENTITY) s = ((ENTITY)o).IfcId();
            else if (o is TypeBase) {
                if (((TypeBase)o).IsNull && attrib.IsDerived) s = "*"; //2022-06-10 (ef): derived attributes which are NULL are written as '*'
                else s = o.ToString();
            }
            else if (typeof(IEnumerable).IsAssignableFrom(o.GetType())) s = o.ToString();
            else s = o.ToString();
return s;
}//------------------------------------------------------------------------------------------------


public static string IfcLineConstText="=IFC"; //BB-2024-04-01: possibiblity for whitespace between = and IFC
public virtual string ToStepLine(){//--------------------------------------------------------------
string s=this.IfcId()+IfcLineConstText+this.GetType().Name.ToUpper()+"(";
AttribListType AttribList=TypeDictionary.GetComponents(this.GetType()).AttribList;
int sep=0;foreach (AttribInfo attrib in AttribList) s+=((++sep>1)?",":"")+StepAttributeOut(attrib.field.GetValue(this),attrib);
s+=");";
if (EndOfLineComment!=null) s+="/* "+EndOfLineComment+" */";
return s;
}//------------------------------------------------------------------------------------------------

}//================================================================================================


public partial class EntityComment:ENTITY{//=======================================================
public override string ToStepLine() {return "/* "+CommentLine+" */";}
}//================================================================================================



public partial class Model{//======================================================================
public void ToStepFile(string filePath="")//-------------------------------------------------------------------------
{
Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
foreach (ENTITY e in EntityList) if (e is ifc.Root) if (((ifc.Root)e).GlobalId == null) ((ifc.Root)e).GlobalId = ifc.GloballyUniqueId.NewId();
//EF-2021-03-02: added support for the definition of a filepath, if the filepath is omitted, we use the headername of the Model
if (string.IsNullOrEmpty(filePath)) filePath = Header.Name + ".ifc";
StreamWriter sw = new StreamWriter(filePath);
sw.WriteLine("ISO-10303-21;");
sw.WriteLine("HEADER;");
sw.WriteLine("FILE_DESCRIPTION (('" + Header.ViewDefinition + "'), '2;1');");
sw.WriteLine("FILE_NAME ('" + Header.Name + "', '" + NetSystem.String.Format("{0:s}", NetSystem.DateTime.Now) + "', ('" + Header.Author + "'), ('" + Header.Organization + "'), '" + Header.PreprocessorVersion + "', '" + Header.OriginatingSystem + "', '" + Header.Authorization + "');");
sw.WriteLine("FILE_SCHEMA (('" + Specification.SchemaName + "'));");
sw.WriteLine("ENDSEC;");
sw.WriteLine("DATA;");
foreach (ENTITY e in EntityList) sw.WriteLine(e.ToStepLine());
sw.WriteLine("ENDSEC;");
sw.WriteLine("END-ISO-10303-21;");
sw.Close();
}// of ToStepFile ---------------------------------------------------------------------------------
}// of Model ======================================================================================



}// ifc ###########################################################################################
