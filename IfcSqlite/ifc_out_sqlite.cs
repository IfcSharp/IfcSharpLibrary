// ifc_out_sqlite.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

//#define EXPORT_COMPLETE_SCHEMA

//EF-2021-04-01: Added preprocessor flag 'INCLUDE_SQLITE' so that the compilation without sqlite-support is possible
#if INCLUDE_SQLITE
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using NetSystem = System;

namespace ifc
{
    public partial class ENTITY
    {
        public virtual void ToSqliteDataSet(ref IfcSqliteDataSet dataSet, bool updateExisting, int prevEntityId)
        {
            string paramName = "";

            // find corresponding datatable
            bool addNewTable = false;
            //SQLiteDataTable dataTable = dataSet.Tables.FirstOrDefault(t => t.Name == this.GetType().Name);
            IfcSqliteDataTable dataTable = (IfcSqliteDataTable)dataSet.Tables[this.GetType().Name];
            if (dataTable == null)
            {
                addNewTable = true;
                dataTable = new IfcSqliteDataTable(this.GetType().Name);
            }

            // find corresponding datarow
            bool addNewRow = false;
            //IfcSqliteDataRow dataRow = dataTable.Rows.FirstOrDefault(r => r.Id == this.LocalId || r.IsEmpty);
            IfcSqliteDataRow dataRow = null;
            if (dataTable.Columns.Contains("Id")) dataRow = dataTable.Select($"Id = {LocalId}").FirstOrDefault() as IfcSqliteDataRow;
            if (dataRow == null)
            {
                addNewRow = true;
                dataRow = dataTable.NewRow() as IfcSqliteDataRow;
            }
            else
            {
                SQLiteDataField idField = dataRow.Fields.FirstOrDefault(f => f.Parameter.ParameterName == "Id");
                if (idField != null) idField.Parameter.Value = this.LocalId;
            }

            if (addNewRow == true || updateExisting == true)
            {
                if (this is CartesianPoint || this is Direction)
                {
                    double X = 0;
                    double Y = 0;
                    double? Z = null;
                    if (this is CartesianPoint cp)
                    {
                        if (cp.Coordinates.Count > 1) { X = (double)cp.Coordinates[0]; Y = (double)cp.Coordinates[1]; }
                        if (cp.Coordinates.Count > 2) { Z = (double)cp.Coordinates[2]; }
                    }
                    else if (this is Direction dir)
                    {
                        if (dir.DirectionRatios.Count > 1) { X = (double)dir.DirectionRatios[0]; Y = (double)dir.DirectionRatios[1]; }
                        if (dir.DirectionRatios.Count > 2) { Z = (double)dir.DirectionRatios[2]; }
                    }
                    dataRow.Fields.Add(new SQLiteDataField(1, "X", DbType.Double, false, X));
                    dataRow.Fields.Add(new SQLiteDataField(2, "Y", DbType.Double, false, Y));
                    dataRow.Fields.Add(new SQLiteDataField(3, "Z", DbType.Double, true, Z));
                }
                else if (this is EntityComment ec)
                {
                    dataRow.Fields.Add(new SQLiteDataField(1, "Comment", DbType.String, true, ec.CommentLine));
                    dataRow.Fields.Add(new SQLiteDataField(2, "PreviousEntity", DbType.Int32, false, prevEntityId));
                }
                else
                {
                    foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                    {
                        IEnumerable<ifcAttribute> ifcAttributes = field.GetCustomAttributes(true).Where(a => a is ifcAttribute).Cast<ifcAttribute>();
                        foreach (ifcAttribute attr in ifcAttributes)
                        {
                            object[] fieldAttributes = null;
                            if (field.FieldType.IsGenericType && field.FieldType.GetGenericArguments()[0].IsEnum && field.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                fieldAttributes = field.FieldType.GetGenericArguments()[0].GetCustomAttributes(true);
                            else
                                fieldAttributes = field.FieldType.GetCustomAttributes(true);

                            ifcSqlAttribute sqlAttribute = fieldAttributes.FirstOrDefault(a => a is ifcSqlAttribute) as ifcSqlAttribute;
                            // each attribute is represented as SQLiteDataField
                            paramName = field.Name.StartsWith("_") ? field.Name.Substring(1) : field.Name;
                            if (sqlAttribute != null)
                            {
                                SQLiteDataField sqliteField = dataRow.Fields.FirstOrDefault(f => f.Parameter.ParameterName == paramName);
                                if (sqliteField == null)
                                {
                                    sqliteField = new SQLiteDataField(attr.OrdinalPosition, paramName, DbTypeFromTableId(sqlAttribute.SqlTableId), attr.optional || attr.derived, SqliteAttributeOut(field, field.GetValue(this)));
                                    dataRow.Fields.Add(sqliteField);
                                }
                                else
                                {
                                    sqliteField.Parameter.Value = SqliteAttributeOut(field, field.GetValue(this));
                                }
                            }
                        }
                    }
                }
            }

            if (addNewRow)
            {
                if (dataRow.Fields.Count > 0)
                {
                    dataRow.Fields.Add(new SQLiteDataField(0, "Id", DbType.Int32, false, this.LocalId));
                    dataRow.Fields.Add(new SQLiteDataField(dataRow.Fields.Count, "EndOfLineComment", DbType.String, true, this.EndOfLineComment));
                }

                // before we add the row, we sort the values by their ordinal position
                dataRow.OrderValuesByOrdinalPosition();
                dataTable.Rows.Add(dataRow);
                if (addNewTable)
                {
                    dataSet.Tables.Add(dataTable);
                }
            }
        }
        public object SqliteAttributeOut(FieldInfo field, object o)
        {
            if (o == null) return DBNull.Value;
            
            if (o is Enum) return o.ToString();
            if (o is string) return o.ToString();
            if (o is SELECT select)
            {
                if (select.IsNull) return "NULL";
                else if (select.SelectType().IsSubclassOf(typeof(TypeBase))) return select.SelectType().Name + "|" + select.SelectValue().ToString();
                else return SqliteAttributeOut(field, select.SelectValue());
            }
            if (o.GetType().IsSubclassOf(typeof(TypeBase))) return ((TypeBase)o).ToSqliteValue();
            if (typeof(ifcListInterface).IsAssignableFrom(o.GetType()))
            {
                //TODO: Implement "ToXML"
                return o.ToString();
            }
            return o.ToString().Replace("#", "");
        }

        public static DbType DbTypeFromTableId(int id)
        {
            switch (id)
            {
                case (int)SqlTable.EntityAttributeOfBinary: return DbType.Binary;
                case (int)SqlTable.EntityAttributeOfBoolean: return DbType.Boolean;
                case (int)SqlTable.EntityAttributeOfEntityRef: return DbType.Int32;
                case (int)SqlTable.EntityAttributeOfFloat: return DbType.Double;
                case (int)SqlTable.EntityAttributeOfInteger: return DbType.Int32;
                default: return DbType.String;
            }
        }

#region DataSet-Interface
        public static Type TypeFromTableId(int id)
        {
            switch (id)
            {
                case (int)SqlTable.EntityAttributeOfBinary: return typeof(byte);
                case (int)SqlTable.EntityAttributeOfBoolean: return typeof(bool);
                case (int)SqlTable.EntityAttributeOfEntityRef: return typeof(int);
                case (int)SqlTable.EntityAttributeOfFloat: return typeof(double);
                case (int)SqlTable.EntityAttributeOfInteger: return typeof(int);
                default: return typeof(string);
            }
        }
        public virtual void ToDataSet(ref DataSet dataSet)
        {
            // find corresponding datatable
            DataTable dataTable = dataSet.Tables[this.GetType().Name];
            if (dataTable == null) dataTable = new DataTable(this.GetType().Name);

            DataColumn dataColumn;
            DataRow dataRow = dataTable.NewRow();
            foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                foreach (Attribute attr in field.GetCustomAttributes(true))
                {
                    if (attr is ifcAttribute ifcAttr)
                    {
                        string columnName = field.Name.StartsWith("_") ? field.Name.Substring(1) : field.Name;
                        object value = SqliteAttributeOut(field, field.GetValue(this));
                        ifcSqlAttribute sqlAttribute = field.FieldType.GetCustomAttributes(true).FirstOrDefault(a => a is ifcSqlAttribute) as ifcSqlAttribute;
                        dataColumn = dataTable.Columns[columnName];
                        if (dataColumn == null)
                        {
                            dataColumn = new DataColumn(columnName, sqlAttribute == null ? typeof(string) : TypeFromTableId(sqlAttribute.SqlTableId));
                            dataTable.Columns.Add(dataColumn);
                            dataColumn.AllowDBNull = ifcAttr.optional;
                        }
                        dataRow.SetField(dataColumn, value);
                    }
                }
            }

            dataColumn = dataTable.Columns["Id"];
            if (dataColumn == null)
            {
                dataColumn = new DataColumn("Id", typeof(int));
                dataTable.Columns.Add(dataColumn);
            }
            dataRow.SetField(dataColumn, this.LocalId);

            // before we add the row, we sort the values by their ordinal position
            foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                foreach (Attribute attr in field.GetCustomAttributes(true))
                {
                    if (attr is ifcAttribute ifcAttr)
                    {
                        string columnName = field.Name.StartsWith("_") ? field.Name.Substring(1) : field.Name;
                        dataColumn = dataTable.Columns[columnName];
                        dataColumn.SetOrdinal(ifcAttr.OrdinalPosition);
                    }
                }
            }

