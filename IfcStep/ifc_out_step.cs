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

public static string StepAttributeOut(object o){//-------------------------------------------------
string s=""; 
          if (o==null)       s="$";  
     else if (o is Enum)     {s="."+o.ToString()+".";}
     else if (o is SELECT)   {if ( ((SELECT)o).IsNull) s="$";
                              else { if (((SELECT)o).SelectValue() is ENTITY) s=((SELECT)o).SelectValue().ToString(); 
                                     else  s="IFC"+((SELECT)o).SelectType().Name.ToUpper()+"("+((SELECT)o).SelectValue().ToString()+")";
                                   }
                             }
     else if (o is ENTITY)                                       s=((ENTITY)o).IfcId();
     else if (o is TypeBase)                                     s=o.ToString();
     else if( typeof(IEnumerable).IsAssignableFrom(o.GetType())) s=o.ToString();
     else                                                        s=o.ToString();
return s;
}//------------------------------------------------------------------------------------------------



public virtual string ToStepLine(){//--------------------------------------------------------------
string s=this.IfcId()+"=IFC"+this.GetType().Name.ToUpper()+"(";
AttribListType AttribList=TypeDictionary.GetComponents(this.GetType()).AttribList;
int sep=0;foreach (FieldInfo field in AttribList) s+=((++sep>1)?",":"")+StepAttributeOut(field.GetValue(this));
s+=");";
if (EndOfLineComment!=null) s+="/* "+EndOfLineComment+" */";
return s;
}//------------------------------------------------------------------------------------------------

}//================================================================================================


public partial class EntityComment:ENTITY{//=======================================================
public override string ToStepLine() {return "/* "+CommentLine+" */";}
}//================================================================================================



public partial class Model{//======================================================================
public void ToStepFile()//-------------------------------------------------------------------------
{
Threading.Thread.CurrentThread.CurrentCulture=CultureInfo.InvariantCulture;
foreach (ENTITY e in EntityList)  if (e is ifc.Root)  if ( ((ifc.Root)e).GlobalId==null) ((ifc.Root)e).GlobalId=ifc.GloballyUniqueId.NewId();
StreamWriter sw=new StreamWriter(Header.name+".ifc");
sw.WriteLine("ISO-10303-21;");
sw.WriteLine("HEADER;");
sw.WriteLine("FILE_DESCRIPTION (('"+Header.description+"'), '2;1');");
sw.WriteLine("FILE_NAME ('"+Header.name+"', '"+NetSystem.String.Format("{0:s}",NetSystem.DateTime.Now)+"', ('"+Header.author+"'), ('"+Header.organization+"'), '"+ Header.preprocessor_version+"', '"+Header.originating_system+"', '"+Header.authorization+"');");
sw.WriteLine("FILE_SCHEMA (('"+Specification.SchemaName+"'));");
sw.WriteLine("ENDSEC;");
sw.WriteLine("DATA;");
foreach (ENTITY e in EntityList) sw.WriteLine(e.ToStepLine());
sw.WriteLine("ENDSEC;");
sw.WriteLine("END-ISO-10303-21;");
sw.Close();
}// of ToStepFile ---------------------------------------------------------------------------------
}// of Model ======================================================================================



}// ifc ###########################################################################################
