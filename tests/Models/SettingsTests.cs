using Xunit;
using openrmf_msg_audit.Models;

namespace tests.Models
{
    public class SettingsTests
    {
        [Fact]
        public void Constructor_InitializesWithNullFields()
        {
            var settings = new Settings();

            Assert.NotNull(settings);
            Assert.Null(settings.ConnectionString);
            Assert.Null(settings.Database);
        }

        [Fact]
        public void Fields_RoundTripExpectedValues()
        {
            var settings = new Settings();
            settings.ConnectionString = "mongodb://localhost:27017";
            settings.Database = "openrmf_audit";

            Assert.Equal("mongodb://localhost:27017", settings.ConnectionString);
            Assert.Equal("openrmf_audit", settings.Database);
            Assert.NotEqual("postgres://localhost", settings.ConnectionString);
            Assert.NotEqual("wrong_db", settings.Database);
        }
    }
}
