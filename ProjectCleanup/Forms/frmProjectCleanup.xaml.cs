using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            List<string> listFloors = new List<string> { "1", "2", "3" };

            foreach (string floor in listFloors)
            {
                cmbFloors.Items.Add(floor);
            }

            cmbFloors.SelectedIndex = 0;
        }

        internal string GetComboboxClient()
        {
            return cmbClient.Text.ToString();
        }

        internal string GetComboboxFloors()
        {
            return cmbFloors.SelectedItem.ToString();
        }

        internal bool GetCheckBoxViews()
        {
            if (chbViews.IsChecked == true)
            {
                return true;
            }

            return false;
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

        internal bool GetCheckBoxGroups()
        {
            if (chbGroups.IsChecked == true)
            {
                return true;
            }

            return false;
        }

        private void btnAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNone_Click(object sender, RoutedEventArgs e)
        {

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
