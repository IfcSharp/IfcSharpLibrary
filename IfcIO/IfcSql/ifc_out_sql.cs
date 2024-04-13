// ifc_out_sql.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using NetSystem=System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;//StringBuilder
using System.Data.SqlClient;

using db;

namespace ifc{//==============================

public partial class ENTITY{//=========================================================================================

private string value="";
private string typestr="";

public void SqlOut1(long GlobalId,int OrdinalPosition,int ListDim1Position, object o){
       if (o is SELECT)           {SqlOut1(GlobalId,OrdinalPosition,ListDim1Position,((SELECT)o).SelectValue());}
  else if (o is ENTITY)           {ifcSqlInstance.cp.EntityAttributeListElementOfEntityRef.Add(new ifcSQL.ifcInstance.EntityAttributeListElementOfEntityRef_Row(GlobalId,OrdinalPosition,ListDim1Position,ifcSqlType.SqlTypeId(o.GetType()),((ENTITY)o).ifcSqlGlobalId));}
  else if (o is ifcListInterface) {Console.WriteLine("LIST OF LIST CURRENTLY NOT  SUPPORTED.");}
  else if (o is TypeBase)         {TypeBase tb=(TypeBase)o;
                                   bool Display=true;
                                   if (tb.GetBaseType()==typeof(String)) if (o.ToString()=="") Display=false;
                                   if (o.ToString()=="null") Display=false;
                                   if (Display) 
                                      {switch (tb.SqlTableId())
                                              {case (int)ifc.SqlTable.EntityAttributeOfFloat  : ifcSqlInstance.cp.EntityAttributeListElementOfFloat  .Add(new ifcSQL.ifcInstance.EntityAttributeListElementOfFloat_Row  (GlobalId,OrdinalPosition,ListDim1Position,ifcSqlType.SqlTypeId(o.GetType()),(double)((TYPE<double>)o).TypeValue)); break;
                                               case (int)ifc.SqlTable.EntityAttributeOfInteger: ifcSqlInstance.cp.EntityAttributeListElementOfInteger.Add(new ifcSQL.ifcInstance.EntityAttributeListElementOfInteger_Row(GlobalId,OrdinalPosition,ListDim1Position,ifcSqlType.SqlTypeId(o.GetType()),(int)((TYPE<int>)o).TypeValue )); break;
                                               case (int)ifc.SqlTable.EntityAttributeOfString : ifcSqlInstance.cp.EntityAttributeListElementOfString .Add(new ifcSQL.ifcInstance.EntityAttributeListElementOfString_Row (GlobalId,OrdinalPosition,ListDim1Position,ifcSqlType.SqlTypeId(o.GetType()),((TYPE<string>)o).TypeValue)); break; // 2024-03-30 (bb) removes o.ToString()
                                               case (int)ifc.SqlTable.EntityAttributeOfBinary : ifcSqlInstance.cp.EntityAttributeListElementOfBinary .Add(new ifcSQL.ifcInstance.EntityAttributeListElementOfBinary_Row (GlobalId,OrdinalPosition,ListDim1Position,ifcSqlType.SqlTypeId(o.GetType()),o.ToString())); break;
                                              }
                                      } 
                                  }
}

public void SqlOut0(long GlobalId,int OrdinalPosition, object o){
  
         if (o==null)               {}
    else if (o is Enum)             {ifcSqlInstance.cp.EntityAttributeOfEnum.Add(new ifcSQL.ifcInstance.EntityAttributeOfEnum_Row(GlobalId,OrdinalPosition,ifcSqlType.SqlTypeId(o.GetType()),((int)o))); } 
    else if (o is SELECT)           {SqlOut0(GlobalId,OrdinalPosition,(ifcSqlType)((SELECT)o).SelectValue());}
    else if (o is ENTITY)           {if (((ENTITY)o).ifcSqlGlobalId>0) ifcSqlInstance.cp.EntityAttributeOfEntityRef.Add(new ifcSQL.ifcInstance.EntityAttributeOfEntityRef_Row(GlobalId,OrdinalPosition,((ifcSqlType)o).SqlTypeId(),((ENTITY)o).ifcSqlGlobalId)); } 
    else if (o is ifcListInterface) {if (((ifcSqlTypeInterface)o).SqlTypeGroupId()==(int)ifc.TypeGroup.LISTTYPE1D) 
                                        {ifcSqlInstance.cp.EntityAttributeOfList.Add(new ifcSQL.ifcInstance.EntityAttributeOfList_Row(GlobalId,OrdinalPosition,ifcSqlType.SqlTypeId(o.GetType()) ));
                                         int ListDim1Position=0;foreach (object item in (IEnumerable)o) SqlOut1(GlobalId,OrdinalPosition,ListDim1Position++,item);
                                        } 
                                     else throw new  ifc.IfcSharpException("SqlOut0: LISTTYPE2D CURRENTLY NOT  SUPPORTED.");
                                    }

    else if (o is TypeBase) {TypeBase tb=(TypeBase)o;// Console.WriteLine(o.ToString());// funktioniert noch nicht 8PropertySingleValue)
                             bool Display=true;
                             if (tb.GetBaseType()==typeof(String)) if (o.ToString()=="") Display=false;
                             if (o.ToString()=="null") Display=false;
                             if (tb.IsNull) Display=false; // 2024-04-02 (bb): added
                             if (Display) 
                                {switch (tb.SqlTableId())
                                        {case (int)ifc.SqlTable.EntityAttributeOfBinary : ifcSqlInstance.cp.EntityAttributeOfBinary .Add(new ifcSQL.ifcInstance.EntityAttributeOfBinary_Row (GlobalId,OrdinalPosition,tb.SqlTypeId(),o.ToString())); break;
                                         case (int)ifc.SqlTable.EntityAttributeOfBoolean: ifcSqlInstance.cp.EntityAttributeOfBoolean.Add(new ifcSQL.ifcInstance.EntityAttributeOfBoolean_Row(GlobalId,OrdinalPosition,tb.SqlTypeId(),(bool)((TYPE<bool>)o).TypeValue)); break;
                                         case (int)ifc.SqlTable.EntityAttributeOfFloat  : ifcSqlInstance.cp.EntityAttributeOfFloat  .Add(new ifcSQL.ifcInstance.EntityAttributeOfFloat_Row  (GlobalId,OrdinalPosition,tb.SqlTypeId(),(double)((TYPE<double>)o).TypeValue)); break;
                                         case (int)ifc.SqlTable.EntityAttributeOfInteger: ifcSqlInstance.cp.EntityAttributeOfInteger.Add(new ifcSQL.ifcInstance.EntityAttributeOfInteger_Row(GlobalId,OrdinalPosition,tb.SqlTypeId(),(int)((TYPE<int>)o).TypeValue )); break;
                                         case (int)ifc.SqlTable.EntityAttributeOfString : ifcSqlInstance.cp.EntityAttributeOfString .Add(new ifcSQL.ifcInstance.EntityAttributeOfString_Row (GlobalId,OrdinalPosition,tb.SqlTypeId(),(string)((TYPE<string>)o).TypeValue)) /*  o.ToString() )) */; break;
                                        }
                                } 
                            }
     else                            {Console.WriteLine("_OTHER");typestr=((ifcSqlType)o).SqlTypeId().ToString();value=o.ToString();Console.WriteLine("_OTHER="+typestr+" -> "+value);}    

}



public virtual void ToSql(int ProjectId)//.........................................................
{
 new ifcSQL.ifcInstance.Entity_Row(GlobalEntityInstanceId:this.ifcSqlGlobalId,EntityTypeId:this.SqlTypeId());
 ifcSqlInstance.cp.Entity.Add(new ifcSQL.ifcInstance.Entity_Row(GlobalEntityInstanceId:this.ifcSqlGlobalId,EntityTypeId:this.SqlTypeId()));
 ifcSqlInstance.cp.EntityInstanceIdAssignment.Add(new ifcSQL.ifcProject.EntityInstanceIdAssignment_Row(ProjectId:ProjectId,ProjectEntityInstanceId:this.LocalId,GlobalEntityInstanceId:this.ifcSqlGlobalId));

 
      if (this is EntityComment)        ifcSqlInstance.cp.EntityAttributeOfString.Add(new ifcSQL.ifcInstance.EntityAttributeOfString_Row(GlobalEntityInstanceId:this.ifcSqlGlobalId,OrdinalPosition:-1,TypeId:-2,Value:((EntityComment)this).CommentLine));
else  if (this.EndOfLineComment!=null)  ifcSqlInstance.cp.EntityAttributeOfString.Add(new ifcSQL.ifcInstance.EntityAttributeOfString_Row(GlobalEntityInstanceId:this.ifcSqlGlobalId,OrdinalPosition:-1,TypeId:-2,Value:this.EndOfLineComment));


     if (this is CartesianPoint) {double X=0;double Y=0;double? Z=null; 
                                  if (((CartesianPoint)this).Coordinates.Count>1) {X=(double)((CartesianPoint)this).Coordinates[0];Y=(double)((CartesianPoint)this).Coordinates[1];}
                                  if (((CartesianPoint)this).Coordinates.Count>2) {Z=(double)((CartesianPoint)this).Coordinates[2];}
                                  ifcSqlInstance.cp.EntityAttributeOfVector.Add(new ifcSQL.ifcInstance.EntityAttributeOfVector_Row(this.ifcSqlGlobalId,1,25,X,Y,Z));  
                                 }
else if (this is Direction)      {double X=0;double Y=0;double? Z=null; 
                                  if (((Direction)this).DirectionRatios.Count>1) {X=(double)((Direction)this).DirectionRatios[0];Y=(double)((Direction)this).DirectionRatios[1];}
                                  if (((Direction)this).DirectionRatios.Count>2) {Z=(double)((Direction)this).DirectionRatios[2];}
                                  ifcSqlInstance.cp.EntityAttributeOfVector.Add(new ifcSQL.ifcInstance.EntityAttributeOfVector_Row(this.ifcSqlGlobalId,1,42,X,Y,Z));  
                                 }
else                             {AttribListType AttribList=TypeDictionary.GetComponents(this.GetType()).AttribList;
                                  foreach (AttribInfo attrib in AttribList) SqlOut0(this.ifcSqlGlobalId,attrib.OrdinalPosition,attrib.field.GetValue(this));
                                }
}//....................................................................................................................

public static ifcSQL._ifcSQL_for_ifcSQL_instance ifcSqlInstance=null;

}// of ENTITY =========================================================================================================





public partial class Model{//==========================================================================================




public void ToSqlFile(int ProjectId=0,long StartGlobalId=1,string DatabaseName="ifcSQL")//.........
{
//if (ENTITY.ifcSqlInstance==null) 
ENTITY.ifcSqlInstance=new ifcSQL._ifcSQL_for_ifcSQL_instance();
FillTables(ProjectId:ProjectId,StartGlobalId:StartGlobalId);

StreamWriter sw = new StreamWriter(Header.Name+".sql");
sw.WriteLine("use "+DatabaseName+"\r\ngo\r\n");foreach(TableBase tb in OrderedInsertList) if (tb.Count>0) sw.WriteLine(tb.InsertString()+"go\r\n");
sw.Close();

}//................................................................................................


public enum eWriteMode{CreateNewProject, OnlyIfEmpty,DeleteBeforeWrite }

//.................................................................................................
 public void ToSql(string ServerName, string DatabaseName = "ifcSQL", eWriteMode WriteMode = eWriteMode.CreateNewProject, int ProjectId = 0, int ProjectGroupId = 0)
 {
  ENTITY.ifcSqlInstance=new ifcSQL._ifcSQL_for_ifcSQL_instance(ServerName: ServerName,DatabaseName:DatabaseName);
  ifcSQL._ifcSQL_for_ifcSQL_instance ifcSQL=ENTITY.ifcSqlInstance;
  ToSql(ifcSQL, WriteMode, ProjectId, ProjectGroupId);
 }
 public void ToSql(string ServerName, string DatabaseName, string UserName, string Password, eWriteMode WriteMode = eWriteMode.CreateNewProject, int ProjectId = 0, int ProjectGroupId = 0)
 {
  ENTITY.ifcSqlInstance=new ifcSQL._ifcSQL_for_ifcSQL_instance(ServerName, DatabaseName, UserName, Password);
  ifcSQL._ifcSQL_for_ifcSQL_instance ifcSQL=ENTITY.ifcSqlInstance;
  ToSql(ifcSQL, WriteMode, ProjectId, ProjectGroupId);
 }

