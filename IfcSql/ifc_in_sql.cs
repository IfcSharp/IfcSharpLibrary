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
Type t=Type.GetType("ifc."+ifc.ENTITY.TypeDictionary.TypeIdNameDict[e.EntityTypeId],true,true);// 2. true: ignoreCase
ENTITY CurrentEntity=(ENTITY)Activator.CreateInstance(t);
CurrentEntity.LocalId=LocalIdFromGlobalIdDict[e.GlobalEntityInstanceId];// e.LocalId;
CurrentEntity.ifcSqlGlobalId=e.GlobalEntityInstanceId;

// commment-handling:
if (CurrentEntity is EntityComment) ((EntityComment)CurrentEntity).CommentLine=((ifcSQL.ifcInstance.EntityAttributeOfString_Row)e.AttributeValueDict[-1]).Value;
else if (e.AttributeValueDict.ContainsKey(-1))  CurrentEntity.EndOfLineComment=((ifcSQL.ifcInstance.EntityAttributeOfString_Row)e.AttributeValueDict[-1]).Value;

object[] TypeCtorArgs=new object[1];
int OrdinalPosition=0;
ENTITY.AttribListType AttribList=ENTITY.TypeDictionary.GetComponents(CurrentEntity.GetType()).AttribList;
foreach (ENTITY.AttribInfo attrib in AttribList)
        {++OrdinalPosition;  
         if (e.AttributeValueDict.ContainsKey(OrdinalPosition))//----------------------------------------------------------------------------------------------------------
            {RowBase rb=e.AttributeValueDict[OrdinalPosition];
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
                                                                          TypeCtorArgs[0]=ifc.IfcString.Decode(a.Value);
                                                                          attrib.field.SetValue(CurrentEntity,Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs));
                                                                         }
         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfEnum_Row)  {ifcSQL.ifcInstance.EntityAttributeOfEnum_Row a=(ifcSQL.ifcInstance.EntityAttributeOfEnum_Row)rb;
                                                                        Type UnderlyingType = Nullable.GetUnderlyingType(   attrib.field.FieldType);
                                                                        if  (UnderlyingType!=null && UnderlyingType.IsEnum) attrib.field.SetValue(CurrentEntity,Enum.ToObject(UnderlyingType, a.Value));
                                                                        else                                                attrib.field.SetValue(CurrentEntity,a.Value); 
                                                                       }
         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfInteger_Row)   {ifcSQL.ifcInstance.EntityAttributeOfInteger_Row a=(ifcSQL.ifcInstance.EntityAttributeOfInteger_Row)rb;
                                                                             object o=Activator.CreateInstance(ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId],a.Value);
                                                                             if (attrib.field.FieldType.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=o;o=Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs);}
                                                                             attrib.field.SetValue(CurrentEntity,o);
                                                                           }
         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfFloat_Row)      {ifcSQL.ifcInstance.EntityAttributeOfFloat_Row a=(ifcSQL.ifcInstance.EntityAttributeOfFloat_Row)rb;
                                                                             object o=Activator.CreateInstance(ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId],a.Value);
                                                                             if (attrib.field.FieldType.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=o;o=Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs);}
                                                                             attrib.field.SetValue(CurrentEntity,o);
                                                                           }

         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row)  {ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row a=(ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row)rb;  
                                                                             if (a.Value>0) 
                                                                                {Type AttributeInstanceType=ifc.ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId]; 
                                                                                 object o=Activator.CreateInstance(AttributeInstanceType);((ENTITY)o).LocalId=LocalIdFromGlobalIdDict[a.Value];
                                                                                 if (attrib.field.FieldType.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=o;o=Activator.CreateInstance(attrib.field.FieldType,TypeCtorArgs);}
                                                                                 attrib.field.SetValue(CurrentEntity,o);
                                                                                } 
                                                                            }

         else if (rb is ifcSQL.ifcInstance.EntityAttributeOfList_Row)    {ifcSQL.ifcInstance.EntityAttributeOfList_Row a=(ifcSQL.ifcInstance.EntityAttributeOfList_Row)rb;  
                                                                          Type GenericType=null;
                                                                          if (attrib.field.FieldType.BaseType.GetGenericArguments().Length>0) GenericType=attrib.field.FieldType.BaseType.GetGenericArguments()[0]; //LengthMeasure or CartesianPoint
                                                                          else                                                   GenericType=attrib.field.FieldType.BaseType.BaseType.GetGenericArguments()[0]; //CompoundPlaneAngleMeasure
                                                                          Type AttributeInstanceType=ifc.ENTITY.TypeDictionary.TypeIdTypeDict[a.TypeId];
                                                                          int ListDim1Count=a.AttributeValueDict.Count; 
                                                                          object[] FieldCtorArgs=new object[ListDim1Count];
                                                                          if (ListDim1Count>0)   
                                                                          if (a.AttributeValueDict[0] is ifcSQL.ifcInstance.EntityAttributeListElementOfEntityRef_Row)
                                                                                 for (int ListDim1Position=0;ListDim1Position<ListDim1Count;ListDim1Position++) 
                                                                                    {int Id=LocalIdFromGlobalIdDict[((ifcSQL.ifcInstance.EntityAttributeListElementOfEntityRef_Row)a.AttributeValueDict[ListDim1Position]).Value]; 
                                                                                     object[] GenericCtorArgs=new object[1]; 
                                                                                     FieldCtorArgs[ListDim1Position]=Activator.CreateInstance(GenericType);  // Console.WriteLine("GenericType= "+GenericType.ToString());
                                                                                          if (GenericType.IsSubclassOf(typeof(SELECT))) ((SELECT)FieldCtorArgs[ListDim1Position]).Id=Id;
                                                                                     else if (GenericType.IsSubclassOf(typeof(ENTITY))) ((ENTITY)FieldCtorArgs[ListDim1Position]).LocalId=Id;
                                                                                     else Console.WriteLine("unkown type"); 
                                                                                     } 
                                                                          attrib.field.SetValue(CurrentEntity,Activator.CreateInstance(attrib.field.FieldType,FieldCtorArgs));
                                                                         }
        }//----------------------------------------------------------------------------------------------------------
    }//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
