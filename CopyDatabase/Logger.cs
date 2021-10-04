using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CopyDatabase
{
    public class Logger
    {
        /// <summary>
        /// id do processo current
        /// </summary>
        private int Id = 0;

        /// <summary>
        /// LogErros
        /// </summary>
        public Logger()
        {
            try
            {
                System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
                if (process != null)
                    Id = process.Id;
            }
            catch
            {
            }
        }

        /// <summary>
        /// Grava log .xml
        /// </summary>
        /// <param name="ex">Obj exception</param>
        /// <returns></returns>
        public bool SetException(Exception ex)
        {
            this.GravaLog(ex);
            return false;
        }

        /// <summary>
        /// Grava log .xml
        /// </summary>
        /// <param name="ex">Obj exception</param>
        /// <param name="xElement">Estrutura xml p/ ser adicionado dentro do log</param>
        public bool SetException(Exception ex, XElement xElement)
        {
            this.GravaLog(ex, xElement);
            return false;
        }

        /// <summary>
        /// Grava log .xml
        /// </summary>
        /// <param name="ex">Obj exception</param>
        /// <param name="xElement">Estrutura xml p/ ser adicionado dentro do log</param>
        private void GravaLog(Exception ex, XElement xElement = null)
        {
            try
            {
                Thread sync = new Thread(new ParameterizedThreadStart(SyncGravaLog));
                sync.Start(new Tuple<Exception, XElement>(ex, xElement));
            }
            catch
            {
            }
        }

        /// <summary>
        /// Grava log .xml em background, ou seja, em thread
        /// </summary>
        /// <param name="obj">obj ParameterizedThreadStart</param>
        private void SyncGravaLog(object obj)
        {
            try
            {
                Tuple<Exception, XElement> tuple = obj as Tuple<Exception, XElement>;
                Exception ex = tuple.Item1 as Exception;
                XElement xElement = null;
                if (tuple.Item2 != null)
                    xElement = tuple.Item2 as XElement;

                if (ex.Message.ToUpper().Contains("Deadlock".ToUpper()))
                    return;

                string data = DateTime.Now.ToString("dd/MM/yyyy").Replace("/", "-");
                string nomeArquivo = string.Format("{0}_{1}_log.xml", data, "App");
                string caminho_log_error = System.IO.Path.Combine(Environment.CurrentDirectory, "Auditoria", "Log", nomeArquivo);

                if (!System.IO.Directory.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "Auditoria", "Log"))) System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.CurrentDirectory, "Auditoria", "Log"));
                if (!System.IO.File.Exists(caminho_log_error))
                {
                    XDocument document = new XDocument(new XDeclaration("1.0", "UTF-8", "Yes"));
                    XElement log = new XElement("Log", "");
                    log.Add(new XAttribute("Terminal", Environment.MachineName));
                    document.Add(log);
                    document.Save(caminho_log_error);
                }

                XElement xml_log = XElement.Load(caminho_log_error);

                XElement error = new XElement("Erro",
                    new XElement("Funcao", ex.TargetSite),
                    new XElement("MsgErro", ex.Message),
                    new XElement("LocalizacaoErro", ex.StackTrace),
                    new XElement("InnerException", ((ex.InnerException != null) ? ex.InnerException.Message : "InnerException Null")));

                error.Add(new XAttribute("DataHora", string.Format("{0} {1}", DateTime.Now.ToString("dd/MM/yyyy"), DateTime.Now.ToString("HH:mm:ss"))));
                error.Add(new XAttribute("UserTerminal", Environment.UserName));

                if (xElement == null)
                {
                    xml_log.Add(error);
                }
                else
                {
                    xElement.Add(error);
                    xml_log.Add(xElement);
                }
                xml_log.Save(caminho_log_error);
            }
            catch
            {
            }
        }
    }
}
