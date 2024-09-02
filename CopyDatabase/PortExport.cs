using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapper;

namespace CopyDatabase
{
    public class PortExport : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private SqlConnection connect_from = null;
        private SqlConnection connect_to = null;

        private Table _Current;

        public Table Current
        {
            get { return _Current; }
            set
            {
                if (_Current == value) return;
                _Current = value;
                this.OnPropertyChanged("Current");
            }
        }

        private List<Table> _Tables;

        public List<Table> Tables
        {
            get { return _Tables; }
            set
            {
                if (_Tables == value) return;
                _Tables = value;
                this.OnPropertyChanged("Tables");
            }
        }

        public event EventHandler Event_Success;
        public bool InTransaction { get; set; }

        public PortExport()
        {
        }

        private void On_Event_Success()
        {
            EventHandler _event = this.Event_Success;
            if (_event != null)
                _event(this, EventArgs.Empty);
        }

        public event EventHandler Event_Executing;

        private void OnEvent_Executing()
        {
            EventHandler _event = this.Event_Executing;
            if (_event != null)
                _event(this, EventArgs.Empty);
        }

        internal void Execute()
        {
            this.Trans();
            this.On_Event_Success();
        }

        private void Trans()
        {
            this.TransExec(this.Tables.FirstOrDefault(c => c.TABLE_NAME == "PW~Grupos" && c.IsChecked));
            this.TransExec(this.Tables.FirstOrDefault(c => c.TABLE_NAME == "PW~Usuarios" && c.IsChecked));
            this.TransExec(this.Tables.FirstOrDefault(c => c.TABLE_NAME == "PW~Tabelas" && c.IsChecked));

            foreach (Table t in this.Tables.Where(c => c.IsChecked))
                this.Trans1(t);
        }

        internal void LoadReferences()
        {
            Columns.SetReferences(this.Tables, connect_to);
        }

        private void Trans1(Table t)
        {
            foreach (Table tf in t.References)
                this.Trans1(tf);
            this.TransExec(t);
        }

