namespace DomainModel
{
    public class ApplicationPermission: DomainObject
    {
        public ApplicationPermission()
        {
        }

        public ApplicationPermission(string name) : base(name)
        {
        }

        public static ApplicationPermission NewInstance()
        {
            return new ApplicationPermission();
        }
    }
}