EntityList.Add(CurrentEntity);
}catch(Exception ex){Console.WriteLine ("ERROR on EvalIfcRow:"+ex.Message);}//Console.ReadLine();}
}




public static Model FromSql(string ServerName,string DatabaseName="ifcSQL_Instance",int ProjectId=0)
{

ifcSQL._ifcSQL_for_ifcSQL_instance  ifcSQLin=new ifcSQL._ifcSQL_for_ifcSQL_instance (ServerName: ServerName,DatabaseName:DatabaseName);
                                    ifcSQLin.LoadAllTables();  

Dictionary<long,ifcSQL.ifcInstance.Entity_Row> Entity_RowDict=new Dictionary<long, ifcSQL.ifcInstance.Entity_Row>();
foreach (ifcSQL.ifcInstance.Entity_Row e in ifcSQLin.cp.Entity) 
        {e.AttributeValueDict=new Dictionary<int,RowBase>();
         Entity_RowDict.Add(e.GlobalEntityInstanceId,e);
        }


ifc.ENTITY.TypeDictionary.FillEntityTypeComponentsDict(); // fill Type Dict


foreach (ifcSQL.ifcInstance.EntityAttributeOfString_Row a in ifcSQLin.cp.EntityAttributeOfString) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfVector_Row a in ifcSQLin.cp.EntityAttributeOfVector) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfBinary_Row a in ifcSQLin.cp.EntityAttributeOfBinary) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfBoolean_Row a in ifcSQLin.cp.EntityAttributeOfBoolean) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row a in ifcSQLin.cp.EntityAttributeOfEntityRef) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfEnum_Row a in ifcSQLin.cp.EntityAttributeOfEnum) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfFloat_Row a in ifcSQLin.cp.EntityAttributeOfFloat) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
foreach (ifcSQL.ifcInstance.EntityAttributeOfInteger_Row a in ifcSQLin.cp.EntityAttributeOfInteger) Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);

