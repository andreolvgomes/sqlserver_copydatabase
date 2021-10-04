using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;

namespace CopyDatabase
{
    public class Columns
    {
        public override string ToString()
        {
            return this.COLUMN_NAME;
        }

        public string COLUMN_NAME { get; set; }
        public bool IS_NULLABLE { get; set; }
        public string DATA_TYPE { get; set; }
        public bool IS_PRIMARYKEY { get; set; }
        public bool IS_AUTOINCREMENT { get; set; }
        public int CHARACTER_MAXIMUM_LENGTH { get; set; }
        public bool TIMESTAMP { get; set; }
        public int NUMERIC_PRECISION_RADIX { get; set; }
        public int NUMERIC_PRECISION { get; set; }
        public int NUMERIC_SCALE { get; set; }

        public Columns()
        {
        }

        public static void SetReferences(List<Table> tables, SqlConnection connection)
        {
            try
            {
                foreach (Table table in tables)
                {
                    string commandText = string.Empty;

                    commandText = string.Format(
    @"SELECT
    tab2.name AS [referenced_table]
FROM sys.foreign_key_columns fkc
INNER JOIN sys.objects obj
    ON obj.object_id = fkc.constraint_object_id
INNER JOIN sys.tables tab1
    ON tab1.object_id = fkc.parent_object_id
INNER JOIN sys.schemas sch
    ON tab1.schema_id = sch.schema_id
INNER JOIN sys.columns col1
    ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
INNER JOIN sys.tables tab2
    ON tab2.object_id = fkc.referenced_object_id
INNER JOIN sys.columns col2
    ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id
where tab1.name = '{0}'", table.TABLE_NAME);

                    foreach (var refer in connection.Query(commandText))
                    {
                        string referenced_table = refer.referenced_table;
                        Table tf = tables.FirstOrDefault(c => c.TABLE_NAME.ToUpper() == referenced_table.ToUpper());

                        if (tf == null)
                            tf = new Table() { TABLE_NAME = referenced_table };

                        if (table.References.FirstOrDefault(c => c.TABLE_NAME == tf.TABLE_NAME) == null)
                            table.References.Add(tf);
                    }
                }
            }
            catch (Exception ex)
            {
                Sistema.Sis.Log.SetException(ex);
            }
        }
    }
}