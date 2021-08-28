using System;

namespace uav.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class UsageAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        private readonly string _usage;
        
        public UsageAttribute(string usage)
        {
            _usage = usage;
        }

        public string Usage => _usage;
    }
}