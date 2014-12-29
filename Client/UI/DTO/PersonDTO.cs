namespace DTO
{
    public class PersonDTO: DTOBase
    {
        private AddressDTO _homeAddress = new AddressDTO();
        private AddressDTO _workAddress = new AddressDTO();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public AddressDTO HomeAddress
        {
            get { return _homeAddress; }
            set { _homeAddress = value; }
        }

        public AddressDTO WorkAddress
        {
            get { return _workAddress; }
            set { _workAddress = value; }
        }

        public override string ToString()
        {
            return FirstName + " " + LastName + ", " + Email + ", " + Id;
        }
    }
}
