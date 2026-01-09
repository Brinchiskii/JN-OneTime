using OneTime.Core.Data.Repository;
using OneTime.Core.Models;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Repository;
using OneTime.Core.Tests.TestHelpers;

namespace OneTime.Core.Tests.Integration;

public class TimeEntryServiceIntegrationTest
{

    [Fact]
    public async Task CreateTimeEntry_Throws_When_Project_Not_Found()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

            
        var timeEntry = new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 123,
            Date = new DateOnly(2025, 10, 15),
            Note = "Test Entry",
            Hours = 8m,
            TimesheetId = 1
        };
            
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => timeEntryService.CreateTimeEntry(timeEntry));

        Assert.Equal("Project not found", ex.ParamName);
    }

    [Fact]
    public async Task CreateTimeEntry_Saves_When_Project_Exists()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });
            
        await context.SaveChangesAsync();
            
        var timeEntry = new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 10,
            Date = new DateOnly(2025, 10, 15),
            Note = "Test Entry",
            Hours = 8m,
            TimesheetId = 1
        };
            
        // Act
        var result = await timeEntryService.CreateTimeEntry(timeEntry);

        // Assert
        Assert.Equal(1, result.TimeEntryId);
        Assert.Equal(10, result.ProjectId);
        Assert.Equal(8m, result.Hours);
        Assert.Equal(new DateOnly(2025, 10, 15), result.Date);
        Assert.Equal("Test Entry", result.Note);
        Assert.Equal(1, result.TimesheetId);
    }

    [Fact]
    public async Task CreateTimeEntry_Throws_When_Hours_Invalid()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });
            
        await context.SaveChangesAsync();
            
        var timeEntryZeroHours = new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 10,
            Date = new DateOnly(2025, 10, 15),
            Note = "Test Entry",
            Hours = 0m,
            TimesheetId = 1
        };
            
        var timeEntryTooManyHours = new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 10,
            Date = new DateOnly(2025, 10, 15),
            Note = "Test Entry",
            Hours = 25,
            TimesheetId = 1
        };
            

        // Act & Assert
        var ex1 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timeEntryService.CreateTimeEntry(timeEntryZeroHours));
        Assert.Equal("Hours must be greater than zero and less than 24", ex1.ParamName);

        var ex2 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => timeEntryService.CreateTimeEntry(timeEntryTooManyHours));
        Assert.Equal("Hours must be greater than zero and less than 24", ex2.ParamName);
    }

    [Fact]
    public async Task CreateTimeEntry_Throws_On_Null_Input()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

            
        await Assert.ThrowsAsync<ArgumentNullException>(() => timeEntryService.CreateTimeEntry(null!));
    }

    [Fact]
    public async Task CreateTimeEntry_Sets_Default_Date_When_Not_Set()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });
            
        await context.SaveChangesAsync();
            
        var timeEntry = new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 10,
            Note = "Test Entry",
            Hours = 8,
            TimesheetId = 1
        };
            
        var result = await timeEntryService.CreateTimeEntry(timeEntry);

        Assert.Equal(default, result.Date);
        Assert.NotEqual(DateOnly.FromDateTime(DateTime.Today), result.Date);
    }

    [Fact]
    public async Task GetTimeEntryByUserWithDetails_UserId_NotOK()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        const int invalidUserIdZero = 0;
        const int invalidUserIdNegative = -1;

        // Act & Assert
        var ex1 = await Assert.ThrowsAsync<ArgumentException>(() => timeEntryService.GetTimeEntriesByUserWithDetails(invalidUserIdZero));
        Assert.Equal("UserId must be greater than zero", ex1.Message);
        var ex2 = await Assert.ThrowsAsync<ArgumentException>(() => timeEntryService.GetTimeEntriesByUserWithDetails(invalidUserIdNegative));
        Assert.Equal("UserId must be greater than zero", ex2.Message);
    }

    [Fact]
    public async Task GetTimeEntryByUserWithDetails_UserId_OK()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        // Adds user to the db
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 0, // <- Employee
            ManagerId = 0
        });

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });

        context.TimeEntries.Add(new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 10,
            Date = new DateOnly(2025, 10, 15),
            Note = "Test Entry",
            Hours = 8m,
            TimesheetId = 1
        });

        await context.SaveChangesAsync();

        // Act + Assert
        var timeEntries = await timeEntryService.GetTimeEntriesByUserWithDetails(1);

        Assert.NotEmpty(timeEntries);
    }
        
    [Fact]
    public async Task GetTimeEntryByUserWithDetails_EmptyList()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);
            
        // Act + Assert
        var result = await timeEntryService.GetTimeEntriesByUserWithDetails(1);
        Assert.Empty(result);
    }
        
    [Fact]
    public async Task GetTimeEntryByUserWithDetails_ListWithEntries()
    {
        // Arrange
        var context = OneTimeContextFactory.CreateInMemoryContext();
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);
            
        // Adds user to the db
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test User",
            Email = "test@test.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 0, // <- Employee
            ManagerId = 0
        });

        context.Projects.Add(new Project
        {
            ProjectId = 10,
            Name = "Test Project",
            Status = 0
        });

        context.TimeEntries.Add(new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 10,
            Date = new DateOnly(2025, 10, 15),
            Note = "Test Entry",
            Hours = 8m,
            TimesheetId = 1
        });
            
        context.TimeEntries.Add(new TimeEntry
        {
            TimeEntryId = 2,
            UserId = 1,
            ProjectId = 10,
            Date = new DateOnly(2025, 10, 16),
            Note = "Test Entry 2",
            Hours = 8,
            TimesheetId = 1
        });
            
        context.SaveChangesAsync();

        // Act
        var result = (await timeEntryService.GetTimeEntriesByUserWithDetails(1)).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.All(result, r => Assert.Equal(1, r.UserId));
        Assert.Equal(2, result.Count);

        // Should be ordered by date ascending
        Assert.True(result[0].Date <= result[1].Date);
    }

    [Fact]
    public async Task ReplaceTimeEntries_Succeeds_Calls_DeleteAddSave()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
            
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context); 
    }

    [Fact]
    public async Task GetTimeEntriesForTimesheet_UserId_Zero_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
            
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        var ex =
            await Assert.ThrowsAsync<ArgumentException>(() => timeEntryService.GetTimeEntriesForTimesheet(0, 1));
        Assert.Equal("UserId must be greater than zero.", ex.Message);
    }
        
    [Fact]
    public async Task GetTimeEntriesForTimesheet_UserId_Negative_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
            
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        var ex =
            await Assert.ThrowsAsync<ArgumentException>(() => timeEntryService.GetTimeEntriesForTimesheet(-1, 1));
        Assert.Equal("UserId must be greater than zero.", ex.Message);
    }
        
    [Fact]
    public async Task GetTimeEntriesForTimesheet_TimesheetId_Zero_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
            
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        var ex =
            await Assert.ThrowsAsync<ArgumentException>(() => timeEntryService.GetTimeEntriesForTimesheet(1, 0));
        Assert.Equal("TimeSheetId must be greater than zero.", ex.Message);
    }
        
    [Fact]
    public async Task GetTimeEntriesForTimesheet_TimesheetId_Negative_Throws()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
            
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);

        var ex =
            await Assert.ThrowsAsync<ArgumentException>(() => timeEntryService.GetTimeEntriesForTimesheet(1, -1));
        Assert.Equal("TimeSheetId must be greater than zero.", ex.Message);
    }

    [Fact]
    public async Task GetTimeEntriesForTimesheet_Returns_Entries()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
            
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test user",
            Email = "test@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 0,
        });

        context.Projects.Add(new Project
        {
            ProjectId = 1,
            Name = "Test Project",
            Status = 0
        });
            
        // Creating timesheet
        context.Timesheets.Add(new Timesheet
        {
            TimesheetId = 1,
            UserId = 1,
            PeriodStart = default,
            PeriodEnd = default,
            DecidedByUserId = null,
            DecidedAt = null,
            Comment = null,
            Status = 0,
        });
            
        // Creating Timeentries to the timesheet
        context.TimeEntries.Add(new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 1,
            Date = default,
            Note = null,
            Hours = 5,
            TimesheetId = 1,
        });
            
        context.TimeEntries.Add(new TimeEntry
        {
            TimeEntryId = 2,
            UserId = 1,
            ProjectId = 1,
            Date = default,
            Note = null,
            Hours = 7,
            TimesheetId = 1,
        });
            
        await context.SaveChangesAsync();
            
        var timeentries = await timeEntryService.GetTimeEntriesForTimesheet(1, 1);
        Assert.NotEmpty(timeentries);
        Assert.Equal(2, timeentries.Count());
    }
        
    [Fact]
    public async Task GetTimeEntriesForTimesheet_Returns_Empty()
    {
        var context = OneTimeContextFactory.CreateInMemoryContext();
            
        var projectRepository = new ProjectRepository(context);
        var timeEntryRepository = new TimeEntryRepository(context);
        var auditRepository = new AuditLogRepository(context);
        var auditService = new AuditLogService(auditRepository);
        var timeEntryService = new TimeEntryService(timeEntryRepository, projectRepository, auditService);
            
        context.JNUsers.Add(new JNUser
        {
            UserId = 1,
            Name = "Test user",
            Email = "test@mail.com",
            PasswordHash = "TestHash",
            PasswordSalt = "TestSalt",
            Role = 0,
        });

        context.Projects.Add(new Project
        {
            ProjectId = 1,
            Name = "Test Project",
            Status = 0
        });
            
        // Creating timesheet
        context.Timesheets.Add(new Timesheet
        {
            TimesheetId = 1,
            UserId = 1,
            PeriodStart = default,
            PeriodEnd = default,
            DecidedByUserId = null,
            DecidedAt = null,
            Comment = null,
            Status = 0,
        });
            
        await context.SaveChangesAsync();
            
        var timeentries = await timeEntryService.GetTimeEntriesForTimesheet(1, 1);
        Assert.Empty(timeentries);
    }
}