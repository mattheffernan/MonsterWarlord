﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Data.Entities;
using SharpRepository.Repository;

namespace Web.Membership
{
    public class CodeFirstMembershipProvider : MembershipProvider
    {
        private IRepository<User> _userRepository;
        public IRepository<User> UserRepository
        {
            get { return _userRepository ?? (_userRepository = DependencyResolver.Current.GetService<IRepository<User>>()); }
            set { _userRepository = value; }
        }

        #region Properties

        private const int TokenSizeInBytes = 16;

        public override string ApplicationName
        {
            get { return GetType().Assembly.GetName().Name; }
            set { ApplicationName = GetType().Assembly.GetName().Name; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return 5; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 6; }
        }

        public override int PasswordAttemptWindow
        {
            get { return 0; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return String.Empty; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }

        #endregion

        #region Functions

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion,
                                                  string passwordAnswer, bool isApproved, object providerUserKey,
                                                  out MembershipCreateStatus status)
        {
            if (string.IsNullOrEmpty(username))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }
            if (string.IsNullOrEmpty(password))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            if (string.IsNullOrEmpty(email))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            string HashedPassword = Crypto.HashPassword(password);
            if (HashedPassword.Length > 128)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (UserRepository.Where(Usr => Usr.Username == username).Any())
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            if (UserRepository.Where(Usr => Usr.Email == email).Any())
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            var NewUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = username,
                    Password = HashedPassword,
                    IsApproved = isApproved,
                    Email = email,
                    CreateDate = DateTime.UtcNow,
                    LastPasswordChangedDate = DateTime.UtcNow,
                    PasswordFailuresSinceLastSuccess = 0,
                    LastLoginDate = DateTime.UtcNow,
                    LastActivityDate = DateTime.UtcNow,
                    LastLockoutDate = DateTime.UtcNow,
                    IsLockedOut = false,
                    LastPasswordFailureDate = DateTime.UtcNow
                };

