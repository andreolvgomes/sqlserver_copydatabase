using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CopyDatabase
{
    public static class Logs
    {
        public static void Success(Table t)
        {
            try
            {
                string path = System.IO.Path.Combine(Environment.CurrentDirectory);
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                path = System.IO.Path.Combine(path, "success.txt");

                if (!System.IO.File.Exists(path))
                {
                    using (TextWriter tw = new StreamWriter(path))
                        tw.WriteLine(t.TABLE_NAME);
                }
                else
                {
                    using (StreamWriter tw = File.AppendText(path))
                        tw.WriteLine(t.TABLE_NAME);
                }
            }
            catch
            {
            }
        }
    }
}