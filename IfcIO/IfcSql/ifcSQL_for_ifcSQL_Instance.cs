// ifcSQL_for_ifcSQL_Instance_db_generated.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)
 
using db;
 
namespace ifcSQL{//########################################################################
// namespace overview and template for filenames:
namespace ifcInstance{}
namespace ifcProject{}
 
// Overview and Template for class-extending:
namespace ifcInstance{//=====================================================================
public partial class Entity_Row : RowBase{}
public partial class EntityAttributeListElementOfBinary_Row : RowBase{}
public partial class EntityAttributeListElementOfEntityRef_Row : RowBase{}
public partial class EntityAttributeListElementOfFloat_Row : RowBase{}
public partial class EntityAttributeListElementOfInteger_Row : RowBase{}
public partial class EntityAttributeListElementOfList_Row : RowBase{}
public partial class EntityAttributeListElementOfListElementOfEntityRef_Row : RowBase{}
public partial class EntityAttributeListElementOfListElementOfFloat_Row : RowBase{}
public partial class EntityAttributeListElementOfListElementOfInteger_Row : RowBase{}
public partial class EntityAttributeListElementOfString_Row : RowBase{}
public partial class EntityAttributeOfBinary_Row : RowBase{}
public partial class EntityAttributeOfBoolean_Row : RowBase{}
public partial class EntityAttributeOfEntityRef_Row : RowBase{}
public partial class EntityAttributeOfEnum_Row : RowBase{}
public partial class EntityAttributeOfFloat_Row : RowBase{}
public partial class EntityAttributeOfInteger_Row : RowBase{}
public partial class EntityAttributeOfList_Row : RowBase{}
public partial class EntityAttributeOfString_Row : RowBase{}
public partial class EntityAttributeOfVector_Row : RowBase{}
public partial class EntityVariableName_Row : RowBase{}
}// namespace ifcInstance -------------------------------------------------------------------
 
namespace ifcProject{//=====================================================================
public partial class EntityInstanceIdAssignment_Row : RowBase{}
public partial class LastGlobalEntityInstanceId_Row : RowBase{}
public partial class Project_Row : RowBase{}
public partial class ProjectGroup_Row : RowBase{}
public partial class ProjectGroupType_Row : RowBase{}
}// namespace ifcProject -------------------------------------------------------------------
 
//#############################################################################################
//#############################################################################################
 
namespace ifcInstance{//=====================================================================
 
public partial class Entity_Row : RowBase{
 public Entity_Row(long GlobalEntityInstanceId, int EntityTypeId){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.EntityTypeId=EntityTypeId;}
 public Entity_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] public long GlobalEntityInstanceId=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int EntityTypeId=0;
}
 
public partial class EntityAttributeListElementOfBinary_Row : RowBase{
 public EntityAttributeListElementOfBinary_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int TypeId, string Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeListElementOfBinary_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcBINARY")] public string Value="";
}
 
public partial class EntityAttributeListElementOfEntityRef_Row : RowBase{
 public EntityAttributeListElementOfEntityRef_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int TypeId, long Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeListElementOfEntityRef_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int TypeId=0;
 [DbField] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long Value=0;
}
 
public partial class EntityAttributeListElementOfFloat_Row : RowBase{
 public EntityAttributeListElementOfFloat_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int EntityTypeId, double Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.EntityTypeId=EntityTypeId;this.Value=Value;}
 public EntityAttributeListElementOfFloat_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int EntityTypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcREAL")] public double Value=0;
}
 
public partial class EntityAttributeListElementOfInteger_Row : RowBase{
 public EntityAttributeListElementOfInteger_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int TypeId, int Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeListElementOfInteger_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcINTEGER")] public int Value=0;
}
 
public partial class EntityAttributeListElementOfList_Row : RowBase{
 public EntityAttributeListElementOfList_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int TypeId){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.TypeId=TypeId;}
 public EntityAttributeListElementOfList_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int TypeId=0;
}
 
public partial class EntityAttributeListElementOfListElementOfEntityRef_Row : RowBase{
 public EntityAttributeListElementOfListElementOfEntityRef_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int ListDim2Position, int TypeId, long Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.ListDim2Position=ListDim2Position;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeListElementOfListElementOfEntityRef_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim2Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int TypeId=0;
 [DbField] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long Value=0;
}
 
