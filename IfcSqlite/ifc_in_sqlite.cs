// ifc_in_sqlite.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

//#define _DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using NetSystem = System;
using System.IO;

namespace ifc
{
    public partial class Model
    {
        public static Model FromSqliteFile(string fullPath)
        {
            SQLiteDatabase database = new SQLiteDatabase();
            Model CurrentModel = new ifc.Model(fullPath.Replace(".sqlite", ""));
            DataSet dataSet = database.GetContentAsDataSet(fullPath);
#if _DEBUG
            Console.WriteLine(string.Format("Reading SQLite-File: {0}", NetSystem.IO.Path.GetFileName(fullPath)));
            Console.WriteLine("======================================================");
#endif
            foreach (DataTable dt in dataSet.Tables)
            {
#if _DEBUG
                Console.WriteLine("______________________________________________________");
                Console.WriteLine(dt.TableName);
                foreach (DataColumn c in dt.Columns) Console.Write(string.Format("{0} ", c.ColumnName));
                Console.Write("\r\n");
#endif
                foreach (DataRow row in dt.Rows)
                {
                    Type entityType = Type.GetType("ifc." + dt.TableName);
                    ENTITY entityInstance;
                    if (entityType == typeof(EntityComment))
                    {
                        EntityComment ec = new EntityComment((string)row["Comment"], (int)row["PreviousEntity"]);
                        entityInstance = ec;
                    }
                    else
                    {
                        object[] ctorArgs = GetEntityConstructorArgs(entityType, row);
                        entityInstance = Activator.CreateInstance(entityType, ctorArgs) as ENTITY;
                    }

                    if (row["Id"] != null) entityInstance.Id = (int)row["Id"];
                    CurrentModel.EntityList.Add(entityInstance);
#if _DEBUG
                    foreach (DataColumn c in dt.Columns) Console.Write(string.Format("{0} ", row[c] is DBNull ? "NULL" : row[c].ToString()));
                    Console.Write("\r\n");
#endif
                }
            }
            Console.WriteLine("======================================================");

            // before we assign the entities, we need to order the list according to the Ids
            CurrentModel.EntityList = CurrentModel.EntityList.OrderBy(e => e.Id).ToList();

            // then we change the position of all EntityComment´s to match the actual order,
            // since they are being read sequentially
            foreach (EntityComment ec in CurrentModel.EntityList.FindAll(e => e.GetType() == typeof(EntityComment)))
            {
                int oldIndex = CurrentModel.EntityList.FindIndex(e => e.Id == ec.Id);
                int newIndex = CurrentModel.EntityList.FindIndex(e => e.Id == ec.PreviousEntityId) + 1;
                var item = CurrentModel.EntityList[oldIndex];
                CurrentModel.EntityList.RemoveAt(oldIndex);

                if (newIndex > oldIndex) newIndex--; // the actual index could have shifted due to the removal
                CurrentModel.EntityList.Insert(newIndex, item);
            }

            CurrentModel.AssignEntities();
            return CurrentModel;
        }

        private static object[] GetEntityConstructorArgs(Type entityType, DataRow dataRow)
        {
            Dictionary<int, object> fieldDict = new Dictionary<int, object>();
            foreach (FieldInfo field in entityType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                foreach (Attribute attr in field.GetCustomAttributes(true))
                {
                    if (attr is ifcAttribute ifcAttribute)
                    {
                        object o = null;
                        string fieldName = field.Name.Replace("_", "");
                        if (fieldName == "Coordinates")
                        {
                            List<LengthMeasure> coords = new List<LengthMeasure>();
                            if (dataRow["X"] != DBNull.Value) coords.Add(new LengthMeasure((double)dataRow["X"]));
                            if (dataRow["Y"] != DBNull.Value) coords.Add(new LengthMeasure((double)dataRow["Y"]));
                            if (dataRow["Z"] != DBNull.Value) coords.Add(new LengthMeasure((double)dataRow["Z"]));
                            o = new List1to3_LengthMeasure(coords.ToArray());
                        }
                        else if (fieldName == "DirectionRatios")
                        {
                            List<Real> coords = new List<Real>();
                            if (dataRow["X"] != DBNull.Value) coords.Add(new Real((double)dataRow["X"]));
                            if (dataRow["Y"] != DBNull.Value) coords.Add(new Real((double)dataRow["Y"]));
                            if (dataRow["Z"] != DBNull.Value) coords.Add(new Real((double)dataRow["Z"]));
                            o = new List2to3_Real(coords.ToArray());
                        }
                        else
                        {
                            object value = dataRow[fieldName];
                            o = GetFieldInstance(field, value);
                        }
                        fieldDict.Add(ifcAttribute.OrdinalPosition, o);
                    }
                }
            }