            UserRepository.Add(NewUser);
            status = MembershipCreateStatus.Success;
            return new MembershipUser(System.Web.Security.Membership.Provider.Name, NewUser.Username, NewUser.UserId, NewUser.Email, null, null,
                                      NewUser.IsApproved, NewUser.IsLockedOut, NewUser.CreateDate.Value,
                                      NewUser.LastLoginDate.Value, NewUser.LastActivityDate.Value,
                                      NewUser.LastPasswordChangedDate.Value, NewUser.LastLockoutDate.Value);
        }

        public string CreateUserAndAccount(string userName, string password, bool requireConfirmation,
                                           IDictionary<string, object> values)
        {
            return CreateAccount(userName, password, requireConfirmation);
        }

        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            User User = null;
            User = UserRepository.FirstOrDefault(Usr => Usr.Username == username);
            if (User == null)
            {
                return false;
            }
            if (!User.IsApproved)
            {
                return false;
            }
            if (User.IsLockedOut)
            {
                return false;
            }
            String HashedPassword = User.Password;
            Boolean VerificationSucceeded = (HashedPassword != null && Crypto.VerifyHashedPassword(HashedPassword, password));
            if (VerificationSucceeded)
            {
                User.PasswordFailuresSinceLastSuccess = 0;
                User.LastLoginDate = DateTime.UtcNow;
                User.LastActivityDate = DateTime.UtcNow;
            }
            else
            {
                int Failures = User.PasswordFailuresSinceLastSuccess;
                if (Failures < MaxInvalidPasswordAttempts)
                {
                    User.PasswordFailuresSinceLastSuccess += 1;
                    User.LastPasswordFailureDate = DateTime.UtcNow;
                }
                else if (Failures >= MaxInvalidPasswordAttempts)
                {
                    User.LastPasswordFailureDate = DateTime.UtcNow;
                    User.LastLockoutDate = DateTime.UtcNow;
                    User.IsLockedOut = true;
                }
            }
            if (VerificationSucceeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }
            User User = null;
            User = UserRepository.FirstOrDefault(Usr => Usr.Username == username);
            if (User != null)
            {
                if (userIsOnline)
                {
                    User.LastActivityDate = DateTime.UtcNow;
                    UserRepository.Update(User);
                }
                return new MembershipUser(System.Web.Security.Membership.Provider.Name, User.Username, User.UserId, User.Email, null, null,
                                          User.IsApproved, User.IsLockedOut, User.CreateDate.Value, User.LastLoginDate.Value,
                                          User.LastActivityDate.Value, User.LastPasswordChangedDate.Value,
                                          User.LastLockoutDate.Value);
            }
            else
            {
                return null;
            }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey is Guid)
            {
            }
            else
            {
                return null;
            }

            User User = null;
            User = UserRepository.Find(p => p.UserId == (Guid) providerUserKey);
            if (User != null)
            {
                if (userIsOnline)
                {
                    User.LastActivityDate = DateTime.UtcNow;
                    UserRepository.Update(User);
                }
                return new MembershipUser(System.Web.Security.Membership.Provider.Name, User.Username, User.UserId, User.Email, null, null,
                                          User.IsApproved, User.IsLockedOut, User.CreateDate.Value, User.LastLoginDate.Value,
                                          User.LastActivityDate.Value, User.LastPasswordChangedDate.Value,
                                          User.LastLockoutDate.Value);
            }
            else
            {
                return null;
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            if (string.IsNullOrEmpty(oldPassword))
            {
                return false;
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                return false;
            }
            User User = null;
            User = UserRepository.FirstOrDefault(Usr => Usr.Username == username);
            if (User == null)
            {
                return false;
            }
            String HashedPassword = User.Password;
            Boolean VerificationSucceeded = (HashedPassword != null &&
                                             Crypto.VerifyHashedPassword(HashedPassword, oldPassword));
            if (VerificationSucceeded)
            {
                User.PasswordFailuresSinceLastSuccess = 0;
            }
            else
            {
                int Failures = User.PasswordFailuresSinceLastSuccess;
                if (Failures < MaxInvalidPasswordAttempts)
                {
                    User.PasswordFailuresSinceLastSuccess += 1;
                    User.LastPasswordFailureDate = DateTime.UtcNow;
                }
                else if (Failures >= MaxInvalidPasswordAttempts)
                {
                    User.LastPasswordFailureDate = DateTime.UtcNow;
                    User.LastLockoutDate = DateTime.UtcNow;
                    User.IsLockedOut = true;
                }
                UserRepository.Update(User);
                return false;
            }
            String NewHashedPassword = Crypto.HashPassword(newPassword);
            if (NewHashedPassword.Length > 128)
            {
                return false;
            }
            User.Password = NewHashedPassword;
            User.LastPasswordChangedDate = DateTime.UtcNow;
            UserRepository.Update(User);
            return true;
        }

        public override bool UnlockUser(string userName)
        {
            User User = null;
            User = UserRepository.FirstOrDefault(Usr => Usr.Username == userName);
            if (User != null)
            {
                User.IsLockedOut = false;
                User.PasswordFailuresSinceLastSuccess = 0;
                UserRepository.Update(User);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetNumberOfUsersOnline()
        {
            DateTime DateActive =
                DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(Convert.ToDouble(System.Web.Security.Membership.UserIsOnlineTimeWindow)));
            return UserRepository.Count(Usr => Usr.LastActivityDate > DateActive);
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            User User = null;
            User = UserRepository.FirstOrDefault(Usr => Usr.Username == username);
            if (User != null)
            {
                UserRepository.Delete(User);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            User User = null;
            User = UserRepository.FirstOrDefault(Usr => Usr.Email == email);
            if (User != null)
            {
                return User.Username;
            }
            else
            {
                return string.Empty;
            }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
                                                                  out int totalRecords)
        {
            var MembershipUsers = new MembershipUserCollection();
            totalRecords = UserRepository.Count(Usr => Usr.Email == emailToMatch);
            IEnumerable<User> Users =
                UserRepository.Where(Usr => Usr.Email == emailToMatch)
                               .OrderBy(Usrn => Usrn.Username)
                               .Skip(pageIndex*pageSize)
                               .Take(pageSize);
            foreach (User user in Users)
            {
                MembershipUsers.Add(new MembershipUser(System.Web.Security.Membership.Provider.Name, user.Username, user.UserId, user.Email,
                                                       null, null, user.IsApproved, user.IsLockedOut, user.CreateDate.Value,
                                                       user.LastLoginDate.Value, user.LastActivityDate.Value,
                                                       user.LastPasswordChangedDate.Value, user.LastLockoutDate.Value));
            }
            return MembershipUsers;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
                                                                 out int totalRecords)
        {
            var MembershipUsers = new MembershipUserCollection();
            totalRecords = UserRepository.Count(Usr => Usr.Username == usernameToMatch);
            IEnumerable<User> Users =
                UserRepository.Where(Usr => Usr.Username == usernameToMatch)
                               .OrderBy(Usrn => Usrn.Username)
                               .Skip(pageIndex*pageSize)
                               .Take(pageSize);
            foreach (User user in Users)
            {
                MembershipUsers.Add(new MembershipUser(System.Web.Security.Membership.Provider.Name, user.Username, user.UserId, user.Email,
                                                       null, null, user.IsApproved, user.IsLockedOut, user.CreateDate.Value,
                                                       user.LastLoginDate.Value, user.LastActivityDate.Value,
                                                       user.LastPasswordChangedDate.Value, user.LastLockoutDate.Value));
            }
            return MembershipUsers;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var MembershipUsers = new MembershipUserCollection();
            totalRecords = UserRepository.Count();
            IEnumerable<User> Users = UserRepository.OrderBy(Usrn => Usrn.Username).Skip(pageIndex*pageSize).Take(pageSize);
            foreach (User user in Users)
            {
                MembershipUsers.Add(new MembershipUser(System.Web.Security.Membership.Provider.Name, user.Username, user.UserId, user.Email,
                                                       null, null, user.IsApproved, user.IsLockedOut, user.CreateDate.Value,
                                                       user.LastLoginDate.Value, user.LastActivityDate.Value,
                                                       user.LastPasswordChangedDate.Value, user.LastLockoutDate.Value));
            }
            return MembershipUsers;
        }

        public string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidUserName);
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            string hashedPassword = Crypto.HashPassword(password);
            if (hashedPassword.Length > 128)
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.InvalidPassword);
            }

            if (UserRepository.Where(Usr => Usr.Username == userName).Any())
            {
                throw new MembershipCreateUserException(MembershipCreateStatus.DuplicateUserName);
            }

            string token = string.Empty;
            if (requireConfirmationToken)
            {
                token = GenerateToken();
            }

            var NewUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = userName,
                    Password = hashedPassword,
                    IsApproved = !requireConfirmationToken,
                    Email = string.Empty,
                    CreateDate = DateTime.UtcNow,
                    LastPasswordChangedDate = DateTime.UtcNow,
                    PasswordFailuresSinceLastSuccess = 0,
                    LastLoginDate = DateTime.UtcNow,
                    LastActivityDate = DateTime.UtcNow,
                    LastLockoutDate = DateTime.UtcNow,
                    IsLockedOut = false,
                    LastPasswordFailureDate = DateTime.UtcNow,
                    ConfirmationToken = token
                };

            UserRepository.Add(NewUser);
            return token;
        }

        private static string GenerateToken()
        {
            using (var prng = new RNGCryptoServiceProvider())
            {
                return GenerateToken(prng);
            }
        }

        internal static string GenerateToken(RandomNumberGenerator generator)
        {
            var tokenBytes = new byte[TokenSizeInBytes];
            generator.GetBytes(tokenBytes);
            return HttpServerUtility.UrlTokenEncode(tokenBytes);
        }

        #endregion

        #region Not Supported

        //CodeFirstMembershipProvider does not support password retrieval scenarios.
        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        //CodeFirstMembershipProvider does not support password reset scenarios.
        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        //CodeFirstMembershipProvider does not support question and answer scenarios.
        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException("Consider using methods from WebSecurity module.");
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotSupportedException("Consider using methods from WebSecurity module.");
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion,
                                                             string newPasswordAnswer)
        {
            throw new NotSupportedException("Consider using methods from WebSecurity module.");
        }

        //CodeFirstMembershipProvider does not support UpdateUser because this method is useless.
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}