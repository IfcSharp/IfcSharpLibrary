// ifc_sqlite_data.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)


//EF-2021-04-01: Added preprocessor flag 'INCLUDE_SQLITE' so that the compilation without sqlite-support is possible
#if INCLUDE_SQLITE
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using NetSystem = System;

namespace ifc
{
    public class SQLiteDataSet
    {
        public IList<SQLiteDataTable> Tables { get; internal set; }

        public SQLiteDataSet()
        {
            this.Tables = new List<SQLiteDataTable>();
        }

        public void Clear()
        {
            Tables.Clear();
        }
    }

    public class SQLiteDataTable
    {
        public string Name { get; private set; }
        public IList<SQLiteDataRow> Rows { get; private set; }

        public SQLiteDataTable(string name)
        {
            this.Name = name;
            this.Rows = new List<SQLiteDataRow>();
        }

    }
    public class SQLiteDataRow
    {
        public IList<SQLiteDataField> Fields { get; private set; }
        public SQLiteDataRow()
        {
            this.Fields = new List<SQLiteDataField>();
        }
        public int Id 
        {
            get 
            {
                int id = -1;
                SQLiteDataField field = Fields.FirstOrDefault(f => f.Parameter.ParameterName == "Id");
                if(field != null && field.Parameter.Value != null)
                    id = (int)field.Parameter.Value;
                
                return id;
            }
        }

        public bool IsEmpty
        {
            get 
            {
                return !Fields.Any(f => f.Parameter.Value != null);
            }
        }

        public void OrderValuesByOrdinalPosition()
        {
            Fields = Fields.OrderBy(value => value.OrdinalPosition).ToList();
        }
    }

    public class SQLiteDataField
    {
        public int OrdinalPosition { get; private set; }
        public SQLiteParameter Parameter { get; private set; }
        public SQLiteDataField(int ordinalPos, string name, NetSystem.Data.DbType dbType)
        {
            Parameter = new SQLiteParameter(name, dbType);
            OrdinalPosition = ordinalPos;
        }
        public SQLiteDataField(int ordinalPos, string name, NetSystem.Data.DbType dbType, bool optional, object value)
        {
            Parameter = new SQLiteParameter(name, dbType);
            Parameter.Value = value;
            Parameter.IsNullable = optional;
            OrdinalPosition = ordinalPos;
        }
        public SQLiteDataField(int ordinalPos, string name, bool optional, object value)
        {
            Parameter = new SQLiteParameter(name);
            Parameter.Value = value;
            Parameter.IsNullable = optional;
            OrdinalPosition = ordinalPos;
        }

        public SQLiteDataField(int ordinalPos, string name, object value)
        {
            Parameter = new SQLiteParameter(name);
            Parameter.Value = value;
            OrdinalPosition = ordinalPos;
        }
    }
}

#endif