using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using QT2K.IDEM.DataModel.Identity;
using QT2K.IDEM.DataModel.Contexts;
using Newtonsoft.Json;

namespace IDEMServicePrototype.Providers
{
    public class IdemMigrationOAuthProvider : OAuthAuthorizationServerProvider, IIdemOAuthProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            if (allowedOrigin == null) allowedOrigin = "*";
            context.OwinContext.Set<string>("as:clientAllowedOrigin", allowedOrigin);

            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            try
            {
                var userManager = context.OwinContext.GetUserManager<IdemMigrationUserManager>();
                aspnet_Membership user = null;
                user = await userManager.FindByNameAsync(context.UserName);

                //verifico l'esistenza dell'utente
                if (user == null)
                {
                    context.SetError("Credenziali non valide", "Username inesistente.");
                    return;
                }

                //abilta CORS aggiungendo le origini abilitate per le richieste (javascript da un altor dominio)
                var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
                if (allowedOrigin == null) allowedOrigin = "*";
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

                //var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
                ////abilta CORS TODO da verificare
                //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

                user = await userManager.FindAsync(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "Username o password non corretta.");
                    return;
                }

                ClaimsIdentity oAuthIdentity = null;

                try
                {
                    //Generazione 
                    oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, OAuthDefaults.AuthenticationType, context.UserName);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //Recupero la lista dei ruoli
                //List<Claim> roles = oAuthIdentity.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

                //creazione delle proprietà con aggiunta della lista dei ruoli
                IdemClaims idemClaims = new IdemClaims();
                idemClaims.UserName = user.UserName;
                idemClaims.Roles = JsonConvert.SerializeObject(await userManager.GetRolesAsync(user.Id));
                var info = userManager.GetUserIdemInfo(user.UserName);
                if (info != null)
                {
                    idemClaims.Id = info.IDPersonale;
                    idemClaims.Cognome = info.Cognome;
                    idemClaims.Nome = info.Nome;
                }

                AuthenticationProperties properties = CreateCustomProperties(idemClaims);
                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);

                context.Validated(ticket);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //cutomizzazione dei parametri della response. Aggiungo username e ruoli utente
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        private static AuthenticationProperties CreateCustomProperties(IdemClaims claims)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", claims.UserName },
                { "roles", claims.Roles}
            };
            if (claims.Id > 0)
                data.Add("id", claims.Id.ToString());
            if (!string.IsNullOrWhiteSpace(claims.Cognome))
                data.Add("cognome", claims.Cognome);
            if (!string.IsNullOrWhiteSpace(claims.Nome))
                data.Add("nome", claims.Nome);

            return new AuthenticationProperties(data);
        }
    }
}