using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CopyDatabase
{
    public class Connections
    {
        public ConnectionDbAbstract ConnectionTo { get; set; }
        public ConnectionDbAbstract ConnectionFrom { get; set; }
    }

    /// <summary>
    /// Interaction logic for SettingConnect.xaml
    /// </summary>
    public partial class SettingConnect : Window
    {
        private ConnectionDb connect_from = null;
        private ConnectionDb connect_to = null;

        public SettingConnect(Window owner)
        {
            InitializeComponent();

            this.Owner = owner;
            this.txtInstancia.Focus();
        }

        public static Connections Connect(Window owner)
        {
            SettingConnect c = new SettingConnect(owner);
            return c.Connect();
        }

        private Connections Connect()
        {
            this.ShowDialog();
            if (this.connected)
                return new Connections() { ConnectionFrom = this.connect_from, ConnectionTo = this.connect_to };
            return null;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!this.ValidConnectFrom()) return;
                if (!this.ValidConnectTo()) return;
                if (this.txtInstancia.Text.ToUpper().Equals(this.txtInstanciaDest.Text.ToUpper()))
                {
                    MessageBox.Show("As instâncias não podem ser iguais");
                    return;
                }

                this.Close();
                this.connected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool ValidConnectTo()
        {
            if (txtInstanciaDest.Text.NullOrEmpty())
            {
                MessageBox.Show("Informe a Instância", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return this.SelectText(txtInstanciaDest);
            }
            if (this.txtDatabaseDest.Text.NullOrEmpty())
            {
                MessageBox.Show("Informe o Database", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return this.SelectText(txtDatabaseDest);
            }
            if ((bool)this.chkDest.IsChecked)
            {
                if (txtUserDest.Text.NullOrEmpty())
                {
                    MessageBox.Show("Informe o usuário do db", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return this.SelectText(txtUserDest);
                }
                if (txtPwdDest.Password.NullOrEmpty())
                {
                    MessageBox.Show("Informe a senha do db", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return this.SelectText(txtPwdDest);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("server=").Append(txtInstanciaDest.Text).Append(";");
            sb.AppendFormat("database={0};", this.txtDatabaseDest.Text);
            sb.Append("connection timeout=15;");

            sb.Append("user id=").Append((txtUserDest.Text.NullOrEmpty()) ? "sa" : txtUserDest.Text).Append(";");
            sb.Append("pwd=").Append((txtPwdDest.Password.NullOrEmpty()) ? "sa" : txtPwdDest.Password).Append(";");

            this.connect_to = new ConnectionDb(sb.ToString());
            this.connect_to.Open();

            return true;
        }        

        private bool ValidConnectFrom()
        {
            if (txtInstancia.Text.NullOrEmpty())
            {
                MessageBox.Show("Informe a Instância", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return this.SelectText(txtInstancia);
            }
            if(this.txtDatabase.Text.NullOrEmpty())
            {
                MessageBox.Show("Informe o Database", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return this.SelectText(txtDatabase);
            }
            if ((bool)this.chk.IsChecked)
            {
                if (txtUser.Text.NullOrEmpty())
                {
                    MessageBox.Show("Informe o usuário do db", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return this.SelectText(txtUser);
                }
                if (txtPwd.Password.NullOrEmpty())
                {
                    MessageBox.Show("Informe a senha do db", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return this.SelectText(txtPwd);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("server=").Append(txtInstancia.Text).Append(";");
            sb.AppendFormat("database={0};", this.txtDatabase.Text);
            sb.Append("connection timeout=15;");

            sb.Append("user id=").Append((txtUser.Text.NullOrEmpty()) ? "sa" : txtUser.Text).Append(";");
            sb.Append("pwd=").Append((txtPwd.Password.NullOrEmpty()) ? "sa" : txtPwd.Password).Append(";");

            this.connect_from = new ConnectionDb(sb.ToString());
            this.connect_from.Open();

            return true;
        }

        public bool connected { get; set; }

        private bool SelectText(TextBox text)
        {
            text.Focus();
            text.SelectAll();
            return false;
        }

        private bool SelectText(PasswordBox pwd)
        {
            pwd.Focus();
            pwd.SelectAll();
            return false;
        }
    }
}