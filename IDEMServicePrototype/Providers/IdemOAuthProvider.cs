using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IDEMServicePrototype.Providers
{
    public class IdemOAuthProvider: OAuthAuthorizationServerProvider, IIdemOAuthProvider
    { }
}