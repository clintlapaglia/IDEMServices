using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QT2K.IDEM.DataModel.Contexts
{
    public partial class aspnet_Membership : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<aspnet_Membership> manager, string authenticationType, string userName = "")
        {
            //// Tenere presente che il valore di authenticationType deve corrispondere a quello definito in CookieAuthenticationOptions.AuthenticationType
            //ClaimsIdentity userIdentity = new ClaimsIdentity();

            ////var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            //// Aggiungere qui i reclami utente personalizzati
            //return userIdentity;

            //custom claims. Decommentare il precdente nel momento in cui si implementano le tabelle di AspNet Identity
            //necessario perchè non usiamo tabelle custom
            SecurityStamp = Guid.NewGuid().ToString();
            Id = UserId.ToString();
            UserName = userName;

            IList<Claim> claimCollection = new List<Claim>
            {
                new Claim(ClaimTypes.Name, this.UserName),
                new Claim(ClaimTypes.Email, this.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claimCollection, "Company Portal");
            return await Task.Run(() => claimsIdentity);
        }
    }

    public class IdemIdentityContext : IdentityDbContext<aspnet_Membership>
    {
        public IdemIdentityContext()
            : base("IdemContext", throwIfV1Schema: false)
        {
            //Configuration.ProxyCreationEnabled = false;
            //Configuration.LazyLoadingEnabled = false;
        }

        public static IdemIdentityContext Create()
        {
            return new IdemIdentityContext();
        }
    }
}
