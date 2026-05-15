using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Moq;
using MongoDB.Driver;
using openrmf_msg_audit.Data;
using openrmf_msg_audit.Models;
using Xunit;

namespace tests.Data;

public class AuditRepositoryTests
{
    [Fact]
    public void AddAudit_CallsInsertOneAndReturnsAudit()
    {
        var audit = new Audit { program = "openrmf", action = "create" };
        var mockCollection = new Mock<IMongoCollection<Audit>>(MockBehavior.Strict);
        var mockDatabase = new Mock<IMongoDatabase>(MockBehavior.Strict);

        mockCollection
            .Setup(c => c.InsertOne(audit, null, default))
            .Verifiable();
        mockDatabase
            .Setup(db => db.GetCollection<Audit>("Audits", null))
            .Returns(mockCollection.Object);

        var context = (AuditContext)RuntimeHelpers.GetUninitializedObject(typeof(AuditContext));
        SetPrivateField(context, "_database", mockDatabase.Object);

        var repository = (AuditRepository)RuntimeHelpers.GetUninitializedObject(typeof(AuditRepository));
        SetPrivateField(repository, "_context", context);

        var result = repository.AddAudit(audit);

        Assert.Same(audit, result);
        mockCollection.Verify(c => c.InsertOne(audit, null, default), Times.Once);
    }

    [Fact]
    public void AddAudit_WhenMongoInsertThrows_PropagatesException()
    {
        var audit = new Audit { program = "openrmf", action = "create" };
        var mockCollection = new Mock<IMongoCollection<Audit>>(MockBehavior.Strict);
        var mockDatabase = new Mock<IMongoDatabase>(MockBehavior.Strict);

        mockCollection
            .Setup(c => c.InsertOne(audit, null, default))
            .Throws(new InvalidOperationException("Insert failed"));
        mockDatabase
            .Setup(db => db.GetCollection<Audit>("Audits", null))
            .Returns(mockCollection.Object);

        var context = (AuditContext)RuntimeHelpers.GetUninitializedObject(typeof(AuditContext));
        SetPrivateField(context, "_database", mockDatabase.Object);

        var repository = (AuditRepository)RuntimeHelpers.GetUninitializedObject(typeof(AuditRepository));
        SetPrivateField(repository, "_context", context);

        var ex = Assert.Throws<InvalidOperationException>(() => repository.AddAudit(audit));

        Assert.Equal("Insert failed", ex.Message);
    }

    [Fact]
    public void AddAudit_WhenAuditIsNull_PropagatesArgumentNullExceptionFromMongo()
    {
        var mockCollection = new Mock<IMongoCollection<Audit>>(MockBehavior.Strict);
        var mockDatabase = new Mock<IMongoDatabase>(MockBehavior.Strict);

        mockCollection
            .Setup(c => c.InsertOne(null!, null, default))
            .Throws(new ArgumentNullException("document"));
        mockDatabase
            .Setup(db => db.GetCollection<Audit>("Audits", null))
            .Returns(mockCollection.Object);

        var context = (AuditContext)RuntimeHelpers.GetUninitializedObject(typeof(AuditContext));
        SetPrivateField(context, "_database", mockDatabase.Object);

        var repository = (AuditRepository)RuntimeHelpers.GetUninitializedObject(typeof(AuditRepository));
        SetPrivateField(repository, "_context", context);

        Assert.Throws<ArgumentNullException>(() => repository.AddAudit(null!));
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
