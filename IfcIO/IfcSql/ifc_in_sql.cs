// ifc_in_sql.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using Threading=System.Threading;
using System.Reflection;

using db;

namespace ifc{//==============================

public partial class ENTITY{//==========================================================================================

}// of ENTITY =========================================================================================================

public partial class Model{//==========================================================================================

public Dictionary<long,int> LocalIdFromGlobalIdDict=new Dictionary<long,int>();

public void EvalIfcRow(ifcSQL.ifcInstance.Entity_Row e)
{try{
Type t=null;
RowBase rb=null;
object o=null;
Type AttributeInstanceType=null;
Type AttributeInstanceType2=null;

try {t=Type.GetType("ifc."+ifc.ENTITY.TypeDictionary.TypeIdNameDict[e.EntityTypeId],true,true);  } catch(Exception) { Log.Add($"ERROR on EvalIfcRow.1, e.EntityTypeId={e.EntityTypeId} not found in IFcSchema {ifc.Specification.SchemaName}", Log.Level.Exception);}// 2. true: ignoreCase
ENTITY CurrentEntity=(ENTITY)Activator.CreateInstance(t);
try  { CurrentEntity.LocalId=LocalIdFromGlobalIdDict[e.GlobalEntityInstanceId];} catch(Exception ex) { Log.Add($"ERROR on EvalIfcRow.2:{ex}", Log.Level.Exception);}
CurrentEntity.ifcSqlGlobalId=e.GlobalEntityInstanceId;
if (CurrentEntity.LocalId>=NextGlobalId) NextGlobalId=CurrentEntity.LocalId+1; // bb 22.09.2023

// commment-handling:
if (CurrentEntity is EntityComment) ((EntityComment)CurrentEntity).CommentLine=((ifcSQL.ifcInstance.EntityAttributeOfString_Row)e.AttributeValueDict[-1]).Value;
else if (e.AttributeValueDict.ContainsKey(-1))  CurrentEntity.EndOfLineComment=((ifcSQL.ifcInstance.EntityAttributeOfString_Row)e.AttributeValueDict[-1]).Value;

object[] TypeCtorArgs=new object[1];
int OrdinalPosition=0;
ENTITY.AttribListType AttribList=ENTITY.TypeDictionary.GetComponents(CurrentEntity.GetType()).AttribList;
foreach (ENTITY.AttribInfo attrib in AttribList)
        {++OrdinalPosition;
         if (e.AttributeValueDict.ContainsKey(OrdinalPosition))//----------------------------------------------------------------------------------------------------------
            {try  {rb=e.AttributeValueDict[OrdinalPosition];} catch(Exception ex) { Log.Add($"ERROR on EvalIfcRow.3:{ex}", Log.Level.Exception);}
         if (rb is ifcSQL.ifcInstance.EntityAttributeOfVector_Row)  {ifcSQL.ifcInstance.EntityAttributeOfVector_Row a=(ifcSQL.ifcInstance.EntityAttributeOfVector_Row)rb;
                                                                     if (a.TypeId==25) {if (a.Z!=null)  ((ifc.CartesianPoint)CurrentEntity).Coordinates=new List1to3_LengthMeasure((LengthMeasure)a.X,(LengthMeasure)a.Y,(LengthMeasure)(double)a.Z);
                                                                                        else            ((ifc.CartesianPoint)CurrentEntity).Coordinates=new List1to3_LengthMeasure((LengthMeasure)a.X,(LengthMeasure)a.Y); 
                                                                                       }
#if IFC2X3
                                                                     if (a.TypeId==42) {if (a.Z!=null)  ((ifc.Direction)CurrentEntity).DirectionRatios=new List2to3_double(a.X,a.Y,(double)a.Z);
                                                                                        else            ((ifc.Direction)CurrentEntity).DirectionRatios=new List2to3_double(a.X,a.Y); 
                                                                                       }

#else
                                                                     if (a.TypeId==42) {if (a.Z!=null)  ((ifc.Direction)CurrentEntity).DirectionRatios=new List2to3_Real((Real)a.X,(Real)a.Y,(Real)(double)a.Z);
                                                                                        else            ((ifc.Direction)CurrentEntity).DirectionRatios=new List2to3_Real((Real)a.X,(Real)a.Y); 
                                                                                       }
#endif   

                                                                    }
         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfString_Row)  {ifcSQL.ifcInstance.EntityAttributeOfString_Row a=(ifcSQL.ifcInstance.EntityAttributeOfString_Row)rb; 
                                                                          try{o=Activator.CreateInstance(ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId],a.Value);} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.4, a.TypeId={a.TypeId}:{ex}", Log.Level.Exception);}
                                                                          if (attrib.field.FieldType.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=o;o=Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs);} 
                                                                          attrib.field.SetValue(CurrentEntity,o);
                                                                         }
         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfEnum_Row)  {ifcSQL.ifcInstance.EntityAttributeOfEnum_Row a=(ifcSQL.ifcInstance.EntityAttributeOfEnum_Row)rb;
                                                                        Type UnderlyingType = Nullable.GetUnderlyingType(   attrib.field.FieldType);
                                                                        if  (UnderlyingType!=null && UnderlyingType.IsEnum) attrib.field.SetValue(CurrentEntity,Enum.ToObject(UnderlyingType, a.Value));
                                                                        else                                                attrib.field.SetValue(CurrentEntity,a.Value); 
                                                                       }
         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfBoolean_Row)   {ifcSQL.ifcInstance.EntityAttributeOfBoolean_Row a=(ifcSQL.ifcInstance.EntityAttributeOfBoolean_Row)rb; //BB-2024-04-01: added bool evaluation
                                                                            try{o=Activator.CreateInstance(type:ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId],(object)a.Value);} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.4, a.TypeId={a.TypeId}:{ex}", Log.Level.Exception);}
                                                                            if (attrib.field.FieldType.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=o;o=Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs);}
                                                                            attrib.field.SetValue(CurrentEntity,o);
                                                                           }
         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfInteger_Row)   {ifcSQL.ifcInstance.EntityAttributeOfInteger_Row a=(ifcSQL.ifcInstance.EntityAttributeOfInteger_Row)rb;
                                                                            try{o=Activator.CreateInstance(ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId],a.Value);} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.4, a.TypeId={a.TypeId}:{ex}", Log.Level.Exception);}
                                                                            if (attrib.field.FieldType.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=o;o=Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs);}
                                                                            attrib.field.SetValue(CurrentEntity,o);
                                                                           }
         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfFloat_Row)     {ifcSQL.ifcInstance.EntityAttributeOfFloat_Row a=(ifcSQL.ifcInstance.EntityAttributeOfFloat_Row)rb;
                                                                            try{o=Activator.CreateInstance(ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId],a.Value);} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.4, a.TypeId={a.TypeId}:{ex}", Log.Level.Exception);}
                                                                            if (attrib.field.FieldType.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=o;o=Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs);}
                                                                            attrib.field.SetValue(CurrentEntity,o);
                                                                           }

         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row)  {ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row a=(ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row)rb;  
                                                                             if (a.Value>0) 
                                                                                {try{AttributeInstanceType=ifc.ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId];} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.4, a.TypeId={a.TypeId}:{ex}", Log.Level.Exception);}
                                                                                 o=Activator.CreateInstance(AttributeInstanceType);
                                                                                 try{((ENTITY)o).LocalId=LocalIdFromGlobalIdDict[a.Value];} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.4, a.Value={a.Value}:{ex}", Log.Level.Exception);}
                                                                                 if (attrib.field.FieldType.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=o;o=Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs);}
                                                                                 attrib.field.SetValue(CurrentEntity,o);
                                                                                } 
                                                                            }

         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfList_Row)    {ifcSQL.ifcInstance.EntityAttributeOfList_Row a=(ifcSQL.ifcInstance.EntityAttributeOfList_Row)rb;  

                                                                          Type GenericType=null; 
                                                                          if (attrib.field.FieldType.BaseType.GetGenericArguments().Length>0) GenericType=attrib.field.FieldType.BaseType.GetGenericArguments()[0]; //LengthMeasure or CartesianPoint
                                                                          else                                                                GenericType=attrib.field.FieldType.BaseType.BaseType.GetGenericArguments()[0]; //CompoundPlaneAngleMeasure
                                                                          
                                                                          int ListDim1Count=a.AttributeValueDict.Count; 
                                                                          object[] FieldCtorArgs=new object[ListDim1Count];

                                                                          if (ListDim1Count>0)   
                                                                          if (a.AttributeValueDict[0] is ifcSQL.ifcInstance.EntityAttributeListElementOfString_Row)
                                                                                 for (int ListDim1Position=0;ListDim1Position<ListDim1Count;ListDim1Position++) 
                                                                                    {try{AttributeInstanceType=ifc.ENTITY.TypeDictionary.TypeIdTypeDict[((ifcSQL.ifcInstance.EntityAttributeListElementOfString_Row)a.AttributeValueDict[ListDim1Position]).TypeId];} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.5 (string),ListDim1Position={ListDim1Position}, a.TypeId={a.TypeId}:{ex}", Log.Level.Exception);}
                                                                                     string txt=((ifcSQL.ifcInstance.EntityAttributeListElementOfString_Row)a.AttributeValueDict[ListDim1Position]).Value; 
                                                                                     // bb 13.04.2024 double step ctor for select types:
                                                                                     if (GenericType.IsSubclassOf(typeof(SELECT))) FieldCtorArgs[ListDim1Position]=Activator.CreateInstance(GenericType,Activator.CreateInstance(AttributeInstanceType,txt)); 
                                                                                     else                                          FieldCtorArgs[ListDim1Position]=Activator.CreateInstance(GenericType,txt); 
                                                                                     } 

                                                                          if (ListDim1Count>0)
                                                                          if (a.AttributeValueDict[0] is ifcSQL.ifcInstance.EntityAttributeListElementOfEntityRef_Row)
                                                                                 for (int ListDim1Position=0;ListDim1Position<ListDim1Count;ListDim1Position++) 
                                                                                    {int Id=0;
                                                                                     try{Id=LocalIdFromGlobalIdDict[((ifcSQL.ifcInstance.EntityAttributeListElementOfEntityRef_Row)a.AttributeValueDict[ListDim1Position]).Value];} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.5 (ref),ListDim1Position={ListDim1Position}:{ex}", Log.Level.Exception);}
                                                                                     FieldCtorArgs[ListDim1Position]=Activator.CreateInstance(GenericType); 
                                                                                          if (GenericType.IsSubclassOf(typeof(SELECT))) ((SELECT)FieldCtorArgs[ListDim1Position]).Id=Id;
                                                                                     else if (GenericType.IsSubclassOf(typeof(ENTITY))) ((ENTITY)FieldCtorArgs[ListDim1Position]).LocalId=Id;
                                                                                     else Log.Add($"ERROR on EvalIfcRow.5 (ref):unkown type", Log.Level.Error);
                                                                                     } 


                                                                          if (ListDim1Count>0)   
                                                                          if (a.AttributeValueDict[0] is ifcSQL.ifcInstance.EntityAttributeListElementOfFloat_Row)
                                                                                 for (int ListDim1Position=0;ListDim1Position<ListDim1Count;ListDim1Position++) 
                                                                                    {double d=((ifcSQL.ifcInstance.EntityAttributeListElementOfFloat_Row)a.AttributeValueDict[ListDim1Position]).Value; 
                                                                                     FieldCtorArgs[ListDim1Position]=Activator.CreateInstance(GenericType,d); 
                                                                                     } 

                                                                          if (ListDim1Count>0)   
                                                                          if (a.AttributeValueDict[0] is ifcSQL.ifcInstance.EntityAttributeListElementOfInteger_Row)
                                                                                 for (int ListDim1Position=0;ListDim1Position<ListDim1Count;ListDim1Position++) 
                                                                                    {int i=((ifcSQL.ifcInstance.EntityAttributeListElementOfInteger_Row)a.AttributeValueDict[ListDim1Position]).Value; 
                                                                                     FieldCtorArgs[ListDim1Position]=Activator.CreateInstance(GenericType,i); 
                                                                                     } 

                                                                          if (ListDim1Count>0)
                                                                          if (a.AttributeValueDict[0] is ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row)
                                                                                 for (int ListDim1Position=0;ListDim1Position<ListDim1Count;ListDim1Position++) 
                                                                                    {try{AttributeInstanceType=ifc.ENTITY.TypeDictionary.TypeIdTypeDict[((ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row)a.AttributeValueDict[ListDim1Position]).TypeId];} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.5 (List),ListDim1Position={ListDim1Position}, a.TypeId={a.TypeId}:{ex}", Log.Level.Exception);}
                                                                                     int TypeId=((ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row)a.AttributeValueDict[ListDim1Position]).TypeId; 
                                                                                     FieldCtorArgs[ListDim1Position]=Activator.CreateInstance(AttributeInstanceType); 

                                                                                     ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row a2=(ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row)a.AttributeValueDict[ListDim1Position];

                                                                          Type GenericType2=null; 
                                                                          if (attrib.field.FieldType.BaseType.GetGenericArguments().Length>0) GenericType2=attrib.field.FieldType.BaseType.GetGenericArguments()[0]; //LengthMeasure or CartesianPoint
                                                                          else                                                                GenericType2=attrib.field.FieldType.BaseType.BaseType.GetGenericArguments()[0]; //CompoundPlaneAngleMeasure


                                                                                     int ListDim2Count=a2.AttributeValueDict.Count; 
                                                                                     object[] FieldCtorArgs2=new object[ListDim2Count];
                                                                                     if (ListDim2Count>0)
                                                                                     if (a2.AttributeValueDict[0] is ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfEntityRef_Row)
                                                                                       for (int ListDim2Position=0;ListDim2Position<ListDim2Count;ListDim2Position++) 
                                                                                           {
                                                                                            try{AttributeInstanceType2=ifc.ENTITY.TypeDictionary.TypeIdTypeDict[((ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfEntityRef_Row)a2.AttributeValueDict[ListDim2Position]).TypeId];} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.5a (List),ListDim2Position={ListDim2Position}, a2.TypeId={a2.TypeId}:{ex}", Log.Level.Exception);}
                                                                                            int Id=0;
                                                                                            try{Id=LocalIdFromGlobalIdDict[((ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfEntityRef_Row)a2.AttributeValueDict[ListDim2Position]).Value];} catch(Exception ex) {Log.Add($"ERROR on EvalIfcRow.8 (ref),ListDim2Position={ListDim2Position}:{ex}", Log.Level.Exception);}
                                                                                          FieldCtorArgs2[ListDim2Position]=Activator.CreateInstance(AttributeInstanceType2); 
                                                                                          ((ENTITY)FieldCtorArgs2[ListDim2Position]).LocalId=Id;
                                                                                     } 
                                                                                     FieldCtorArgs[ListDim1Position]=Activator.CreateInstance(GenericType,FieldCtorArgs2); 
                                                                                     } 
 

                                                                          attrib.field.SetValue(CurrentEntity,Activator.CreateInstance(attrib.field.FieldType,FieldCtorArgs));
                                                                         }


        }//----------------------------------------------------------------------------------------------------------
    }//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
