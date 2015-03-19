using System;
using System.Collections.Generic;
using System.Security.Principal;
using UtilityFunctions;

namespace DomainModel
{
    public class User: Person, IPrincipal
    {
        public User()
        {
            ApplicationRoles = new List<ApplicationRole>();
            Password = "";
        }

        public User(string Name): base(Name)
        {
            ApplicationRoles = new List<ApplicationRole>();
            Password = "";
        }

        /// <summary>
        /// The hashed password and never the real password
        /// </summary>
        public string Password { get; set; }

        public List<ApplicationRole> ApplicationRoles { get; set; }

        /// <summary>
        /// IPrincipals IsInRole implementation to check roles.
        /// User HasPermission to verify fine grained user permissions.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsInRole(string role)
        {
            foreach (ApplicationRole applicationRole in ApplicationRoles)
            {
                if (string.Equals(applicationRole.Name, role, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check users authorizations on what the user is allowed to do within the system.
        /// These permissions are usually maintained by the business (a manager, 
        /// a business admin, a super user, etc.)
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public bool HasPermission(List<ApplicationPermission> permissions)
        {
            foreach (var applicationPermissionToCheck in permissions)
            {
                foreach (var applicationRole in ApplicationRoles)
                {
                    foreach (ApplicationPermission applicationPermission in applicationRole.ApplicationPermissions)
                    {
                        if (string.Equals(applicationPermission.Name, applicationPermissionToCheck.Name, StringComparison.CurrentCultureIgnoreCase))
                            return true;
                    }
                }
            }
            return false;
        }

        public virtual IIdentity Identity { get; set; }

        public static User NewInstance(string email, string password)
        {
            if ((string.IsNullOrEmpty(email)) || (string.IsNullOrEmpty(password)))
                throw new ApplicationException("email and/or password must not be null");

            if (!Utility.ValidateEmail(email))
                throw new ApplicationException("email address is invalid");

            User user = new User();
            user.Email = email;

            // We do not want to store the actual password for security reasons.
            // We only store the hash of the password. When we compare passwords
            // at a later point, we will use the hash supplied with the hash
            // that we have stored in order to validate the user.
            user.Password = Utility.CalculateSHA1Hash(password);

            return user;
        }

        /// <summary>
        /// passwordToCompare must be the hash value of the password
        /// </summary>
        /// <param name="passwordToCompare"></param>
        /// <returns></returns>
        public bool ArePassswordsSame(string passwordToCompare)
        {
            if (string.IsNullOrEmpty(passwordToCompare))
                return false;

            if (passwordToCompare != Password)
                return false;

            return true;
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if ((string.IsNullOrEmpty(oldPassword)) || (string.IsNullOrEmpty(newPassword)))
                return false;

            // oldPassword must be the hash value of the existing password
            if (oldPassword != Password)
                return false;

            Password = newPassword;
            return true;
        }

    }
}
