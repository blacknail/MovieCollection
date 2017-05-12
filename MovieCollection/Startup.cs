using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MovieCollection.Startup))]
namespace MovieCollection
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
