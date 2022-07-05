// ifc_sqlite_database.cs, Copyright (c) 2020, Bernhard Simon Bock, Friedrich Eder, MIT License (see https://github.com/IfcSharp/IfcSharpLibrary/tree/master/Licence)


//EF-2021-04-01: Added preprocessor flag 'INCLUDE_SQLITE' so that the compilation without sqlite-support is possible
#if INCLUDE_SQLITE

using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NetSystem = System;

//TODO: implement database interfaces for
// 1. MariaDB
// 2. MySQL
// 3. SQLite (System.Data.SQLite)
// 4. SQLite (Mono.Data.Sqlite)

namespace ifc
{
    class IfcSharpSqLiteDatabase : IDisposable
    {
        private SqliteConnection Connection { get; set; }
        private SqliteTransaction ActiveTransaction { get; set; }

        public IfcSharpSqLiteDatabase()
        {

        }
        public IfcSharpSqLiteDatabase(string fullPath)
        {
            Log.Add($"Creating Database '{fullPath}'", Log.Level.Info);
            InitDatabase(fullPath);
        }

        public void ConnectToDatabase(string fullPath)
        {
            if (Connection != null)
            {
                CloseConnection();
                Connection = null;
            }

            string connection = $"Data Source={fullPath};Version=3;";
            Connection = new SqliteConnection(connection);
        }
        public int InitDatabase(string fullPath)
        {
            // if the File already exists, check if it´s in use
            if (File.Exists(fullPath))
            {
                if (IsFileLocked(fullPath))
                {
                    string msg = "The SQLite-File is locked.\nChange the Filename or close the locking application and try again.";
                    throw new IOException(msg);
                }
            }

            // create the database-file
            try
            {
                SqliteConnection.CreateFile(fullPath);
                // connect to the database and open the connection
                string connection = $"Data Source={fullPath};Version=3;";
                this.Connection = new SqliteConnection(connection);

                //this.Connection.Open();
                //// due to the m:n relation of certain tables
                //// we need to turn on FOREIGN-Keys
                //SQLiteCommand command = new SQLiteCommand(this.Connection);
                //BeginTransaction();
                //command.CommandText = "PRAGMA foreign_keys = ON;";
                //command.ExecuteNonQuery();
                //CommitTransaction();
                //CloseConnection();
            }
            catch (IfcSharpException e)
            {
                Log.Add(e.Message + "\n" + e.StackTrace, Log.Level.Exception);
                CancelChanges();
                CloseConnection();
                return 0;
            }

            return 1;
        }

