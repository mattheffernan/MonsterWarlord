using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Domain.Entities;
using SharpRepository.Repository;

namespace Web.Membership
{
    public class CodeFirstRoleProvider : RoleProvider
    {
        private IRepository<Role> _roleRepository;  
        public IRepository<Role> RolesRepository
        {
            get { return _roleRepository ?? (_roleRepository = DependencyResolver.Current.GetService<IRepository<Role>>()); }
            set { _roleRepository = value; }
        }
        private IRepository<User> _userRepository;
        public IRepository<User> UserRepository
        {
            get { return _userRepository ?? (_userRepository = DependencyResolver.Current.GetService<IRepository<User>>()); }
            set { _userRepository = value; }
        }
        

        public override string ApplicationName
        {
            get { return GetType().Assembly.GetName().Name; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                ApplicationName = GetType().Assembly.GetName().Name;
            }
        }

        public override bool RoleExists(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return false;
            }
            Role role = RolesRepository.FirstOrDefault(p => p.RoleName == roleName);
            return role != null;
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            if (string.IsNullOrEmpty(roleName))
            {
                return false;
            }
            User user = UserRepository.FirstOrDefault(p => p.Username == username);
            if (user == null)
            {
                return false;
            }
            Role role = RolesRepository.FirstOrDefault(p => p.RoleName == roleName);
            if (role == null)
            {
                return false;
            }
            return user.Roles.Contains(role);
        }

        public override string[] GetAllRoles()
        {
            return RolesRepository.Select(p => p.RoleName).ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return null;
            }
            Role role = RolesRepository.FirstOrDefault(p => p.RoleName == roleName);
            return role != null ? role.Users.Select(p => p.Username).ToArray() : null;
        }

        public override string[] GetRolesForUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }
            User user = UserRepository.FirstOrDefault(p => p.Username == username);
            return user != null ? user.Roles.Select(p => p.RoleName).ToArray() : null;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return null;
            }

            if (string.IsNullOrEmpty(usernameToMatch))
            {
                return null;
            }

            return (
                       from role in RolesRepository
                       from user in role.Users
                       where role.RoleName == roleName && user.Username.Contains(usernameToMatch)
                       select user.Username
                   ).ToArray();
        }

        public override void CreateRole(string roleName)
        {
            if (!string.IsNullOrEmpty(roleName))
            {
                Role role = RolesRepository.FirstOrDefault(p => p.RoleName == roleName);
                if (role == null)
                {
                    var newRole = new Role
                        {
                            RoleId = Guid.NewGuid(),
                            RoleName = roleName
                        };
                    RolesRepository.Add(newRole);
                }
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return false;
            }
            Role role = RolesRepository.FirstOrDefault(p => p.RoleName == roleName);
            if (role == null)
            {
                return false;
            }
            if (throwOnPopulatedRole)
            {
                if (role.Users.Any())
                {
                    return false;
                }
            }
            else
            {
                role.Users.Clear();
            }
            RolesRepository.Delete(role);
            return true;
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            List<User> users = UserRepository.Where(p => usernames.Contains(p.Username)).ToList();
            List<Role> roles = RolesRepository.Where(p => roleNames.Contains(p.RoleName)).ToList();
            foreach (User user in users)
            {
                foreach (Role role in roles)
                {
                    if (!user.Roles.Contains(role))
                    {
                        user.Roles.Add(role);
                    }
                }
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (String username in usernames)
            {
                String us = username;
                User user = UserRepository.FirstOrDefault(p => p.Username == us);
                if (user != null)
                {
                    foreach (String roleName in roleNames)
                    {
                        String rl = roleName;
                        Role role = user.Roles.FirstOrDefault(p => p.RoleName == rl);
                        if (role != null)
                        {
                            user.Roles.Remove(role);
                        }
                    }
                }
            }
        }
    }
}