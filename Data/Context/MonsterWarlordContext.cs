using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using Domain.Entities;
using Domain.Membership;

namespace Data.Context
{
    public class MonsterWarlordContext : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            ConfigureModel(modelBuilder);

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MonsterWarlordContext, Configuration>());

            base.OnModelCreating(modelBuilder);
        }

        private void ConfigureModel(DbModelBuilder modelBuilder)
        {
            var entityMethod = typeof(DbModelBuilder).GetMethod("Entity");

            var entityTypes = Assembly.GetAssembly(typeof(Entity)).GetTypes()
                                      .Where(x => x.IsSubclassOf(typeof(Entity)) && !x.IsAbstract);

            foreach (var type in entityTypes)
            {
                entityMethod.MakeGenericMethod(type).Invoke(modelBuilder, new object[] { });
            }
        }
    }

    public class Configuration : DbMigrationsConfiguration<MonsterWarlordContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MonsterWarlordContext context)
        {
            context.Set<User>().Add(CreateAccount("admin", "password1", false));
        }

        #region seedMethods

        public User CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            string hashedPassword = Crypto.HashPassword(password);

            string token = string.Empty;

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

            return NewUser;
        }
        #endregion
    }
}