EntityList.Add(CurrentEntity);
}catch(Exception){Log.Add($"ERROR on EvalIfcRow.6, e.GlobalEntityInstanceId={e.GlobalEntityInstanceId}, e.EntityTypeId={e.EntityTypeId} at IFcSchema {ifc.Specification.SchemaName}", Log.Level.Exception);}
}



public static Dictionary<long,ifcSQL.ifcInstance.Entity_Row> Entity_RowDict=new Dictionary<long, ifcSQL.ifcInstance.Entity_Row>();
public static ifcSQL._ifcSQL_for_ifcSQL_instance  ifcSQLin=null;

public static Model FromSql(string ServerName, string DatabaseName = "ifcSQL_Instance", int ProjectId = 0,
 ifcSQL.AttributeUpdater updater = null)
{
 ifcSQLin=new ifcSQL._ifcSQL_for_ifcSQL_instance (ServerName: ServerName,DatabaseName:DatabaseName);
 return FromSql(ifcSQLin, ProjectId, updater);
}
public static Model FromSql(string ServerName, string DatabaseName, string UserName, string Password, int ProjectId = 0,
 ifcSQL.AttributeUpdater updater = null)
{
 ifcSQLin=new ifcSQL._ifcSQL_for_ifcSQL_instance (ServerName,DatabaseName,UserName,Password);
 return FromSql(ifcSQLin, ProjectId, updater);
}
public static Model FromSql(ifcSQL._ifcSQL_for_ifcSQL_instance ifcSQLin,int ProjectId=0,ifcSQL.AttributeUpdater updater=null)
{
 
                                    ifcSQLin.conn.Open(); 
if (ProjectId>0)                    ifcSQLin.ExecuteNonQuery("app.SelectProject "+ProjectId);
                                    ifcSQLin.conn.Close(); 
                                    ifcSQLin.LoadAllTables();  

// fill Entity_RowDict with instancing an empty AttributeValueDict
foreach (ifcSQL.ifcInstance.Entity_Row e in ifcSQLin.cp.Entity) Entity_RowDict.Add(e.GlobalEntityInstanceId,e);
foreach (ifcSQL.ifcInstance.Entity_Row e in ifcSQLin.cp.Entity) e.AttributeValueDict=new Dictionary<int,RowBase>();


ifc.ENTITY.TypeDictionary.FillEntityTypeComponentsDict(); // fill Type Dict

// assign (nested) Attribute rows to entities
foreach (ifcSQL.ifcInstance.EntityAttributeOfString_Row    a in ifcSQLin.cp.EntityAttributeOfString   ) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfVector_Row    a in ifcSQLin.cp.EntityAttributeOfVector   ) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfBinary_Row    a in ifcSQLin.cp.EntityAttributeOfBinary   ) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfBoolean_Row   a in ifcSQLin.cp.EntityAttributeOfBoolean  ) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row a in ifcSQLin.cp.EntityAttributeOfEntityRef) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfEnum_Row      a in ifcSQLin.cp.EntityAttributeOfEnum     ) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfFloat_Row     a in ifcSQLin.cp.EntityAttributeOfFloat    ) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfInteger_Row   a in ifcSQLin.cp.EntityAttributeOfInteger  ) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);

