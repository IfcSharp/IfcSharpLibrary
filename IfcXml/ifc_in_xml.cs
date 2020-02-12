// ifc_in_xml.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

#undef TRACE

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using Threading=System.Threading;
using System.Reflection;

using System.Xml;
using System.Xml.XPath;



namespace ifc{//==============================

public partial class ENTITY{//==========================================================================================

/*
Entityausgabe
1. Typen und enum als Attribute (ausser SELECT und Listen) 
2. Selects (Name des Attributes, dann nach der Klasse mit "-wrapper")
3. Listentypen (Auflistng der Instanztpnamen, zumindest bei EntityRef-Listen)
4. EntityRefs (Name des Attributes, nicht der Klasse)
5. InversElemente nach gleichem Schema wie Entityausgabe

  <IfcExtrudedAreaSolid Depth="40">
	  <SweptArea xsi:type="IfcRectangleProfileDef" ProfileType="area" ProfileName="Panel" XDim="800" YDim="2040">
	  </SweptArea>
	  <Position xsi:nil="true" ref="i1"/>
	  <ExtrudedDirection xsi:nil="true" ref="i6"/>
  </IfcExtrudedAreaSolid>
*/

public string XmlId=null;
public string XmlRefId=null;

public class EntityXmlTypeInfo{
public Dictionary<string,FieldInfo>      XmlAttributeNameIfcTypeAttributeDict=new Dictionary<string,FieldInfo>(); 
public Dictionary<string,FieldInfo>      XmlAttributeNameIfcEnumAttributeDict=new Dictionary<string,FieldInfo>();
public Dictionary<string,FieldInfo>      XmlAttributeNameIfcNEnmAttributeDict=new Dictionary<string,FieldInfo>(); // nullable enum
public Dictionary<string,FieldInfo>        XmlElementNameIfcEntityAttributeDict=new Dictionary<string,FieldInfo>();
public Dictionary<string,FieldInfo>        XmlElementNameIfcSelectAttributeDict=new Dictionary<string,FieldInfo>();
public Dictionary<string,FieldInfo>        XmlElementNameIfcListAttributeDict=new Dictionary<string,FieldInfo>();
public Dictionary<string,FieldInfo>        XmlElementNameIfcInverseEntityDict=new Dictionary<string,FieldInfo>();
public Dictionary<string,FieldInfo>        XmlElementNameIfcInverseEntityListDict=new Dictionary<string,FieldInfo>();
#if(TRACE)
public override string ToString() //..........................
{string s="Attributes of type "+this.GetType().Name+":\n";
s+="XmlAttributeNameIfcTypeAttributeDict:\n";  foreach(KeyValuePair<string,FieldInfo> kvp in XmlAttributeNameIfcTypeAttributeDict)   s+="  "+kvp.Key+" of type "+kvp.Value.FieldType.Name+"\n";
s+="XmlAttributeNameIfcEnumAttributeDict:\n";  foreach(KeyValuePair<string,FieldInfo> kvp in XmlAttributeNameIfcEnumAttributeDict)   s+="  "+kvp.Key+" of type "+kvp.Value.FieldType.Name+"\n";
s+="XmlAttributeNameIfcNEnmAttributeDict:\n";  foreach(KeyValuePair<string,FieldInfo> kvp in XmlAttributeNameIfcNEnmAttributeDict)   s+="  "+kvp.Key+" of type "+kvp.Value.FieldType.Name+"\n";
s+="XmlElementNameIfcEntityAttributeDict:\n";  foreach(KeyValuePair<string,FieldInfo> kvp in XmlElementNameIfcEntityAttributeDict)   s+="  "+kvp.Key+" of type "+kvp.Value.FieldType.Name+"\n";
s+="XmlElementNameIfcSelectAttributeDict:\n";  foreach(KeyValuePair<string,FieldInfo> kvp in XmlElementNameIfcSelectAttributeDict)   s+="  "+kvp.Key+" of type "+kvp.Value.FieldType.Name+"\n";
s+="XmlElementNameIfcListAttributeDict:\n";    foreach(KeyValuePair<string,FieldInfo> kvp in XmlElementNameIfcListAttributeDict)     s+="  "+kvp.Key+" of type "+kvp.Value.FieldType.Name+"\n";
s+="XmlElementNameIfcInverseEntityDict:\n";    foreach(KeyValuePair<string,FieldInfo> kvp in XmlElementNameIfcInverseEntityDict)     s+="  "+kvp.Key+" of type "+kvp.Value.FieldType.Name+"\n";
s+="XmlElementNameIfcInverseEntityListDict:\n";foreach(KeyValuePair<string,FieldInfo> kvp in XmlElementNameIfcInverseEntityListDict) s+="  "+kvp.Key+" of type "+kvp.Value.FieldType.Name+"\n";
return s;
}//..........................
#endif
        }



private static Dictionary<Type,EntityXmlTypeInfo> EntityXmlAttributeNameIfcAttributeDict=new Dictionary<Type,EntityXmlTypeInfo>();
public                         EntityXmlTypeInfo  XmlTypeInfo=null;

public void AssignXmlFields(){XmlTypeInfo=AssignXmlFieldsToEntityType(this.GetType());}
private static EntityXmlTypeInfo AssignXmlFieldsToEntityType(Type t)
{
EntityXmlTypeInfo  XmlTypeInfo=null;
if (!EntityXmlAttributeNameIfcAttributeDict.ContainsKey(t))
   { XmlTypeInfo=new EntityXmlTypeInfo();
     EntityXmlAttributeNameIfcAttributeDict.Add(t,XmlTypeInfo);
    foreach (FieldInfo field in t.GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) 
      foreach (Attribute attr in field.GetCustomAttributes(true)) // TYPE, ENUM, NULLABLE ENUM
        {string FieldName=field.Name;if (FieldName.StartsWith("_")) FieldName=FieldName.Substring(1);
         if (attr is ifcAttribute       ) {if (field.FieldType.IsSubclassOf(typeof(TypeBase)))                             XmlTypeInfo.XmlAttributeNameIfcTypeAttributeDict.Add(FieldName,field); 
                                      else if (field.FieldType.IsSubclassOf(typeof(Enum)))                                 XmlTypeInfo.XmlAttributeNameIfcEnumAttributeDict.Add(FieldName,field); 
                                      else if ( (Nullable.GetUnderlyingType(field.FieldType)!=null) 
                                             && (Nullable.GetUnderlyingType(field.FieldType).IsSubclassOf(typeof(Enum))) ) XmlTypeInfo.XmlAttributeNameIfcNEnmAttributeDict.Add(FieldName,field); 
                                      else if (field.FieldType.IsSubclassOf(typeof(SELECT)))                               XmlTypeInfo.XmlElementNameIfcSelectAttributeDict.Add(FieldName,field); 
                                      else if (field.FieldType.IsSubclassOf(typeof(ENTITY)))                               XmlTypeInfo.XmlElementNameIfcEntityAttributeDict.Add(FieldName,field); 
                                      else if (typeof(ifcListInterface).IsAssignableFrom(field.FieldType))                 XmlTypeInfo.XmlElementNameIfcListAttributeDict  .Add(FieldName,field); 
                                          }
         if (attr is ifcInverseAttribute) {if (typeof(ifcListInterface).IsAssignableFrom(field.FieldType))                 XmlTypeInfo.XmlElementNameIfcInverseEntityListDict.Add(FieldName,field);
                                           else                                                                            XmlTypeInfo.XmlElementNameIfcInverseEntityDict    .Add(FieldName,field);
                                          }
        }
 // ggf. mit vorangesteltem Ifc (nur bei Instanzen!)
   }
else XmlTypeInfo=EntityXmlAttributeNameIfcAttributeDict[t];
return XmlTypeInfo;
}



public static object[] GetFieldCtorArgsFromXml(Type GenericType,XmlNode xml)
{
object[] FieldCtorArgs=new object[xml.ChildNodes.Count];

#if(TRACE)
Console.WriteLine("EVAL LIST: "+GenericType.Name); 
#endif
int ListPos=-1;
foreach (XmlNode n in xml.ChildNodes) 
                  {//.....................................................
                   ListPos++;      
                   string ListElement=n.InnerText;// Console.WriteLine("n.InnerText="+n.InnerText);
                       if (GenericType==typeof(   Int32)) {Console.WriteLine("is Int32 XXXXXXXXXXXXXXX");  FieldCtorArgs[ListPos]=Int32.Parse(n.InnerText);} // tritt das überhaupt auf ?
                  else if (GenericType.IsSubclassOf(typeof(TypeBase))) {Console.WriteLine("is TypeBase");
                                                                        object[] GenericCtorArgs=new object[1];
                                                                                 GenericCtorArgs[0]=Activator.CreateInstance(GenericType); //LengthMeasure or CartesianPoint
                                                                   Type     GenericBaseType=GenericType.BaseType.GetGenericArguments()[0];    //Double from LengthMeasure -> TYPE<double> -> double
                                                                        if (GenericBaseType==typeof(String)) {if (ListElement=="$") GenericCtorArgs[0]=""; else  GenericCtorArgs[0]=ifc.IfcString.Decode(ListElement);}
                                                                   else if (GenericBaseType==typeof(   int)) {                      GenericCtorArgs[0]=int.Parse(ListElement);}
                                                                   else if (GenericBaseType==typeof( Int32)) {                      GenericCtorArgs[0]=Int32.Parse(ListElement);}
                                                                   else if (GenericBaseType==typeof(double)) {                      GenericCtorArgs[0]=double.Parse(ListElement,CultureInfo.InvariantCulture);}

                                                                   FieldCtorArgs[ListPos]=Activator.CreateInstance(GenericType,GenericCtorArgs);
                                                                  }
                   else if (GenericType.IsSubclassOf(typeof(ENTITY)))   {//Console.WriteLine("ListElement is ENTITY, n.Name="+n.Name);
                                                                         try {FieldCtorArgs[ListPos]=EvalEntityNode(Model.CurrentModel,n);}catch(Exception e){throw new Exception("FieldCtorArgs[ListPos]: Field: "+n.Name+": "+e.Message);  } 
                                                                        }


                   else if (GenericType.IsSubclassOf(typeof(SELECT)))   {//Console.WriteLine("is SELECT:"+n.Name);
                                                                         
                                                                               try{ string TypeName=n.Name.Substring(3); if (TypeName.EndsWith("-wrapper")) TypeName=TypeName.Substring(0,TypeName.Length-8);
                                                                                    Type t=Type.GetType("ifc."+TypeName,true,true); // Console.WriteLine("t.Name="+t.Name);
                                                                                   
                                                                                   
                                                                                   object[] CtorArg=new object[1];
                                                                                   if (t.IsSubclassOf(typeof(ENTITY)))       {//Console.WriteLine("is ENTITY"); Console.WriteLine("n.Name="+n.Name);
                                                                                                                              try {CtorArg[0]=EvalEntityNode(Model.CurrentModel,n,n.Name.Substring(3));}catch(Exception e){throw new Exception("GenericCtorArgs[0]: Field: "+n.Name+": "+e.Message);  } 
                                                                                                                            }                     
                                                                                   else  if (t.IsSubclassOf(typeof(TypeBase)))  
                                                                                      {object[] GenericCtorArgs=new object[1];
                                                                                            if (t.IsSubclassOf(typeof(TYPE<string>))) {if (ListElement=="$") GenericCtorArgs[0]=""; else  GenericCtorArgs[0]=ifc.IfcString.Decode(ListElement);}
                                                                                       else if (t.IsSubclassOf(typeof(TYPE<   int>))) {                      GenericCtorArgs[0]=int.Parse(ListElement);}
                                                                                       else if (t.IsSubclassOf(typeof(TYPE< Int32>))) {                      GenericCtorArgs[0]=Int32.Parse(ListElement);}
                                                                                       else if (t.IsSubclassOf(typeof(TYPE<double>))) {                      GenericCtorArgs[0]=double.Parse(ListElement,CultureInfo.InvariantCulture);}
                                                                                       else throw new Exception("FieldCtorArgs[ListPos]: Select-Field-TYPE: "+n.Name+": unknown TYPE "+t.Name);
                                                                                       CtorArg[0]=Activator.CreateInstance(t,GenericCtorArgs);
                                                                                      }
                                                                      /*
                                                                              Console.WriteLine("X: "+GenericCtorArgs[0].GetType().Name);
                                                                              Console.WriteLine("Y: "+t.Name);
                                                                              Console.WriteLine("Z: "+GenericType.Name);
                                                                              Console.WriteLine("A: "+TreeOfParents(n,n.Name));
                                                                              object CtorArg=Activator.CreateInstance(t,GenericCtorArgs);
                                                                        */
                                                                              //Console.WriteLine("B: "+TreeOfParents(n,n.Name));
                                                                              FieldCtorArgs[ListPos]=Activator.CreateInstance(GenericType,CtorArg);     
                                                                              //Console.WriteLine("C: "+TreeOfParents(n,n.Name));                                                            
                                                                                       
                                                                                  }catch(Exception e){throw new Exception("FieldCtorArgs[ListPos]: Select-Field: "+n.Name+": "+e.Message);  } 
                                                                         }

                                                                         
                   else {Console.WriteLine("TODO List++TYPE: Base="+GenericType.Name+" not supportet. in\n"+n.Name); }// andere Typen
                  }//.....................................................
return FieldCtorArgs;
}



private static Dictionary<Type,Dictionary<int,FieldInfo>> EntityStepPosIfcAttributeDict=new Dictionary<Type, Dictionary<int, FieldInfo>>();
private static                 Dictionary<int,FieldInfo>        StepPosIfcAttributeDict=null;

public void AssignFields(){AssignFieldsToEntityType(this.GetType());}
private static void AssignFieldsToEntityType(Type t)
{
if (!EntityStepPosIfcAttributeDict.ContainsKey(t))
   { EntityStepPosIfcAttributeDict.Add(t,new Dictionary<int,FieldInfo>());
    foreach (FieldInfo field in t.GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) 
      foreach (Attribute attr in field.GetCustomAttributes(true)) 
        if (attr is ifcAttribute) EntityStepPosIfcAttributeDict[t].Add(((ifcAttribute)attr).OrdinalPosition,field);
   }
StepPosIfcAttributeDict=EntityStepPosIfcAttributeDict[t];
}





public void EvalTypeString(FieldInfo field, string Value)//------------------------------------------------------------------
{
#if(TRACE)
Console.WriteLine("  EvalTypeString="+field.Name+":"+Value);
#endif
try{field.SetValue(this,ENTITY.Parse2TYPE(Value,field.FieldType));}
 catch(Exception e){throw new Exception("EvalTypeString:"+field.Name+": "+field.FieldType.ToString()+": "+e.Message);}
}//--------------------------------------------------------------------------------------------------------------------

public void EvalEnumString(FieldInfo field, string Value)//------------------------------------------------------------------
{
#if(TRACE)
Console.WriteLine("  EvalEnumString="+field.Name+":"+Value);
#endif
try{object FieldInstance=Activator.CreateInstance(field.FieldType);
            FieldInstance=Enum.Parse(field.FieldType,Value.ToUpper());
     field.SetValue(this,FieldInstance);
    }
 catch(Exception e){throw new Exception("EvalEnumString:"+field.Name+": ("+Value+") "+field.FieldType.ToString()+": "+e.Message);}
}//--------------------------------------------------------------------------------------------------------------------

public void EvalNEnmString(FieldInfo field, string Value)//------------------------------------------------------------------
{
#if(TRACE)
Console.WriteLine("  EvalNEnmString="+field.Name+":"+Value);
#endif
try{object FieldInstance=Activator.CreateInstance(field.FieldType);
            FieldInstance=Enum.Parse(Nullable.GetUnderlyingType(field.FieldType),Value.ToUpper());
     field.SetValue(this,FieldInstance);
    }
 catch(Exception e){throw new Exception("EvalNEnmString:"+field.Name+": ("+Value+") "+field.FieldType.ToString()+": "+e.Message);}
}//--------------------------------------------------------------------------------------------------------------------


public class EntityField{public ENTITY e;public FieldInfo f;public EntityField(ENTITY e,FieldInfo f){this.e=e;this.f=f;}}
public static List<EntityField> UnassignedEntityFieldList=new List<EntityField>();

public void EvalEntityNode(FieldInfo field, XmlNode n)//------------------------------------------------------------------
{
#if(TRACE)
Console.WriteLine("  EvalEntityNode="+field.Name+":"+n.Name+" typeof "+field.FieldType.Name);
#endif
     if ( ((XmlElement)n).HasAttribute("href") ) {ENTITY e=(ENTITY)Activator.CreateInstance(field.FieldType);e.XmlRefId=n.Attributes["href"].Value;field.SetValue(this,e);UnassignedEntityFieldList.Add(new EntityField(this,field));} 
else if ( ((XmlElement)n).HasAttribute( "ref") ) {ENTITY e=(ENTITY)Activator.CreateInstance(field.FieldType);e.XmlRefId=n.Attributes[ "ref"].Value;field.SetValue(this,e);UnassignedEntityFieldList.Add(new EntityField(this,field));} 
else try {
          string EntityTypeName=( ((XmlElement)n).HasAttribute("xsi:type") )?n.Attributes["xsi:type"].Value.Substring(3) : field.FieldType.Name; 
//Console.WriteLine(EntityTypeName);
          field.SetValue(this,EvalEntityNode(Model.CurrentModel,n,EntityTypeName));
         }catch(Exception e){throw new Exception("EvalEntityNode:"+field.FieldType.ToString()+": "+e.Message);  } 
 

}//--------------------------------------------------------------------------------------------------------------------

public void EvalSelectNode(FieldInfo field, XmlNode n)//------------------------------------------------------------------
{
//EvalEntityNodeName();
#if(TRACE)

//Console.ReadLine(); // Typ von n.Name ermiiiteln => field.FieldType 
#endif
foreach (XmlNode n2 in n.ChildNodes) 
        {//Console.WriteLine("               "+n2.Name);  
         try{object[] TypeCtorArgs=new object[1]; 
          string TypeName=n2.Name.Substring(3); if (TypeName.EndsWith("-wrapper")) TypeName=TypeName.Substring(0,TypeName.Length-8);
             Type t=Type.GetType(typeName:"ifc."+TypeName,throwOnError:true,ignoreCase:true);// 2. true: ignoreCase   
                  if (t.IsSubclassOf(typeof(ENTITY))) {TypeCtorArgs[0]=EvalEntityNode(Model.CurrentModel, n2); } 
             else if (t.IsSubclassOf(typeof(TypeBase))) {TypeCtorArgs[0]=ENTITY.Parse2TYPE(n2.InnerText,t);} 
             //else if (t.IsSubclassOf(typeof(SELECT))) {TypeCtorArgs[0]=ENTITY.EvalSelectNode(n2); }  noch Fallunterscheidungen vgl. [ifcSQL].[ifcSchemaTool].[SelectTypesAndSelectedTypeGroupsWithDifferentSelectedTypes]
             else {Console.WriteLine("no TypeInstance createtd for SELECTed-Type="+"ifc."+n2.Name.Substring(3));}

                      field.SetValue(this,Activator.CreateInstance(field.FieldType,TypeCtorArgs));
            }catch(Exception e){throw new Exception("EvalSelectNode:"+field.FieldType.ToString()+": "+e.Message);} //   Console.WriteLine("SELECT="+n.Name+" of "+xml.Name);Console.ReadLine();                                              
        }
}//--------------------------------------------------------------------------------------------------------------------

public void EvalListNode(FieldInfo field, XmlNode n)//------------------------------------------------------------------
{
#if(TRACE)
Console.WriteLine("  LIST="+field.Name+":"+n.Name);//Console.ReadLine(); // Typ von n.Name ermiiiteln => field.FieldType 
#endif
Type GenericType=ENTITY.GetGenericType(field.FieldType); //Console.WriteLine("xml.Name="+xml.Name); //Console.WriteLine("n.Name="+n.Name+" fieldTypeName="+field.FieldType.Name);
try {//Console.WriteLine("EvalListNode:"+field.FieldType.Name+" # "+n.Name+" GenericType="+GenericType.Name);
        object[] o=GetFieldCtorArgsFromXml(GenericType,n); 
//     Console.WriteLine("XXXXXXXX:"+o.Length+" for ListType "+field.FieldType.Name);
//     if (o.Length>0) Console.WriteLine("YYYYYYYYY:"+o[0].ToString()+":"+o[0].GetType().Name);
     field.SetValue(this,Activator.CreateInstance(field.FieldType,o));//ifc.TableColumn tc=new TableColumn(); //tc.ReferencePath.InnerReference.ListPositions.li
}catch(Exception e){throw new Exception("EvalListNode:"+field.FieldType.ToString()+": "+e.Message); }
}//--------------------------------------------------------------------------------------------------------------------

public void EvalInverseEntityNode(FieldInfo field, XmlNode n)//------------------------------------------------------------------
{
#if(TRACE)
Console.WriteLine("  EvalInverseEntityNode="+field.Name+":"+n.Name);//Console.ReadLine(); // Typ von n.Name ermiiiteln => field.FieldType 
#endif
//EvalInverseEntityNode
}//--------------------------------------------------------------------------------------------------------------------

public void EvalInverseEntityListNode(FieldInfo field, XmlNode n)//------------------------------------------------------------------
{ 
#if(TRACE)
Console.WriteLine("  EvalInverseEntityListNode="+field.Name+":"+n.Name);//Console.ReadLine(); // Typ von n.Name ermiiiteln => field.FieldType 
#endif
//ifc.Project p=new Project();
//ifc.RelContainedInSpatialStructure r=new RelContainedInSpatialStructure();r.RelatedElements
Type GenericType=GetGenericType(field.FieldType); //Console.WriteLine("xml.Name="+xml.Name); //Console.WriteLine("n.Name="+n.Name+" fieldTypeName="+field.FieldType.Name);
try {//Console.WriteLine("field.FieldType:"+field.FieldType.Name+" GenericType.Name:"+GenericType.Name);
//ifc.Building b=new Building(); //b.ContainsElements
        object[] o=GetFieldCtorArgsFromXml(GenericType,n); 
     //Console.WriteLine("XXXXXXXX:"+o.Length+" for ListType "+field.FieldType.Name);// ifc.BuildingElementProxy  //Set1toUnbounded_Product pp=new Set1toUnbounded_Product()
     //if (o.Length>0) Console.WriteLine("YYYYYYYYY:"+o[0].ToString()+":"+o[0].GetType().Name);
     //Console.WriteLine(this.XmlTypeInfo.ToString());
     field.SetValue(this,Activator.CreateInstance(field.FieldType,o));} 
catch(Exception e){throw new Exception("EvalInverseEntityListNode:"+field.FieldType.ToString()+": "+e.Message); }
}//--------------------------------------------------------------------------------------------------------------------

public static string TreeOfParents(XmlNode xml, string s)
{
s= xml.ParentNode.Name+"."+s;
if (xml.ParentNode.ParentNode.ParentNode!=null) return TreeOfParents(xml.ParentNode,s); else return s;
}



public static Dictionary<string,ENTITY> XmlIdDict=new Dictionary<string, ENTITY>();

public static ENTITY EvalEntityNode(Model CurrentModel,XmlNode xml,string TypeName=null)
{



ENTITY CurrentEntity=null; 
//Console.WriteLine("CCC");
try{
//ifc.Axis2Placement
//ifc.RelDeclares
//ifc.ConversionBasedUnit u=new ConversionBasedUnit();u.ConversionFactor.ValueComponent=new Value()
#if(TRACE)
Console.WriteLine("ENTITY= "+xml.Name+": TypeName="+((TypeName!=null)?TypeName:xml.Name.Substring(3)));
#endif

//if (TypeName==null) if ( ((XmlElement)xml).HasAttribute("xsi:type") ) Console.WriteLine("xml.Attributes[\"xsi:type\"].Value="+xml.Attributes["xsi:type"].Value);
//if (TypeName!=null) if (xml.Name=="Bound") if ( ((XmlElement)xml).HasAttribute("xsi:type") ) Console.WriteLine("xml.Attributes[\"xsi:type\"].Value="+xml.Attributes["xsi:type"].Value+"#"+TypeName);


if (TypeName==null) TypeName=( ((XmlElement)xml).HasAttribute("xsi:type") )?xml.Attributes["xsi:type"].Value.Substring(3) : xml.Name.Substring(3);

//if (TypeName=="SIUnit") Console.WriteLine("A");

Type t=Type.GetType(typeName:"ifc."+TypeName,throwOnError:true,ignoreCase:true);// 2. true: ignoreCase

//if (TypeName=="SIUnit") Console.WriteLine("B");

CurrentEntity=(ENTITY)Activator.CreateInstance(t);CurrentEntity.AddNext(); // in default-Ctor nicht enthalten

//if (TypeName=="SIUnit") Console.WriteLine("C");



if (xml.Attributes["id"]!=null) {CurrentEntity.XmlId=xml.Attributes["id"].Value;if (!XmlIdDict.ContainsKey(CurrentEntity.XmlId)) XmlIdDict.Add(CurrentEntity.XmlId,CurrentEntity); 
                                 else throw new Exception("double id="+xml.Attributes["id"].Value+" in Xml");
//                                 Model.log.WriteLine(TreeOfParents(xml,xml.Name)+" (id="+xml.Attributes["id"].Value+")"); // ifc.ConversionBasedUnitWithOffset
                                }
//else                             Model.log.WriteLine(TreeOfParents(xml,xml.Name)); 


if (CurrentEntity is Direction)     {if (      ((XmlElement)xml).HasAttribute("DirectionRatios") ) 
                                        {string[] ListElements=xml.Attributes["DirectionRatios"].Value.Split(' ');
                                                    ((Direction)CurrentEntity).DirectionRatios=new List2to3_Real(); foreach (string Element in ListElements) 
                                                    ((Direction)CurrentEntity).DirectionRatios.Add(new Real(double.Parse(Element)));
                                         return CurrentEntity;
                                        }
                                    } // andernfalls Ref-Element unter Entity auswerten
if (CurrentEntity is CartesianPoint){if (       ((XmlElement)xml).HasAttribute("Coordinates") ) 
                                         {string[] ListElements=xml.Attributes["Coordinates"].Value.Split(' ');
                                                ((CartesianPoint)CurrentEntity).Coordinates=new List1to3_LengthMeasure();foreach (string Element in ListElements) 
                                                ((CartesianPoint)CurrentEntity).Coordinates.Add(new LengthMeasure(double.Parse(Element)));
                                          return CurrentEntity;
                                         }
                                    }// andernfalls Ref-Element unter Entity auswerten

CurrentEntity.AssignXmlFields(); //if (CurrentEntity is ifc.Project) Console.WriteLine(CurrentEntity.XmlTypeInfo.ToString());

foreach (XmlAttribute a in xml.Attributes) 
        {if (CurrentEntity.XmlTypeInfo.XmlAttributeNameIfcTypeAttributeDict.ContainsKey(a.Name)) CurrentEntity.EvalTypeString(CurrentEntity.XmlTypeInfo.XmlAttributeNameIfcTypeAttributeDict[a.Name],a.Value);
    else if (CurrentEntity.XmlTypeInfo.XmlAttributeNameIfcEnumAttributeDict.ContainsKey(a.Name)) CurrentEntity.EvalEnumString(CurrentEntity.XmlTypeInfo.XmlAttributeNameIfcEnumAttributeDict[a.Name],a.Value);
    else if (CurrentEntity.XmlTypeInfo.XmlAttributeNameIfcNEnmAttributeDict.ContainsKey(a.Name)) CurrentEntity.EvalNEnmString(CurrentEntity.XmlTypeInfo.XmlAttributeNameIfcNEnmAttributeDict[a.Name],a.Value);
    else {if ( (a.Name!="id") && (a.Name!="href") && (a.Name!="href") && (a.Name!="xsi:nil") && (a.Name!="xsi:type") ) Console.WriteLine("unknown attribute="+a.Name+" of element "+xml.Name); } 
        }
foreach (XmlNode n in xml.ChildNodes)
        {
         if (CurrentEntity.XmlTypeInfo.  XmlElementNameIfcEntityAttributeDict.ContainsKey(n.Name)){CurrentEntity.EvalEntityNode           (CurrentEntity.XmlTypeInfo.  XmlElementNameIfcEntityAttributeDict[n.Name],n);}
    else if (CurrentEntity.XmlTypeInfo.  XmlElementNameIfcSelectAttributeDict.ContainsKey(n.Name)){CurrentEntity.EvalSelectNode           (CurrentEntity.XmlTypeInfo.  XmlElementNameIfcSelectAttributeDict[n.Name],n);}
    else if (CurrentEntity.XmlTypeInfo.    XmlElementNameIfcListAttributeDict.ContainsKey(n.Name)){CurrentEntity.EvalListNode             (CurrentEntity.XmlTypeInfo.    XmlElementNameIfcListAttributeDict[n.Name],n);}
    else if (CurrentEntity.XmlTypeInfo.    XmlElementNameIfcInverseEntityDict.ContainsKey(n.Name)){CurrentEntity.EvalInverseEntityNode    (CurrentEntity.XmlTypeInfo.    XmlElementNameIfcInverseEntityDict[n.Name],n);}
    else if (CurrentEntity.XmlTypeInfo.XmlElementNameIfcInverseEntityListDict.ContainsKey(n.Name)){CurrentEntity.EvalInverseEntityListNode(CurrentEntity.XmlTypeInfo.XmlElementNameIfcInverseEntityListDict[n.Name],n);}
   else if (n.Name=="#comment"){CurrentEntity.EndOfLineComment=n.InnerText;}
    else {//try{ } 
          Console.WriteLine("unknown childNode="+n.Name+" of parent "+xml.Name+" of type "+TypeName); 
          Console.WriteLine(CurrentEntity.XmlTypeInfo.ToString());
         } // ifc.ConversionBasedUnit //ifc.GeometricRepresentationContext
        }

//Console.WriteLine("C");

}catch(Exception e){Console.WriteLine ("ERROR on EvalEntityNode:"+e.Message);Console.WriteLine (xml.Name);}//Console.ReadLine();}

return CurrentEntity;
}




//public static void ClearEntityList(){EntityList.Clear();Header.Clear();}


//public static Dictionary<int,ENTITY> EntityDict=new Dictionary<int,ENTITY>();




}//ENTITY=====================================================================================================================

public partial class Model{//==========================================================================================


public void EvalHeaderNode(XmlNode xml)
{
foreach (XmlNode n in xml)
  switch (n.Name) {case "name": Header.name=n.InnerText;break;
                   case "time_stamp": Header.time_stamp=n.InnerText;break;
                   case "author": Header.author=n.InnerText;break;
                   case "organization": Header.organization=n.InnerText;break;
                   case "preprocessor_version": Header.preprocessor_version=n.InnerText;break;
                   case "originating_system": Header.originating_system=n.InnerText;break;
                   case "authorization": Header.authorization=n.InnerText;break;
                   case "documentation": Header.documentation=n.InnerText;break;          
                   default:break;  
                  }

}

public void EvalXmlNode(XmlNode xml)
{
//Model.log.WriteLine("EvalXmlNode="+xml.Name);
//Console.WriteLine("BBB: "+xml.Name);
if (xml.Name.StartsWith("Ifc")) if (!xml.Name.EndsWith("-wrapper")) ENTITY.EvalEntityNode(this,xml);
if (xml.Name=="header") EvalHeaderNode(xml);
//foreach (XmlNode n in xml.ChildNodes) EvalXmlNode(n);
//EntityList.Add((ENTITY)CurrentEntity); //Console.WriteLine(((ENTITY)CurrentEntity).ToIfc());
}



//public static StreamWriter log=new StreamWriter("ifc_in_ifcXml.log");

public static Model CurrentModel;

public static Model FromXmlFile(string FileName)
{

CurrentModel=new ifc.Model(FileName.Replace(".ifcXml",""));
StreamReader sr=new StreamReader(FileName);

XmlDocument doc = new XmlDocument (); 
			doc.Load (FileName);  //log.WriteLine("root EvalXmlNode");
//foreach (XmlNode xml in doc.ChildNodes) foreach (XmlNode n in xml.ChildNodes)  log.WriteLine("root EvalXmlNode="+xml.Name+"."+n.Name);
foreach (XmlNode xml in doc.ChildNodes) foreach (XmlNode n in xml.ChildNodes)  CurrentModel.EvalXmlNode(n);

// Alles Entity-Attribute duchlaufen
foreach (ENTITY.EntityField ef in ENTITY.UnassignedEntityFieldList) ef.f.SetValue(ef.e,ENTITY.XmlIdDict[ ((ENTITY)ef.f.GetValue(ef.e)).XmlRefId]);
//foreach (ENTITY e in EntityList) if (e.XmlRefId!=null) try{e=ENTITY.XmlIdDict[n.Attributes["href"].Value];}catch(Exception){Console.WriteLine("e.XmlRefId="+e.XmlRefId+" nicht gefunden.");}  // bei Baumstrukturauswertung noch nicht definiert

//log.Close();
CurrentModel.AssignEntities();
return CurrentModel;
}// of FromStepFile
}// of Model ==========================================================================================================

}// ifc ==============================



