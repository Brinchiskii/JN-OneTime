using Microsoft.EntityFrameworkCore;
using OneTime.Core.Data.Repository;
using OneTime.Core.Models;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Tests.TestHelpers;

namespace OneTime.Core.Tests.Integration;

public class AuditLogServiceIntegrationTest
{

    [Fact]
    public async Task Log_Writes_Record_With_All_Fields()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();

        var auditRepo = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepo);

        var actorUserId = 42;
        var action = "TimesheetCreated";
        var entityType = "Timesheet";
        var entityId = 1;
        var details = "Timesheet created";
        var timestamp = DateTime.Now;
        
        var result = await auditService.Log(actorUserId, action, entityType, entityId, details);
        
        var auditLog = await context.AuditLogs.FirstAsync();
        
        Assert.Equal(actorUserId, auditLog.ActorUserId);
        Assert.Equal(action, auditLog.Action);
        Assert.Equal(entityType, auditLog.EntityType);
        Assert.Equal(entityId, auditLog.EntityId);
        Assert.Equal(details, auditLog.Details);
        Assert.True(auditLog.Timestamp > DateTime.Now.AddMinutes(-1));
    }
    
    [Fact]
    public async Task Get_Forwards_Filters_To_Repository()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();

        var auditRepo = new AuditLogRepository(context);
        var auditService  = new AuditLogService(auditRepo);

        context.AuditLogs.Add(new AuditLog
        {
            AuditLogId = 1,
            ActorUserId = 1,
            Timestamp = DateTime.Now,
            Action = "TimesheetCreated",
            EntityType = "Timesheet",
            EntityId = 1,
            Details = "Timesheet created",
        });
        
        context.AuditLogs.Add(new AuditLog
        {
            AuditLogId = 2, 
            ActorUserId = 99, 
            Timestamp = DateTime.Now, 
            Action = "OtherAction", 
            EntityType = "OtherType", 
            EntityId = 999, 
            Details = "Should not match",
        });
        
        await context.SaveChangesAsync();
        
        var result = await auditService.Get("Timesheet", "TimesheetCreated", 1);
        
        Assert.Single(result);
        Assert.Equal(1, result.First().AuditLogId);
    }
}