            dataTable.Rows.Add(dataRow);
            if (dataSet.Tables[dataTable.TableName] == null)
            {
                dataSet.Tables.Add(dataTable);
            }
        }
#endregion

    }
    public partial class EntityComment : ENTITY
    {
        public int PreviousEntityId;
        public EntityComment(string CommentLine, int PreviousEntityId) : this(CommentLine)
        {
            this.PreviousEntityId = PreviousEntityId;
        }
    }

    public partial class TYPE<T> : TypeBase
    {
        public override object ToSqliteValue()
        {
            if (IsNull) return DBNull.Value;
            else if (typeof(T).Equals(typeof(double))) return ((double)(object)TypeValue).ToString("0.0000000000", NetSystem.Globalization.CultureInfo.InvariantCulture).TrimEnd('0');
            else return TypeValue.ToString();
        }
    }

    public partial class LIST<T> : List<T>, ifcListInterface, ifcSqlTypeInterface
    {
        public string ToXML() { return ""; }
    }

    public partial class Model
    {
        public void ToSqliteFile(string filePath="")
        {
            AssignEntities();
            if(string.IsNullOrEmpty(filePath)) filePath = Header.Name + ".sqlite";
            IfcSqliteDataSet sqliteDataSet = new IfcSqliteDataSet();

#if EXPORT_COMPLETE_SCHEMA
            BuildIfcDataSet(ref ifcDataSet);
#endif

            Log.Add($"Exporting '{EntityList.Count}' Entities to SQLite-File...", Log.Level.Info);
            int prevEntityId = 0;
            for (int i = 0; i < EntityList.Count; i++)
            {
                ENTITY e = EntityList[i];//ifc.Repository.CurrentModel.EntityList[i];
                if (e is ifc.Root) if (((ifc.Root)e).GlobalId == null) ((ifc.Root)e).GlobalId = ifc.GloballyUniqueId.NewId();
                e.ToSqliteDataSet(ref sqliteDataSet, true, prevEntityId);
                prevEntityId = e.LocalId;
            }

            //TODO: Check if custom DataSet Class 'SQLiteDataSet' can be omitted and instead use a regular 'DataSet'
            //DataSet dataSet = new DataSet("IfcDataSet");
            //foreach (ENTITY e in ENTITY.EntityList)
            //{
            //    e.ToDataSet(ref dataSet);
            //}

            IfcSharpSqLiteDatabase database = new IfcSharpSqLiteDatabase(filePath);
            database.FillFromDataSet(sqliteDataSet);

            Log.Add("Finished Export", Log.Level.Info);
        }

        private bool BuildIfcDataSet(ref IfcSqliteDataSet ifcDataSet)
        {
            if (ifcDataSet == null)
                return false;

            SQLiteDataField sqliteField;
            string paramName;

            foreach (Type t in Assembly.GetAssembly(typeof(ifc.ENTITY)).GetTypes())
            {
                if (t.IsClass)
                {
                    if (!t.IsAbstract)
                    {
                        if (t.IsSubclassOf(typeof(ifc.ENTITY)))
                        {
                            IfcSqliteDataTable dataTable = new IfcSqliteDataTable(t.Name);
                            IfcSqliteDataRow dataRow = dataTable.NewRow() as IfcSqliteDataRow;
                            foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                            {
                                foreach (Attribute attr in field.GetCustomAttributes(true))
                                {
                                    if (attr is ifcAttribute ifcAttribute)
                                    {
                                        object[] fieldAttributes = null;
                                        if ((field.FieldType.IsGenericType) && (field.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>)) && (field.FieldType.GetGenericArguments()[0].IsEnum))
                                        {
                                            fieldAttributes = field.FieldType.GetGenericArguments()[0].GetCustomAttributes(true);
                                        }
                                        else
                                        {
                                            fieldAttributes = field.FieldType.GetCustomAttributes(true);
                                        }
                                        if (null != fieldAttributes)
                                        {
                                            foreach (Attribute attr2 in fieldAttributes)
                                            {
                                                if (attr2 is ifc.ifcSqlAttribute sqlAttribute)
                                                {
                                                    paramName = field.Name.StartsWith("_") ? field.Name.Substring(1) : field.Name;
                                                    sqliteField = new SQLiteDataField(ifcAttribute.OrdinalPosition, paramName, ENTITY.DbTypeFromTableId(sqlAttribute.SqlTableId));
                                                    sqliteField.Parameter.IsNullable = ifcAttribute.optional || ifcAttribute.derived;
                                                    dataRow.Fields.Add(sqliteField);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            dataRow.Fields.Add(new SQLiteDataField(0, "Id", DbType.Int32));
                            dataRow.Fields.Add(new SQLiteDataField(dataRow.Fields.Count, "EndOfLineComment", DbType.String));

                            // before we add the row, we sort the values by their ordinal position
                            dataRow.OrderValuesByOrdinalPosition();

                            dataTable.Rows.Add(dataRow);
                            ifcDataSet.Tables.Add(dataTable);
                        }
                    }
                }
            }
            return true;
        }
    }
}

#endif