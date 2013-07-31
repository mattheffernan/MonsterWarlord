using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using Domain.Entities;
using SharpRepository.Repository;

namespace Web.Membership
{
    public sealed class WebSecurity
    {
        private readonly IRepository<User> _userRepository;

        public WebSecurity(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public static HttpContextBase Context
        {
            get { return new HttpContextWrapper(HttpContext.Current); }
        }

        public static HttpRequestBase Request
        {
            get { return Context.Request; }
        }

        public static HttpResponseBase Response
        {
            get { return Context.Response; }
        }

        public static System.Security.Principal.IPrincipal User
        {
            get { return Context.User; }
        }

        public static bool IsAuthenticated
        {
            get { return User.Identity.IsAuthenticated; }
        }

        public MembershipCreateStatus Register(string username, string password, string email, bool isApproved, string firstName, string lastName)
        {
            MembershipCreateStatus createStatus;
            System.Web.Security.Membership.CreateUser(username, password, email, null, null, isApproved, Guid.NewGuid(), out createStatus);

            if (createStatus == MembershipCreateStatus.Success)
            {
                User user = _userRepository.FirstOrDefault(Usr => Usr.Username == username);
                user.FirstName = firstName;
                user.LastName = lastName;
                _userRepository.Update(user);
                if (isApproved)
                {
                    FormsAuthentication.SetAuthCookie(username, false);
                }
            }

            return createStatus;
        }

        public static Boolean Login(string Username, string Password, bool persistCookie  = false)
        {

            bool success = System.Web.Security.Membership.ValidateUser(Username, Password);
            if (success)
            {
                FormsAuthentication.SetAuthCookie(Username, persistCookie);
            }
            return success;

        }

        public static void Logout()
        {
            FormsAuthentication.SignOut();
        }

        public static MembershipUser GetUser(string Username)
        {
            return System.Web.Security.Membership.GetUser(Username);
        }

        public static bool ChangePassword(string userName, string currentPassword, string newPassword)
        {
            bool success = false;
            try
            {
                MembershipUser currentUser = System.Web.Security.Membership.GetUser(userName, true);
                success = currentUser.ChangePassword(currentPassword, newPassword);
            }
            catch (ArgumentException)
            {
                
            }

            return success;
        }

        public static bool DeleteUser(string Username)
        {
            return System.Web.Security.Membership.DeleteUser(Username);
        }

        public static int GetUserId(string userName)
        {
            MembershipUser user = System.Web.Security.Membership.GetUser(userName);
            return (int)user.ProviderUserKey;
        }
       
        public static string CreateAccount(string userName, string password)
        {
            return CreateAccount(userName, password, requireConfirmationToken: false);
        }

        public static string CreateAccount(string userName, string password, bool requireConfirmationToken = false)
        {
            CodeFirstMembershipProvider CodeFirstMembership = System.Web.Security.Membership.Provider as CodeFirstMembershipProvider;
            return CodeFirstMembership.CreateAccount(userName, password, requireConfirmationToken);
        }

        public static string CreateUserAndAccount(string userName, string password)
        {
            return CreateUserAndAccount(userName, password, propertyValues: null, requireConfirmationToken: false);
        }

        public static string CreateUserAndAccount(string userName, string password, bool requireConfirmation)
        {
            return CreateUserAndAccount(userName, password, propertyValues: null, requireConfirmationToken: requireConfirmation);
        }

        public static string CreateUserAndAccount(string userName, string password, IDictionary<string, object> values)
        {
            return CreateUserAndAccount(userName, password, propertyValues: values, requireConfirmationToken: false);
        }

        public static string CreateUserAndAccount(string userName, string password, object propertyValues = null, bool requireConfirmationToken = false)
        {
            CodeFirstMembershipProvider CodeFirstMembership = System.Web.Security.Membership.Provider as CodeFirstMembershipProvider;

            IDictionary<string, object> values = null;
            if (propertyValues != null)
            {
                values = new RouteValueDictionary(propertyValues);
            }

            return CodeFirstMembership.CreateUserAndAccount(userName, password, requireConfirmationToken, values);
        }
       
        public static List<MembershipUser> FindUsersByEmail(string Email, int PageIndex, int PageSize)
        {
            int totalRecords;
            return System.Web.Security.Membership.FindUsersByEmail(Email, PageIndex, PageSize, out totalRecords).Cast<MembershipUser>().ToList();
        }

        public static List<MembershipUser> FindUsersByName(string Username, int PageIndex, int PageSize)
        {
            int totalRecords;
            return System.Web.Security.Membership.FindUsersByName(Username, PageIndex, PageSize, out totalRecords).Cast<MembershipUser>().ToList();
        }

        public static List<MembershipUser> GetAllUsers(int PageIndex, int PageSize)
        {
            int totalRecords;
            return System.Web.Security.Membership.GetAllUsers(PageIndex, PageSize, out totalRecords).Cast<MembershipUser>().ToList();
        }

        public static void InitializeDatabaseConnection(string connectionStringName, string userTableName, string userIdColumn, string userNameColumn, bool autoCreateTables)
        {
          
        }

        public static void InitializeDatabaseConnection(string connectionString, string providerName, string userTableName, string userIdColumn, string userNameColumn, bool autoCreateTables)
        {
          
        }
    }
}