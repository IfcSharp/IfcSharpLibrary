// ifc_schema.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using NetSystem=System;
using System.Collections.Generic;
using System.Reflection;

namespace ifc{//===================================================================================

public partial class ENTITY{//=====================================================================



public class AttribListType:List<FieldInfo>{//-----------------------------------------------------
public       AttribListType(){}
public       AttribListType(NetSystem.Type EntityType){
TemporaryAttribDict.Clear();int VarCount=0;
foreach (FieldInfo field in EntityType.GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) 
   foreach (NetSystem.Attribute attr in field.GetCustomAttributes(true)) if (attr is ifcAttribute) {TemporaryAttribDict.Add(((ifcAttribute)attr).OrdinalPosition,field);VarCount++;}
for (int i=1;i<=VarCount;i++) this.Add(TemporaryAttribDict[i]);
 }
private static Dictionary<int,FieldInfo> TemporaryAttribDict=new Dictionary<int,FieldInfo>();
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
foreach (NetSystem.Type t in NetSystem.Reflection.Assembly.GetAssembly(typeof(ifc.ENTITY)).GetTypes()) 
  if (t.IsClass) if (!t.IsAbstract) if (t.IsSubclassOf(typeof(ifc.ENTITY))) EntityTypeComponentsList.Add(new ComponentsType(t));
foreach (ComponentsType ct in EntityTypeComponentsList) EntityTypeComponentsDict.Add(ct.EntityType,ct);

foreach ( NetSystem.Reflection.Assembly a in NetSystem.AppDomain.CurrentDomain.GetAssemblies())  foreach (NetSystem.Type t in a.GetTypes()) 
        {if (   (t.IsEnum)      
             || (t.IsSubclassOf(typeof(ifc.ENTITY)))
             || (t.IsSubclassOf(typeof(ifc.SELECT))) 
             || (t.IsSubclassOf(typeof(ifc.TypeBase)))
             || (typeof(ifcListInterface).IsAssignableFrom(t)) 
            ) foreach (NetSystem.Attribute attr in t.GetCustomAttributes(true)) if (attr is ifcSqlAttribute) 
                      {TypeIdNameDict.Add( ((ifcSqlAttribute)attr).SqlTypeId, t.Name);
                       TypeIdTypeDict.Add( ((ifcSqlAttribute)attr).SqlTypeId, t);
                       }                                                                          
        }
}//................................................................................................

public static Dictionary<int,string> TypeIdNameDict=new Dictionary<int, string>();
public static Dictionary<int,NetSystem.Type> TypeIdTypeDict=new Dictionary<int,NetSystem.Type >();

}//------------------------------------------------------------------------------------------------


}// of ENTITY =====================================================================================


}// ifc============================================================================================
