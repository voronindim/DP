using System;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using LibStorage;

namespace LibStorage
{
    public class StorageImpl : Storage
    {
        private readonly string host = "localhost";
        private readonly ILogger<StorageImpl> _logger;
        private IConnectionMultiplexer _conn;
        private IConnectionMultiplexer _connRu;
        private IConnectionMultiplexer _connEu;
        private IConnectionMultiplexer _connOther;
        private readonly string _allTextsKey = "allTextsKey";
        public StorageImpl(ILogger<StorageImpl> logger)
        {
            _logger = logger;
            _conn = ConnectionMultiplexer.Connect(host);
            _connRu = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_RUS", EnvironmentVariableTarget.User));
            _connEu = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_EU", EnvironmentVariableTarget.User));
            _connOther = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_OTHER", EnvironmentVariableTarget.User));
        }

        public string value(string shardKey, string key) { 
            var db = getConnection(shardKey).GetDatabase();
            return db.KeyExists(key) ? db.StringGet(key) : string.Empty;
        }

        public void store(string shardKey, string key, string value) { 
            var database = getConnection(shardKey).GetDatabase();
            if (!database.StringSet(key, value)) { 
                _logger.LogWarning("Fiiled to save {0} : {1}", key, value);
            }

        }
        public void storeText(string shardKey, string key, string text)
        {
            store(shardKey, key, text);
            storeTextToSet(shardKey, text);
        }
        public void storeNewShardKey(string shardKey, string segmentId)
        {
            var db = _conn.GetDatabase();
            if (!db.StringSet(shardKey, segmentId))
            {   
                _logger.LogWarning("Failed to save {0}: {1}", shardKey, segmentId);
            }
        }
        public bool isTextExist(string text)
        {
            var dbRu = _connRu.GetDatabase();
            var dbEu = _connEu.GetDatabase();
            var dbOther = _connOther.GetDatabase();
            return dbRu.SetContains(_allTextsKey, text) || dbEu.SetContains(_allTextsKey, text) || dbOther.SetContains(_allTextsKey, text);
        }
        public string GetSegmentId(string shardKey)
        {
            var db = _conn.GetDatabase();
            return db.KeyExists(shardKey) ? db.StringGet(shardKey) : string.Empty;
        }
        private void storeTextToSet(string shardKey, string text)
        {
            var db = getConnection(shardKey).GetDatabase();
            db.SetAdd(_allTextsKey, text);
        }
        private IConnectionMultiplexer getConnection(string shardKey)
        {
            var db = _conn.GetDatabase();
            if (!db.KeyExists(shardKey))
            {
                _logger.LogWarning("Shard key \"{0}\" doesn't exist", shardKey);
                return _conn;
            }
            var segmentId = db.StringGet(shardKey);
            switch (segmentId)
            {
                case Constants.SEGMENT_ID_RUS:
                    return _connRu;
                case Constants.SEGMENT_ID_EU:
                    return _connEu;
                case Constants.SEGMENT_ID_OTHER:
                    return _connOther;
                default:
                    _logger.LogWarning("Segment {0} doesn't exist", segmentId);
                    return _conn;
            }
        }
        public string getSegmentId(string shardKey)
        { 
            var db = _conn.GetDatabase();
            return db.KeyExists(shardKey) ? db.StringGet(shardKey) : string.Empty;
        }
    }

}