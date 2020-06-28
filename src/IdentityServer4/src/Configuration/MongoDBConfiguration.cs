using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer4.Configuration
{
    public class MongoDBConfiguration
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public SslSettings SslSettings { get; set; }
    }
}
