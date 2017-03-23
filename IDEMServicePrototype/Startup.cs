#define IDEMV3COMPATIBILITY

using IDEMServicePrototype;
using IDEMServicePrototype.App_Start;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;
using System.Web.Http;
using System;
using Microsoft.Owin.Security.OAuth;
using IDEMServicePrototype.Properties;
using QT2K.IDEM.DataModel.Contexts;
using QT2K.IDEM.DataModel.Identity;
using IDEMServicePrototype.Providers;

[assembly: OwinStartupAttribute(typeof(Startup))]
namespace IDEMServicePrototype
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //configurazione autenticazione
            ConfigureOAuth(app);

            //configurazione webapi
            WebApiConfig.Register(config);

            //configurazione unity per DI
           // DependencyInjectionConfig.Register(config);

            //configurazione cors
            app.UseCors(CorsOptions.AllowAll);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            // Configurare il contesto di database e la gestione utenti per l'utilizzo di un'unica istanza per richiesta
#if IDEMV3COMPATIBILITY
            app.CreatePerOwinContext(IdemIdentityContext.Create);
            app.CreatePerOwinContext<IdemMigrationUserManager>(IdemMigrationUserManager.Create);
#else
            app.CreatePerOwinContext(IdemIdentityContext.Create);
            app.CreatePerOwinContext<IdemUserManager>(IdemUserManager.Create);
#endif
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(Settings.Default.TokenLifetime),
                Provider = new IdemMigrationOAuthProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

        }
    }
}