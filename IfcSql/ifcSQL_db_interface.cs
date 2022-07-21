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


public class DbField : System.Attribute { public bool PrimaryKey=false; public bool SortAscending=false; public bool SortDescending=false;}
[System.AttributeUsage(System.AttributeTargets.All,AllowMultiple = true)] public class References : System.Attribute {public string RefTableSchema=null;public string RefTableName=null;public string RefTableColName=null;}
public class UserType : System.Attribute {public string schema=null;public string name=null;}



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
}//------------------------------------------------------------------------------------------------

public partial class TableBase : List<Object>{//-----------------------------------------------------------
public string TableName="-";
public TableSet tableSet=null;
public virtual void SelectAll(string where=""){}
public virtual string InsertString(){return "-";} // better using Interface
public virtual void BulkInsert(){} 
public string order;
}// of TableBase ----------------------------------------------------------------------------------

public partial class RowList<T> : TableBase where T : new(){//---------------------------------------------
public               RowList(string order=""){this.order=order;}
public override void SelectAll(string where=""){SqlCommand cmd = new SqlCommand("select * from "+TableName+" "+where+" "+order,tableSet.conn);
                                 using (SqlDataReader reader = cmd.ExecuteReader()) while (reader.Read()) {Object rb = new T();this.Add(((RowBase)rb).FromReader(reader));}
                                ((RowBase)(object)new T()).Load(this);
                                }
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
public  TableSet(string ServerName,string DatabaseName){this.ServerName=ServerName;this.DatabaseName=DatabaseName;AssignTableNames();
                                                        conn=new SqlConnection("Persist Security Info=False;Integrated Security=true;Initial Catalog="+DatabaseName+";server="+ServerName);
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

public string DatabaseName="-";
public string ServerName="-";
public  SqlConnection conn=null;

public DbCommand Command(string cmd) {return new SqlCommand  (cmd,conn);}
public void ExecuteNonQuery(string sql, bool DoOpenAndClose=false){if (DoOpenAndClose) conn.Open();Command(sql).ExecuteNonQuery();if (DoOpenAndClose) conn.Close();}
public int  ExecuteIntegerScalar  (string sql, bool DoOpenAndClose=false){if (DoOpenAndClose) conn.Open();var result=Command(sql).ExecuteScalar();int i=(int)result;if (DoOpenAndClose) conn.Close();return i;}
public long ExecuteLongScalar  (string sql, bool DoOpenAndClose=false){if (DoOpenAndClose) conn.Open();var result=Command(sql).ExecuteScalar();long i=(long)result;if (DoOpenAndClose) conn.Close();return i;}

}//------------------------------------------------------------------------------------------------


}//of namspace db =================================================================================