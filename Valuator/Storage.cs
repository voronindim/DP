using System.Collections.Generic;

namespace Valuator
{   
    public interface Storage
    {
        void store(string key, string value);
        string value(string key);
        Dictionary<string, string> values(string startKey);        
    }
}