 public void ToSql(ifcSQL._ifcSQL_for_ifcSQL_instance ifcSQL, eWriteMode WriteMode = eWriteMode.CreateNewProject, int ProjectId=0,int ProjectGroupId=0)
{
long LastGlobalId=0;
ifcSQL.conn.Open(); 
if (WriteMode==eWriteMode.CreateNewProject) ProjectId=ifcSQL.ExecuteIntegerScalar("declare @r as int;exec @r=[ifcSQL].[app].[NewProjectId] @ProjectName='"+Header.Name+"',@ProjectDescription='"+Header.ViewDefinition+"',@ProjectGroupId="+ProjectGroupId+",@SpecificationId="+Specification.SpecificationId+",@Author='"+Header.Author+"',@Organization='"+Header.Organization+"',@OriginatingSystem='"+Header.OriginatingSystem+"',@Documentation='"+Header.Documentation+"';select @r");                                            
if (ProjectId==0) ProjectId=ifcSQL.ExecuteIntegerScalar("SELECT cp.ProjectId()"); else ifcSQL.ExecuteNonQuery("app.SelectProject "+ProjectId);
            int EntityCount=ifcSQL.ExecuteIntegerScalar("SELECT count(*) from cp.Entity"); if (WriteMode==eWriteMode.OnlyIfEmpty) if (EntityCount>0) {ifcSQL.conn.Close();throw new NetSystem.Exception("Project with ProjectId="+ProjectId+" is not empty while using eWriteMode.OnlyIfEmpty");}
if (WriteMode==eWriteMode.DeleteBeforeWrite) ifcSQL.ExecuteNonQuery("app.DeleteProjectEntities "+ProjectId);
                            ifcSQL.ExecuteNonQuery("ifcProject.NewLastGlobalId "+ProjectId+", "+this.EntityList.Count); //BB-2024-04-01: from ifc.Repository.CurrentModel to this Model
               LastGlobalId=ifcSQL.ExecuteLongScalar("SELECT ifcProject.LastGlobalId("+ProjectId+")");
ifcSQL.conn.Close();

long CurrentGlobalId=LastGlobalId-this.EntityList.Count; //BB-2024-04-01: from ifc.Repository.CurrentModel to this Model

FillTables(ProjectId:ProjectId,StartGlobalId: LastGlobalId-this.EntityList.Count+1); //BB-2024-04-01: from ifc.Repository.CurrentModel to this Model

ifcSQL.conn.Open();
foreach(TableBase tb in OrderedInsertList) if (tb.Count>0) {Console.WriteLine(tb.TableName); tb.BulkInsert();}
ifcSQL.conn.Close();                                       

}//................................................................................................


List<TableBase> OrderedInsertList=new List<TableBase>();

private void FillTables(int ProjectId, long StartGlobalId){//......................................
long GlobalId=StartGlobalId;
ifcSQL._ifcSQL_for_ifcSQL_instance ifcSQL=ENTITY.ifcSqlInstance;
AssignEntities(); 
foreach (ENTITY e in EntityList) e.ifcSqlGlobalId=GlobalId++;
foreach (ENTITY e in EntityList) e.ToSql(ProjectId);

OrderedInsertList.Add(ifcSQL.cp.Entity);
OrderedInsertList.Add(ifcSQL.cp.EntityInstanceIdAssignment);
OrderedInsertList.Add(ifcSQL.cp.EntityVariableName);

OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfBinary);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfBoolean);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfEntityRef);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfEnum);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfFloat);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfInteger);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfString);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfVector);

OrderedInsertList.Add(ifcSQL.cp.EntityAttributeOfList);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfBinary);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfEntityRef);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfFloat);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfInteger);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfString);

OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfList);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfListElementOfEntityRef);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfListElementOfFloat);
OrderedInsertList.Add(ifcSQL.cp.EntityAttributeListElementOfListElementOfInteger);

}//..........................................................................................


}// of Model ==========================================================================================================



}//ifc ================================================================================================================
