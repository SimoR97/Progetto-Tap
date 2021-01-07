using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
   internal static class BasicControl
    {

        public static void IfNullThrow(object checkIfNull) => _ = checkIfNull ?? throw new ArgumentNullException(nameof(checkIfNull));

        public static void NameSiteNotBetweenRangeThrow(string sitName)
        { 
            if( sitName.Length < DomainConstraints.MinSiteName || sitName.Length > DomainConstraints.MaxSiteName)
                  throw new ArgumentException(nameof(sitName) + $" The name lenght is < {DomainConstraints.MinSiteName} or > {DomainConstraints.MaxSiteName} for the allowed length ");
        }

        public static void TimezoneNotBetweenRangeThrow(int timezone)
        {
            if (timezone < DomainConstraints.MinTimeZone || timezone > DomainConstraints.MaxTimeZone)
                    throw new ArgumentOutOfRangeException(nameof(timezone) + $" The timezone  is < {DomainConstraints.MinTimeZone} or > {DomainConstraints.MaxTimeZone}  ");
        }

      public static void CheckIfMultipleNull(IEnumerable<object> checkIfNullList)
      {
          foreach (var checkIfNull in checkIfNullList)
              IfNullThrow(checkIfNull);
      }

      public static void CheckIfStringIsValid(string connectionString)
      {
          IfNullThrow(connectionString);
          if (!connectionString.Contains("Data Source="))
              throw new UnavailableDbException(nameof(connectionString) + "Not in the right format");
         

      }
    }
}
