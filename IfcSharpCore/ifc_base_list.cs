
// ifc_base_list.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)


using System;
using System.Collections.Generic;

namespace ifc{//==============================

public partial interface ifcListInterface{
Type GetItemsType();
}

public partial class LIST<T> : List<T>,ifcListInterface,ifcSqlTypeInterface{
public int    SqlTypeId(){return ((ifcSqlAttribute)this.GetType().GetCustomAttributes(true)[0]).SqlTypeId;}
public int    SqlTypeGroupId(){return ((ifcSqlAttribute)this.GetType().GetCustomAttributes(true)[0]).SqlTypeGroupId;}
public int    SqlTableId(){return ((ifcSqlAttribute)this.GetType().GetCustomAttributes(true)[0]).SqlTableId;}
public       LIST(){}
public       LIST(int MinOccurs,int MaxOccurs){this.MinOccurs=MinOccurs;this.MaxOccurs=MaxOccurs;}
public Type GetItemsType(){return typeof(T);}
public override string ToString(){string s="(";int pos=0; foreach (T item in this) {if (++pos>1) s+=","; s+=ENTITY.StepAttributeOut(item);};s+=")";return s; }
public int MinOccurs=0;
public int MaxOccurs=0;
public bool IsNull{get{return (this.Count==0);}set{if (value) this.Clear();}}
}

public class Array1to2       <T>:LIST<T>{public Array1to2(Array1to2<T> cc):base(1,2){foreach(T c in cc) this.Add(c);} public Array1to2():base(1,2){} public Array1to2(T e):base(1,2){this.Add(e);} public Array1to2(T e1,T e2):base(1,2){this.Add(e1);this.Add(e2);} }
public class List0toUnbounded<T>:LIST<T>{public List0toUnbounded(List0toUnbounded<T> cc):base(0,0){foreach(T c in cc) this.Add(c);} public List0toUnbounded():base(0,0){} public List0toUnbounded(params T[] items):base(0,0){foreach (T e in items)  this.Add(e);} }
public class List1to3        <T>:LIST<T>{public List1to3(List1to3<T> cc):base(1,3){foreach(T c in cc) this.Add(c);} public List1to3():base(1,3){} public List1to3(T t1,T t2):base(1,3){this.Add(t1);this.Add(t2);}  public List1to3(T t1,T t2,T t3):base(1,3){this.Add(t1);this.Add(t2);this.Add(t3);} }
public class List1toUnbounded<T>:LIST<T>{public List1toUnbounded(List1toUnbounded<T> cc):base(1,0){foreach(T c in cc) this.Add(c);}public List1toUnbounded():base(1,0){} public List1toUnbounded(params T[] items):base(1,0){foreach (T e in items)  this.Add(e);} }
public class List1to2        <T>:LIST<T>{public List1to2(List1to2<T> cc):base(1,2){foreach(T c in cc) this.Add(c);}public List1to2():base(1,2){} public List1to2(T t1,T t2):base(1,2){this.Add(t1);this.Add(t2);} }
public class List2to2        <T>:LIST<T>{public List2to2(List2to2<T> cc):base(2,2){foreach(T c in cc) this.Add(c);}public List2to2():base(2,2){} public List2to2(T t1,T t2):base(2,2){this.Add(t1);this.Add(t2);} }
public class List2to3        <T>:LIST<T>{public List2to3(List2to3<T> cc):base(2,3){foreach(T c in cc) this.Add(c);}public List2to3():base(2,3){} public List2to3(T t1,T t2):base(2,3){this.Add(t1);this.Add(t2);} public List2to3(T t1,T t2,T t3):base(2,3){this.Add(t1);this.Add(t2);this.Add(t3);} }
public class List2toUnbounded<T>:LIST<T>{public List2toUnbounded(List2toUnbounded<T> cc):base(2,0){foreach(T c in cc) this.Add(c);}public List2toUnbounded():base(2,0){} public List2toUnbounded(params T[] items):base(2,0){foreach (T e in items)  this.Add(e);} }
public class List3to3        <T>:LIST<T>{public List3to3        (List3to3<T> cc):base(3,3){foreach(T c in cc) this.Add(c);}public List3to3        ():base(3,3){} public List3to3(T t1,T t2,T t3):base(3,3){this.Add(t1);this.Add(t2);this.Add(t3);} }
public class List3to4        <T>:LIST<T>{public List3to4        (List3to4<T> cc):base(3,4){foreach(T c in cc) this.Add(c);}public List3to4        ():base(3,4){} public List3to4(T t1,T t2,T t3):base(3,4){this.Add(t1);this.Add(t2);this.Add(t3);}public List3to4(T t1,T t2,T t3,T t4):base(3,4){this.Add(t1);this.Add(t2);this.Add(t3);this.Add(t4);} }
public class List3toUnbounded<T>:LIST<T>{public List3toUnbounded(List3toUnbounded<T> cc):base(3,0){foreach(T c in cc) this.Add(c);}public List3toUnbounded():base(3,0){} public List3toUnbounded(params T[] items):base(3,0){foreach (T e in items)  this.Add(e);} }
public class Set0toUnbounded <T>:LIST<T>{public Set0toUnbounded(Set0toUnbounded<T> cc):base(0,0){foreach(T c in cc) this.Add(c);}public Set0toUnbounded():base(0,0){} public Set0toUnbounded(params T[] items):base(0,0){foreach (T e in items)  this.Add(e);} }
public class Set1to2         <T>:LIST<T>{public Set1to2(Set1to2<T> cc):base(1,2){foreach(T c in cc) this.Add(c);}public Set1to2():base(1,2){} public Set1to2(T t1,T t2):base(){this.Add(t1);this.Add(t2);} }
public class Set1to5         <T>:LIST<T>{public Set1to5(Set1to5<T> cc):base(1,5){foreach(T c in cc) this.Add(c);}public Set1to5():base(1,5){} public Set1to5(params T[] items):base(1,5){foreach (T e in items)  this.Add(e);} }

public class Set1toUnbounded <T>:LIST<T>{public Set1toUnbounded(Set1toUnbounded<T> cc):base(1,0){foreach(T c in cc) this.Add(c);}public Set1toUnbounded():base(1,0){} public Set1toUnbounded(params T[] items):base(1,0){foreach (T e in items)  this.Add(e);} }

public class Set2toUnbounded <T>:LIST<T>{public Set2toUnbounded(Set2toUnbounded<T> cc):base(2,0){foreach(T c in cc) this.Add(c);}public Set2toUnbounded():base(2,0){} public Set2toUnbounded(params T[] items):base(2,0){foreach (T e in items)  this.Add(e);} }
public class UniqueList1to2  <T>:LIST<T>{public UniqueList1to2(UniqueList1to2<T> cc):base(1,2){foreach(T c in cc) this.Add(c);}public UniqueList1to2():base(1,2){} public UniqueList1to2(T t1,T t2):base(1,2){this.Add(t1);this.Add(t2);} }

public class List1toUnboundedUnique<T>:List1toUnbounded<T>{public List1toUnboundedUnique(List1toUnboundedUnique<T> cc):base(){foreach(T c in cc) this.Add(c);} public List1toUnboundedUnique():base(){} public List1toUnboundedUnique(params T[] items):base(){foreach (T e in items)  this.Add(e);} }
public class List3toUnboundedUnique<T>:List3toUnbounded<T>{public List3toUnboundedUnique(List3toUnboundedUnique<T> cc):base(){foreach(T c in cc) this.Add(c);} public List3toUnboundedUnique():base(){} public List3toUnboundedUnique(params T[] items):base(){foreach (T e in items)  this.Add(e);} }
public class List2to2Unique<T>:List2to2<T>{public List2to2Unique(List2to2Unique<T> cc):base(){foreach(T c in cc) this.Add(c);}public List2to2Unique():base(){} public List2to2Unique(T t1,T t2):base(){this.Add(t1);this.Add(t2);} }


}// ifc=======================================