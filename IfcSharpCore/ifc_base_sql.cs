// ifc_base_sql.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

namespace ifc{//==============================

enum SqlTable{
_NULL=-2, // BaseTypeGroup: _NULL, NestLevel=-2
SELECT=-1, // BaseTypeGroup: SELECT, NestLevel=-1
EntityAttributeOfVector=0, // BaseTypeGroup: Vector (2d and 3d), NestLevel=0
EntityAttributeOfBinary=1, // BaseTypeGroup: Binary, NestLevel=0
EntityAttributeOfBoolean=2, // BaseTypeGroup: Boolean, NestLevel=0
EntityAttributeOfEntityRef=3, // BaseTypeGroup: EntityRef, NestLevel=0
EntityAttributeOfEnum=4, // BaseTypeGroup: Enum, NestLevel=0
EntityAttributeOfFloat=5, // BaseTypeGroup: Float, NestLevel=0
EntityAttributeOfInteger=6, // BaseTypeGroup: Integer, NestLevel=0
EntityAttributeOfString=7, // BaseTypeGroup: String, NestLevel=0
EntityAttributeOfList=8, // BaseTypeGroup: List1, NestLevel=0
EntityAttributeListElementOfBinary=9, // BaseTypeGroup: Binary, NestLevel=1
EntityAttributeListElementOfEntityRef=10, // BaseTypeGroup: EntityRef, NestLevel=1
EntityAttributeListElementOfFloat=11, // BaseTypeGroup: Float, NestLevel=1
EntityAttributeListElementOfInteger=12, // BaseTypeGroup: Integer, NestLevel=1
EntityAttributeListElementOfString=13, // BaseTypeGroup: String, NestLevel=1
EntityAttributeListElementOfList=14, // BaseTypeGroup: List2, NestLevel=1
EntityAttributeListElementOfListElementOfEntityRef=15, // BaseTypeGroup: EntityRef, NestLevel=2
EntityAttributeListElementOfListElementOfFloat=16, // BaseTypeGroup: Float, NestLevel=2
EntityAttributeListElementOfListElementOfInteger=17, // BaseTypeGroup: Integer, NestLevel=2
}

}// ifc==============================
