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

        public IAlarmClock alarmClock { get; set; }
        public string connectionString { get; set; }

        public Site(string name,int timeZone,int sessionExpirationInSeconds,double minimunBidIncrement)
        {
            Name = name;
            Timezone = timeZone;
            SessionExpirationInSeconds = sessionExpirationInSeconds;
            MinimumBidIncrement = minimunBidIncrement;
        }

        
        public void CleanupSessions()
        {
            using (var ctx = new AuctionContext(connectionString))
            {

                var sessionsToClean = ctx.Sessions
                                        .Where(s => s.SiteName.Equals(Name) && s.ValidUntill < DateTime.Now);

                //controllare se il tempo di durata della sessione(oggetto) è scaduto [da implementare]
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
            if (!(null == username || null == password))
            {
                if (username.Length >= DomainConstraints.MinUserName && username.Length <= DomainConstraints.MaxUserName && password.Length >= DomainConstraints.MinUserPassword)
                {
                    using (var ctx = new AuctionContext(connectionString))
                    {
                        var query = ctx.Sites.Where(s => s.SiteName.Equals(Name)).Single();
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

                        // username usr = new username(username, password);
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
            using (var ctx = new AuctionContext(connectionString))
            {
                var siteToDelete = ctx.Sites
                                .Where(s => s.SiteName.Equals(Name))
                                .SingleOrDefault();
                try
                {

                    ctx.Sites.Remove(siteToDelete);
                    ctx.SaveChanges();
                }
                catch (Exception e )
                {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        public IEnumerable<IAuction> GetAuctions(bool onlyNotEnded)
        {
            using (var ctx = new AuctionContext(connectionString))
            {
                var query = ctx.Sites
                            .Where(s => s.SiteName.Equals(Name))
                            .Select(s => s.Auctions);
                if (query.Any())
                {
                    foreach (var item in query)
                    {
                        foreach (var auctionField in item)
                        {
                            Auction auction = new Auction();//(auctionField.AuctionId,auctionField.CurrentPrice,auctionField.EndsOn,auctionField.FirstBid,auctionField.Seller);
                            yield return auction;
                           
                        }
                    }
                }
                else
                    throw new Exception("qualcosa è andato storto mentre cercavo di raccogliere le Aste");


            }
        }

        public ISession GetSession(string sessionId)
        {
            if (null != sessionId)
            {
                using (var ctx = new AuctionContext(connectionString))
                {
                    var query = ctx.Sessions
                                .Where(s => s.SessionId.Equals(sessionId))
                                .SingleOrDefault();
                    if (null != query && query.ValidUntill > alarmClock.Now)
                        return new Session(query.SessionId, query.ValidUntill, new User(query.Username)) { connectionString = connectionString };
                    else
                        return null;
         
                }
            }
            else
                throw new ArgumentNullException(nameof(sessionId) + "sessionId  must not be null");
        }

        public IEnumerable<ISession> GetSessions()
        {
            using (var ctx = new AuctionContext(connectionString))
            {
                var query = ctx.Sites
                            .Where(s => s.SiteName.Equals(Name))
                            .SingleOrDefault();
                            
                if (null != query)
                {
                    
                    foreach (var sessionField in query.Sessions)
                    {
                        var result = GetSession(sessionField.SessionId);
                        if (null !=result)
                            yield return result ;
                        
                    }
   
                   
                }

               
            }
            


        }

        public IEnumerable<IUser> GetUsers()
        {
            using (var ctx = new AuctionContext(connectionString))
            {
                var query = ctx.Sites
                            .Where(s => s.SiteName.Equals(Name))
                            .Select(s => s.Users);
                            
                            
                if (query.Any())
                {
                    foreach (var item in query)
                    {
                        foreach (var userField in item)
                        {
                            User user = new User(userField.Username);
                            user.connectionString = connectionString;
                            yield return user;
                        }
                        
                        
                    }
                }
                else
                    throw new Exception("qualcosa è andato storto mentre cercavo di raccogliere gli utenti");
                    

            }
        }

        public ISession Login(string username, string password)
        {
            if (!(null == username || null == password))
            {
                if ((username.Length >= DomainConstraints.MinUserName && username.Length <= DomainConstraints.MaxSiteName && password.Length >= DomainConstraints.MinUserPassword))
                {
                    using (var ctx = new AuctionContext(connectionString))
                    {

                        var query = ctx.Users
                                    .Where(s => s.Username.Equals(username) && s.Password.Equals(password) && s.SiteName.Equals(Name)).SingleOrDefault();
                                    
                        if(null != query)
                        {
                            
                            foreach (var sessions in query.Sessions)
                            {
 
                                if (sessions.ValidUntill > alarmClock.Now )
                                {
                                    Session session = new Session(sessions.SessionId, sessions.ValidUntill, new User(sessions.Username));
                                    session.connectionString = connectionString;
                                    return session;
                                }
                            }
                            var newSession = new SessionImpl(alarmClock.Now.AddSeconds(SessionExpirationInSeconds), username, Name);
                            try
                            {
                                ctx.Sessions.Add(newSession);
                                ctx.SaveChanges();
                            }
                            catch (Exception e)
                            {

                                throw new Exception(e.Message);
                            }

                            Session sessionsx = new Session(newSession.SessionId, newSession.ValidUntill, new User(username));
                            sessionsx.connectionString = connectionString;
                            return sessionsx;
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
