using System.Collections.Generic;

namespace LibStorage
{   
    public interface Storage
    {
        void store(string shardKey, string key, string value);
        void storeText(string shardKey, string key, string text);
        void storeNewShardKey(string chardKey, string segmentId);
        string value(string shardKey, string key);
        bool isTextExist(string text);
        string getSegmentId(string shardKey);       
    }
}