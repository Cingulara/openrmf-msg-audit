// Copyright (c) Cingulara LLC 2020 and Tutela LLC 2020. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using openrmf_msg_audit.Models;
using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace openrmf_msg_audit.Data {
    public class AuditRepository : IAuditRepository
    {
        private readonly AuditContext _context = null;

        public AuditRepository(Settings settings)
        {
            _context = new AuditContext(settings);
        }

        private ObjectId GetInternalId(string id)
        {
            ObjectId internalId;
            if (!ObjectId.TryParse(id, out internalId))
                internalId = ObjectId.Empty;

            return internalId;
        }
        
        public Audit AddAudit(Audit item)
        {
            try
            {
                _context.Audits.InsertOne(item);
                return item;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

    }
}