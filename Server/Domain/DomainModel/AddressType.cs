namespace DomainModel
{
    public class AddressType : Enumeration
    {
        public static readonly AddressType Home = new AddressType(0, "Home Address");
        public static readonly AddressType Work = new AddressType(1, "Work Address");

        private AddressType(int value, string displayName) : base(value, displayName)
        {
        }

        public AddressType()
        {
        }
    }
}