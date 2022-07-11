// ifc_in_sqlite.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

//#define _DEBUG

//EF-2021-04-01: Added preprocessor flag 'INCLUDE_SQLITE' so that the compilation without sqlite-support is possible
#if INCLUDE_SQLITE

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
            IfcSharpSqLiteDatabase database = new IfcSharpSqLiteDatabase();
            Model model = new ifc.Model(fullPath.Replace(".sqlite", ""));
            DataSet dataSet = database.GetContentAsDataSet(fullPath);
            Log.Add($"Reading SQLite-File: {fullPath}", Log.Level.Info);
            
            foreach (DataTable dt in dataSet.Tables)
            {
#if DEBUG
                Console.WriteLine("______________________________________________________");
                Console.WriteLine(dt.TableName);
                foreach (DataColumn c in dt.Columns) Console.Write($"{c.ColumnName}");
                Console.Write("\r\n");
#endif
                foreach (DataRow row in dt.Rows)
                {
                    Type entityType = Type.GetType("ifc." + dt.TableName);
                    if (entityType == null) {
                        Log.Add($"Could not find Type of 'ifc.{dt.TableName}", Log.Level.Error);
                        continue;
                    }

                    ENTITY entityInstance;
                    if (entityType == typeof(EntityComment))
                    {
                        int prevEntityId = row["PreviousEntity"] is int ? (int)row["PreviousEntity"] : (int)(long)row["PreviousEntity"];
                        EntityComment ec = new EntityComment((string)row["Comment"], prevEntityId);
                        entityInstance = ec;
                    }
                    else
                    {
                        entityInstance = ENTITY.GetInstanceFromDbRecord(entityType, row);
                    }
                    if (entityInstance == null) {
                        Log.Add($"Could not create Instance of '{entityType.Name}", Log.Level.Error);
                        continue;
                    }

                    if (row["Id"] != null) {
                        int localId = row["Id"] is int ? (int)row["Id"] : (int)(long)row["Id"];
                        entityInstance.LocalId = localId;
                    }
                    model.EntityList.Add(entityInstance);
#if DEBUG
                    foreach (DataColumn c in dt.Columns) Console.Write($"{row[c]}");
                    Console.Write("\r\n");
#endif
                }
            }

            // before we assign the entities, we need to order the list according to the Ids
            model.EntityList = model.EntityList.OrderBy(e => e.LocalId).ToList();

            // TODO: this might need a rework
            // then we change the position of all EntityComment´s to match the actual order,
            // since they are being read sequentially
            foreach (EntityComment ec in model.EntityList.FindAll(e => e.GetType() == typeof(EntityComment)))
            {
                int oldIndex = model.EntityList.FindIndex(e => e.LocalId == ec.LocalId);
                int newIndex = model.EntityList.FindIndex(e => e.LocalId == ec.PreviousEntityId) + 1;
                ENTITY entity = model.EntityList[oldIndex];
                model.EntityList.RemoveAt(oldIndex);

                if (newIndex > oldIndex) newIndex--; // the actual index could have shifted due to the removal
                model.EntityList.Insert(newIndex, entity);
            }

            model.AssignEntities();
            return model;
        }

        
        

        
    }

    public partial class ENTITY
    {
        public static ENTITY GetInstanceFromDbRecord(Type type, DataRow record)
        {
            object instance = Activator.CreateInstance(type);
            Dictionary<int, FieldInfo> attributes = ENTITY.GetAttributesOfObject(instance);
            for (int i = 1; i <= attributes.Count; i++)
            {
                FieldInfo field = attributes[i];
                object o;
                string fieldName = field.Name.Replace("_", "");
                if (type == typeof(CartesianPoint))//(fieldName == "Coordinates")
                {
                    List<LengthMeasure> coords = new List<LengthMeasure>();
                    if (record["X"] != DBNull.Value) coords.Add(new LengthMeasure((double)record["X"]));
                    if (record["Y"] != DBNull.Value) coords.Add(new LengthMeasure((double)record["Y"]));
                    if (record["Z"] != DBNull.Value) coords.Add(new LengthMeasure((double)record["Z"]));
                    o = new List1to3_LengthMeasure(coords.ToArray());
                }
                else if (type == typeof(Direction))//(fieldName == "DirectionRatios")
                {
#if IFC2X3
                    List<double> coords = new List<double>();
                    if (dataRow["X"] != DBNull.Value) coords.Add((double)dataRow["X"]);
                    if (dataRow["Y"] != DBNull.Value) coords.Add((double)dataRow["Y"]);
                    if (dataRow["Z"] != DBNull.Value) coords.Add((double)dataRow["Z"]);
                    o=new List2to3_double();
                    ((List2to3_double)o).Add(coords[0]);
                    ((List2to3_double)o).Add(coords[1]);
                    ((List2to3_double)o).Add(coords[2]);
#else
                    List<Real> coords = new List<Real>();
                    if (record["X"] != DBNull.Value) coords.Add(new Real((double)record["X"]));
                    if (record["Y"] != DBNull.Value) coords.Add(new Real((double)record["Y"]));
                    if (record["Z"] != DBNull.Value) coords.Add(new Real((double)record["Z"]));
                    o = new List2to3_Real(coords.ToArray());
#endif
                }
                else
                {
                    object value = record[fieldName];
                    o = GetFieldInstance(field, value);
                }
                field.SetValue(instance, o);

                if (i == attributes.Count) field.SetValue(instance, record["EndOfLineComment"] is DBNull ? null : record["EndOfLineComment"]);
            }
            return instance as ENTITY;
        }
        
        private static object GetFieldInstance(FieldInfo field, object value)
        {
            Type fieldType = field.FieldType;
            object o = null;

            if (value == null || value is DBNull) return null;
            else if (fieldType.IsSubclassOf(typeof(Enum))) o = Enum.Parse(fieldType, (string)value);
            else if (fieldType.IsSubclassOf(typeof(SELECT))) o = ParseSelect(fieldType, value);
            else if (fieldType.IsSubclassOf(typeof(ENTITY)))
            {
                o = Activator.CreateInstance(fieldType);
                int localId = value is int ? (int)value : (int)(long)value;
                ((ENTITY)o).LocalId = localId;
            }
            else if (fieldType.IsSubclassOf(typeof(TypeBase))) o = Parse2TYPE(value.ToString(), fieldType);//ParseBaseType(fieldType, value);
            else if ((Nullable.GetUnderlyingType(fieldType) != null) && Nullable.GetUnderlyingType(fieldType).IsSubclassOf(typeof(Enum)))
            {
                if ((string)value != "NULL") o = Enum.Parse(Nullable.GetUnderlyingType(fieldType), (string)value);
            }
            else if (typeof(ifcListInterface).IsAssignableFrom(fieldType)) {
                // for now any list is stored as its STEP representation
                // so we just use the STEP parser to get the data back
                // TODO: implement more suitable/database-oriented way to store lists
                o = Parse2LIST(fieldType, (string) value);
            }
            else Console.WriteLine("FieldType: '" + field.FieldType.Name + "' not supported."); //not implemented types

            return o;
        }
        
        private static object ParseBaseType(Type type, object value)
        {
            Type baseType = type.BaseType.GetGenericArguments()[0];
            if (baseType.BaseType.GetGenericArguments().Length > 0) baseType = baseType.BaseType;
            if (value is DBNull || (baseType == typeof(string) && (string)value == "NULL")) return null;
            return Activator.CreateInstance(type, Convert.ChangeType(value, baseType));
        }
        
        private static object ParseSelect(Type selectType, object value)
        {
            object select = Activator.CreateInstance(selectType);
            if (value is string s)
            {
                if (s == "NULL") return null;
                if (int.TryParse(s, out int id)) ((SELECT)select).Id = id;
                else 
                {
                    Type valueType = Type.GetType("ifc." + s.Split('|')[0], true, false);
                    if (valueType.IsSubclassOf(typeof(TypeBase)))
                    {
                        object arg = Parse2TYPE(s.Split('|')[1], valueType);//ParseBaseType(valueType, s.Split('|')[1]);
                        select = Activator.CreateInstance(selectType, arg);
                    }
                }
            }
            else if (selectType.IsSubclassOf(typeof(TypeBase))) return ParseBaseType(selectType, value);
            return select;
        }
    }
}

#endif