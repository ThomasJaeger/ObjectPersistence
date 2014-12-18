using System.Collections.Generic;
using System.Windows;
using DTO;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<PersonDTO> _personList = new List<PersonDTO>();
        private PersonDTO _personDto;

        public MainWindow()
        {
            InitializeComponent();
            UpdateUI();
            txtFirstName.Focus();
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
            if (ApplicationFacade.CreatePerson(txtFirstName.Text, txtLastName.Text, txtEmail.Text))
                UpdateUI();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            _personDto.FirstName = txtFirstName.Text;
            _personDto.LastName = txtLastName.Text;
            _personDto.Email = txtEmail.Text;
            if (ApplicationFacade.SavePerson(_personDto))
                UpdateUI();
        }

        private void lstPeople_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
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

        private void btnClearAllFields_Click(object sender, RoutedEventArgs e)
        {
            lblId.Content = "";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtFirstName.Focus();
        }
    }
}
