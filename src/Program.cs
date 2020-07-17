// Copyright (c) Cingulara LLC 2020 and Tutela LLC 2020. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using System;
using NATS.Client;
using System.Text;
using NLog;
using NLog.Config;
using openrmf_msg_audit.Models;
using openrmf_msg_audit.Classes;
using openrmf_msg_audit.Data;
using Newtonsoft.Json;

using MongoDB.Bson;

namespace openrmf_msg_audit
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration($"{AppContext.BaseDirectory}nlog.config");

            var logger = LogManager.GetLogger("openrmf-msg-audit");

            // Create a new connection factory to create a connection.
            ConnectionFactory cf = new ConnectionFactory();
            // add the options for the server, reconnecting, and the handler events
            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.MaxReconnect = -1;
            opts.ReconnectWait = 2000;
            opts.Name = "openrmf-msg-audit";
            opts.Url = Environment.GetEnvironmentVariable("NATSSERVERURL");
            opts.AsyncErrorEventHandler += (sender, events) =>
            {
                logger.Info("NATS client error. Server: {0}. Message: {1}. Subject: {2}", events.Conn.ConnectedUrl, events.Error, events.Subscription.Subject);
            };

            opts.ServerDiscoveredEventHandler += (sender, events) =>
            {
                logger.Info("A new server has joined the cluster: {0}", events.Conn.DiscoveredServers);
            };

            opts.ClosedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Closed: {0}", events.Conn.ConnectedUrl);
            };

            opts.ReconnectedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Reconnected: {0}", events.Conn.ConnectedUrl);
            };

            opts.DisconnectedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Disconnected: {0}", events.Conn.ConnectedUrl);
            };
            
            // Creates a live connection to the NATS Server with the above options
            IConnection c = cf.CreateConnection(opts);

            // Setup a new Audit record based on the data sent and just save it
            EventHandler<MsgHandlerEventArgs> newAuditEvent = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("New NATS subject: {0}", natsargs.Message.Subject);
                    
                    // setup the MondoDB connection
                    Settings s = new Settings();
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DBTYPE")) || Environment.GetEnvironmentVariable("DBTYPE").ToLower() == "mongo") {
                        s.ConnectionString = Environment.GetEnvironmentVariable("DBCONNECTION");
                        s.Database = Environment.GetEnvironmentVariable("DB");
                    }
                    // setup the database repo for systems
                    AuditRepository _auditRepo = new AuditRepository(s);
                    Audit newAudit = JsonConvert.DeserializeObject<Audit>(Compression.DecompressString(Encoding.UTF8.GetString(natsargs.Message.Data)));
                    logger.Info("Saving Audit Information for {0}", newAudit.program, newAudit.action, newAudit.auditId);
                    _auditRepo.AddAudit(newAudit);
                    logger.Info("Saved Audit Information for {0}", newAudit.program, newAudit.action, newAudit.auditId);
                    _auditRepo = null;
                    newAudit = null;
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error saving new audit information: {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                }
            };

            // The simple way to create an asynchronous subscriber
            // is to simply pass the event in.  Messages will start
            // arriving immediately.
            logger.Info("setting up the OpenRMF new audit subscriptions");
            IAsyncSubscription asyncNew = c.SubscribeAsync("openrmf.audit.>", newAuditEvent);
        }
    }
}
