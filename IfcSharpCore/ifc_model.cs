
// ifc_model.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace ifc{//==============================

public partial class Repository{//==========================================================================================

public static ifc.Model CurrentModel=new ifc.Model();

}//========================================================================================================


    public partial class Model
    {//==========================================================================================
        public Model() { }
        public Model(string Name) { this.Header.Name = Name; }
        public Model(string Name, string ViewDefinition, string Author, string Organization, string OriginatingSystem, string Documentation) {
            this.Header.Name = Name;
            this.Header.ViewDefinition = ViewDefinition;
            this.Header.Author = Author;
            this.Header.Organization = Organization;
            this.Header.OriginatingSystem = OriginatingSystem;
            this.Header.Documentation = Documentation;
        }

        public int NextGlobalId = 1;
        public int NextGlobalCommentId = 0;
        public ifc.HeaderData Header = new ifc.HeaderData();
        public List<ENTITY> EntityList = new List<ENTITY>();

        public void ClearEntityList() { EntityList.Clear(); Header.Reset(); }
        public Dictionary<int, ENTITY> EntityDict = new Dictionary<int, ENTITY>();

        // 2022-04-04 (ef): formatting
        public void AssignEntities() {
            EntityDict.Clear();
            foreach (ENTITY e in EntityList) /* if (e.LocalId>0) */ if (!EntityDict.ContainsKey(e.LocalId)) { EntityDict.Add(e.LocalId, e); } else Log.Add("AssignEntities: #="+e.LocalId+" already exist! (double Entry)", Log.Level.Exception);
            foreach (ENTITY e in EntityList) /* if (e.LocalId>0) */
                    {//####################################################################################################
                Dictionary<int, FieldInfo> VarDict = new Dictionary<int, FieldInfo>();
                int VarCount = 0; foreach (FieldInfo field in e.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)) foreach (Attribute attr in field.GetCustomAttributes(true)) if (attr is ifcAttribute) { VarDict.Add(((ifcAttribute)attr).OrdinalPosition, field); VarCount++; }
                for (int i = 1; i <= VarCount; i++) {//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    FieldInfo field = VarDict[i];

                    if (field.FieldType.IsSubclassOf(typeof(ENTITY))) {
                        ENTITY E = (ENTITY)field.GetValue(e);
                        if (E != null) {
                            if (E.LocalId > 0) if (EntityDict.ContainsKey(E.LocalId)) field.SetValue(e, EntityDict[E.LocalId]); /* E=EntityDict[E.Id];*/
                                else Console.WriteLine("E.Id=" + E.LocalId + " not found");
                        }
                    }
                    else if (field.FieldType.IsSubclassOf(typeof(SELECT))) {
                        SELECT S = (SELECT)field.GetValue(e);
                        if (S != null) {//...........................................
                            if (S.Id > 0 && EntityDict.ContainsKey(S.Id)) S.SetValueAndType(EntityDict[S.Id], EntityDict[S.Id].GetType());
                            else if (!S.IsNull) {
                                ENTITY E = null; if (S != null) if (S.SelectType().IsSubclassOf(typeof(ENTITY))) E = (ENTITY)S.SelectValue();
                                if (E != null) if (E.LocalId > 0 && EntityDict.ContainsKey(E.LocalId)) S.SetValueAndType(EntityDict[E.LocalId], EntityDict[E.LocalId].GetType());
                            }
                        }//...........................................

                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(field.FieldType)) if (field.GetValue(e) != null) {//==================================================================

                            Dictionary<int, object> VarDict1 = new Dictionary<int, object>();
                            int VarCount1 = 0; foreach (object item in (IEnumerable)field.GetValue(e)) if (item != null) VarDict1.Add(VarCount1++, item);
                            object[] FieldCtorArgs = new object[VarCount1];
                            Type GenericType = null;
                            if (field.FieldType.BaseType.GetGenericArguments().Length > 0) GenericType = field.FieldType.BaseType.GetGenericArguments()[0]; //LengthMeasure or CartesianPoint
                            else GenericType = field.FieldType.BaseType.BaseType.GetGenericArguments()[0]; //CompoundPlaneAngleMeasure


                            if ((GenericType != null) && (   GenericType.IsSubclassOf(typeof(ENTITY))
                                                          || GenericType.IsSubclassOf(typeof(SELECT))
                                                          || typeof(IEnumerable).IsAssignableFrom(GenericType) // 2024-05-01 (bb) added list of lists
                                                         )
                               ) {//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                               for (int i1 = 0; i1 < VarCount1; i1++) {//------------------------------------------------------
                                    object item = VarDict1[i1];
                                    if (typeof(IEnumerable).IsAssignableFrom(item.GetType())) { // 2024-05-01 (bb) added list of lists
                                        Dictionary<int, object> VarDict2 = new Dictionary<int, object>();
                                        int VarCount2 = 0; foreach (object item2 in (IEnumerable)item) if (item2 != null) VarDict2.Add(VarCount2++, item2);
                                        object[] FieldCtorArgs2 = new object[VarCount2];
                                        if (VarCount2>0) if (VarDict2[0] is ENTITY)  // 2024-05-05 (bb) checking entity-type
                                          {for (int i2 = 0; i2 < VarCount2; i2++)
                                                   {object item2 = VarDict2[i2];
                                                    ENTITY E = (ENTITY)item2; 
                                                    if (E != null) if (E.LocalId > 0) {//........................
                                                    if (EntityDict.ContainsKey(E.LocalId)) E = EntityDict[E.LocalId]; else Log.Add("List Level 2: E.Id="+e.LocalId+" not found", Log.Level.Error);
                                                                                      }
                                                    FieldCtorArgs2[i2] = E;
                                                   }
                                          FieldCtorArgs[i1] = Activator.CreateInstance(item.GetType(), FieldCtorArgs2);
                                         }

                                        if (VarCount2>0) if (VarDict2[0] is TYPE<double>)  // 2024-05-05 (bb) checking base-type
                                          {for (int i2 = 0; i2 < VarCount2; i2++) FieldCtorArgs2[i2] = VarDict2[i2];
                                          FieldCtorArgs[i1] = Activator.CreateInstance(item.GetType(), FieldCtorArgs2);
                                          }

                                        if (VarCount2>0) if (VarDict2[0] is TYPE<int>)  // 2024-05-17 (bb) checking base-type
                                          {for (int i2 = 0; i2 < VarCount2; i2++) FieldCtorArgs2[i2] = VarDict2[i2];
                                          FieldCtorArgs[i1] = Activator.CreateInstance(item.GetType(), FieldCtorArgs2);
                                          }


                                       }
                                    else if (item is SELECT) {//Console.WriteLine("SELECT item "+((SELECT)item).Id +" "+((SELECT)item).SelectType().Name);
                                        SELECT s = item as SELECT;
                                        if (s.Id == 0) {
                                            if (s.SelectType().IsSubclassOf(typeof(ENTITY))) {
                                                s.Id = ((ENTITY)s.SelectValue()).LocalId;
                                            }
                                            else {
                                                // 2022-04-04 (ef): added 'else' case
                                                // sometimes the SELECT can be define 'inline' (e.g.: IFCINDEXEDPOLYCURVE(#200,(IFCLINEINDEX((1,2)),IFCARCINDEX((2,3,4))...)
                                                // here we can only set the select directly, as there is no id for its instance
                                                FieldCtorArgs[i1] = s;
                                            }
                                        }
                                        if (s.Id > 0) {//SELECT s=new SELECT(); /*((SELECT)item)*/ 
                                            s.SetValueAndType(EntityDict[s.Id], EntityDict[s.Id].GetType());
                                            FieldCtorArgs[i1] = s;// Console.WriteLine(GenericType.Name+": ");
                                        }
                                    }
                                    else if (item is ENTITY) {//===================
                                        if (((ENTITY)item).LocalId > 0) {
                                            ENTITY E = (ENTITY)item; // Console.WriteLine("((ENTITY)item).Id="+((ENTITY)item).Id );
                                            if (E != null) if (E.LocalId > 0) {//........................
                                                    if (EntityDict.ContainsKey(E.LocalId)) E = EntityDict[E.LocalId]; else Log.Add("List Level 1: E.Id="+e.LocalId+" not found", Log.Level.Error);
                                                }
                                            FieldCtorArgs[i1] = E;
                                        }//........................
                                    }//===================
                                }//---------------------------------------------------
                                field.SetValue(e, Activator.CreateInstance(field.FieldType, FieldCtorArgs)); // ERROR !!

                            }//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                             //       Console.WriteLine("end list");
                        }//==============================================================
                }//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++// of foreach field
            }//#################################################################################################### //of foreach Entity
        }//of void

    }//========================================================================================================








}// ifc=======================================