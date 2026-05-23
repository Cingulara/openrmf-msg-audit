using Xunit;
using openrmf_msg_audit.Models;
using System;

namespace tests.Models
{
    public class AuditTests
    {
        [Fact]
        public void Constructor_AssignsNonEmptyAuditId()
        {
            var audit = new Audit();

            Assert.NotNull(audit);
            Assert.NotEqual(Guid.Empty, audit.auditId);
            Assert.False(audit.auditId == Guid.Empty);
        }

        [Fact]
        public void Properties_RoundTripExpectedValues()
        {
            var created = DateTime.UtcNow;
            var audit = new Audit
            {
                program = "save",
                created = created,
                action = "edit",
                userid = Guid.NewGuid().ToString(),
                username = "my.username",
                fullname = "My F Name",
                email = "dale@example.com",
                url = "https://www.openrmf.io",
                message = "This is a test"
            };

            Assert.Equal("save", audit.program);
            Assert.Equal(created, audit.created);
            Assert.Equal("edit", audit.action);
            Assert.False(string.IsNullOrWhiteSpace(audit.userid));
            Assert.Equal("my.username", audit.username);
            Assert.Equal("My F Name", audit.fullname);
            Assert.Equal("dale@example.com", audit.email);
            Assert.Equal("https://www.openrmf.io", audit.url);
            Assert.Equal("This is a test", audit.message);
            Assert.NotEqual("delete", audit.action);
        }

        [Fact]
        public void DefaultStringProperties_AreNullUntilAssigned()
        {
            var audit = new Audit();

            Assert.Null(audit.program);
            Assert.Null(audit.action);
            Assert.Null(audit.userid);
            Assert.Null(audit.username);
            Assert.Null(audit.fullname);
            Assert.Null(audit.email);
            Assert.Null(audit.url);
            Assert.Null(audit.message);
        }
    }
}
