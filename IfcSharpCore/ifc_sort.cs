// ifc_sort.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using NetSystem=System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace ifc{//==============================

public partial class ENTITY{//==========================================================================================
public bool IsAssigned=false; // true=alle Entity-Verweise wurden vorher definiert
public int  SortPos=0;
}// of ENTITY =========================================================================================================


public partial class Model{//==========================================================================================


private static Dictionary<int,ENTITY> AssignedEntityDict=new Dictionary<int,ENTITY>();
private static int GlobalSortPos=0;

public int SetAssignedEntityForSort()
{
int cnt=0;
foreach (ENTITY e in EntityList) if (!e.IsAssigned)
        {e.IsAssigned=true;
          //if (e.Id== 103)  Console.WriteLine(e.ToIfc());
         foreach (FieldInfo field in e.GetType().GetFields(BindingFlags.Public|BindingFlags.Instance|BindingFlags.FlattenHierarchy)) 
foreach (NetSystem.Attribute attr in field.GetCustomAttributes(true)) if (attr is ifcAttribute) // nur IFC-Atribute
                 {if (field.FieldType.IsSubclassOf(typeof(ENTITY))) 
                          {ENTITY E=(ENTITY)field.GetValue(e);
                          // if (E!=null) if (E.Id>0) Console.WriteLine(field.Name+"="+E.Id+" "+E.IsAssigned+" "+AssignedEntityDict.ContainsKey(E.Id));
                           if (E!=null) if (E.LocalId>0) if (!AssignedEntityDict.ContainsKey(E.LocalId)) e.IsAssigned=false; 
                         //  if (e.Id==1) Console.WriteLine(field.Name+" "+e.IsAssigned);
                          }
                  else if (field.FieldType.IsSubclassOf(typeof(SELECT))) 
                          {SELECT E=(SELECT)field.GetValue(e);
                           if (E!=null) if (E.Id>0) if (!AssignedEntityDict.ContainsKey(E.Id)) e.IsAssigned=false; 
                          }
                  else if( typeof(IEnumerable).IsAssignableFrom(field.FieldType)) if (field.GetValue(e)!=null) 
                            foreach (object item in (IEnumerable)field.GetValue(e)) if (item!=null)
                             { if (item is SELECT) if (((SELECT)item).Id>0) if (!AssignedEntityDict.ContainsKey(((SELECT)item).Id)) e.IsAssigned=false; 
                               if (item is ENTITY) if (((ENTITY)item).LocalId>0) if (!AssignedEntityDict.ContainsKey(((ENTITY)item).LocalId)) e.IsAssigned=false; 
                             }
                 }// of foreach field
       //   if (e.Id==1) 
      //   Console.WriteLine(e.Id+" IsAssigned="+e.IsAssigned);
          if (e.IsAssigned) /* if (!(e is ifc.EntityComment)) */ {e.SortPos=++GlobalSortPos;try{AssignedEntityDict.Add(e.LocalId,e);}catch(NetSystem.Exception ex){throw new NetSystem.Exception(ex.Message+e.ToStepLine());};cnt++;}//Console.WriteLine(cnt+": "+e.Id);}
        }//of foreach Entity
//Console.WriteLine("----------------------");
return cnt;
}


public void SortEntities()
{
AssignedEntityDict.Clear();
foreach (ENTITY e in EntityList) e.IsAssigned=false;
GlobalSortPos=0;
int cnt=0; do{cnt=SetAssignedEntityForSort();} while (cnt>0);
bool FirstDisplay=true;
foreach (ENTITY e in EntityList) if (!e.IsAssigned) if (FirstDisplay)  {NetSystem.Console.WriteLine("ifc.Model.SortEntities: NOT ASSIGNED: "+e.ToStepLine());FirstDisplay=false;};
//here better an exception
}//of void


}//=====================================================================================================================



}//ifc==============================
