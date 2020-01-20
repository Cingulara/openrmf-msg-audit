// Copyright (c) Cingulara LLC 2020 and Tutela LLC 2020. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using MongoDB.Driver;
using openrmf_msg_audit.Models;

namespace openrmf_msg_audit.Data
{
    public class AuditContext
    {
        private readonly IMongoDatabase _database = null;

        public AuditContext(Settings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Database);
        }

        public IMongoCollection<Audit> Audits
        {
            get
            {
                return _database.GetCollection<Audit>("Audits");
            }
        }
    }
}