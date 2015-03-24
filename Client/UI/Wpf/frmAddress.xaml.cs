using System.Collections.Generic;
using System.Windows;
using DTO;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for frmAddress.xaml
    /// </summary>
    public partial class frmAddress : Window
    {
        private AddressDTO _address;
        public List<AddressTypeDTO> AddressTypes { get; set; }

        public AddressDTO Address
        {
            get
            {
                if (_address == null)
                    _address = new AddressDTO();
                return _address;
            }
            set { _address = value; }
        }

        public frmAddress()
        {
            InitializeComponent();
        }

        private void UpdateUI()
        {
            txtLine1.Text = Address.Line1;
            txtLine2.Text = Address.Line2;
            txtLine3.Text = Address.Line3;
            txtCity.Text = Address.City;
            txtState.Text = Address.State;
            txtZip.Text = Address.Zip;
            txtCountry.Text = Address.Country;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Address.AddressType = cmbAddressType.SelectedItem as AddressTypeDTO;
            Address.Line1 = txtLine1.Text;
            Address.Line2 = txtLine2.Text;
            Address.Line3 = txtLine3.Text;
            Address.City = txtCity.Text;
            Address.State = txtState.Text;
            Address.Zip = txtZip.Text;
            Address.Country = txtCountry.Text;

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            UpdateUI();
        }
    }
}
