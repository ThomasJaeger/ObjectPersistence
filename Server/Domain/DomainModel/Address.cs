namespace DomainModel
{
    public class Address: DomainObject
    {
        private AddressType _addressType = AddressType.Home;

        public Address()
        {
        }

        public Address(string name) : base(name)
        {
        }

        public Address(AddressType addressType)
        {
            _addressType = addressType;
        }

        public AddressType AddressType
        {
            get { return _addressType; }
            set { _addressType = value; }
        }

        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        public override string ToString()
        {
            return Line1 + ", " + City + ", " + State + ", " + Zip;
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
        public static Address NewInstance()
        {
            return new Address(AddressType.Home);
        }
    }
}
