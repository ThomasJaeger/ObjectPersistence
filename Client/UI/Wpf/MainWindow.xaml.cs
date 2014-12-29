using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DTO;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<PersonDTO> _personList = new List<PersonDTO>();
        private PersonDTO _personDto = new PersonDTO();
        private frmAddress _frmAddress = new frmAddress();
        private List<AddressTypeDTO> _addressTypes = new List<AddressTypeDTO>();

        public MainWindow()
        {
            InitializeComponent();
            Init();
            UpdateUI();
            txtFirstName.Focus();
        }

        private void Init()
        {
            _addressTypes = ApplicationFacade.GetAllAddressTypes();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            _personList = ApplicationFacade.GetAllPeople();
            lstPeople.ItemsSource = _personList;
            if (_personList.Count > 0)
                lstPeople.SelectedIndex = 0;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            AssignValuesToPersonDto();

            if (ApplicationFacade.CreatePerson(_personDto))
                UpdateUI();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            AssignValuesToPersonDto();

            if (ApplicationFacade.SavePerson(_personDto))
                UpdateUI();
        }

        private void lstPeople_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearFields();
            _personDto = lstPeople.SelectedItem as PersonDTO;
            if (_personDto != null)
            {
                lblId.Content = _personDto.Id;
                txtFirstName.Text = _personDto.FirstName;
                txtLastName.Text = _personDto.LastName;
                txtEmail.Text = _personDto.Email;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_personDto == null) return;
            ApplicationFacade.DeletePerson(_personDto.Id);
            UpdateUI();
        }

        private void AssignValuesToPersonDto()
        {
            _personDto.FirstName = txtFirstName.Text;
            _personDto.LastName = txtLastName.Text;
            _personDto.Email = txtEmail.Text;
        }

        private void btnClearAllFields_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            _personDto = new PersonDTO();

            lblId.Content = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtFirstName.Focus();

            _frmAddress.txtLine1.Text = "";
            _frmAddress.txtLine2.Text = "";
            _frmAddress.txtLine3.Text = "";
            _frmAddress.txtCity.Text = "";
            _frmAddress.txtState.Text = "";
            _frmAddress.txtZip.Text = "";
            _frmAddress.txtCountry.Text = "";
        }

        private void btnHomeAddress_Click(object sender, RoutedEventArgs e)
        {
            if (_personDto == null) return;

            _frmAddress.AddressTypes = _addressTypes;
            _frmAddress.cmbAddressType.ItemsSource = _frmAddress.AddressTypes;
            if (_frmAddress.AddressTypes.Count > 0)
            {
                // NOTE: Make sure you have overridden the Equals() method for the DTOBase class
                // in order for the IndexOf to work correctly.
                _frmAddress.cmbAddressType.SelectedIndex =
                    _frmAddress.cmbAddressType.Items.IndexOf(_personDto.HomeAddress.AddressType);
            }

            _frmAddress.Address = _personDto.HomeAddress;
            _frmAddress.ShowDialog();
            _personDto.HomeAddress = _frmAddress.Address;
        }

        private void btnWorkAddress_Click(object sender, RoutedEventArgs e)
        {
            if (_personDto == null) return;

            _frmAddress.AddressTypes = _addressTypes;
            _frmAddress.cmbAddressType.ItemsSource = _frmAddress.AddressTypes;
            if (_frmAddress.AddressTypes.Count > 1)
            {
                // NOTE: Make sure you have overridden the Equals() method for the DTOBase class
                // in order for the IndexOf to work correctly.
                _frmAddress.cmbAddressType.SelectedIndex =
                    _frmAddress.cmbAddressType.Items.IndexOf(_personDto.WorkAddress.AddressType);
            }

            _frmAddress.Address = _personDto.WorkAddress;
            _frmAddress.ShowDialog();
            _personDto.WorkAddress = _frmAddress.Address;
        }
    }
}