        private void TransExec(Table t_executing)
        {
            if (t_executing == null)
                return;

            this.Current = t_executing;
            if (t_executing.IsTrans) return;

            using (LoadColumns d = new LoadColumns(this.connect_to, this.connect_from))
                t_executing.Columns = d.GetColumns(t_executing.TABLE_NAME);

            try
            {
                this.OnEvent_Executing();

                t_executing.Executing = true;
                if (t_executing.Columns.FirstOrDefault(c => c.IS_AUTOINCREMENT) != null)
                    this.connect_to.Execute(string.Format("SET IDENTITY_INSERT dbo.[{0}] ON", t_executing.TABLE_NAME));
                t_executing.Count = this.connect_from.Query<int>(string.Format("select count(1) from dbo.[{0}]", t_executing.TABLE_NAME)).FirstOrDefault();

                using (SqlCommand command = this.Command(t_executing))
                {
                    try
                    {
                        connect_from.OpenIfClosed();
                        // faz transferência dos dados
                        using (SqlCommand command_read = this.connect_from.CreateCommand())
                        {
                            command_read.CommandText = string.Format("select top {0} * from dbo.[{1}]", t_executing.Count, t_executing.TABLE_NAME);
                            using (SqlDataReader reader = command_read.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    command.Parameters.Clear();
                                    foreach (Columns column in t_executing.Columns)
                                    {
                                        if (column.DATA_TYPE.Equals("image"))
                                            command.Parameters.Add(new SqlParameter() { ParameterName = string.Format("@{0}", column.COLUMN_NAME.Replace(" ", "_").Replace("~", "_")), SqlDbType = SqlDbType.Image, Value = reader[column.COLUMN_NAME] });// .AddWithValue(string.Format("@{0}", column.COLUMN_NAME.Replace(" ", "_").Replace("~", "_")), reader[column.COLUMN_NAME]);
                                        else if (column.DATA_TYPE.Equals("varbinary"))
                                            command.Parameters.Add(new SqlParameter() { ParameterName = string.Format("@{0}", column.COLUMN_NAME.Replace(" ", "_").Replace("~", "_")), SqlDbType = SqlDbType.VarBinary, Value = reader[column.COLUMN_NAME] });// .AddWithValue(string.Format("@{0}", column.COLUMN_NAME.Replace(" ", "_").Replace("~", "_")), reader[column.COLUMN_NAME]);
                                        else
                                            command.Parameters.AddWithValue(string.Format("@{0}", column.COLUMN_NAME.Replace(" ", "_").Replace("~", "_")), reader[column.COLUMN_NAME]);
                                    }
                                    t_executing.Count_Current++;
                                    t_executing.Percent = (t_executing.Count_Current * 100) / t_executing.Count;

                                    //if (t_executing.Count_Current == 100)
                                    //    throw new Exception("asdf");
                                    try
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        if (this.InTransaction)
                                            throw;
                                    }
                                    System.Windows.Forms.Application.DoEvents();
                                }
                            }
                        }

                        if (command.Transaction != null)
                            command.Transaction.Commit();

                        Logs.Success(t_executing);
                    }
                    catch (Exception ex)
                    {
                        if (command.Transaction != null)
                            command.Transaction.Rollback();

                        t_executing.Error = true;
                        Sistema.Sis.Log.SetException(ex, this.Xml(t_executing));
                    }
                }
            }
            catch (Exception ex)
            {
                t_executing.Error = true;
                Sistema.Sis.Log.SetException(ex, this.Xml(t_executing));
            }
            finally
            {
                t_executing.IsTrans = true;
                t_executing.Executing = false;
                if (t_executing.Columns.FirstOrDefault(c => c.IS_AUTOINCREMENT) != null)
                    this.connect_to.Execute(string.Format("SET IDENTITY_INSERT dbo.[{0}] OFF", t_executing.TABLE_NAME));
            }
        }

        private System.Xml.Linq.XElement Xml(Table t_executing)
        {
            XElement xElement = new XElement("Transf");
            xElement.Add(new XAttribute("TABLE_NAME", t_executing.TABLE_NAME));
            xElement.Add(new XAttribute("Row_count", t_executing.Row_count));
            return xElement;
        }

        private SqlCommand Command(Table t_executing)
        {
            SqlCommand command = this.connect_to.CreateCommand();
            if (this.InTransaction)
                command.Transaction = this.connect_to.BeginTransaction(IsolationLevel.Serializable, "tr_trans");

            command.CommandTimeout = 7200;
            StringBuilder insert_into = new StringBuilder();
            StringBuilder values = new StringBuilder();

            for (int i = 0; i < t_executing.Columns.Count; i++)
            {
                Columns c = t_executing.Columns[i] as Columns;
                if (i > 0)
                {
                    insert_into.Append(", ");
                    values.Append(", ");
                }
                insert_into.Append(string.Format("[{0}]", c.COLUMN_NAME));
                values.Append(string.Format("@{0}", c.COLUMN_NAME.Replace(" ", "_").Replace("~", "_")));
            }

            if (t_executing.Columns.Count(c => c.IS_PRIMARYKEY) > 0)
            {
                StringBuilder textif = new StringBuilder(string.Format("if not exists (select top 1 [{0}] from dbo.[{1}] where ", t_executing.Columns.FirstOrDefault().COLUMN_NAME, t_executing.TABLE_NAME));
                int j = 0;
                foreach (Columns c in t_executing.Columns.Where(c => c.IS_PRIMARYKEY))
                {
                    if (j > 0)
                        textif.Append(" and ");
                    textif.Append(string.Format("{0} = @{1}", c.COLUMN_NAME, c.COLUMN_NAME.Replace(" ", "_").Replace("~", "_")));
                    j++;
                }
                textif.Append(")");
                command.CommandText = textif.ToString();
                command.CommandText += "\nbegin";
                command.CommandText += string.Format("\n\t\t\tinsert into dbo.[{0}] ({1}) values ({2})", t_executing.TABLE_NAME, insert_into.ToString(), values.ToString());
                command.CommandText += "\nend";
            }
            else
            {
                command.CommandText = string.Format("\t\t\tinsert into dbo.[{0}] ({1}) values ({2})", t_executing.TABLE_NAME, insert_into.ToString(), values.ToString());
            }
            return command;
        }

        internal void SetCnn(Connections cnn)
        {
            this.connect_from = cnn.ConnectionFrom;
            this.connect_to = cnn.ConnectionTo;
        }

        internal void LoadTables()
        {
            try
            {
                var query = @"select
o.name as TABLE_NAME,
ddps.row_count as Row_count
from sys.indexes as i
  inner join sys.objects as o on i.OBJECT_ID = o.OBJECT_ID
  inner join sys.dm_db_partition_stats as ddps on i.OBJECT_ID = ddps.OBJECT_ID and i.index_id = ddps.index_id 
where i.index_id < 2  AND o.is_ms_shipped = 0 and (o.name not like 'RC%' and o.name not like 'R0%' and o.name not like 'R0%') and ddps.row_count > 0
order by o.name";

                //var tables = connect_from.Query("select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE' and TABLE_NAME <> 'Libcomsenha' order by TABLE_NAME");
                Tables = connect_from.Query<Table>(query).ToList();
                foreach (var table in Tables)
                {
                    try
                    {
                        table.IsChecked = true;
                        table.PropertyChanged += new PropertyChangedEventHandler(OnTablePropertyChanged);
                    }
                    catch (Exception ex)
                    {
                        Sistema.Sis.Log.SetException(ex);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void OnTablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                Table table = sender as Table;
                if (table.IsChecked)
                    this.IsCheckReferences(table);
            }
        }

        private void IsCheckReferences(Table table)
        {
            foreach (Table references in table.References)
                references.IsChecked = true;
        }
    }
}