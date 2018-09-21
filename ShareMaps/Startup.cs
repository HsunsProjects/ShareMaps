using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ShareMaps.Startup))]
namespace ShareMaps
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
