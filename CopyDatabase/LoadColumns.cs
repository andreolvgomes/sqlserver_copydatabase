using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace CopyDatabase
{
    public class LoadColumns : IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Conexão com o db Cliente
        /// </summary>
        private SqlConnection connection_to = null;

        /// <summary>
        /// Conexão com o db Servidor
        /// </summary>
        private SqlConnection connection_from = null;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="connection_to">Conexão com o db Cliente</param>
        /// <param name="connection_from">Conexão com o db Servidor</param>
        public LoadColumns(SqlConnection connection_to, SqlConnection connection_from)
        {
            this.connection_to = connection_to;
            this.connection_from = connection_from;
        }

        /// <summary>
        /// Organiza e retorna colunas da tabela
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public List<Columns> GetColumns(string table)
        {
            List<Columns> columns_result = new List<Columns>();
            try
            {
                string commandTextColumns = string.Format(
@"SELECT * FROM (SELECT DISTINCT
	c.COLUMN_NAME,
	CAST(CASE WHEN c.IS_NULLABLE = 'NO' THEN 0 ELSE 1 END AS BIT) AS IS_NULLABLE,
	c.DATA_TYPE,
    CAST(CASE WHEN c.CHARACTER_MAXIMUM_LENGTH IS NULL THEN 0 ELSE c.CHARACTER_MAXIMUM_LENGTH END AS INT) AS CHARACTER_MAXIMUM_LENGTH,
    CAST(CASE WHEN c.NUMERIC_PRECISION IS NULL THEN 0 ELSE c.NUMERIC_PRECISION END AS INT) AS NUMERIC_PRECISION,
--	CAST(CASE WHEN tc.CONSTRAINT_TYPE IS NULL THEN 0 ELSE 1 END AS BIT) AS IS_PRIMARYKEY,
--	CAST(COLUMNPROPERTY(OBJECT_ID(c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS BIT) AS IS_AUTOINCREMENT,
	c.ORDINAL_POSITION,
    COALESCE(NUMERIC_PRECISION_RADIX, 0) AS NUMERIC_PRECISION_RADIX,
    COALESCE(NUMERIC_SCALE, 0) AS NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS c
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu on c.COLUMN_NAME = kcu.COLUMN_NAME AND c.TABLE_NAME = kcu.TABLE_NAME
LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc on kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
WHERE c.TABLE_NAME = '{0}' and DATA_TYPE <> 'TIMESTAMP') AS r
ORDER BY r.ORDINAL_POSITION", table);

                List<Columns> columns_to = this.connection_to.Query<Columns>(commandTextColumns).ToList();
                List<Columns> columns_from = this.connection_from.Query<Columns>(commandTextColumns).ToList();

                /// retorna somente as colunas que existem no svr e no cli, caso exista alguma
                /// outra coluna em um dos lados, então estas poderão ser ignoradas, senão causará erros
                /// 
                columns_result = (from oFull in columns_to
                                  where (from oFilter in columns_from
                                         select oFilter.COLUMN_NAME
                                    ).Contains(oFull.COLUMN_NAME)
                                  select oFull).ToList();

                Dictionary<string, bool> identityColumns = LoadColumnsIdentity();
                Dictionary<string, bool> primaryColumns = LoadColumnsPrimary();

                foreach (Columns c in columns_result)
                {
                    if (c.DATA_TYPE.ToUpper() == "TEXT")
                        c.CHARACTER_MAXIMUM_LENGTH = 0;
                    else if (c.DATA_TYPE.ToUpper() == "IMAGE")
                        c.CHARACTER_MAXIMUM_LENGTH = 0;
                    else if (c.DATA_TYPE.ToUpper() == "TIMESTAMP")
                        c.TIMESTAMP = true;
                    c.IS_AUTOINCREMENT = IsIdentity(identityColumns, c.COLUMN_NAME, table);
                    c.IS_PRIMARYKEY = IsPrimaryKey(primaryColumns, c.COLUMN_NAME, table);
                }
            }
            catch (Exception ex)
            {
                Sistema.Sis.Log.SetException(ex);
                throw;
            }
            return columns_result;
        }

        /// <summary>
        /// Carrega campos identity da tabela
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, bool> LoadColumnsIdentity()
        {
            string sql = "select COLUMN_NAME, TABLE_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = 'dbo' and COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1 order by TABLE_NAME";
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            var dt = connection_to.Query(sql);
            foreach (var dr in dt)
            {
                dic[(dr.TABLE_NAME + dr.COLUMN_NAME).ToUpperInvariant()] = true;
            }
            return dic;
        }

        /// <summary>
        /// Carrega campos chaves da tabela
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, bool> LoadColumnsPrimary()
        {
            string sql = @"SELECT INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.COLUMN_NAME, INFORMATION_SCHEMA.TABLE_CONSTRAINTS.TABLE_NAME from 
                            INFORMATION_SCHEMA.TABLE_CONSTRAINTS, 
                            INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE 
                        WHERE 
                            INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.Constraint_Name = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.Constraint_Name
                            AND INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE.Table_Name = INFORMATION_SCHEMA.TABLE_CONSTRAINTS.Table_Name
                            AND Constraint_Type = 'PRIMARY KEY'";
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            var dt = connection_to.Query(sql);
            foreach (var dr in dt)
            {
                dic[(dr.TABLE_NAME + dr.COLUMN_NAME).ToUpperInvariant()] = true;
            }
            return dic;
        }

        /// <summary>
        /// Analisa se determinada coluna é identity
        /// </summary>
        /// <param name="identityColumns"></param>
        /// <param name="columnName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool IsIdentity(Dictionary<string, bool> identityColumns, string columnName, string tableName)
        {
            string key = (tableName + columnName).ToUpperInvariant();
            return identityColumns.ContainsKey(key);
        }

        /// <summary>
        /// Analisa se determinada coluna faz parte da PK
        /// </summary>
        /// <param name="primaryColumns"></param>
        /// <param name="columnName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool IsPrimaryKey(Dictionary<string, bool> primaryColumns, string columnName, string tableName)
        {
            string key = (tableName + columnName).ToUpperInvariant();
            return primaryColumns.ContainsKey(key);
        }
    }
}
