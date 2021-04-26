using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PetShop.Startup))]
namespace PetShop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
