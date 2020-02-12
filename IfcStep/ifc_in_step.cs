// ifc_in_step.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using Threading=System.Threading;
using System.Reflection;




namespace ifc{//==============================
 

public partial class ENTITY{//==========================================================================================

public static string ReplaceCharAt(string s,int i, char c){char[] array = s.ToCharArray();array[i] = c;s = new string(array);return s;}

//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
public static object Parse2TYPE(string value,Type FieldType)
{Type  BaseType=FieldType.BaseType.GetGenericArguments()[0]; 
 if (BaseType.BaseType.GetGenericArguments().Length>0) BaseType=BaseType.BaseType;//.GetGenericArguments()[0];

//Console.WriteLine("Parse2TYPE.BaseType= "+BaseType.Name+" "+value); Console.ReadLine();

 object NewType=null;
 object[] TypeCtorArgs=new object[1];
       if ( (value=="$") || (value=="*") ) NewType=Activator.CreateInstance(FieldType);
 else {//=====================================================================
            if (BaseType==typeof(String)) { if (value=="$") TypeCtorArgs[0]=""; else  TypeCtorArgs[0]=ifc.IfcString.Decode(value);NewType=Activator.CreateInstance(FieldType,TypeCtorArgs); }
       else if (BaseType==typeof(int)) { TypeCtorArgs[0]=int.Parse(value);NewType=Activator.CreateInstance(FieldType,TypeCtorArgs);}
       else if (BaseType==typeof(double)) {  TypeCtorArgs[0]=double.Parse(value,CultureInfo.InvariantCulture);NewType=Activator.CreateInstance(FieldType,TypeCtorArgs);}
       else if (BaseType==typeof(bool)){TypeCtorArgs[0]=(value==".T.");NewType=Activator.CreateInstance(FieldType,TypeCtorArgs);}
       else Console.WriteLine("UNKNOWN TYPE: Base="+BaseType.Name+" value="+value);   
      } //=====================================================================
//Console.WriteLine(NewType);
 return NewType;
}
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


/*
Beispiel: List1to3_LengthMeasure
--------
Beispiel:  CartesianPoint.List1to3_LengthMeasure ( List1to3_LengthMeasure:List1to3<LengthMeasure is TYPE:TYPE<Double>>
FieldType: List1to3_LengthMeasure
FieldType.BaseType: List1to3<LengthMeasure>
FieldType.BaseType.GetGenericArguments()[0]: LengthMeasure
FieldType.BaseType.GetGenericArguments()[0].BaseType: TYPE<Double>
FieldType.BaseType.GetGenericArguments()[0].GetGenericArguments()[0]: Double
 * Ziel dieser Funktion: NewInstance=new List1to3_LengthMeasure(new LengthMeasure(0.1),new LengthMeasure(1.2),new LengthMeasure(2.3));
 Beipiel 2:
 Polyline.List2toUnbounded_CartesianPoint ( List2toUnbounded_CartesianPoint:List2toUnbounded<CartesianPoint: is ENTITY, not TYPE<ENTITY> !!
  [ifcSql(TypeId:  21)] public partial class List2toUnbounded_CartesianPoint:List2toUnbounded<CartesianPoint>{public List2toUnbounded_CartesianPoint(List2toUnbounded<CartesianPoint> value):base(value){} public List2toUnbounded_CartesianPoint(){} public List2toUnbounded_CartesianPoint(params CartesianPoint[] items):base(){foreach (CartesianPoint e in items)  this.Add(e);} new bool IsNull{get{return (this.Count==0);}set{if (value) this.Clear();}} }
*/

public static Type GetGenericType(Type FieldType){
if (FieldType.BaseType.GetGenericArguments().Length>0) return FieldType.BaseType.GetGenericArguments()[0]; //LengthMeasure or CartesianPoint
else                                                   return FieldType.BaseType.BaseType.GetGenericArguments()[0]; //CompoundPlaneAngleMeasure
}

public static object[] GetFieldCtorArgs(Type GenericType,string[] ListElements)
{
 object[] FieldCtorArgs=new object[ListElements.Length];