            fieldDict.Add(fieldDict.Count() + 1, dataRow["EndOfLineComment"].GetType() == typeof(DBNull) ? null : dataRow["EndOfLineComment"]);

            List<object> args = new List<object>();
            foreach (var o in fieldDict.OrderBy(i => i.Key))
                args.Add(o.Value);

            return args.ToArray();
        }
        private static object GetFieldInstance(FieldInfo field, object value)
        {
            Type fieldType = field.FieldType;
            object o = null;

            if (value == null || value.GetType() == typeof(DBNull)) return null;
            else if (fieldType.IsSubclassOf(typeof(Enum))) o = Enum.Parse(fieldType, (string)value);
            else if (fieldType.IsSubclassOf(typeof(SELECT))) o = ParseSelect(fieldType, value);
            else if (fieldType.IsSubclassOf(typeof(ENTITY)))
            {
                o = Activator.CreateInstance(fieldType);
                if(value != null) ((ENTITY)o).Id = (int)value;
            }
            else if (fieldType.IsSubclassOf(typeof(TypeBase))) o = ParseBaseType(fieldType, value);
            else if ((Nullable.GetUnderlyingType(fieldType) != null) && Nullable.GetUnderlyingType(fieldType).IsSubclassOf(typeof(Enum)))
            {
                if ((string)value != "NULL") o = Enum.Parse(Nullable.GetUnderlyingType(fieldType), (string)value);
            }
            else if (typeof(ifcListInterface).IsAssignableFrom(fieldType)) o = ParseSTEPList(fieldType, value);
            else Console.WriteLine("FieldType: '" + field.FieldType.Name + "' not supported."); //not implemented types

            return o;
        }

        private static object ParseBaseType(Type type, object value)
        {
            Type baseType = type.BaseType.GetGenericArguments()[0];
            if (baseType.BaseType.GetGenericArguments().Length > 0) baseType = baseType.BaseType;
            if (value.GetType() == typeof(DBNull) || (baseType == typeof(string) && (string)value == "NULL")) return null;
            else return Activator.CreateInstance(type, Convert.ChangeType(value, baseType));
        }
        private static object ParseSelect(Type selectType, object value)
        {
            object select = Activator.CreateInstance(selectType);
            if (value.GetType() == typeof(string))
            {
                string s = (string)value;
                if (int.TryParse(s, out int id)) ((SELECT)select).Id = id;
                else 
                {
                    Type valueType = Type.GetType("ifc." + s.Split('|')[0], true, false);
                    if (valueType.IsSubclassOf(typeof(TypeBase)))
                    {
                        object arg = ParseBaseType(valueType, (object)s.Split('|')[1]);
                        select = Activator.CreateInstance(selectType, arg);
                    }
                }
            }
            else if (selectType.IsSubclassOf(typeof(TypeBase))) return ParseBaseType(selectType, value);
            return select;
        }

        private static object ParseSTEPList(Type fieldType, object value)
        {
            //from ifc_in_step.cs: get type of list element
            Type genericType;
            if (fieldType.BaseType.GetGenericArguments().Length > 0)
                genericType = fieldType.BaseType.GetGenericArguments()[0];
            else
                genericType = fieldType.BaseType.BaseType.GetGenericArguments()[0];

