// ifc_base.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)


/*

SELECT-Types contains mixed Elements like Types AND Entities (ENTITY for TrimmingSelect.CartesianPoint, TYPE for TrimmingSelect.ParameterValue)
SELECT-Types can't be base classes, otherwise multiple inheritance (e.g. TYPE and ENTITY) would arise, what is not suported by c#. 
SELECT-Types must checked for reading against all contained typs AND their derivations (permission check) e.g. Unit(DerivedUnit,MonetaryUnit,NamedUnit), but also SIUnit, because SIUnit is subtype of NamedUnit.
SELECT-Types can have multible possible types (not Entities), wich have the same base-type. So the the type-name nust be explicit to be named.
eg.: Value (DerivedMeasureValue,MeasureValue,SimpleValue),  SimpleValue(Label<string>, Text<string>)  
So SELECT must contain the instanced Typ-name, e.g. IFCBOOLEAN(.T.), IFCBOOLEAN(.F.)


Types don't need a explicit Type-name, e.g. GloballyUniqueId

Entity-Element wich have Type-names here have a preceding underscore.  
Alternatively, an @ prefix would be possible.
Since element names only very rarely have type names, the preceding @ is omitted for reasons of better readability.

IfcSharp does not require the use of nullable base types, as it only affects classes (that are per se nullable). Only for enum data types nullable is sometimes required.

*/



using NetSystem=System;

namespace ifc{//==============================

enum TypeGroup {_NULL=-1,BASETYPE=0,LISTTYPE1D=1,LISTTYPE2D=2,TYPE=3,ENUM=4,ENTITY=5,SELECT=6,LISTTYPE1DOF2D=7}

public class ifcSqlAttribute:NetSystem.Attribute{public ifcSqlAttribute(int TypeGroupId, int TypeId, int TableId=0){this.SqlTypeId=TypeId;this.SqlTypeGroupId=TypeGroupId;this.SqlTableId=TableId;}
                                       public int SqlTypeId=0;
                                       public int SqlTypeGroupId=(int)TypeGroup._NULL;
                                       public int SqlTableId=0;
                                      }


public interface ifcSqlTypeInterface{
int SqlTypeId();
int SqlTypeGroupId();
int SqlTableId();
bool IsNull{get;set;}
}

public interface ifcParseInterface{
object Parse(string value);
}

public class ifcType{ // place-holder
}

public class ifcSqlType:ifcType,ifcSqlTypeInterface{
public int    SqlTypeId(){return ((ifcSqlAttribute)this.GetType().GetCustomAttributes(true)[0]).SqlTypeId;}
public int    SqlTypeGroupId(){return ((ifcSqlAttribute)this.GetType().GetCustomAttributes(true)[0]).SqlTypeGroupId;}
public int    SqlTableId(){return ((ifcSqlAttribute)this.GetType().GetCustomAttributes(true)[0]).SqlTableId;}
public static int SqlTypeId(NetSystem.Type t){ return ((ifcSqlAttribute)t.GetCustomAttributes(true)[0]).SqlTypeId;}
public static int SqlTypeGroupId(NetSystem.Type t){return ((ifcSqlAttribute)t.GetCustomAttributes(true)[0]).SqlTypeGroupId;}
public static int SqlTableId(NetSystem.Type t){return ((ifcSqlAttribute)t.GetCustomAttributes(true)[0]).SqlTableId;}


private bool _IsNull=true;
public bool IsNull{get{return _IsNull;}set{_IsNull = value;}}
}

class Exception:NetSystem.Exception {public Exception (NetSystem.String reason) : base ("ifcSharp:"+reason){}}

public partial class GloballyUniqueId          :TYPE<string>    {public  static string CreateNewId(){ return IfcGuid.ToIfcGuid(NetSystem.Guid.NewGuid()); }
                                                                 public  static GloballyUniqueId NewId(){ return new GloballyUniqueId(CreateNewId()); }
                                                                }


}//ifc==============================