public partial class EntityAttributeListElementOfListElementOfFloat_Row : RowBase{
 public EntityAttributeListElementOfListElementOfFloat_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int ListDim2Position, int TypeId, double Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.ListDim2Position=ListDim2Position;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeListElementOfListElementOfFloat_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim2Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcREAL")] public double Value=0;
}
 
public partial class EntityAttributeListElementOfListElementOfInteger_Row : RowBase{
 public EntityAttributeListElementOfListElementOfInteger_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int ListDim2Position, int TypeId, int Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.ListDim2Position=ListDim2Position;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeListElementOfListElementOfInteger_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim2Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcINTEGER")] public int Value=0;
}
 
public partial class EntityAttributeListElementOfString_Row : RowBase{
 public EntityAttributeListElementOfString_Row(long GlobalEntityInstanceId, int OrdinalPosition, int ListDim1Position, int TypeId, string Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.ListDim1Position=ListDim1Position;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeListElementOfString_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int ListDim1Position=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] [References(RefTableSchema="ifcSchema",RefTableName="Type",RefTableColName="TypeId")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcSTRING")] public string Value="";
}
 
public partial class EntityAttributeOfBinary_Row : RowBase{
 public EntityAttributeOfBinary_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId, string Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeOfBinary_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcBINARY")] public string Value="";
}
 
public partial class EntityAttributeOfBoolean_Row : RowBase{
 public EntityAttributeOfBoolean_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId, bool Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeOfBoolean_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcBOOLEAN")] public bool Value=false;
}
 
public partial class EntityAttributeOfEntityRef_Row : RowBase{
 public EntityAttributeOfEntityRef_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId, long Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeOfEntityRef_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
 [DbField] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long Value=0;
}
 
public partial class EntityAttributeOfEnum_Row : RowBase{
 public EntityAttributeOfEnum_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId, int Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeOfEnum_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
 [DbField] [UserType(schema="ifcEnum",name="Id")] public int Value=0;
}
 
public partial class EntityAttributeOfFloat_Row : RowBase{
 public EntityAttributeOfFloat_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId, double Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeOfFloat_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcREAL")] public double Value=0;
}
 
public partial class EntityAttributeOfInteger_Row : RowBase{
 public EntityAttributeOfInteger_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId, int Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeOfInteger_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcINTEGER")] public int Value=0;
}
 
public partial class EntityAttributeOfList_Row : RowBase{
 public EntityAttributeOfList_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;}
 public EntityAttributeOfList_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
}
 
public partial class EntityAttributeOfString_Row : RowBase{
 public EntityAttributeOfString_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId, string Value){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;this.Value=Value;}
 public EntityAttributeOfString_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcSTRING")] public string Value="";
}
 
public partial class EntityAttributeOfVector_Row : RowBase{
 public EntityAttributeOfVector_Row(long GlobalEntityInstanceId, int OrdinalPosition, int TypeId, double X, double Y, double? Z){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.OrdinalPosition=OrdinalPosition;this.TypeId=TypeId;this.X=X;this.Y=Y;this.Z=Z;}
 public EntityAttributeOfVector_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcOrder",name="Position")] public int OrdinalPosition=0;
 [DbField] [UserType(schema="ifcSchema",name="Id")] public int TypeId=0;
 [DbField] [UserType(schema="ifcType",name="ifcREAL")] public double X=0;
 [DbField] [UserType(schema="ifcType",name="ifcREAL")] public double Y=0;
 [DbField] [UserType(schema="ifcType",name="ifcREAL")] public double? Z=null;
}
 
