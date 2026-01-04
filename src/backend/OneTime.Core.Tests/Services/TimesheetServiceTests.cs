
using OneTime.Core.Services.Repository;
using OneTime.Core.Data.Repository;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Tests.TestHelpers;

namespace OneTime.Core.Tests.Services;

public class TimesheetServiceTests
{
    [Fact]
    public async Task CreateTimesheet_Already_Exists_Throws()
    {
        // Arrange
        var userId = 1;
        var periodStart = new DateOnly(2025, 11, 1);
        var periodEnd = new DateOnly(2025, 11, 30);
            
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        context.Timesheets.Add(new Timesheet
        {
            UserId = userId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Status = (int)TimesheetStatus.Pending
        });

        await context.SaveChangesAsync();
            
        // Act + Assert 
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            timesheetService.CreateTimesheet(userId, periodStart, periodEnd));
    }

    [Fact]
    public async Task CreateTimesheet_No_TimeEntries_Throws()
    {
        // Arrange
        var userId = 1;
        var periodStart = new DateOnly(2025, 12, 31);
        var periodEnd = new DateOnly(2025, 12, 30);

        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            timesheetService.CreateTimesheet(userId, periodStart, periodEnd));

    }

    [Fact]
    public async Task CreateTimesheet_UserId_Is_Zero()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.CreateTimesheet(0, new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 31)));
        Assert.Equal("User ID must be greater than zero.", ex.ParamName);
    }
        
    [Fact]
    public async Task CreateTimesheet_UserId_Is_Negative()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.CreateTimesheet(-1, new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 31)));
        Assert.Equal("User ID must be greater than zero.", ex.ParamName);
    }

    [Fact]
    public async Task CreateTimesheet_Succeeds()
    {
        // Arrange
        var userId = 1;
        var periodStart = new DateOnly(2025, 10, 1);
        var periodEnd = new DateOnly(2025, 10, 31);

        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);

        context.TimeEntries.Add(new TimeEntry
        {
            UserId = userId,
            Date = new DateOnly(2025, 10, 15),
            Hours = 8m,
            ProjectId = 1,
            Note = "Test Entry"
        });

        await context.SaveChangesAsync();

        // Act
        var timesheet = await timesheetService.CreateTimesheet(userId, periodStart, periodEnd);

        // Assert
        Assert.NotNull(timesheet);
        Assert.Equal(userId, timesheet.UserId);
        Assert.Equal(periodStart, timesheet.PeriodStart);
        Assert.Equal(periodEnd, timesheet.PeriodEnd);
        Assert.Equal((int)TimesheetStatus.Draft, timesheet.Status);
    }

    [Fact]
    public async Task GetTimeentriesForPendingTimesheet_LeaderId_Is_Zero_Or_Negative()
    {
        var periodStart = new DateOnly(2025, 12, 1);
        var periodEnd = new DateOnly(2025, 12, 7);
            
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex1 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimeentriesForPendingTimesheet(0, periodStart, periodEnd));
        Assert.Equal("Leader ID must be greater than zero.", ex1.ParamName);
        var ex2 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimeentriesForPendingTimesheet(-1, periodStart, periodEnd));
        Assert.Equal("Leader ID must be greater than zero.", ex2.ParamName);
    }

    [Fact]
    public async Task GetTimeentriesForPendingTimesheet_StartPeriod_Is_After_EndPeriod()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimeentriesForPendingTimesheet(1, new DateOnly(2025, 12, 10), new DateOnly(2025, 12, 7)));
        Assert.Equal("Start date must be before or equal to end date.", ex.ParamName);
    }
        
    [Fact]
    public async Task GetTimeentriesForPendingTimesheet_Returns_TimeEntries()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();

        // 1) Mock user
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test Employee",
            Email = "test@example.com",
            PasswordHash = "",
            PasswordSalt = "",
            Role = 2,
            ManagerId = 1
        });

        // 2) Mock project
        context.Projects.Add(new Project
        {
            ProjectId = 1,
            Name = "Test Project",
            Status = 0
        });

        // 3) Mock timesheet
        context.Timesheets.Add(new Timesheet
        {
            UserId = 1,
            PeriodStart = new DateOnly(2025, 12, 1),
            PeriodEnd = new DateOnly(2025, 12, 31),
            Status = (int)TimesheetStatus.Pending // Pending
        });

        // 4) Mock time entry
        context.TimeEntries.Add(new TimeEntry
        {
            UserId = 1,
            ProjectId = 1,
            Date = new DateOnly(2025, 12, 3),
            Note = null,
            Hours = 7,
            TimesheetId = 1
        });
        context.TimeEntries.Add(new TimeEntry
        {
            UserId = 1,
            ProjectId = 1,
            Date = new DateOnly(2025, 12, 4),
            Note = null,
            Hours = 8,
            TimesheetId = 1
        });

        await context.SaveChangesAsync();

            
        // Act + Assert
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);

        var entries = await timesheetService.GetTimeentriesForPendingTimesheet(
            leaderId: 1,
            start: new DateOnly(2025, 12, 1),
            end: new DateOnly(2025, 12, 31));

        Assert.Collection(entries,
            item =>
            {
                Assert.Equal(7, item.Hours);
                Assert.Equal(new DateOnly(2025, 12, 3), item.Date);
            },
            item =>
            {
                Assert.Equal(8, item.Hours);
                Assert.Equal(new DateOnly(2025, 12, 4), item.Date);
            });
    }

    [Fact]
    public async Task GetTimeentriesForPendingTimesheet_Returns_EmptyList_When_No_Pending_Timesheets()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();

        // 1) Mock user
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test Employee",
            Email = "test@example.com",
            PasswordHash = "",
            PasswordSalt = "",
            Role = 2,
            ManagerId = 1
        });

        // 2) Mock project
        context.Projects.Add(new Project
        {
            ProjectId = 1,
            Name = "Test Project",
            Status = 0
        });

        // 3) Mock timesheet
        context.Timesheets.Add(new Timesheet
        {
            UserId = 1,
            PeriodStart = new DateOnly(2025, 12, 1),
            PeriodEnd = new DateOnly(2025, 12, 31),
            Status = (int)TimesheetStatus.Pending // Pending
        });
            
        await context.SaveChangesAsync();
            
        // Act + Assert
        var service = new TimesheetRepository(context);
            
        var entries = await service.GetTimeentriesForPendingTimesheet(
            leaderId: 1,
            start: new DateOnly(2025, 12, 1),
            end: new DateOnly(2025, 12, 31));
            
        Assert.Empty(entries);
    }

    [Fact]
    public async Task UpdateTimeSheet_TimesheetId_Is_Zero_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.UpdateTimeSheet(0, 1, "", 1));
        Assert.Equal("Timesheet ID must be greater than zero.", ex.ParamName);
    }
        
    [Fact]
    public async Task UpdateTimeSheet_TimesheetId_Is_Negative_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.UpdateTimeSheet(-1, 1, "", 1));
        Assert.Equal("Timesheet ID must be greater than zero.", ex.ParamName);
    }
        
    [Fact]
    public async Task UpdateTimeSheet_Timesheet_NotFound_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => timesheetService.UpdateTimeSheet(1, 1, "", 1));
        Assert.Equal("Timesheet not found.", ex.Message);
    }
        
    [Fact]
    public async Task UpdateTimeSheet_Invalid_Status_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        context.Timesheets.Add(new Timesheet
        {
            UserId = 1,
            PeriodStart = new DateOnly(2025, 12, 1),
            PeriodEnd = new DateOnly(2025, 12, 31),
            Status = (int)TimesheetStatus.Pending // Pending
        });
            
        await context.SaveChangesAsync();
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.UpdateTimeSheet(1, 6, "", 1));
        Assert.Equal("Invalid timesheet status value.", ex.ParamName);
    }
        
    [Fact]
    public async Task UpdateTimeSheet_To_Approved_Sets_Leader_Decision_And_Logs()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test Manager",
            Email = "manager@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Manager,
            ManagerId = null
        });
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test Employee",
            Email = "employee@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Employee,
            ManagerId = 1
        });

        context.Timesheets.Add(new Timesheet
        {
            TimesheetId = 1,
            UserId = 2,
            PeriodStart = default,
            PeriodEnd = default,
            DecidedByUserId = 2,
            DecidedAt = DateTime.Now,
            Comment = "Test comment",
            Status = 0
        });
            
        await context.SaveChangesAsync();
            
        var timesheet = await timesheetService.UpdateTimeSheet(1, 1, "Test comment", 1);
            
        Assert.Equal(1, timesheet.DecidedByUserId);
        Assert.Equal((int)TimesheetStatus.Approved, timesheet.Status);
        Assert.Equal("Test comment", timesheet.Comment);
        Assert.NotEmpty(context.AuditLogs);
    }
        
    [Fact]
    public async Task UpdateTimeSheet_To_Rejected_Sets_Leader_Decision_And_Logs()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test Manager",
            Email = "manager@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Manager,
            ManagerId = null
        });
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test Employee",
            Email = "employee@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Employee,
            ManagerId = 1
        });

        context.Timesheets.Add(new Timesheet
        {
            TimesheetId = 1,
            UserId = 2,
            PeriodStart = default,
            PeriodEnd = default,
            DecidedByUserId = 2,
            DecidedAt = DateTime.Now,
            Comment = "Test comment",
            Status = 0
        });
            
        await context.SaveChangesAsync();
            
        var timesheet = await timesheetService.UpdateTimeSheet(1, 2, "Test comment", 1);
            
        Assert.Equal(1, timesheet.DecidedByUserId);
        Assert.Equal((int)TimesheetStatus.Rejected, timesheet.Status);
        Assert.Equal("Test comment", timesheet.Comment);
        Assert.NotEmpty(context.AuditLogs);
    }
        
    [Fact]
    public async Task UpdateTimeSheet_To_Pending_Transitions_Correctly()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test Manager",
            Email = "manager@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Manager,
            ManagerId = null
        });
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test Employee",
            Email = "employee@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Employee,
            ManagerId = 1
        });

        context.Timesheets.Add(new Timesheet
        {
            TimesheetId = 1,
            UserId = 2,
            PeriodStart = default,
            PeriodEnd = default,
            DecidedByUserId = 1,
            DecidedAt = DateTime.Now,
            Comment = "Test comment",
            Status = 2
        });
            
        await context.SaveChangesAsync();
            
        var timesheet = await timesheetService.UpdateTimeSheet(1, 0, "Test comment");
            
        Assert.Equal(2, timesheet.DecidedByUserId);
        Assert.Equal((int)TimesheetStatus.Pending, timesheet.Status);
        Assert.Equal("Test comment", timesheet.Comment);
        Assert.NotEmpty(context.AuditLogs);
    }
        
    [Fact]
    public async Task UpdateTimeSheet_To_Draft_Transitions_Correctly()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test Manager",
            Email = "manager@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Manager,
            ManagerId = null
        });
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 2,
            Name = "Test Employee",
            Email = "employee@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Employee,
            ManagerId = 1
        });

        context.Timesheets.Add(new Timesheet
        {
            TimesheetId = 1,
            UserId = 2,
            PeriodStart = default,
            PeriodEnd = default,
            DecidedByUserId = 1,
            DecidedAt = DateTime.Now,
            Comment = "Test comment",
            Status = 2
        });
            
        await context.SaveChangesAsync();
            
        var timesheet = await timesheetService.UpdateTimeSheet(1, 3, "Test comment");
            
        Assert.Equal(2, timesheet.DecidedByUserId);
        Assert.Equal((int)TimesheetStatus.Draft, timesheet.Status);
        Assert.Equal("Test comment", timesheet.Comment);
        Assert.NotEmpty(context.AuditLogs);
    }

    [Fact]
    public async Task GetTimesheetByUserAndDate_UserId_Is_Zero_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimesheetByUserAndDate(0, new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 1)));
        Assert.Equal("User ID must be greater than zero.", ex.ParamName);
    }
        
    [Fact]
    public async Task GetTimesheetByUserAndDate_UserId_Is_Negative_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimesheetByUserAndDate(-1, new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 1)));
        Assert.Equal("User ID must be greater than zero.", ex.ParamName);
    }

    [Fact]
    public async Task GetTimesheetByUserAndDate_StartAfterEnd_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);
            
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timesheetService.GetTimesheetByUserAndDate(1, new DateOnly(2025, 12, 10), new DateOnly(2025, 12, 7)));
        Assert.Equal("Start date must be before or equal to end date.", ex.ParamName);
    }

    [Fact]
    public async Task GetTimesheetByUserAndDate_Returns_Timesheet()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);

        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test Employee",
            Email = "employee@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Employee,
            ManagerId = null
        });
            
        context.Timesheets.Add(new Timesheet
        {
            TimesheetId = 1,
            UserId = 1,
            PeriodStart = new DateOnly(2025, 12, 1),
            PeriodEnd = new DateOnly(2025, 12, 31),
            Status = 0
        });
            
        await context.SaveChangesAsync();
            
        var timesheet = await timesheetService.GetTimesheetByUserAndDate(1, new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 31));
            
        Assert.NotNull(timesheet);
    }
        
    [Fact]
    public async Task GetTimesheetByUserAndDate_Returns_Null_When_NotFound()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timesheetRepo = new TimesheetRepository(context);
        var timesheetService = new TimesheetService(timesheetRepo, auditService);

        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test Employee",
            Email = "employee@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = (int)UserRole.Employee,
            ManagerId = null
        });
            
        await context.SaveChangesAsync();
            
        var timesheet = await timesheetService.GetTimesheetByUserAndDate(1, new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 31));
            
        Assert.Null(timesheet);
    }
}