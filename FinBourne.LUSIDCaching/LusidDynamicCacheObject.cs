using System;
using System.Collections.Generic;
using System.Text;

namespace FinBourne.LUSIDCaching
{
    /// <summary>
    /// A basic object that has the main purpose of keeping track of when the object was last accessed
    /// </summary>
    public class LusidDynamicCacheObject
    {
        public string Key { get; set; } 
        public object Value { get; set; }
        public long LastAccessed { get; set; }
    }
}
