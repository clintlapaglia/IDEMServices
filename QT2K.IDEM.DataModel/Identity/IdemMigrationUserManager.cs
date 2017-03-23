using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using QT2K.IDEM.DataModel.Contexts;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections.Generic;
using Core.ViewModels;
using Business.Gestori.Base;

namespace QT2K.IDEM.DataModel.Identity
{
    public class IdemMigrationUserManager : UserManager<aspnet_Membership>, IIdemUserManager
    {
        public IdemMigrationUserManager(IUserStore<aspnet_Membership> store)
            : base(store)
        {
            PasswordHasher = new IdemMigrationPasswordHasher();
        }

        public override async Task<aspnet_Membership> FindByNameAsync(string userName)
        {
            using (var dc = new IdemContext())
            {
                var user = await (from d in dc.aspnet_Membership
                                  where d.aspnet_Users.UserName.Equals(userName, System.StringComparison.OrdinalIgnoreCase)
                                  select d).SingleOrDefaultAsync();
                return user;
            }
        }

        public override async Task<aspnet_Membership> FindAsync(string userName, string password)
        {
            using (var dc = new IdemContext())
            {
                var user = await FindByNameAsync(userName);
                if (user != null)
                {
                    if (PasswordHasher.VerifyHashedPassword($"{user.Password}|{user.PasswordFormat}|{user.PasswordSalt}", password) == PasswordVerificationResult.SuccessRehashNeeded)
                    {
                        return user;
                    }
                }
                return null;
            }
        }

        public override async Task<IList<string>> GetRolesAsync(string userId)
        {
            using (var dc = new IdemContext())
            {
                var userRoles = await (from d in dc.aspnet_Users
                                       where d.UserId.ToString().Equals(userId, System.StringComparison.OrdinalIgnoreCase)
                                       select d.aspnet_Roles).SingleOrDefaultAsync();
                return userRoles.Select(c => c.RoleName).ToList();
            }
        }

        public int? GetUserIdemId(string userName)
        {
            return new GestoreAnagrafePersonale().VerificaEsistenzaUserName(userName);
        }

        public AnagrafePersonaleViewModel GetUserIdemInfo(string userName)
        {
            var business = new GestoreAnagrafePersonale();
            int? idPersonale = business.VerificaEsistenzaUserName(userName);
            if (idPersonale.HasValue)
                return business.Recupera(idPersonale.Value);
            return null;
        }

        public static IdemMigrationUserManager Create(IdentityFactoryOptions<IdemMigrationUserManager> options, IOwinContext context)
        {
            var manager = new IdemMigrationUserManager(new UserStore<aspnet_Membership>(context.Get<IdemIdentityContext>()));
            // Configurare la logica di convalida per i nomi utente
            manager.UserValidator = new UserValidator<aspnet_Membership>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configurare la logica di convalida per le password
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<aspnet_Membership>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }
}
