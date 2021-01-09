using Ninject.Modules;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class AuctionSiteModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAuction>().To<Auction>();
            Bind<ISession>().To<Session>();
            Bind<ISite>().To<Site>();
            Bind<ISiteFactory>().To<SiteFactory>();
            Bind<IUser>().To<User>();
        }
    }
}
