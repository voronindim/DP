using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator
{
    public class StorageImpl : Storage
    {
        private readonly ILogger<StorageImpl> _logger;
        private IConnectionMultiplexer _conn;

        public StorageImpl(ILogger<StorageImpl> logger) { 
            _logger = logger;
            _conn = ConnectionMultiplexer.Connect("localhost");
        }

        public string value(string key) { 
            var database = _conn.GetDatabase();
            if (database.KeyExists(key)) { 
                return database.StringGet(key);
            }
            _logger.LogWarning("Key \"{0}\" doesn't exist", key);
            return "";
        }

        public void store(string key, string value) { 
            var database = _conn.GetDatabase();
            if (!database.StringSet(key, value)) { 
                _logger.LogWarning("Fiiled to save {0} : {1}", key, value);
            }

        }

        public Dictionary<string, string> values(string startKey) { 
            var server = _conn.GetServer("localhost", 6379);

            Dictionary<string, string> values = new Dictionary<string, string>();

            foreach (var key in server.Keys(pattern: startKey + "*")) { 
                values.Add(key, value(key));
            }

            return values;

        }
    }

}