using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class SqlConnectionExtensions
    {
        public static void OpenIfClosed(this SqlConnection cnn)
        {
            if (cnn.State != Data.ConnectionState.Open)
                cnn.Open();
        }
    }
}