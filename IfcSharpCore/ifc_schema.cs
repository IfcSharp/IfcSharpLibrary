// ifc_schema.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using NetSystem=System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace ifc{//===================================================================================

public partial class ENTITY{//=====================================================================

public class AttribInfo{
public       AttribInfo(FieldInfo field,int OrdinalPosition, bool IsDerived,bool optional){this.field=field;this.OrdinalPosition=OrdinalPosition;this.IsDerived=IsDerived;this.optional=optional;}
public FieldInfo field=null;
public int OrdinalPosition=0;
public bool IsDerived=false;
public bool optional=false; 
}

public class AttribListType:List<AttribInfo>{//-----------------------------------------------------
public       AttribListType(){}
public       AttribListType(NetSystem.Type EntityType){
TemporaryAttribDict.Clear();int VarCount=0;
foreach (FieldInfo field in EntityType.GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) 
   foreach (NetSystem.Attribute attr in field.GetCustomAttributes(true)) if (attr is ifcAttribute) {
       TemporaryAttribDict.Add(((ifcAttribute)attr).OrdinalPosition,
           new AttribInfo(field,
               ((ifcAttribute)attr).OrdinalPosition, 
               ((ifcAttribute)attr).derived,
               ((ifcAttribute)field.GetCustomAttributes(true).Where(a=>a is ifcAttribute).Select(a=>(ifcAttribute)a).FirstOrDefault()).optional));
       VarCount++;
   }
for (int i=1;i<=VarCount;i++) this.Add(TemporaryAttribDict[i]);
 }
public static Dictionary<int,AttribInfo> TemporaryAttribDict=new Dictionary<int,AttribInfo>();
};//------------------------------------------------------------------------------------------------

public class InversListType:List<FieldInfo>{//-----------------------------------------------------
public       InversListType(){}
public       InversListType(NetSystem.Type EntityType){foreach (FieldInfo field in EntityType.GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) 
                                                         foreach (NetSystem.Attribute attr in field.GetCustomAttributes(true)) if (attr is ifcInverseAttribute) this.Add(field);
                                                      }
}//------------------------------------------------------------------------------------------------

public class ComponentsType{//---------------------------------------------------------------------
public       ComponentsType(){}
public       ComponentsType(NetSystem.Type EntityType){this.EntityType=EntityType;AttribList=new AttribListType(EntityType);InversList=new InversListType(EntityType);}
public NetSystem.Type EntityType=null;
public AttribListType AttribList=null;
public InversListType InversList=null;
}//------------------------------------------------------------------------------------------------

public static class TypeDictionary{//--------------------------------------------------------------

public static ComponentsType GetComponents(NetSystem.Type EntityType){//...........................
if (!EntityTypeComponentsDict.ContainsKey(EntityType)) EntityTypeComponentsDict.Add(EntityType,new ComponentsType(EntityType));
return EntityTypeComponentsDict[EntityType];
}//................................................................................................

public static Dictionary<NetSystem.Type,ComponentsType> EntityTypeComponentsDict=new Dictionary<NetSystem.Type,ComponentsType>();
public static List<ComponentsType> EntityTypeComponentsList=new List<ComponentsType>();

public static void FillEntityTypeComponentsDict(){//.............................................................
EntityTypeComponentsList.Clear();
EntityTypeComponentsDict.Clear();
TypeIdNameDict.Clear();
TypeIdTypeDict.Clear();
foreach (NetSystem.Type t in NetSystem.Reflection.Assembly.GetAssembly(typeof(ifc.ENTITY)).GetTypes()) 
  if (t.IsClass) if (!t.IsAbstract) if (t.IsSubclassOf(typeof(ifc.ENTITY))) EntityTypeComponentsList.Add(new ComponentsType(t));
foreach (ComponentsType ct in EntityTypeComponentsList) EntityTypeComponentsDict.Add(ct.EntityType,ct);

Assembly currentAssembly = NetSystem.AppDomain.CurrentDomain.GetAssemblies().First((a =>
    NetSystem.Diagnostics.Process.GetCurrentProcess().ProcessName == a.GetName().Name));

foreach (NetSystem.Reflection.Assembly a in NetSystem.AppDomain.CurrentDomain.GetAssemblies())
{
    //if (NetSystem.Diagnostics.Process.GetCurrentProcess().ProcessName != a.GetName().Name) continue;
    //2023-10-23: ef : check any other loaded assembly located in the same folder as the currently running assembly
    //                 this way it is also possible to have the IfcSharpLibrary contained in a separate project/assembly
    if (NetSystem.IO.Path.GetDirectoryName(currentAssembly.Location) !=
        NetSystem.IO.Path.GetDirectoryName(a.Location)) continue;
    
    foreach (NetSystem.Type t in a.GetTypes())
    {
        if ((t.IsEnum)
            || (t.IsSubclassOf(typeof(ifc.ENTITY)))
            || (t.IsSubclassOf(typeof(ifc.SELECT)))
            || (t.IsSubclassOf(typeof(ifc.TypeBase)))
            || (typeof(ifcListInterface).IsAssignableFrom(t))
           )
            foreach (NetSystem.Attribute attr in t.GetCustomAttributes(true))
                if (attr is ifcSqlAttribute)
                    if (((ifcSqlAttribute)attr).SqlTypeId != 0)
                    {
                        if (TypeIdNameDict.ContainsKey(((ifcSqlAttribute)attr).SqlTypeId))
                            throw new NetSystem.Exception(
                                "Error on FillEntityTypeComponentsDict: double (Sql)TypeId=" +
                                ((ifcSqlAttribute)attr).SqlTypeId);
                        TypeIdNameDict.Add(((ifcSqlAttribute)attr).SqlTypeId, t.Name);
                        TypeIdTypeDict.Add(((ifcSqlAttribute)attr).SqlTypeId, t);
                    }
    }
}
}//................................................................................................

public static Dictionary<int,string> TypeIdNameDict=new Dictionary<int, string>();
public static Dictionary<int,NetSystem.Type> TypeIdTypeDict=new Dictionary<int,NetSystem.Type >();

}//------------------------------------------------------------------------------------------------


}// of ENTITY =====================================================================================


}// ifc============================================================================================