foreach (ifcSQL.ifcInstance.EntityAttributeOfList_Row      a in ifcSQLin.cp.EntityAttributeOfList)      a.AttributeValueDict=new Dictionary<int,RowBase>();
foreach (ifcSQL.ifcInstance.EntityAttributeOfList_Row      a in ifcSQLin.cp.EntityAttributeOfList)      Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);

foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfEntityRef_Row a in ifcSQLin.cp.EntityAttributeListElementOfEntityRef) ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfBinary_Row    a in ifcSQLin.cp.EntityAttributeListElementOfBinary)    ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfFloat_Row     a in ifcSQLin.cp.EntityAttributeListElementOfFloat)     ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfInteger_Row   a in ifcSQLin.cp.EntityAttributeListElementOfInteger)   ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfString_Row    a in ifcSQLin.cp.EntityAttributeListElementOfString)    ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);

foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row      a in ifcSQLin.cp.EntityAttributeListElementOfList)      ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row      a in ifcSQLin.cp.EntityAttributeListElementOfList)      a.AttributeValueDict=new Dictionary<int,RowBase>();

foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfEntityRef_Row a in ifcSQLin.cp.EntityAttributeListElementOfListElementOfEntityRef) ((ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row) ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict[a.ListDim1Position]).AttributeValueDict.Add(a.ListDim2Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfFloat_Row     a in ifcSQLin.cp.EntityAttributeListElementOfListElementOfFloat    ) ((ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row) ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict[a.ListDim1Position]).AttributeValueDict.Add(a.ListDim2Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfInteger_Row   a in ifcSQLin.cp.EntityAttributeListElementOfListElementOfInteger  ) ((ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row) ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict[a.ListDim1Position]).AttributeValueDict.Add(a.ListDim2Position,a);