foreach (ifcSQL.ifcInstance.EntityAttributeOfList_Row a in ifcSQLin.cp.EntityAttributeOfList) 
        {Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict.Add(a.OrdinalPosition,a);
         a.AttributeValueDict=new Dictionary<int,RowBase>();
        }
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfEntityRef_Row a in ifcSQLin.cp.EntityAttributeListElementOfEntityRef) ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfBinary_Row    a in ifcSQLin.cp.EntityAttributeListElementOfBinary)    ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfFloat_Row     a in ifcSQLin.cp.EntityAttributeListElementOfFloat)     ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfInteger_Row   a in ifcSQLin.cp.EntityAttributeListElementOfInteger)   ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfString_Row    a in ifcSQLin.cp.EntityAttributeListElementOfString)    ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict.Add(a.ListDim1Position,a);


foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row a in ifcSQLin.cp.EntityAttributeListElementOfList) 
        {ifcSQL.ifcInstance.EntityAttributeOfList_Row lr=((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]);
         lr.AttributeValueDict.Add(a.ListDim1Position,a);
         a.AttributeValueDict=new Dictionary<int,RowBase>();
        }

foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfEntityRef_Row a in ifcSQLin.cp.EntityAttributeListElementOfListElementOfEntityRef) ((ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row) ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict[a.ListDim1Position]).AttributeValueDict.Add(a.ListDim2Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfFloat_Row     a in ifcSQLin.cp.EntityAttributeListElementOfListElementOfFloat    ) ((ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row) ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict[a.ListDim1Position]).AttributeValueDict.Add(a.ListDim2Position,a);
foreach (ifcSQL.ifcInstance.EntityAttributeListElementOfListElementOfInteger_Row   a in ifcSQLin.cp.EntityAttributeListElementOfListElementOfInteger  ) ((ifcSQL.ifcInstance.EntityAttributeListElementOfList_Row) ((ifcSQL.ifcInstance.EntityAttributeOfList_Row)Entity_RowDict[a.GlobalEntityInstanceId].AttributeValueDict[a.OrdinalPosition]).AttributeValueDict[a.ListDim1Position]).AttributeValueDict.Add(a.ListDim2Position,a);

ifcSQL.ifcProject.Project_Row CurrentProJect=(ifcSQL.ifcProject.Project_Row)ifcSQLin.cp.Project[0];

Model NewModel=new Model();
foreach (ifcSQL.ifcProject.EntityInstanceIdAssignment_Row eia in ifcSQLin.cp.EntityInstanceIdAssignment) NewModel.LocalIdFromGlobalIdDict[eia.GlobalEntityInstanceId]=(int)eia.ProjectEntityInstanceId; // create and fill LocalGlobal Dict
NewModel.Header.Init(name:CurrentProJect.ProjectName,description:CurrentProJect.ProjectDescription,author:"Bernhard Simon Bock, Friedrich Eder",preprocessor_version:"IfcSharp");
foreach (ifcSQL.ifcInstance.Entity_Row e in ifcSQLin.cp.Entity) NewModel.EvalIfcRow(e);
NewModel.AssignEntities();
return NewModel;

}// of FromSql
}// of Model ==========================================================================================================


}//ifc=================================================================================================================


//#################################################################################################################################################################
//#################################################################################################################################################################


namespace ifcSQL{//########################################################################

namespace ifcInstance{//=====================================================================
public partial class Entity_Row                           : RowBase{public Dictionary<int,RowBase> AttributeValueDict=null;}
public partial class EntityAttributeOfList_Row            : RowBase{public Dictionary<int,RowBase> AttributeValueDict=null;}
public partial class EntityAttributeListElementOfList_Row : RowBase{public Dictionary<int,RowBase> AttributeValueDict=null;}
}// namespace ifcInstance -------------------------------------------------------------------

}// namespace ifcSQL ########################################################################
