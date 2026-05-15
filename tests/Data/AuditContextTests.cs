using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Moq;
using MongoDB.Driver;
using openrmf_msg_audit.Data;
using openrmf_msg_audit.Models;
using Xunit;

namespace tests.Data;

public class AuditContextTests
{
    [Fact]
    public void Audits_ReturnsCollectionFromDatabase()
    {
        var mockCollection = new Mock<IMongoCollection<Audit>>(MockBehavior.Strict);
        var mockDatabase = new Mock<IMongoDatabase>(MockBehavior.Strict);
        mockDatabase
            .Setup(db => db.GetCollection<Audit>("Audits", null))
            .Returns(mockCollection.Object);

        var context = (AuditContext)RuntimeHelpers.GetUninitializedObject(typeof(AuditContext));
        SetPrivateField(context, "_database", mockDatabase.Object);

        var audits = context.Audits;

        Assert.Same(mockCollection.Object, audits);
        mockDatabase.Verify(db => db.GetCollection<Audit>("Audits", null), Times.Once);
    }

    [Fact]
    public void Audits_WhenDatabaseIsMissing_ThrowsNullReferenceException()
    {
        var context = (AuditContext)RuntimeHelpers.GetUninitializedObject(typeof(AuditContext));

        Assert.Throws<NullReferenceException>(() => _ = context.Audits);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null)
        {
            throw new InvalidOperationException($"Field '{fieldName}' was not found on type '{target.GetType().Name}'.");
        }

        field.SetValue(target, value);
    }
}
