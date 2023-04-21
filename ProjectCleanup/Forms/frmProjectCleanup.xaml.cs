using System;
using System.Collections.Generic;
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


namespace ProjectCleanup
{
    /// <summary>
    /// Interaction logic for Window.xaml
    /// </summary>
    public partial class frmProjectCleanup : Window
    {
        public frmProjectCleanup()
        {
            InitializeComponent();

            List<string> listClients = new List<string> { "Central Texas", "Dallas/Ft Worth",
                "Florida", "Houston", "Maryland", "Minnesota", "Oklahoma", "Pennsylvania",
                "Southeast", "Virginia", "West Virginia" };

            foreach (string client in listClients)
            {
                cmbClient.Items.Add(client);
            }

            cmbClient.SelectedIndex = 0;
        }

        internal string GetComboxClient()
        {
            return cmbClient.SelectedItem.ToString();
        }

        internal string GetComboxFloors()
        {
            return cmbFloors.SelectedItem.ToString();
        }

        internal bool GetCheckBoxSchedules()
        {
            if (chbSchedules.IsChecked == true)
            {
                return true;
            }

            return false;
        }

        internal bool GetCheckBoxCode()
        {
            if (chbCode.IsChecked == true)
            {
                return true;
            }

            return false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult= false;
            this.Close();
        }
    }
}
