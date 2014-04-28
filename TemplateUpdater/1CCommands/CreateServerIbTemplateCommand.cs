using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateUpdater._1CCommands
{
    public class CreateServerIbTemplateCommand
    {
        private readonly Logger _log = LogManager.GetLogger("CreateServerIbTemplateCommand");

        private readonly string _ibName;

        /// <summary>
        /// Создание серверного шаблона
        /// </summary>
        /// <param name="templateName">Название шаблона</param>
        /// <param name="ibName">Название базы, в которую он будет загружаться</param>
        public CreateServerIbTemplateCommand(string ibName)
        {
            _ibName = ibName;
        }

        public string Execute()
        {
            try
            {
                var restorePath = Path.Combine((ConfigurationManager.AppSettings["db_template_prefix_path"]),
                     _ibName + ".bak");
                
                
                var connectionString = ConfigurationManager.ConnectionStrings["db_billing"];

                var factory = DbProviderFactories.GetFactory(connectionString.ProviderName);
                _log.Info("Restore path: {0}; connection string: {1}; ib name: {2}", restorePath, connectionString, _ibName);

                _log.Trace("Начало загрузки шаблона {0}", _ibName);


                using (var connection = factory.CreateConnection())
                {
                    connection.ConnectionString = connectionString.ConnectionString;
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandTimeout = 600;
                        command.CommandText = String.Format(
                           @"
                            BACKUP DATABASE [{0}] 
	                        TO  DISK = N'{1}' 
	                        WITH NOFORMAT
	                        , INIT
	                        , NAME = N'{2}-Полная База данных Резервное копирование'
	                        , SKIP
	                        , NOREWIND
	                        , NOUNLOAD
	                        , STATS = 10", _ibName, restorePath, _ibName);

                        _log.Info("Query: {0}", command.CommandText);

                        command.CommandType = System.Data.CommandType.Text;

                        command.ExecuteNonQuery();

                        _log.Info("Шаблон {0} успешно загружен", _ibName);
                    }
                }
                return "Успешно завершено.";
            }
            catch (Exception e)
            {
                _log.Error("Ошибка при загрузке шаблона {0}: {1}", _ibName, e.Message);
                return string.Format("ERROR: Ошибка при загрузке шаблона {0}: {1}", _ibName, e.Message);
            }

        }
    }
}