// assign current Project from the first project row
ifcSQL.ifcProject.Project_Row CurrentProJect=(ifcSQL.ifcProject.Project_Row)ifcSQLin.cp.Project[0];

// create a new model
Model NewModel=new Model();

// assign local id's
foreach (ifcSQL.ifcProject.EntityInstanceIdAssignment_Row eia in ifcSQLin.cp.EntityInstanceIdAssignment) NewModel.LocalIdFromGlobalIdDict[eia.GlobalEntityInstanceId]=(int)eia.ProjectEntityInstanceId; // create and fill LocalGlobal Dict

// fill the header
NewModel.Header.Init(Name:CurrentProJect.ProjectName, ViewDefinition:CurrentProJect.ProjectDescription,Author:"",PreprocessorVersion:"from ifcSQL via IfcSharp");

if (updater!=null){updater.Assign(ifcSQLin,NewModel);updater.Update();}

// cretae ifc-entities from linked database rows
foreach (ifcSQL.ifcInstance.Entity_Row e in ifcSQLin.cp.Entity) NewModel.EvalIfcRow(e);

// assign reverse-elements
NewModel.AssignEntities();

return NewModel;

}// of FromSql
}// of Model ==========================================================================================================


}//ifc=================================================================================================================


//#################################################################################################################################################################
//#################################################################################################################################################################


namespace ifcSQL{//########################################################################

public class AttributeUpdater{
public ifcSQL._ifcSQL_for_ifcSQL_instance ifcSQLin=null;
public ifc.Model Model=null;
public void Assign(ifcSQL._ifcSQL_for_ifcSQL_instance ifcSQLin,ifc.Model Model){this.ifcSQLin=ifcSQLin;this.Model=Model;}
public virtual void Update(){}
}


namespace ifcInstance{//=====================================================================
public partial class Entity_Row                           : RowBase{public Dictionary<int,RowBase> AttributeValueDict=null;}
public partial class EntityAttributeOfList_Row            : RowBase{public Dictionary<int,RowBase> AttributeValueDict=null;}
public partial class EntityAttributeListElementOfList_Row : RowBase{public Dictionary<int,RowBase> AttributeValueDict=null;}
}// namespace ifcInstance -------------------------------------------------------------------

}// namespace ifcSQL ########################################################################