            string[] listElements = ((string)value).TrimStart('(').TrimEnd(')').Split(','); 
            if (listElements.Length == 0) Console.WriteLine(string.Format("empty list at {0}, {1}", fieldType, value));
            object[] args = GetStepListArgs(genericType, listElements);

            return Activator.CreateInstance(fieldType, args);
        }

        private static object ParseXMLList(Type fieldType, string xml)
        {
            //TODO: Implement
            return null;
        }

        private static object[] GetStepListArgs(Type genericType, string[] listElements)
        {
            // from ifc_in_step.cs
            // TODO: refactor
            List<object> args = new List<object>();
            foreach(string elem in listElements)
            {
                if (genericType == typeof(Int32))
                {
                    object[] genericCtorArgs = new object[1];
                    genericCtorArgs[0] = Activator.CreateInstance(genericType);
                    genericCtorArgs[0] = Int32.Parse(elem);
                    args.Add(Int32.Parse(elem));
                }
                else if (genericType.IsSubclassOf(typeof(TypeBase)))
                {
                    object[] genericCtorArgs = new object[1];
                    genericCtorArgs[0] = Activator.CreateInstance(genericType); //LengthMeasure or CartesianPoint
                    Type genericBaseType = genericType.BaseType.GetGenericArguments()[0];    //Double from LengthMeasure -> TYPE<double> -> double
                    if (genericBaseType == typeof(String)) { if (elem == "$") genericCtorArgs[0] = ""; else genericCtorArgs[0] = ifc.IfcString.Decode(elem); }
                    else if (genericBaseType == typeof(int)) { genericCtorArgs[0] = int.Parse(elem); }
                    else if (genericBaseType == typeof(Int32)) { genericCtorArgs[0] = Int32.Parse(elem); }
                    else if (genericBaseType == typeof(double)) { genericCtorArgs[0] = double.Parse(elem, NetSystem.Globalization.CultureInfo.InvariantCulture); }

                    args.Add(Activator.CreateInstance(genericType, genericCtorArgs));
                }
                else if (genericType.IsSubclassOf(typeof(ENTITY)))
                {
                    object o = Activator.CreateInstance(genericType);
                    ((ENTITY)o).Id = int.Parse(elem.Trim(' ').Substring(1));
                    args.Add(o);
                }
                else if (genericType.IsSubclassOf(typeof(SELECT)))
                {
                    object o = Activator.CreateInstance(genericType);
                    if ((elem.Length > 0) && elem[0] == '#') { ((SELECT)o).Id = int.Parse(elem.Trim(' ').Substring(1)); }
                    else
                    {
                        int posLpar = elem.IndexOf('(');
                        int posRpar = elem.Length - 1;//.LastIndexOf(')');
                        string body = elem.Substring(posLpar + 1, posRpar - posLpar - 1); // Argumenkörper extrahieren
                        string elementName = elem.Substring(0, posLpar);
                        try
                        {
                            Type t = Type.GetType(elementName, true, true);
                            if (t.IsSubclassOf(typeof(TypeBase)))
                            {
                                object[] genericCtorArgs = new object[1];
                                if (t.IsSubclassOf(typeof(TYPE<string>))) { if (elem == "$") genericCtorArgs[0] = ""; else genericCtorArgs[0] = ifc.IfcString.Decode(body); }
                                else if (t.IsSubclassOf(typeof(TYPE<int>))) { genericCtorArgs[0] = int.Parse(body); }
                                else if (t.IsSubclassOf(typeof(TYPE<Int32>))) { genericCtorArgs[0] = Int32.Parse(body); }
                                else if (t.IsSubclassOf(typeof(TYPE<double>))) { genericCtorArgs[0] = double.Parse(body, NetSystem.Globalization.CultureInfo.InvariantCulture); }
                                o = Activator.CreateInstance(t, genericCtorArgs);
                            }
                        }
                        catch (Exception e) { Console.WriteLine(e.Message); }
                    }
                    args.Add(o);
                }
                else { Console.WriteLine("TODO List TYPE: Base=" + genericType.Name + " not supportet."); }//not implemented types
            }
            return args.ToArray();
        }
    }
}

