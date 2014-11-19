using System;

namespace Microsoft.TeamFoundation.Client
{
    public abstract class BaseServerInfo
    {
        protected BaseServerInfo(string name) 
        {
            Name = name;
        }

        public abstract Uri Uri { get; }
        public string Name { get; set; }
    }
}
