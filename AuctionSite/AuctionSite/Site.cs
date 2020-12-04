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
            throw new NotImplementedException();
        }

        public void CreateUser(string username, string password)
        {
            if (!(null == username || null == password))
            {
                if (username.Length >= DomainConstraints.MinUserName && username.Length <= DomainConstraints.MaxSiteName && password.Length >= DomainConstraints.MinUserPassword)
                {
                    using (var ctx = new AuctionContext(connectionString))
                    {
                        var query = ctx.Sites.Where(s => s.SiteName.Equals(Name)).FirstOrDefault().Users;
                        foreach (var item in query)
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

                            throw new NameAlreadyInUseException(e.InnerException+" "+nameof(username));
                        }
                       
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
            throw new NotImplementedException();
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
                            Auction auction = new Auction(auctionField.AuctionId,auctionField.CurrentPrice,auctionField.EndsOn,auctionField.FirstBid,auctionField.Seller);
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
            throw new NotImplementedException();
        }

        public IEnumerable<ISession> GetSessions()
        {
            using (var ctx = new AuctionContext(connectionString))
            {
                var query = ctx.Sites
                            .Where(s => s.SiteName.Equals(Name))
                            .Select(s => s.Sessions);


                if (query.Any())
                {
                    foreach (var item in query)
                    {
                        foreach (var sessionField in item)
                        {
                            Session session = new Session(sessionField.SessionId,sessionField.ValidUntill);
                            yield return session;
                        }


                    }
                }
                else
                    throw new Exception("qualcosa è andato storto mentre cercavo di raccogliere gli utenti");


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
            throw new NotImplementedException();
        }
    }
}