public partial class EntityVariableName_Row : RowBase{
 public EntityVariableName_Row(long GlobalEntityInstanceId, string VarableName){this.GlobalEntityInstanceId=GlobalEntityInstanceId;this.VarableName=VarableName;}
 public EntityVariableName_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
 [DbField] [UserType(schema="ifcType",name="ifcSTRING")] public string VarableName="";
}
 
}// namespace ifcInstance -------------------------------------------------------------------
 
 
namespace ifcProject{//=====================================================================
 
public partial class EntityInstanceIdAssignment_Row : RowBase{
 public EntityInstanceIdAssignment_Row(int ProjectId, long ProjectEntityInstanceId, long GlobalEntityInstanceId){this.ProjectId=ProjectId;this.ProjectEntityInstanceId=ProjectEntityInstanceId;this.GlobalEntityInstanceId=GlobalEntityInstanceId;}
 public EntityInstanceIdAssignment_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcProject",name="Id")] [References(RefTableSchema="ifcProject",RefTableName="Project",RefTableColName="ProjectId")] public int ProjectId=0;
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcInstance",name="Id")] public long ProjectEntityInstanceId=0;
 [DbField] [UserType(schema="ifcInstance",name="Id")] [References(RefTableSchema="ifcInstance",RefTableName="Entity",RefTableColName="GlobalEntityInstanceId")] public long GlobalEntityInstanceId=0;
}
 
public partial class LastGlobalEntityInstanceId_Row : RowBase{
 public LastGlobalEntityInstanceId_Row(int ProjectId, long GlobalEntityInstanceId){this.ProjectId=ProjectId;this.GlobalEntityInstanceId=GlobalEntityInstanceId;}
 public LastGlobalEntityInstanceId_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcProject",name="Id")] public int ProjectId=0;
 [DbField] [UserType(schema="ifcInstance",name="Id")] public long GlobalEntityInstanceId=0;
}
 
public partial class Project_Row : RowBase{
 public Project_Row(int ProjectId, string ProjectName, string ProjectDescription, int ProjectGroupId, int SpecificationId){this.ProjectId=ProjectId;this.ProjectName=ProjectName;this.ProjectDescription=ProjectDescription;this.ProjectGroupId=ProjectGroupId;this.SpecificationId=SpecificationId;}
 public Project_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcProject",name="Id")] public int ProjectId=0;
 [DbField] [UserType(schema="Text",name="ToString")] public string ProjectName="";
 [DbField] [UserType(schema="Text",name="Description")] public string ProjectDescription="";
 [DbField] [UserType(schema="ifcProject",name="Id")] [References(RefTableSchema="ifcProject",RefTableName="ProjectGroup",RefTableColName="ProjectGroupId")] public int ProjectGroupId=0;
 [DbField] [UserType(schema="ifcSchema",name="GroupId")] [References(RefTableSchema="ifcSpecification",RefTableName="Specification",RefTableColName="SpecificationId")] public int SpecificationId=0;
}
 
public partial class ProjectGroup_Row : RowBase{
 public ProjectGroup_Row(int ProjectGroupId, string ProjectGroupName, string ProjectGroupDescription, int? ParentProjectGroupId, int ProjectGroupTypeId){this.ProjectGroupId=ProjectGroupId;this.ProjectGroupName=ProjectGroupName;this.ProjectGroupDescription=ProjectGroupDescription;this.ParentProjectGroupId=ParentProjectGroupId;this.ProjectGroupTypeId=ProjectGroupTypeId;}
 public ProjectGroup_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcProject",name="Id")] public int ProjectGroupId=0;
 [DbField] [UserType(schema="Text",name="ToString")] public string ProjectGroupName="";
 [DbField] [UserType(schema="Text",name="Description")] public string ProjectGroupDescription="";
 [DbField] [UserType(schema="ifcProject",name="Id")] [References(RefTableSchema="ifcProject",RefTableName="ProjectGroup",RefTableColName="ProjectGroupId")] public int? ParentProjectGroupId=null;
 [DbField] [UserType(schema="ifcProject",name="Id")] [References(RefTableSchema="ifcProject",RefTableName="ProjectGroupType",RefTableColName="ProjectGroupTypeId")] public int ProjectGroupTypeId=0;
}
 
public partial class ProjectGroupType_Row : RowBase{
 public ProjectGroupType_Row(int ProjectGroupTypeId, string ProjectGroupTypeName, string ProjectGroupTypeDescription){this.ProjectGroupTypeId=ProjectGroupTypeId;this.ProjectGroupTypeName=ProjectGroupTypeName;this.ProjectGroupTypeDescription=ProjectGroupTypeDescription;}
 public ProjectGroupType_Row(){}
 [DbField(PrimaryKey=true, SortAscending=true)] [UserType(schema="ifcProject",name="Id")] public int ProjectGroupTypeId=0;
 [DbField] [UserType(schema="Text",name="ToString")] public string ProjectGroupTypeName="";
 [DbField] [UserType(schema="Text",name="Description")] public string ProjectGroupTypeDescription="";
}
 
}// namespace ifcProject -------------------------------------------------------------------
 
 
public partial class cp_Schema:SchemaBase{// -------------------------------------------------------------------
public TableBase Entity=new RowList<ifcInstance.Entity_Row>();
public TableBase EntityAttributeListElementOfBinary=new RowList<ifcInstance.EntityAttributeListElementOfBinary_Row>();
public TableBase EntityAttributeListElementOfEntityRef=new RowList<ifcInstance.EntityAttributeListElementOfEntityRef_Row>();
public TableBase EntityAttributeListElementOfFloat=new RowList<ifcInstance.EntityAttributeListElementOfFloat_Row>();
public TableBase EntityAttributeListElementOfInteger=new RowList<ifcInstance.EntityAttributeListElementOfInteger_Row>();
public TableBase EntityAttributeListElementOfList=new RowList<ifcInstance.EntityAttributeListElementOfList_Row>();
public TableBase EntityAttributeListElementOfListElementOfEntityRef=new RowList<ifcInstance.EntityAttributeListElementOfListElementOfEntityRef_Row>();
public TableBase EntityAttributeListElementOfListElementOfFloat=new RowList<ifcInstance.EntityAttributeListElementOfListElementOfFloat_Row>();
public TableBase EntityAttributeListElementOfListElementOfInteger=new RowList<ifcInstance.EntityAttributeListElementOfListElementOfInteger_Row>();
public TableBase EntityAttributeListElementOfString=new RowList<ifcInstance.EntityAttributeListElementOfString_Row>();
public TableBase EntityAttributeOfBinary=new RowList<ifcInstance.EntityAttributeOfBinary_Row>();
public TableBase EntityAttributeOfBoolean=new RowList<ifcInstance.EntityAttributeOfBoolean_Row>();
public TableBase EntityAttributeOfEntityRef=new RowList<ifcInstance.EntityAttributeOfEntityRef_Row>();
public TableBase EntityAttributeOfEnum=new RowList<ifcInstance.EntityAttributeOfEnum_Row>();
public TableBase EntityAttributeOfFloat=new RowList<ifcInstance.EntityAttributeOfFloat_Row>();
public TableBase EntityAttributeOfInteger=new RowList<ifcInstance.EntityAttributeOfInteger_Row>();
public TableBase EntityAttributeOfList=new RowList<ifcInstance.EntityAttributeOfList_Row>();
public TableBase EntityAttributeOfString=new RowList<ifcInstance.EntityAttributeOfString_Row>();
public TableBase EntityAttributeOfVector=new RowList<ifcInstance.EntityAttributeOfVector_Row>();
public TableBase EntityInstanceIdAssignment=new RowList<ifcProject.EntityInstanceIdAssignment_Row>();
public TableBase EntityVariableName=new RowList<ifcInstance.EntityVariableName_Row>();
public TableBase Project=new RowList<ifcProject.Project_Row>();
}// of cp_Schema // -------------------------------------------------------------------
 

 
 /// <summary>DataSource with the name "ifcSQL" for Software "ifc_in_out_sql"</summary>
public partial class _ifcSQL_for_ifcSQL_instance:TableSet{ //assign Tables to the TableSet
public _ifcSQL_for_ifcSQL_instance(string ServerName, string DatabaseName="ifcSQL_Instance"):base(ServerName,DatabaseName){}
public _ifcSQL_for_ifcSQL_instance(string ServerName,string DatabaseName,string UserName,string Password,bool DirectLoad=false):base(ServerName,DatabaseName,UserName,Password,DirectLoad){}
public _ifcSQL_for_ifcSQL_instance():base(){}
public cp_Schema cp =new cp_Schema();
}
}// namespace ifc_in_out_sql ########################################################################
