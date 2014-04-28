using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using TemplateUpdater.Helpers;
using TemplateUpdater.Infrastructure;

namespace TemplateUpdater.Pages
{
    /// <summary>
    /// Interaction logic for ListPage1.xaml
    /// </summary>
    public partial class UpdatedTemplates : UserControl
    {
        private readonly Logger _log = LogManager.GetLogger("UpdatedTemplates");

        private readonly XmlDataProvider TemplatesConfig = Application.Current.TryFindResource("TemplatesConfig") as XmlDataProvider;

        public UpdatedTemplates()
        {
            InitializeComponent();
            
        }

        private void TemplateSelector_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            RefreshXmlProvider();
        }

        private void TemplateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshXmlProvider();
        }

        private void UpdateTemplate_Click(object sender, RoutedEventArgs e)
        {
            var xmlSelectedItem = TemplateSelector.SelectedItem as XmlLinkedNode;
            var selectedTemplate = CustomXmlSerializer.GetSelectedTemplateInfoFromXml(xmlSelectedItem.InnerXml);
            var textResult = string.Format(@"Template Name is {0}\n
                CurVersion: {1}\n
                LastVersion: {2}\n", selectedTemplate.IbName, selectedTemplate.CurrentVersion, selectedTemplate.ActualVersion);

            UpdatingProcess(xmlSelectedItem.InnerXml);

            RefreshXmlProvider();

        }

        private void RefreshXmlProvider()
        {
            //XmlDataProvider provider = TemplateSelector.DataContext as XmlDataProvider;
            //if (provider != null)
            //    provider.Refresh();
            _log.Info("Begin Refresh");

            var source = new Uri(ConfigurationManager.AppSettings["TemplatesXmlPath"]);

            try
            {
                if (TemplatesConfig != null)
                {
                    TemplatesConfig.Source = source;
                    _log.Info("TemplatesConfig source: {0}", TemplatesConfig.Source);

                    _log.Info("source.LocalPath {0}", source.LocalPath);
                    //TemplatesConfig.Document.Save(source.LocalPath);
                    _log.Info("Save document");

                    TemplatesConfig.Refresh();
                    _log.Info("Refresh xml");
                }
            }
            catch (Exception ex)
            {
                _log.Error("Refresh xml error: {0}", ex.Message);
            }
            
        }

        

        enum StateProcess 
        { 
            CopyToTempFolder = 1,
            RunInfobaseTemplate = 2,
            UpdateTemplateWithCfu = 3,
            CopyFileIbToTemplateFolder = 4,            
            UploadInfobaseDT = 5,
            DownloadDTToServerIb = 6,
            CreateServerTemplate = 7
        }

        // должна быть соблюдена строгая последовательность состояний
        Dictionary<StateProcess, string> stateProcess = new Dictionary<StateProcess, string>() 
        {
            { StateProcess.CopyToTempFolder, "Копирование фалового шаблона во временную папку" },
            { StateProcess.RunInfobaseTemplate, "Запуск файлового шаблона" },
            { StateProcess.UpdateTemplateWithCfu, "Обновление файлового шаблона с помощью *.cfu" },
            { StateProcess.CopyFileIbToTemplateFolder, "Перемещение в папку файловых шаблонов" },            
            { StateProcess.UploadInfobaseDT, "Выгрузка *.dt" },
            { StateProcess.DownloadDTToServerIb, "Загрузка *.dt в соответствующую серверную базу" },
            { StateProcess.CreateServerTemplate, "Создание обновлённого серверного шаблона" }
        };


        private void UpdatingProcess(string innerXml)
        {
            var resultInfo = new Dictionary<string, string>();

            var stateNumber = 1;
            
            UpdatingProgress.Visibility = Visibility.Visible;
            UpdatingState.Visibility = Visibility.Visible;
            UpdatingProcent.Visibility = Visibility.Visible;

            foreach (var state in stateProcess) 
            {
                UpdatingProgress.Value = (stateNumber * 100 / stateProcess.Keys.ToArray().Length);
                UpdatingState.Text = "Состояние: " + state.Value;
                System.Windows.Forms.Application.DoEvents();

                var result = "";
                var stateKey = state.Key;
                var updateManager = new TemplateUpdateManager(innerXml);
                _log.Info("State {0}", state);

                

                switch (stateKey) 
                { 
                    case StateProcess.CopyToTempFolder:
                        result = updateManager.CopyTemplateToTempFolder();
                        break;
                    case StateProcess.RunInfobaseTemplate:
                        result = updateManager.RunInfobaseTemplate("DESIGNER");
                        break;
                    case StateProcess.UpdateTemplateWithCfu:
                        result = updateManager.UpdateIbTemplateWithCFU();
                        break;
                    case StateProcess.CopyFileIbToTemplateFolder:
                        result = updateManager.CopyFileIbToTemplateFolder();
                        break;                    
                    case StateProcess.UploadInfobaseDT:
                        result = updateManager.UploadInfobaseDT();
                        break;
                    case StateProcess.DownloadDTToServerIb:
                        result = updateManager.DownloadDTToServerIb();
                        break;
                    case StateProcess.CreateServerTemplate:
                        result = updateManager.CreateServerInfobaseTemplate();
                        break;
                }

                //updateManager.CloseDesigner(); раскомментировать когда проблема будет решена
                
                resultInfo.Add(state.Value, result);

                if (CustomChecker.IsProcessFailedDuringExecution(result))
                {
                    var alert = GetAlert(state.Value, result);
                    CheckClick(alert);
                }
                else 
                {
                    var note = GetNotification(state.Value, result);
                    CheckClick(note);
                }

                stateNumber++;
            }

            var content = "";

            foreach (var res in resultInfo)
                content += string.Format(@"{0}: {1} \n", res.Key, res.Value);

            new ModernDialog 
            { 
                Title = "Information", 
                Content = content
            }.ShowDialog();

            UpdatingProgress.Visibility = Visibility.Hidden;
            UpdatingState.Visibility = Visibility.Hidden;
            UpdatingProcent.Visibility = Visibility.Hidden;
        }

        private MessageBoxResult GetAlert(string state, string error)
        {
            var content = string.Format(@"Состояние: {0}\n
                                        Результат: {1}", state, error);

            return ModernDialog.ShowMessage(content, "ERROR", MessageBoxButton.YesNo);
        }

        private MessageBoxResult GetNotification(string state, string result)
        {
            var content = string.Format(@"Состояние: {0}\n
                                        Результат: {1}", state, result);

            return ModernDialog.ShowMessage(content, "INFO", MessageBoxButton.OKCancel);
        }

        private void CheckClick(MessageBoxResult result)
        {
            if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel)
            {
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshXmlProvider();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshXmlProvider();
        }

        

    }

    
}
