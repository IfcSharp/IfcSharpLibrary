// ifc_base_select.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)


using System;

namespace ifc{//==============================

public class SELECT:ifcSqlType{//--------------------------------
public Type SelectType(){return _Type;}
public object SelectValue(){return _SelectValue;}
public void SetValueAndType(object _SelectValue,Type _Type){this._SelectValue=_SelectValue;this._Type=_Type;}
protected object _SelectValue;
protected Type _Type;
private int _Id=0;
public int Id{get{return _Id;}set{ IsNull=(value==0);_Id = value;}}

}//---------------------------------------------------

}// ifc=======================================