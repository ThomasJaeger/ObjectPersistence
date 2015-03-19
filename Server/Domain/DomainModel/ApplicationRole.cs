using System.Collections.Generic;

namespace DomainModel
{
    public class ApplicationRole: DomainObject
    {
        private List<ApplicationPermission> _applicationPermissions = new List<ApplicationPermission>();

        /// <summary>
        /// The actualt permissions on what a user is allowed to do within he system.
        /// For example, Access Ledger, Print Ledger, etc. these are defined
        /// within the domain and business rules. You probably want the business
        /// users or customers be able to maintain these permissions so that
        /// they can manage their users within the system.
        /// </summary>
        public List<ApplicationPermission> ApplicationPermissions
        {
            get { return _applicationPermissions; }
            set { _applicationPermissions = value; }
        }

        /// <summary>
        /// A detailed note about the role and what this role is allowed
        /// to do within the system.
        /// </summary>
        public string Description { get; set; }

        public ApplicationRole()
        {
            Description = string.Empty;
        }

        public ApplicationRole(string Name): base(Name)
        {
            Description = string.Empty;
        }

        public static ApplicationRole NewInstance()
        {
            return new ApplicationRole();
        }
    }
}
