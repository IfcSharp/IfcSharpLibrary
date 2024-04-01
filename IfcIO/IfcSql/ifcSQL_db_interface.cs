//  ifcSQL_db_interface.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Globalization;
using System.Text;

namespace db{//====================================================================================


public class DbField : System.Attribute { public bool PrimaryKey=false;public string PkName=null; public bool SortAscending=false; public bool SortDescending=false;
                                                                                            public static db.DbField    Value(object o, string FieldName){foreach (object a in o.GetType().GetField(FieldName).GetCustomAttributes(typeof(db.DbField   ))) return (db.DbField   )a;return null;}} // Value (bb) 10.06.2023 
[System.AttributeUsage(System.AttributeTargets.All,AllowMultiple = true)] public class References : System.Attribute {public string FkName=null;public string RefPkName=null;public string RefTableSchema=null;public string RefTableName=null;public string RefTableColName=null;
                                                                                            public static db.References Value(object o, string FieldName){foreach (object a in o.GetType().GetField(FieldName).GetCustomAttributes(typeof(db.References))) return (db.References)a;return null;}} // Value (bb) 10.06.2023 
public class UserType : System.Attribute {public string schema=null;public string name=null;public static db.UserType   Value(object o, string FieldName){foreach (object a in o.GetType().GetField(FieldName).GetCustomAttributes(typeof(db.UserType  ))) return (db.UserType  )a;return null;}} // Value (bb) 29.12.2022 
public class Comment  : System.Attribute {public string text=null;                          public static db.Comment    Value(object o, string FieldName){foreach (object a in o.GetType().GetField(FieldName).GetCustomAttributes(typeof(db.Comment   ))) return (db.Comment   )a;return null;}} // (bb) 29.12.2022 
public class SqlType  : System.Attribute {public string name=null;public int size=0;        public static db.SqlType    Value(object o, string FieldName){foreach (object a in o.GetType().GetField(FieldName).GetCustomAttributes(typeof(db.SqlType   ))) return (db.SqlType   )a;return null;}} // Value (bb) 10.06.2023 



public partial class RowBase{//--------------------------------------------------------------------
public       RowBase(){}
public       RowBase(SqlDataReader reader){FromReader(reader);}
public RowBase FromReader(SqlDataReader reader){foreach (FieldInfo field in this.GetType().GetFields()) foreach (Attribute attr in field.GetCustomAttributes(inherit:false)) if (attr is DbField) if (!reader.IsDBNull(reader.GetOrdinal(field.Name)) ) field.SetValue(this,reader[field.Name]);return this;}
public void AddDataTableColumns(DataTable table){foreach (FieldInfo field in this.GetType().GetFields()) foreach (Attribute attr in field.GetCustomAttributes(inherit:false)) if (attr is DbField) table.Columns.Add(new DataColumn(columnName:field.Name,dataType:Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType));}
public DataRow DataTableRow(DataTable table){DataRow row=table.NewRow();foreach (FieldInfo field in this.GetType().GetFields()) foreach (Attribute attr in field.GetCustomAttributes(inherit:false)) if (attr is DbField)  row[field.Name]=(Nullable.GetUnderlyingType(field.FieldType)!= null)?(field.GetValue(this)??DBNull.Value):field.GetValue(this);return row;}
public string InsertStringOpen(string TableName){string s="INSERT INTO "+TableName+"(";int col=0;foreach (FieldInfo field in this.GetType().GetFields()) foreach (Attribute attr in field.GetCustomAttributes(inherit:false)) if (attr is DbField) s+=((++col>1)?",":"")+field.Name;s+=") VALUES\r\n";return s;} 
public string InsertStringValuesRow(){string s="(";int col=0;foreach (FieldInfo field in this.GetType().GetFields()) foreach (Attribute attr in field.GetCustomAttributes(inherit:false)) if (attr is DbField) s+=((++col>1)?",":"")+DbFieldValueStr(field);s+=")\r\n";return s;} 
public string InsertStringClose(){return "\r\n";}

public string DbFieldValueStr(FieldInfo field) {//.................................................
     if (field.FieldType==typeof(System.String))   return "\'"+field.GetValue(this).ToString()+"\'";
else if (field.FieldType==typeof(System.DateTime)) return "\'"+((DateTime)field.GetValue(this)).ToString("yyy-MM-ddTHH:mm:ss.fff")+"\'"; //SET startDate = CONVERT(datetime,'2015-03-11T23:59:59.000',126)
else if (field.FieldType==typeof(System.Double))    return String.Format(CultureInfo.InvariantCulture,"{0:F3}",(double)field.GetValue(this));
else if (field.FieldType==typeof(System.Decimal))   return String.Format(CultureInfo.InvariantCulture,"{0:F3}",(double)(decimal)field.GetValue(this));
else if (field.FieldType==typeof(System.Single))    return String.Format(CultureInfo.InvariantCulture,"{0:F3}",(Single)field.GetValue(this));
else if (field.FieldType==typeof(System.Int32))     return field.GetValue(this).ToString();
else if (field.FieldType==typeof(System.Int64))     return field.GetValue(this).ToString();
else if (field.FieldType==typeof(Nullable<double>)) { if (field.GetValue(this)==null) return "null"; else return field.GetValue(this).ToString();}
else if (field.FieldType==typeof(System.Boolean))   return (field.GetValue(this).ToString()=="True")?"-1":"0";
else if (field.FieldType==typeof(System.Byte))      return field.GetValue(this).ToString();
else return "unknown Type "+field.FieldType.ToString();
}//................................................................................................
public virtual void Load(TableBase rows){}
public RowBase Clone(){return (RowBase)this.MemberwiseClone();} // (bb) 10.06.2023 
public object  ValueOf(string ColName){foreach (FieldInfo field in this.GetType().GetFields()) foreach (Attribute attr in field.GetCustomAttributes(inherit:false)) if (attr is DbField) if (field.Name==ColName) return field.GetValue(this);return null;} // (bb) 10.06.2023 
}//------------------------------------------------------------------------------------------------

public partial class TableBase : List<Object>{//-----------------------------------------------------------
public string TableName="-";
public TableSet tableSet=null;
public virtual void SelectAll(string where=""){}
public virtual void Load(){}
public virtual string InsertString(){return "-";} // better using Interface
public virtual void BulkInsert(){} 
public string order;
}// of TableBase ----------------------------------------------------------------------------------

public partial class RowList<T> : TableBase where T : new(){//---------------------------------------------
public               RowList(string order=""){this.order=order;}
public override void SelectAll(string where=""){string sql="select * from "+TableName+" "+where+" "+order; // (bb) 20.05.2023 exception handling
                                                SqlCommand cmd = new SqlCommand(sql,tableSet.conn);
                                                try {using (SqlDataReader reader = cmd.ExecuteReader()) while (reader.Read()) {Object rb = new T();this.Add(((RowBase)rb).FromReader(reader));}}
                                                catch(Exception e) {ifc.Log.Add(e.Message+"\n"+sql,ifc.Log.Level.Exception);}
                                                //((RowBase)(object)new T()).Load(this);
                                               }
public override void Load   (){((RowBase)(object)new T()).Load(this);} // (bb) 30.10.2022 seperated load for serialsation

public override string InsertString(){Object o = new T();RowBase rb=((RowBase)o);string s=rb.InsertStringOpen(TableName);int pos=0;foreach (RowBase row in this) s+=((++pos>1)?",":"")+row.InsertStringValuesRow();s+=rb.InsertStringClose();return s; }

public override void BulkInsert(){using (SqlBulkCopy bulkCopy = new SqlBulkCopy(tableSet.conn))// ..........................
                                        {            bulkCopy.DestinationTableName = TableName; //Console.WriteLine("TableName="+TableName+": "+InsertString());
                                                     bulkCopy.WriteToServer(FilledDataTable()); // if (TableName=="[cp].[EntityAttributeOfString]") {bulkCopy.BatchSize=100000;bulkCopy.BulkCopyTimeout=3;} sometimes timeout, don't no why
                                        }
                                 }//.........................................................................................................................
public DataTable FilledDataTable() {//............................................................
                                     DataTable table = new DataTable();
                                     Object rb = new T();((RowBase)rb).AddDataTableColumns(table); 
                                     foreach (RowBase row in this) table.Rows.Add(row.DataTableRow(table)); 
                                     return table; 
                                    }//............................................................

}//------------------------------------------------------------------------------------------------


public partial class SchemaBase{}//----------------------------------------------------------------

public partial class TableSet{//---------------------------------------------------------------------------
public  TableSet(){AssignTableNames();}
public  TableSet(string ServerName,string DatabaseName,bool DirectLoad=false){this.ServerName=ServerName;this.DatabaseName=DatabaseName;AssignTableNames();
                                                                              conn=new SqlConnection("Persist Security Info=False;Integrated Security=true;Initial Catalog="+DatabaseName+";server="+ServerName);
                                                                              if (DirectLoad) {LoadAllTables();LoadAllMaps();}
                                                                             }
public  TableSet(string ServerName,string DatabaseName,string UserName,string Password,bool DirectLoad=false){this.ServerName=ServerName;this.DatabaseName=DatabaseName;AssignTableNames();
                                                                              conn=new SqlConnection("Data Source="+ServerName+";Network Library=DBMSSOCN;Initial Catalog="+DatabaseName+";User ID="+UserName+";Password='"+Password+"'");
                                                                              if (DirectLoad) {LoadAllTables();LoadAllMaps();}
                                                                             }




public void AssignTableNames(){foreach (FieldInfo SchemaField in this.GetType().GetFields()) if (SchemaField.GetValue(this) is SchemaBase)
                                   foreach (FieldInfo TableField in SchemaField.GetValue(this).GetType().GetFields()) if (TableField.GetValue(SchemaField.GetValue(this)) is TableBase) 
                                           { ((TableBase)TableField.GetValue(SchemaField.GetValue(this))).TableName="["+this.DatabaseName+"].["+SchemaField.Name+"].["+TableField.Name+"]";
                                             ((TableBase)TableField.GetValue(SchemaField.GetValue(this))).tableSet=this;
                                           }   
                              }
public void LoadAllTables(){conn.Open();
                            foreach (FieldInfo SchemaField in this.GetType().GetFields()) if (SchemaField.GetValue(this) is SchemaBase)
                                   foreach (FieldInfo TableField in SchemaField.GetValue(this).GetType().GetFields()) if (TableField.GetValue(SchemaField.GetValue(this)) is TableBase) 
                                       ((TableBase)TableField.GetValue(SchemaField.GetValue(this))).SelectAll();
                            conn.Close();
                           }
public void LoadAllMaps(){foreach (FieldInfo SchemaField in this.GetType().GetFields()) if (SchemaField.GetValue(this) is SchemaBase)
                                   foreach (FieldInfo TableField in SchemaField.GetValue(this).GetType().GetFields()) if (TableField.GetValue(SchemaField.GetValue(this)) is TableBase) 
                                       ((TableBase)TableField.GetValue(SchemaField.GetValue(this))).Load();
                         }

[XmlIgnore] public string DatabaseName="-";  // (bb) 17.02.2024 [XmlIgnore] for serialisation
[XmlIgnore] public string ServerName="-";    // (bb) 17.02.2024 [XmlIgnore] for serialisation
[XmlIgnore] public  SqlConnection conn=null; // (bb) 30.10.2022 [XmlIgnore] for serialisation

public DbCommand Command(string cmd) {return new SqlCommand  (cmd,conn);}
public void ExecuteNonQuery(string sql, bool DoOpenAndClose=false){if (DoOpenAndClose) conn.Open();Console.WriteLine(sql);/*Console.ReadLine();*/ Command(sql).ExecuteNonQuery();if (DoOpenAndClose) conn.Close();}
public int  ExecuteIntegerScalar  (string sql, bool DoOpenAndClose=false){if (DoOpenAndClose) conn.Open();var result=Command(sql).ExecuteScalar();int i=(int)result;if (DoOpenAndClose) conn.Close();return i;}
public long ExecuteLongScalar  (string sql, bool DoOpenAndClose=false){if (DoOpenAndClose) conn.Open();var result=Command(sql).ExecuteScalar();long i=(long)result;if (DoOpenAndClose) conn.Close();return i;}
public string ExecuteStringScalar  (string sql, bool DoOpenAndClose=false){if (DoOpenAndClose) conn.Open();var result=Command(sql).ExecuteScalar();string s=(string)result;if (DoOpenAndClose) conn.Close();return s;}

}//------------------------------------------------------------------------------------------------


}//of namspace db =================================================================================