        public static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            FileInfo fileInfo = new FileInfo(filePath);
            try
            {
                stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //1. still being written to
                //2. or being processed by another thread
                //3. or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        public void Dispose()
        {
            this.Connection.Close();
            this.Connection.Dispose();
        }

        /// <summary>
        /// Clears the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public int TruncateTable(string tableName)
        {
            if (this.Connection.State != NetSystem.Data.ConnectionState.Open)
            {
                this.Connection.Open();
            }
            int returnValue = 0;
            SqliteCommand command = new SqliteCommand(string.Format("DELETE FROM {0};", tableName), this.Connection);
            try
            {
                returnValue = command.ExecuteNonQuery();
            }
            catch (SqliteException sqle)
            {
                // Handle DB exception
                if (sqle.ErrorCode != SQLiteErrorCode.Error) {
                    Log.Add(sqle.ErrorCode.ToString(), Log.Level.Exception);
                    throw new IfcSharpException($"Unhandled SqliteException '{sqle.ErrorCode}' in 'TruncateTable");
                }
            }
            finally
            {
                this.Connection.Close();
            }
            return returnValue;
        }

        public void FillFromDataSet(SQLiteDataSet dataSet)
        {
            Log.Add($"Filling Database from DataSet...", Log.Level.Info);

            try
            {
                OpenConnection();
                BeginTransaction();

                foreach (SQLiteDataTable table in dataSet.Tables)
                {
                    // first, create the table with its columns
                    CreateTable(table);

                    // second, add all entries from the DatabaseTable-Object
                    InsertTable(table);
                }
                CommitTransaction();
                CloseConnection();
            }
            catch (IfcSharpException e)
            {
                string msg = "Exception in 'CreateDatabase(SqliteDataSet dataSet)':\n" + e.Message;
                Log.Add(msg, Log.Level.Exception);
                CloseConnection();
            }
        }

        /// <summary>
        /// Cancels the changes.
        /// </summary>
        /// <returns></returns>
        public int CancelChanges()
        {
            if (this.Connection != null && this.Connection.State == NetSystem.Data.ConnectionState.Open)
            {
                try {
                    if (ActiveTransaction != null) {
                        ActiveTransaction.Rollback();
                    }
                }
                catch (Exception e)
                {
                    Log.Add(e.Message, Log.Level.Exception);
                }
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <returns></returns>
        public int CloseConnection()
        {
            if (Connection == null)
            {
                Log.Add("Error in 'OpenConnection()': this.DatabaseConnection == null", Log.Level.Error);
                return 0;
            }

            if (Connection.State == NetSystem.Data.ConnectionState.Open)
                Connection.Close();

            return 1;
        }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns></returns>
        private int BeginTransaction()
        {
            if (this.Connection == null)
            {
                Log.Add("Error in 'BeginTransaction()': this.DatabaseConnection == null", Log.Level.Error);
                return 0;
            }

            if (this.Connection.State == NetSystem.Data.ConnectionState.Closed)
                OpenConnection();

            try
            {
                this.ActiveTransaction = this.Connection.BeginTransaction();
            }
            catch (IfcSharpException e)
            {
                Log.Add("Exception in 'BeginTransaction()': " + e.Message, Log.Level.Exception);
                return 0;
            }
            return 1;
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <returns></returns>
        private int CommitTransaction()
        {
            if (this.ActiveTransaction == null)
            {
                Log.Add("Error in 'CommitTransaction()': this.DatabaseConnection == null", Log.Level.Error);
                return 0;
            }

            this.ActiveTransaction.Commit();

            return 1;
        }

        internal void InsertDataSet(SQLiteDataSet dataSet)
        {
            OpenConnection();
            BeginTransaction();

            foreach (SQLiteDataTable table in dataSet.Tables)
                InsertTable(table);

            CommitTransaction();
            CloseConnection();
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <returns></returns>
        private int OpenConnection()
        {
            if (this.Connection == null)
            {
                Log.Add("Error in 'CommitTransaction()': this.DatabaseConnection == null", Log.Level.Error);
                return 0;
            }

            if (this.Connection.State != ConnectionState.Open)
                this.Connection.Open();

            return 1;
        }

        public DataSet GetContentAsDataSet(string fullPath)
        {
            DataSet dataSet = new DataSet();
            ConnectToDatabase(fullPath);
            OpenConnection();

            // first we get a list of all tables from the db
            SqliteCommand command = new SqliteCommand("SELECT name FROM sqlite_master WHERE type = 'table';", this.Connection);
            SqliteDataReader dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                string tableName = dataReader.GetString(0);
                SqliteDataAdapter dataAdapter = new SqliteDataAdapter($"SELECT * FROM {tableName}", this.Connection);

                dataAdapter.FillSchema(dataSet, SchemaType.Source, tableName);
                dataAdapter.Fill(dataSet, tableName);
            }

            return dataSet;
        }

        private string GetColumnAttributes(SQLiteDataTable table)
        {
            List<string> AttributePair = table.Rows[0].Fields.Select(p => string.Format("'{0}' {1} {2}", p.Parameter.ParameterName, p.Parameter.DbType, p.Parameter.IsNullable ? "NULL" : "NOT NULL")).ToList();
            string attributes = string.Join(",", AttributePair);
            if (attributes == "") attributes = "EMPTY_TABLE";
            else attributes += ", CONSTRAINT id_pk PRIMARY KEY (Id)";

            return attributes;
        }
        private int CreateTable(SQLiteDataTable table)
        {
            int returnValue = 0;
            SqliteCommand sqlCommand = new SqliteCommand(this.Connection);
            string commandText = string.Format("CREATE TABLE IF NOT EXISTS '{0}' ({1});", table.Name, GetColumnAttributes(table));
            try
            {
                sqlCommand.CommandText = commandText;
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                string msg = string.Format("Exception in executing command: '{0}':\n{1}", commandText, e.Message);
                Log.Add(msg, Log.Level.Exception);
                returnValue = -1;
            }
            return returnValue;
        }

        private void InsertTable(SQLiteDataTable table)
        {
            if (table != null)
                foreach (SQLiteDataRow row in table.Rows)
                    InsertRow(table.Name, row);
        }

        private void InsertRow(string tableName, SQLiteDataRow row)
        {
            List<string> valueNames = row.Fields.Select(p => p.Parameter.ParameterName).ToList();
            List<string> valuePlaceholders = row.Fields.Select(p => "@" + p.Parameter.ParameterName).ToList();
            List<object> values = row.Fields.Select(p => p.Parameter.Value).ToList();
            // we dont insert when: no ValueNames are given _OR_ all Values are "NULL"
            if (valueNames.Count <= 0 || values.All(item => item == null))
                return;
            string commandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, string.Join(",", valueNames), string.Join(",", valuePlaceholders));
            try
            {
                SqliteCommand command = new SqliteCommand(commandText, this.Connection);
                for (int i = 0; i < values.Count; i++)
                    command.Parameters.AddWithValue(valuePlaceholders[i], values[i]);
                int retVal = command.ExecuteNonQuery();
            }
            catch (SqliteException sqliteExc)
            {
                string msg = string.Format("Error on 'INSERT INTO {0}': {1}", tableName, sqliteExc.Message.Split('\n')[1]);
                Console.WriteLine(msg);
                Log.Add(msg, Log.Level.Exception);
            }
            catch (Exception e)
            {
                string msg = string.Format("Exception in executing command: '{0}':\n{1}", commandText, e.Message);
                Log.Add(msg, Log.Level.Exception);
            }

        }

#region DataSet-Interface

        public static Dictionary<Type, DbType> TypeToDbType = new Dictionary<Type, DbType>
        {
            {typeof(byte) , DbType.Byte },
            {typeof(sbyte) , DbType.SByte},
            {typeof(short) , DbType.Int16},
            {typeof(ushort) , DbType.UInt16},
            {typeof(int) , DbType.Int32},
            {typeof(uint) , DbType.UInt32},
            {typeof(long) , DbType.Int64},
            {typeof(ulong) , DbType.UInt64},
            {typeof(float) , DbType.Single},
            {typeof(double) , DbType.Double},
            {typeof(decimal) , DbType.Decimal},
            {typeof(bool) , DbType.Boolean},
            {typeof(string) , DbType.String},
            {typeof(char) , DbType.StringFixedLength},
            {typeof(Guid) , DbType.Guid},
            {typeof(DateTime) , DbType.DateTime},
            {typeof(DateTimeOffset) , DbType.DateTimeOffset},
        };
        public void CreateDatabase(DataSet dataSet)
        {
            try
            {
                OpenConnection();
                BeginTransaction();

                foreach (DataTable dt in dataSet.Tables)
                {
                    CreateTable(dt);
                    //SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(string.Format("CREATE TABLE IF NOT EXISTS '{0}' ({1});", dt.TableName, string.Join(",", dt.Columns))), this.Connection);
                    //dataAdapter.AcceptChangesDuringFill = false;
                    //dataAdapter.Fill(dt);
                }
                CommitTransaction();
                CloseConnection();
            }
            catch (Exception e)
            {
                string msg = "Exception in 'CreateDatabase(DataSet dataSet)':\n" + e.Message;
                Log.Add(msg, Log.Level.Exception);
                //CloseConnection();
            }
        }
        private string GetColumnAttributes(DataTable table)
        {
            List<string> AttributePair = new List<string>();
            foreach (DataColumn c in table.Columns)
                AttributePair.Add(string.Format("'{0}' {1} {2}", c.ColumnName, TypeToDbType[c.DataType], c.AllowDBNull ? "NULL" : "NOT NULL"));

            string attributes = string.Join(",", AttributePair);
            if (attributes == "") attributes = "EMPTY_TABLE";
            else attributes += "CONSTRAINT id_pk PRIMARY KEY (Id)";

            return attributes;
        }

        private int CreateTable(DataTable table)
        {
            int returnValue = 0;
            SqliteCommand sqlCommand = new SqliteCommand(this.Connection);
            string commandText = string.Format("CREATE TABLE IF NOT EXISTS '{0}' ({1});", table.TableName, GetColumnAttributes(table));
            try
            {
                sqlCommand.CommandText = commandText;
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                string msg = string.Format("Exception in executing command: '{0}':\n{1}", commandText, e.Message);
                Log.Add(msg, Log.Level.Exception);
                returnValue = -1;
            }
            return returnValue;
        }
#endregion
    }
}

#endif