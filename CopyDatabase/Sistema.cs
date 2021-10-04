using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyDatabase
{
    public class Sistema
    {
        private static object syncRoot = new Object();
        public static Sistema _sis;

        public static Sistema Sis
        {
            get
            {
                lock (syncRoot)
                {
                    if (_sis == null)
                        _sis = new Sistema();
                    return _sis;
                }
            }
        }

        public Logger Log { get; private set; }

        private Sistema()
        {
            Log = new Logger();
        }
    }
}