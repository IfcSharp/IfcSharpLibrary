// ifc_base_type.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Globalization;

namespace ifc{//==============================

public class TypeBase:ifcSqlType{
public virtual Type GetBaseType(){return null;}
public static char StringChar='\'';
public static bool HasStringChar=true;
public virtual object ToSqliteValue() { return DBNull.Value; }//EF20200131:Added custom converter method for all TypeBase objects
}

public partial class TYPE<T>:TypeBase{//,ifcParseInterface {//--------------------------------
public       TYPE(){IsNull=true;}
public       TYPE(T v){IsNull=false;TypeValue=v;}
public T TypeValue;
public override Type GetBaseType(){return typeof(T);}

public override string ToString(){
            if (IsNull)  return "$";
            //EF-2021-03-02: commented out 'TrimEnd'. for trailing zeros, this is considered as schema violation, because the decimalpoint has no following zero
            //((double)(object)TypeValue).ToString("0.0000000000", CultureInfo.InvariantCulture).TrimEnd('0'); 
            else if (typeof(T).Equals(typeof(double))) return string.Format("{0:0.0###########}", (double)(object)TypeValue);
            else if (typeof(T).Equals(typeof(bool))) return ((bool)(object)TypeValue) ? ".T." : ".F.";
            else if (typeof(T).Equals(typeof(string))) { if (HasStringChar) return StringChar + ((string)(object)TypeValue).ToString() + StringChar; else return ((string)(object)TypeValue).ToString(); }
            else return TypeValue.ToString();
        }
}//-----------------------------------------------------


}// ifc=======================================