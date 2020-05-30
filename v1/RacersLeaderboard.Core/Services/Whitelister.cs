using System.Collections.Generic;
using System.Linq;

namespace RacersLeaderboard.Core.Services
{
    public interface IWhitelister
    {
        bool IsWhitelisted(string customerId);
        bool IsWhitelisted(int customerId);
    }

    public class Whitelister : IWhitelister
    {
        private List<string> _custIds;
        public Whitelister(List<string> custIds)
        {
            _custIds = custIds;
        }

        public bool IsWhitelisted(string customerId) => _custIds.Any(id => id == customerId);
        public bool IsWhitelisted(int customerId) => _custIds.Any(id => id == customerId.ToString());
    }
}
