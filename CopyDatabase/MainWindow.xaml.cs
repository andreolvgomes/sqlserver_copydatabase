using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace CopyDatabase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PortExport provider;

        public MainWindow()
        {
            InitializeComponent();

            this.provider = new PortExport();
            this.DataContext = this.provider;
            this.provider.Event_Success += new EventHandler(Success);
            this.provider.Event_Executing += new EventHandler(Executing);
        }

        private void Executing(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ls.ScrollIntoView(this.provider.Current);
            }));
        }

        private void Success(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.btn.IsEnabled = true;
                this.btnConnect.IsEnabled = true;
                this.chkTransaction.IsEnabled = true;
            }));
            MessageBox.Show("Concluído");
        }

        private void Trans(object sender, RoutedEventArgs e)
        {
            this.btn.IsEnabled = false;
            this.btnConnect.IsEnabled = false;
            this.chkTransaction.IsEnabled = false;
            this.provider.Tables.ForEach(c => c.IsTrans = false);
            this.provider.InTransaction = (bool)this.chkTransaction.IsChecked;
            new Thread(new ThreadStart(this.provider.Execute)).Start();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Connections cnn = SettingConnect.Connect(this);
                if (cnn != null)
                {
                    this.provider.SetCnn(cnn);
                    this.provider.LoadTables();

                    this.btn.IsEnabled = true;
                    this.chkTransaction.IsEnabled = true;
                    this.chkAll.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            if (this.provider.Tables == null || this.provider.Tables.Count == 0)
            {
                check.IsChecked = false;
                return;
            }
            
            if ((bool)check.IsChecked)
                this.provider.Tables.ForEach(s => s.IsChecked = true);
            else
                this.provider.Tables.ForEach(s => s.IsChecked = false);
        }
    }
}