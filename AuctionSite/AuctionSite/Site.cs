using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using static AuctionSite.BasicControl;
namespace AuctionSite
{
    public class Site : ISite
    {
        public string Name { get;  }

        public int Timezone { get;  }

        public int SessionExpirationInSeconds { get;  }
        public double MinimumBidIncrement { get;  }

        public IAlarmClock AlarmClock { get; set; }
        public string ConnectionString { get; set; }

        public Site(string name,int timeZone,int sessionExpirationInSeconds,double minimunBidIncrement)
        {
            Name = name;
            Timezone = timeZone;
            SessionExpirationInSeconds = sessionExpirationInSeconds;
            MinimumBidIncrement = minimunBidIncrement;
        }

        private void IsDeleted()
        {
            using (var ctx = new AuctionContext(ConnectionString) )
            {
              var site =  ctx.Sites.SingleOrDefault(s => s.SiteName.Equals(Name));
                if (null == site)
                    throw new InvalidOperationException("the site has been deleted");
            }
                   
          
        }
        public void CleanupSessions()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var sessionsToClean = ctx.Sites
                                        .Where(s => s.SiteName.Equals(Name))
                                        .Select(s => s.Sessions.Where(a => a.ValidUntill <= AlarmClock.Now)).SingleOrDefault();

                try
                {
                   
                    ctx.Sessions.RemoveRange(sessionsToClean);
                    ctx.SaveChanges();
                    
                }
                catch (Exception e)
                {

                    throw new Exception(e.InnerException + nameof(e));
                }
               
                            
            }
        }

        public void CreateUser(string username, string password)
        {
            CheckIfMultipleNull(new object[]{username,password});
            
            UsernameNotBetweenRangeThrow(username);
            PasswordLengthIsAboveMinimumAllowedThrow(password);
            
            using (var ctx = new AuctionContext(ConnectionString))
            {
                var site = ctx.Sites.SingleOrDefault(s => s.SiteName.Equals(Name));
                IsDeleted();
                if (site.Users.Any(item => item.Username == username))
                    throw new NameAlreadyInUseException(nameof(username));
                
                try
                {
                    ctx.Users.Add(new UserImpl(username, password, Name));
                    ctx.SaveChanges();
                }
                catch (Exception e)
                {

                    throw new NameAlreadyInUseException(e.InnerException + " " + nameof(username));
                }

            }

        }

        public void Delete()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var siteToDelete = ctx.Sites
                    .SingleOrDefault(s => s.SiteName.Equals(Name));
                try
                {
                    ctx.Auctions.RemoveRange(siteToDelete.Auctions);
                    ctx.Sessions.RemoveRange(siteToDelete.Sessions);
                    ctx.Users.RemoveRange(siteToDelete.Users);
                    ctx.Sites.Remove(siteToDelete);
                    ctx.SaveChanges();
                }
                catch (Exception e )
                {
                    throw new Exception (e.Message + e.InnerException);
                }
            }
        }

        public IEnumerable<IAuction> GetAuctions(bool onlyNotEnded)
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var query = ctx.Sites
                    .SingleOrDefault(s => s.SiteName.Equals(Name));
                var list = new List<IAuction>();
                foreach (var auctionField in query.Auctions)
                    if (onlyNotEnded)
                    {
                        if (auctionField.EndsOn > AlarmClock.Now)
                            list.Add(new Auction(auctionField.AuctionId, new User(auctionField.Seller.Username, auctionField.Seller.SiteName) { ConnectionString=ConnectionString,AlarmClock=AlarmClock}, auctionField.Description, auctionField.EndsOn, auctionField.SiteName) { ConnectionString = ConnectionString, AlarmClock = AlarmClock });
                    }
                    else
                        list.Add(new Auction(auctionField.AuctionId, new User(auctionField.Seller.Username, auctionField.Seller.SiteName) { ConnectionString = ConnectionString, AlarmClock = AlarmClock }, auctionField.Description, auctionField.EndsOn, auctionField.SiteName) { ConnectionString = ConnectionString, AlarmClock = AlarmClock });
                return list;      
                
            }
        }

        public ISession GetSession(string sessionId)
        {
            if (null != sessionId)
            {
                using (var ctx = new AuctionContext(ConnectionString))
                {
                    IsDeleted();
                    var query = ctx.Sites
                                .Where(s => s.SiteName.Equals(Name))
                                .Select(s => s.Sessions.FirstOrDefault(a => a.SessionId.Equals(sessionId)))
                                .SingleOrDefault();
                   
                    if (null != query && query.ValidUntill > AlarmClock.Now)
                        return new Session(query.SessionId, query.ValidUntill, new User(query.Username,query.SiteName) { ConnectionString=ConnectionString, AlarmClock =AlarmClock}) { ConnectionString = ConnectionString ,AlarmClock=AlarmClock };
                    return null;

                }
            }

            throw new ArgumentNullException(nameof(sessionId) + "sessionId  must not be null");
        }

        internal void OnRingingEvent()
        {
            this.CleanupSessions();
        }

        public IEnumerable<ISession> GetSessions()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var site = ctx.Sites
                    .Where(s => s.SiteName.Equals(Name))
                    .Include(s => s.Sessions)
                    .SingleOrDefault();
                    
                return GetSessionSafe(site);

                IEnumerable<ISession> GetSessionSafe(SiteImpl siteIm) { 

                    foreach (var session in siteIm.Sessions)
                    {
                        var result = GetSession(session.SessionId);
                        if (null !=result)
                            yield return result ;
                            
                    }
                }


            }
            


        }

        public IEnumerable<IUser> GetUsers()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var site = ctx.Sites
                    .SingleOrDefault(s => s.SiteName.Equals(Name));

                return site.Users.Select(userField => new User(userField.Username, userField.SiteName) {ConnectionString = ConnectionString, AlarmClock = AlarmClock}).Cast<IUser>().ToList();

            }
        }

        public ISession Login(string username, string password)
        {
            CheckIfMultipleNull(new object []{username,password});
            
            UsernameNotBetweenRangeThrow(username);
            PasswordLengthIsAboveMinimumAllowedThrow(password);
        
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var user = ctx.Sites
                            .Where(s=>s.SiteName.Equals(Name))
                            .Select(s => s.Users.FirstOrDefault(userImpl => userImpl.Username.Equals(username) && userImpl.Password.Equals(password)))
                            .SingleOrDefault();
                try
                {
                    IfNullThrow(user);
                }
                catch (ArgumentNullException)
                {
                    return null;
                }

                foreach (var sessions in user.Sessions)
                {

                    if (sessions.ValidUntill > AlarmClock.Now )
                    {
                        sessions.ValidUntill = AlarmClock.Now.AddSeconds(SessionExpirationInSeconds);
                        ctx.SaveChanges();
                        return new Session(sessions.SessionId, sessions.ValidUntill, new User(sessions.Username,sessions.SiteName)) { ConnectionString = ConnectionString, AlarmClock = AlarmClock };
                    }
                        
                    
                }
                var newSession = new SessionImpl(AlarmClock.Now.AddSeconds(SessionExpirationInSeconds), username, Name);
                try
                {
                    ctx.Sessions.Add(newSession);
                    ctx.SaveChanges();
                }
                catch (Exception e)
                {

                    throw new Exception(e.Message);
                }
                return new Session(newSession.SessionId, newSession.ValidUntill, new User(username, Name) { ConnectionString=ConnectionString,AlarmClock=AlarmClock}) { ConnectionString=ConnectionString,AlarmClock=AlarmClock};
               
                
            }
                
            
        }


    }
}
