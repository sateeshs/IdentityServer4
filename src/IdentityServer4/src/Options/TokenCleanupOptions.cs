using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.Options
{
    public class TokenCleanupOptions
    {
        public int Interval { get; set; } = 60;
    }
}
