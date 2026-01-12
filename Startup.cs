using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Khati.Startup))]
namespace Khati
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