 for (int ListPos=0;ListPos<ListElements.Length;ListPos++)
                  {//.....................................................
                   string ListElement=ListElements[ListPos];
                       if (GenericType==typeof(   Int32)) {object[] GenericCtorArgs=new object[1];
                                                                    GenericCtorArgs[0]=Activator.CreateInstance(GenericType);  // ereugen
                                                                    GenericCtorArgs[0]=Int32.Parse(ListElement);               // Wert zuweisen                      ---> wetere Verwendng?
                                                           FieldCtorArgs[ListPos]=Int32.Parse(ListElement);//Activator.CreateInstance(GenericType,GenericCtorArgs);
                                                          }
                  else if (GenericType.IsSubclassOf(typeof(TypeBase))) {object[] GenericCtorArgs=new object[1];
                                                                                 GenericCtorArgs[0]=Activator.CreateInstance(GenericType); //LengthMeasure or CartesianPoint
                                                                   Type     GenericBaseType=GenericType.BaseType.GetGenericArguments()[0];    //Double from LengthMeasure -> TYPE<double> -> double
                                                                        if (GenericBaseType==typeof(String)) {if (ListElement=="$") GenericCtorArgs[0]=""; else  GenericCtorArgs[0]=ifc.IfcString.Decode(ListElement);}
                                                                   else if (GenericBaseType==typeof(   int)) {                      GenericCtorArgs[0]=int.Parse(ListElement);}
                                                                   else if (GenericBaseType==typeof( Int32)) {                      GenericCtorArgs[0]=Int32.Parse(ListElement);}
                                                                   else if (GenericBaseType==typeof(double)) {                      GenericCtorArgs[0]=double.Parse(ListElement,CultureInfo.InvariantCulture);}

                                                                   FieldCtorArgs[ListPos]=Activator.CreateInstance(GenericType,GenericCtorArgs);
                                                                  }
                   else if (GenericType.IsSubclassOf(typeof(ENTITY)))   {object o=Activator.CreateInstance(GenericType);
                                                                         ((ENTITY)o).Id=int.Parse(ListElement.Trim(' ').Substring(1));
                                                                         FieldCtorArgs[ListPos]=o;
                                                                        }
                   else if (GenericType.IsSubclassOf(typeof(SELECT)))   {object o=Activator.CreateInstance(GenericType);
                                                                         ListElement=ListElement.Trim(' ');
                                                                         if ((ListElement.Length>0) && ListElement[0]=='#') { ((SELECT)o).Id=int.Parse(ListElement.Trim(' ').Substring(1)); } // hat SELECT id ? //  Console.WriteLine("C:"+ ((SELECT)o).Id +" "+( ((SELECT)o).IsNull).ToString() );
                                                                         else {ListElement=ListElement.Replace("IFC","ifc."); 
                                                                               int posLpar=ListElement.IndexOf('('); 
                                                                               int posRpar=ListElement.Length-1;//.LastIndexOf(')');
                                                                               string body=ListElement.Substring(posLpar+1,posRpar-posLpar-1); // Argumenkörper extrahieren
                                                                               string ElementName=ListElement.Substring(0,posLpar);
                                                                               try{Type t=Type.GetType(ElementName,true,true); 
                                                                                   if (t.IsSubclassOf(typeof(TypeBase)))  
                                                                                      {object[] GenericCtorArgs=new object[1];
                                                                                            if (t.IsSubclassOf(typeof(TYPE<string>))) {if (ListElement=="$") GenericCtorArgs[0]=""; else  GenericCtorArgs[0]=ifc.IfcString.Decode(body);}
                                                                                       else if (t.IsSubclassOf(typeof(TYPE<   int>))) {                      GenericCtorArgs[0]=int.Parse(body);}
                                                                                       else if (t.IsSubclassOf(typeof(TYPE< Int32>))) {                      GenericCtorArgs[0]=Int32.Parse(body);}
                                                                                       else if (t.IsSubclassOf(typeof(TYPE<double>))) {                      GenericCtorArgs[0]=double.Parse(body,CultureInfo.InvariantCulture);}
                                                                                       o=Activator.CreateInstance(t,GenericCtorArgs);
                                                                                      }
                                                                                  }catch(Exception){} 
                                                                              }
                                                                         FieldCtorArgs[ListPos]=o;                                                                  } 
                   else {Console.WriteLine("TODO List++TYPE: Base="+GenericType.Name+" not supportet. in\n"+CurrentLine); }// andere Typen
                  }//.....................................................
return FieldCtorArgs;
}

public static object Parse2LIST(string value,Type FieldType)
{
 if(!typeof(IEnumerable).IsAssignableFrom(FieldType)) Console.WriteLine("Parse2LIST: "+FieldType+" is not IEnumerable");
 if ( (value=="$") || (value=="*") ) return Activator.CreateInstance(FieldType); // warum  nicht null ?
Type GenericType=GetGenericType(FieldType);
 string[] ListElements=value.TrimStart('(').TrimEnd(')').Split(','); if (ListElements.Length==0) Console.WriteLine("ListElements.Length=0");
 return Activator.CreateInstance(FieldType,GetFieldCtorArgs(GenericType,ListElements));
 } //++++++++++++++++++++++++++++++++++++++++++++++++++++

 
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


public static object ParseSelect(string Element,object o)
{
//Console.WriteLine("Parse SELECT: "+Element);
string ElementName=Element.Replace("IFC","ifc.");
int posLpar=ElementName.IndexOf('(');
int posRpar=ElementName.LastIndexOf(')');

string body=ElementName.Substring(posLpar+1,posRpar-posLpar-1); // Argumenkörper extrahieren
ElementName=ElementName.Substring(0,posLpar);
try{Type t=Type.GetType(ElementName,true,true); // Console.WriteLine("A");
    if (t.IsSubclassOf(typeof(TypeBase))) {object[] TypeCtorArgs=new object[1];// Console.WriteLine("B");
                                           Type SelectType=o.GetType(); // Console.WriteLine("C");
                                           TypeCtorArgs[0]=Parse2TYPE(body,t); // Console.WriteLine("D");
                                           //Console.WriteLine(ElementName+": "+body+ " o: "+o.GetType().Name+" [0]= "+TypeCtorArgs[0].GetType().Name);
                                           o=Activator.CreateInstance(SelectType,TypeCtorArgs); // Console.WriteLine("E");
                                          }
   }catch(Exception e){Console.WriteLine(ElementName+" body="+body+" ERROR SELECT: "+posLpar+" "+e.Message+"\n"+CurrentLine);}// 2. true: ignoreCase
return o;
}



public static string CurrentLine="";
public static string CurrentEntityComment="";
//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

public static void ParseIfcLine(Model CurrentModel,string line)
{
//Console.WriteLine("parse="+line);
CurrentLine=line;
int CommentOpenPos=line.IndexOf("/*"); if (CommentOpenPos>=0) {CurrentEntityComment=line.Substring(CommentOpenPos+2).Replace("*/","");line=line.Substring(0,CommentOpenPos);  
//Console.WriteLine("CommentOpenPos="+CommentOpenPos+" "+CurrentEntityComment);  
}
 // if (EndOfLineComment!=null) s+="/* "+EndOfLineComment+" */";
if (CommentOpenPos!=0){//====================================================================================================

int posA=line.IndexOf('=');
int posLpar=line.IndexOf('(');
int posRpar=line.LastIndexOf(')');
string ElementName=line.Substring(posA+1,posLpar-posA-1).TrimStart(' ').Substring(3);
string body=line.Substring(posLpar+1,posRpar-posLpar-1); // Argumentkörper extrahieren
bool TxtOpen=false;
int ParOpen=0;
for (int i=0;i<body.Length;i++) {     if ((!TxtOpen) && (body[i]=='\'')) {TxtOpen=true; }//body=ReplaceCharAt(body,i,'[');}
                                 else if ((!TxtOpen) && (body[i]=='(' )) {ParOpen++; }//body=ReplaceCharAt(body,i,'<');}
                                 else if ((!TxtOpen) && (body[i]==')' )) {ParOpen--;}//body=ReplaceCharAt(body,i,'>');}
                                 else if (( TxtOpen) && (body[i]=='\'')) {TxtOpen=false;}//body=ReplaceCharAt(body,i,']');}
                                 if ((TxtOpen) || (ParOpen>0)) if (body[i]==',') {body=ReplaceCharAt(body,i,(char)9); }
                                }
string[] args=body.Split(',');
for (int i=0;i<args.Length;i++) args[i]=args[i].Replace((char)9,',');
try{Type t=Type.GetType("ifc."+ElementName,true,true);// 2. true: ignoreCase
object CurrentEntity=Activator.CreateInstance(t); if (CommentOpenPos>0) ((ENTITY)CurrentEntity).EndOfLineComment=CurrentEntityComment;
((ENTITY)CurrentEntity).Id=int.Parse(line.Substring(1,posA-1));
Dictionary<int,FieldInfo> VarDict=new Dictionary<int,FieldInfo>();
int VarCount=0;

//foreach (FieldInfo field in CurrentEntity.GetType().GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) Console.WriteLine(field.Name);

foreach (FieldInfo field in CurrentEntity.GetType().GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) 
    foreach (Attribute attr in field.GetCustomAttributes(true)) if (attr is ifcAttribute) {VarDict.Add(((ifcAttribute)attr).OrdinalPosition,field);VarCount++;} //  if (attr is ifcAttribute) //Console.WriteLine(VarCount+": "+attr.OrdinalPosition+": "+field.Name); 
for (int i=1;i<=VarCount;i++)       
    {FieldInfo field=VarDict[i]; // Console.Write(field.Name+", ");
     string value="$";
     if (i<=args.GetLength(0)) value=args[i-1].Trim(' ').Trim('\'');
          if (field.FieldType==typeof(String))  {if (value=="$") field.SetValue(CurrentEntity,""/*null*/); else field.SetValue(CurrentEntity,ifc.IfcString.Decode(value));}
     else if (field.FieldType==typeof(int))     {if (value=="$") field.SetValue(CurrentEntity,0); else field.SetValue(CurrentEntity,int.Parse(value));}
     else if (field.FieldType.IsSubclassOf(typeof(TypeBase))) {try{field.SetValue(CurrentEntity,Parse2TYPE(value,field.FieldType));}catch(Exception e){throw new Exception("Parse.Type: Field "+i+": "+field.FieldType.ToString()+": "+e.Message);}
                                                              } //tb.GetBaseType()
     else if (field.FieldType.IsSubclassOf(typeof(SELECT)))  {
                                                                  //Console.WriteLine("begin SELECT: "+line);
                                                                try{object o=Activator.CreateInstance(field.FieldType);
                                                                       if ((value.Length>0) && (value[0]=='$')) {} 
                                                                  else if ((value.Length>0) && (value[0]=='*')) {} 
                                                                  else if ((value.Length>0) && (value[0]=='#')) {((SELECT)o).Id=int.Parse(value.Substring(1)); }
                                                                  else  o=ParseSelect(value,o); 
                                                                  field.SetValue(CurrentEntity,o);
                                                                 }catch(Exception e){throw new Exception("xx Parse.SELECT: Field "+i+": "+field.FieldType.ToString()+": "+e.Message);}
                                                             }
     else if (field.FieldType.IsSubclassOf(typeof(ENTITY)))  {try{object o=null; //falls $
                                                                  if (value.Length>0) if (value[0]=='*') o=Activator.CreateInstance(field.FieldType);
                                                                  if (value.Length>0) if (value[0]=='#'){o=Activator.CreateInstance(field.FieldType);((ENTITY)o).Id=int.Parse(value.Substring(1));}
                                                                  field.SetValue(CurrentEntity,o);
                                                                 }catch(Exception e){throw new Exception("Parse.ENTITY: Field "+i+": "+field.FieldType.ToString()+": "+e.Message);}
                                                             }
     else if (field.FieldType.IsSubclassOf(typeof(Enum)))    {try{object FieldInstance=Activator.CreateInstance(field.FieldType);
                                                                  if ((value.Length>0) && (value[0]=='$')) FieldInstance=0;
                                                                  else FieldInstance=Enum.Parse(field.FieldType, value.Substring(1,value.Length-2));
                                                                  field.SetValue(CurrentEntity,FieldInstance);
                                                                 }catch(Exception e){throw new Exception("Parse.Enum: Field "+i+": "+field.FieldType.ToString()+": "+e.Message);}
                                                             }
     else if ( (Nullable.GetUnderlyingType(field.FieldType)!=null) && (Nullable.GetUnderlyingType(field.FieldType).IsSubclassOf(typeof(Enum))) ) 
                                                             {try{object FieldInstance=null; 
                                                                  if ((value.Length>0) && (value[0]!='$')) {FieldInstance=Activator.CreateInstance(field.FieldType);
                                                                                                            FieldInstance=Enum.Parse(Nullable.GetUnderlyingType(field.FieldType), value.Substring(1,value.Length-2));
                                                                                                           }
                                                                  field.SetValue(CurrentEntity,FieldInstance);
                                                                 }catch(Exception e){throw new Exception("Parse.Enum: Field "+i+": "+field.FieldType.ToString()+": "+e.Message);}
                                                              }
     //else if( typeof(IEnumerable).IsAssignableFrom(field.FieldType)){
     else if (typeof(ifcListInterface).IsAssignableFrom(field.FieldType)) {
                                                                    try{if (value=="$") field.SetValue(CurrentEntity,null); 
                                                                         else field.SetValue(CurrentEntity,Parse2LIST(value,field.FieldType));
                                                                        }catch(Exception e){Console.WriteLine("Parse.LIST:"+line);
                                                                    throw new Exception("Parse.LIST: Field "+i+": "+field.FieldType.ToString()+": "+e.Message);}
                                                                    }
     else  Console.WriteLine(i+": is "+field.FieldType.Name+ "???????x? "+field.FieldType.Name+"  "+ line); //ifc.WallStandardCase
    }

CurrentModel.EntityList.Add((ENTITY)CurrentEntity); //Console.WriteLine();Console.WriteLine("AAA "+((ENTITY)CurrentEntity).ToString());
}catch(Exception e){Console.WriteLine ("ERROR on ParseIfcLine:"+e.Message);Console.WriteLine (line);}//Console.ReadLine();}
          }//====================================================================================================
else {EntityComment ec=new EntityComment();ec.CommentLine=CurrentEntityComment;CurrentModel.EntityList.Add(ec);}
}



}//ENTITY=====================================================================================================================


public partial class Model{//==========================================================================================




public static Model FromStepFile(string FileName)
{
//ifc.Repository.CurrentModel.
Model CurrentModel=new ifc.Model(FileName.Replace(".ifc",""));
StreamReader sr=new StreamReader(FileName);
string line="";
//while ( (line = sr.ReadLine()) != null  && (line.Length>1) && ((line+' ')[0]!='#') ) {};//Header.Add(line);
while ( (line = sr.ReadLine()) != null  && (line.Length>1) && ((line+' ')[0]!='#') && (!line.StartsWith("DATA")) ) {};//Header.Add(line);
//Console.WriteLine("XX");
//if (line.Length>1) ENTITY.ParseIfcLine(CurrentModel,line); 
while ( (line = sr.ReadLine()) != null) if (line.Length>3) 
      {line=line.TrimStart(' '); //Console.WriteLine(line);
       if (line.Length>3) if (line[0]=='#' || (line[0]=='/' && line[1]=='*') ) ENTITY.ParseIfcLine(CurrentModel,line);  
      }
sr.Close(); 
CurrentModel.AssignEntities();
return CurrentModel;
}// of FromStepFile
}// of Model ==========================================================================================================

}// ifc==============================




