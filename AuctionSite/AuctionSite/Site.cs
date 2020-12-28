using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

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
              var site =  ctx.Sites.Where(s => s.SiteName.Equals(Name)).SingleOrDefault();
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
                    foreach (var auction in sessionsToClean)
                        ctx.Auctions.RemoveRange(auction.Auctions);

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
            if (!(null == username || null == password))
            {
                if (username.Length >= DomainConstraints.MinUserName && username.Length <= DomainConstraints.MaxUserName && password.Length >= DomainConstraints.MinUserPassword)
                {
                    using (var ctx = new AuctionContext(ConnectionString))
                    {
                        var query = ctx.Sites.Where(s => s.SiteName.Equals(Name)).Single();
                        IsDeleted();
                        if (null != query)
                        {
                            foreach (var item in query.Users)
                            {
                                if (item.Username == username)
                                    throw new NameAlreadyInUseException(nameof(username));
                            }
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
                        else
                            throw new InvalidOperationException("the site has been deleted");

                        
                    }

                }
                else
                    throw new ArgumentException("Username or Password too long/short");

            }
            else
                throw new ArgumentNullException("Username or Password empty");
        }

        public void Delete()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var siteToDelete = ctx.Sites
                                .Where(s => s.SiteName.Equals(Name))
                                .SingleOrDefault();
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
                            .Where(s => s.SiteName.Equals(Name))
                            .SingleOrDefault();
                List<IAuction> list = new List<IAuction>();
                foreach (var auctionField in query.Auctions)
                    if (onlyNotEnded)
                    {
                        if (auctionField.EndsOn > AlarmClock.Now)
                            list.Add(new Auction(auctionField.AuctionId, new User(auctionField.Seller.Username, auctionField.Seller.SiteName) { ConnectionString=ConnectionString,AlarmClock=AlarmClock}, auctionField.Description, auctionField.EndsOn, auctionField.SiteName) { ConnectionString = ConnectionString, AlarmClock = AlarmClock });
                    }
                    else
                        list.Add(new Auction(auctionField.AuctionId, new User(auctionField.Seller.Username, auctionField.Seller.SiteName) { ConnectionString = ConnectionString, AlarmClock = AlarmClock }, auctionField.Description, auctionField.EndsOn, auctionField.SiteName) { ConnectionString = ConnectionString, AlarmClock = AlarmClock });

                //(auctionField.AuctionId,auctionField.CurrentPrice,auctionField.EndsOn,auctionField.FirstBid,auctionField.Seller);
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
                                .Select(s => s.Sessions.Where(a => a.SessionId.Equals(sessionId)).FirstOrDefault())
                                .SingleOrDefault();
                    
                    if (null != query && query.ValidUntill > AlarmClock.Now)
                        return new Session(query.SessionId, query.ValidUntill, new User(query.Username,query.SiteName) { ConnectionString=ConnectionString, AlarmClock =AlarmClock}) { ConnectionString = ConnectionString ,AlarmClock=AlarmClock };
                    else
                        return null;
         
                }
            }
            else
                throw new ArgumentNullException(nameof(sessionId) + "sessionId  must not be null");
        }

        public IEnumerable<ISession> GetSessions()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var query = ctx.Sites
                            .Where(s => s.SiteName.Equals(Name))
                            .SingleOrDefault();

                foreach (var sessionField in query.Sessions)
                {
                    var result = GetSession(sessionField.SessionId);
                    if (null !=result)
                        yield return result ;
                        
                }
   
               
            }
            


        }

        public IEnumerable<IUser> GetUsers()
        {
            using (var ctx = new AuctionContext(ConnectionString))
            {
                IsDeleted();
                var query = ctx.Sites
                            .Where(s => s.SiteName.Equals(Name))
                             .SingleOrDefault();
                List<IUser> list = new List<IUser>();
                foreach (var userField in query.Users)
                        list.Add(new User(userField.Username,userField.SiteName) { ConnectionString=ConnectionString , AlarmClock=AlarmClock});

                return list;

            }
        }

        public ISession Login(string username, string password)
        {
            if (!(null == username || null == password))
            {
                if ((username.Length >= DomainConstraints.MinUserName && username.Length <= DomainConstraints.MaxSiteName && password.Length >= DomainConstraints.MinUserPassword))
                {
                    using (var ctx = new AuctionContext(ConnectionString))
                    {
                        IsDeleted();
                        var query = ctx.Users
                                    .Where(s => s.Username.Equals(username) && s.Password.Equals(password) && s.SiteName.Equals(Name)).SingleOrDefault();
                                    
                        if(null != query)
                        {
                            
                            foreach (var sessions in query.Sessions)
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

                        return null;
                        
                    }
                }
                else
                    throw new ArgumentException();
            }
            else
                throw new ArgumentNullException();
        }


    }
}
