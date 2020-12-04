using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class SiteFactory : ISiteFactory
    {
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


        private static bool CheckIfNull(string toCheck) { return toCheck.Equals(null); }

        private static void CheckIfStringIsValid(string connectionString)
        {
            if (!(null == connectionString))
            {
                if (!connectionString.Contains("Data Source="))
                    throw new UnavailableDbException(nameof(connectionString) + "Not in the right format");
            }
            else
                throw new ArgumentNullException();

        }
        public void CreateSiteOnDb(string connectionString, string name, int timezone, int sessionExpirationTimeInSeconds, double minimumBidIncrement)
        {
            CheckIfStringIsValid(connectionString);
            if (!(null == name))
            {
                if (name.Length >= DomainConstraints.MinSiteName && name.Length <= DomainConstraints.MaxSiteName)


                {
                    if (timezone >= DomainConstraints.MinTimeZone && timezone <= DomainConstraints.MaxTimeZone )
                    {
                        if (sessionExpirationTimeInSeconds >= 0 && minimumBidIncrement >= 0)
                        {
                            //if 
                            using (var ctx = new AuctionContext(connectionString))
                            {
                                if (CheckConnection(ctx))
                                {
                                    var tmp = ctx.Sites.Where(s => true);

                                    foreach (var item in tmp)
                                    {
                                        if (name == item.SiteName)
                                                
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
                    else
                        throw new ArgumentOutOfRangeException();
                }
                else
                    throw new ArgumentException();
            }
            else
                throw new ArgumentNullException();
        }

        public IEnumerable<string> GetSiteNames(string connectionString)
        {
            CheckIfStringIsValid(connectionString);
            
            using (var ctx = new AuctionContext(connectionString))
            {
                if (CheckConnection(ctx))
                {
                    var names = new List<string>();
                    IQueryable<SiteImpl> site = ctx.Sites.Where(s => true);
                    foreach (var siteItem in site)
                    {
                        names.Add(siteItem.SiteName);

                    }
                    return names;
                        
                }
                else
                    throw new UnavailableDbException();
            }

        }

        public int GetTheTimezoneOf(string connectionString, string name)
        {
            CheckIfStringIsValid(connectionString);
            if (!(null == name))
            {
                if (name.Length >= DomainConstraints.MinSiteName && name.Length <= DomainConstraints.MaxSiteName)
                {
                    using (var ctx = new AuctionContext(connectionString))
                    {
                        if (CheckConnection(ctx))
                        {
                            var site = ctx.Sites.Where(s => s.SiteName.Equals(name));
                           
                            if (site.Any())
                            {
                                foreach (var item in site)
                                {
                                    return item.TimeZone;
                                }
                            }

                            else
                                throw new InexistentNameException(nameof(name));
                        }
                        else
                            throw new UnavailableDbException();

                    }
                }
                else
                    throw new ArgumentException(nameof(name) + "the name lenght is < or > for the allowed length ");

            }

            throw new ArgumentNullException(nameof(name));
        }

        public ISite LoadSite(string connectionString, string name, IAlarmClock alarmClock)
        {
            CheckIfStringIsValid(connectionString);
            if (!(null == name||null == alarmClock ))
            {
                if (name.Length >= DomainConstraints.MinSiteName && name.Length <= DomainConstraints.MaxSiteName)
                {
                    using (var ctx = new AuctionContext(connectionString))
                    {
                        if (CheckConnection(ctx))
                        {
                            
                            var site = ctx.Sites.Where(s => s.SiteName.Equals(name));
                            if (site.Any())
                            {

                                var item = site.FirstOrDefault();
                                if (item.TimeZone == alarmClock.Timezone)
                                {
                                    Site st = new Site(item.SiteName, item.TimeZone, item.SessionExpirationInSeconds, item.MinimunBidIncrement);
                                    st.alarmClock = alarmClock;
                                    st.connectionString = connectionString;
                                    return st;
                                }
                                else
                                    throw new ArgumentException(nameof(alarmClock.Timezone) + "Different from : "+item.TimeZone);
                            
                                
                            }
                            else
                                throw new InexistentNameException(nameof(name));
                        }
                        else
                            throw new UnavailableDbException();

                    }
                }
                else
                    throw new ArgumentException(nameof(name) + "the name lenght is < or > for the allowed length ");

            }
            else
                throw new ArgumentNullException(nameof(name));
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
                throw new UnavailableDbException(e.Message + " The string is malformed"); ;
            }



        }
    }
}
