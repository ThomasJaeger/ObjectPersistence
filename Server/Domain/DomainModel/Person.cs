using System.Management.Instrumentation;

namespace DomainModel
{
    public class Person: DomainObject
    {
        // Since Person is a complex object, we need to guarantee that any child objects
        // such as the Address objects have been initialized correctly. 
        // For example, if the client did not provide an address DTO inside the 
        // Person DTO, should we initialize the Work Address object with default values
        // such as an Address with a "Work" address type. The home address is a 
        // required address and will be verified inside the person controller, but,
        // the work address is optional.
        // Since our domain contains all business rules, our Person class will
        // enforce the correct address types with the initial creation of the 
        // Person object.
        private Address _homeAddress = Address.NewInstance();
        private Address _workAddress= new Address(AddressType.Work);
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public Address HomeAddress
        {
            get { return _homeAddress; }
            set { _homeAddress = value; }
        }

        public Address WorkAddress
        {
            get { return _workAddress; }
            set { _workAddress = value; }
        }

        public Person()
        {
        }

        public Person(string name) : base(name)
        {
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

        /// <summary>
        /// Execute any internal methods of this class and apply business rules
        /// before handing out a new instance of this class.
        /// 
        /// You may not have any business rules, yet. This is fine and
        /// perfectly normal. As the domain is being developed and especially
        /// being maintained over a long time, you will most likely come back
        /// here and apply new rules to this class. The NewInstance()
        /// factory method will make sure you apply these rules first.
        /// </summary>
        /// <returns></returns>
        public static Person NewInstance()
        {
            return new Person();
        }
    }
}
