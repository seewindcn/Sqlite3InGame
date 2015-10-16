using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

using Mono.Data.Sqlite;

namespace sqlite3
{
	public class Store: IDisposable
	{
		public string dbFile;
		public SqliteConnection conn = null;

		public Store()
		{
		}

		~Store ()
		{
			Close ();
		}

		public void Dispose ()
		{
			Close ();
		}

		public string Connect (string dbFile, string password)
		{
			this.dbFile = dbFile;
			var dbStr = "DbLinqProvider=Sqlite;Data Source=" + dbFile;
			Debug.Log("Connect:" + dbStr);
			conn = new SqliteConnection (dbStr);
			try {
				conn.SetPassword(password);
				conn.Open ();
				return "";
			} catch (Exception err) {
				Debug.Log(err);
				conn = null;
				return err.ToString();
			}
		}

		public void Close ()
		{
			if (conn != null) {
				conn.Close ();
				conn = null;
			}
		}

		public SqliteDataReader ExecuteQuery (string sqlQuery)
		{
			var dbCommand = conn.CreateCommand ();
			dbCommand.CommandText = sqlQuery;
			var reader = dbCommand.ExecuteReader ();
			dbCommand.Dispose ();
			return reader;
		}

		public int ExecuteNonQuery (string sql)
		{
			var dbCommand = conn.CreateCommand ();
			dbCommand.CommandText = sql;
			var count = dbCommand.ExecuteNonQuery ();
			dbCommand.Dispose ();
			return count;
		}

		public object[] LoadFields(string tableName, string query, params string[] fields)
		{
			if (string.IsNullOrEmpty(query)) query = "1=1";
			string fs = string.Join(",", fields);
			var sql = string.Format("select {0} from {1} where {2}", fs, tableName, query);
			Debug.Log("sql="+ sql);
			var reader = ExecuteQuery(sql);

			var len = fields.Length;
			object[] values = new object[len];
			while (reader.Read())
			{
				for (int i = 0; i < len; i++)
				{
					values[i] = reader[fields[i]];
				}
			}
			reader.Close();
			return values;
		}

	}
}





////
////
