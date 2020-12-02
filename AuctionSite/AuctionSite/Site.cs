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
                        var query = ctx.Sites.Where(s => s.SiteName.Equals(Name)).FirstOrDefault();
                        var users = query.Users;
                        foreach (var item in users)
                        {
                            if (item.Username == username)
                                throw new NameAlreadyInUseException(nameof(username));    
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
            throw new NotImplementedException();
        }

        public ISession GetSession(string sessionId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISession> GetSessions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IUser> GetUsers()
        {
            throw new NotImplementedException();
        }

        public ISession Login(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
