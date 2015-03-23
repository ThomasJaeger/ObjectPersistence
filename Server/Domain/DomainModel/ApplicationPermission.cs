namespace DomainModel
{
    /// <summary>
    /// The actual permissions on what a user is allowed to do within he system.
    /// For example, Access Ledger, Print Ledger, etc. these are defined
    /// within the domain and business rules. You probably want the business
    /// users or customers be able to maintain these permissions so that
    /// they can manage their users within the system.
    /// </summary>
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
