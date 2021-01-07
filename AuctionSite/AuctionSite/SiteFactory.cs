using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using static AuctionSite.BasicControl;//classe addettata ai controlli 
namespace AuctionSite
{
    public class SiteFactory : ISiteFactory

    {
        //Definisco ogni quanto le sessioni scadute debbano esssere pulite (5 minuti) 
        private const int CleanUpExpiredSessionsTime = 5 * 60 * 1000; 

        
        private static bool CheckConnection(AuctionContext ac)
        {

            try
            {

                ac.Database.Connection.Open();
                ac.Database.Connection.Close();

            }
            catch (SqlException)
            {
                return false;
            }
            return true;


        }


        

      
        public void CreateSiteOnDb(string connectionString, string name, int timezone, int sessionExpirationTimeInSeconds, double minimumBidIncrement)
        {
            CheckIfStringIsValid(connectionString);
            IfNullThrow(name);
            NameSiteNotBetweenRangeThrow(name);
            TimezoneNotBetweenRangeThrow(timezone);
                   
            if (sessionExpirationTimeInSeconds > 0 && minimumBidIncrement > 0)
            {
              
                using (var ctx = new AuctionContext(connectionString))
                {
                    if (CheckConnection(ctx))
                    {
                        var siteList = ctx.Sites.Where(s => true);

                        foreach (var site in siteList)
                        {
                            if (name == site.SiteName)
                                    throw new NameAlreadyInUseException(nameof(name));
                        }
                        try
                        {
                            ctx.Sites.Add(new SiteImpl(name, timezone, minimumBidIncrement, sessionExpirationTimeInSeconds));
                            ctx.SaveChanges();
                        }
                        catch (Exception e)
                        {

                            throw new NameAlreadyInUseException(e.InnerException+nameof(name)); ;
                        }
                        
                        
                    }
                    else
                        throw new UnavailableDbException();
                }
            }
            else
                throw new ArgumentOutOfRangeException();
            
        }

        public IEnumerable<string> GetSiteNames(string connectionString)
        {
            CheckIfStringIsValid(connectionString);
            
            using (var ctx = new AuctionContext(connectionString))
            {
                if (CheckConnection(ctx))
                {
                    var names = new List<string>();
                    var siteList = ctx.Sites.Where(s => true);
                    foreach (var site in siteList)
                        names.Add(site.SiteName);

                    
                    return names;
                        
                }

                throw new UnavailableDbException();
            }

        }

        public int GetTheTimezoneOf(string connectionString, string name)
        {
            CheckIfStringIsValid(connectionString);
            IfNullThrow(name);
            NameSiteNotBetweenRangeThrow(name);
                
            using (var ctx = new AuctionContext(connectionString))
            {
                if (CheckConnection(ctx))
                {
                    var site = ctx.Sites.SingleOrDefault(s => s.SiteName.Equals(name));
                   
                    if (null != site)
                        return site.TimeZone;
                    throw new InexistentNameException(nameof(name));
                }

                throw new UnavailableDbException();

            }
            
        }

        
        public ISite LoadSite(string connectionString, string name, IAlarmClock alarmClock)
        {
            CheckIfStringIsValid(connectionString);
            CheckIfMultipleNull(new object[]{name,alarmClock});
            NameSiteNotBetweenRangeThrow(name);
            using (var ctx = new AuctionContext(connectionString))
            {
                if (CheckConnection(ctx))
                {
                    
                    var site = ctx.Sites.SingleOrDefault(s => s.SiteName.Equals(name));
                    if (null != site)
                    {

                        if (site.TimeZone == alarmClock.Timezone)
                        {
                            
                            var siteNew = new Site(site.SiteName, site.TimeZone, site.SessionExpirationInSeconds, site.MinimunBidIncrement) { AlarmClock = alarmClock, ConnectionString = connectionString };
                            var alarm =alarmClock.InstantiateAlarm(CleanUpExpiredSessionsTime);
                            alarm.RingingEvent += siteNew.OnRingingEvent;
                            return siteNew;
                        }

                        throw new ArgumentException(nameof(alarmClock.Timezone) + "Different from : "+site.TimeZone);


                    }

                    throw new InexistentNameException(nameof(name));
                }

                throw new UnavailableDbException();

            }
                
            
        }

       
        public void Setup(string connectionString)
        {
            CheckIfStringIsValid(connectionString);
            //drop previuosly version of the database 
            if(Database.Exists(connectionString))
                Database.Delete(connectionString);
            //Database.SetInitializer(new DropCreateDatabaseAlways<AuctionContext>());
            try
            {
                using (var ctx = new AuctionContext(connectionString,true))
                {
                    ctx.Database.Create();
                    if (!CheckConnection(ctx)) throw new UnavailableDbException("The Db is not responding");
                    ctx.Database.Initialize(false);
                    ctx.Sites.Create();
                    ctx.Users.Create();
                    ctx.Sessions.Create();
                    ctx.Auctions.Create();
                    ctx.SaveChanges();
                    
                }
            }
            catch (Exception e)
            {
                throw new UnavailableDbException(e.InnerException + " The string is malformed"); ;
            }



        }
    }
}
