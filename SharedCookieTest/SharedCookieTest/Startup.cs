using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SharedCookieTest.Startup))]
namespace SharedCookieTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
