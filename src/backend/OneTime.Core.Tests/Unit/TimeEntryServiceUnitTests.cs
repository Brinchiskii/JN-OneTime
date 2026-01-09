using Moq;
using OneTime.Core.Models;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Core.Tests.Unit;

public class TimeEntryServiceUnitTests
{
    [Fact]
    public async Task CreateTimeEntry_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        var projectRepoMock = new Mock<IProjectRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimeEntryService(
            timeEntryRepoMock.Object,
            projectRepoMock.Object,
            auditServiceMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateTimeEntry(null!));
        
        projectRepoMock.VerifyNoOtherCalls();
        timeEntryRepoMock.VerifyNoOtherCalls();
        auditServiceMock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task CreateTimeEntry_ProjectNotFound_ThrowsArgumentNullException()
    {
        // Arrange
        var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        var projectRepoMock = new Mock<IProjectRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var timeEntry = new TimeEntry
        {
            UserId = 1,
            ProjectId = 999,
            Hours = 8,
            TimesheetId = 1
        };
        
        projectRepoMock
            .Setup(r => r.GetById(999))!
            .ReturnsAsync((Project?)null);

        var service = new TimeEntryService(
            timeEntryRepoMock.Object,
            projectRepoMock.Object,
            auditServiceMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreateTimeEntry(timeEntry));

        Assert.Equal("Project not found", ex.ParamName);
        
        projectRepoMock.Verify(r => r.GetById(999), Times.Once);
        timeEntryRepoMock.Verify(r => r.Add(It.IsAny<TimeEntry>()), Times.Never);
        auditServiceMock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task CreateTimeEntry_InvalidHours_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        var projectRepoMock = new Mock<IProjectRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();
        
        projectRepoMock
            .Setup(r => r.GetById(1))
            .ReturnsAsync(new Project { ProjectId = 1 });

        var service = new TimeEntryService(
            timeEntryRepoMock.Object,
            projectRepoMock.Object,
            auditServiceMock.Object);

        var zeroHoursEntry = new TimeEntry
        {
            UserId = 1,
            ProjectId = 1,
            Hours = 0,
            TimesheetId = 1
        };

        var tooManyHoursEntry = new TimeEntry
        {
            UserId = 1,
            ProjectId = 1,
            Hours = 25,
            TimesheetId = 1
        };

        // Act + Assert
        var ex1 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateTimeEntry(zeroHoursEntry));
        Assert.Equal("Hours must be greater than zero and less than 24", ex1.ParamName);

        var ex2 = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateTimeEntry(tooManyHoursEntry));
        Assert.Equal("Hours must be greater than zero and less than 24", ex2.ParamName);
        
        timeEntryRepoMock.Verify(r => r.Add(It.IsAny<TimeEntry>()), Times.Never);
        auditServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTimeEntry_ValidInput_CallsRepoAndAudit()
    {
        // Arrange
        var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        var projectRepoMock = new Mock<IProjectRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var entry = new TimeEntry
        {
            TimeEntryId = 1,
            UserId = 1,
            ProjectId = 10,
            Hours = 8,
            Date = new DateOnly(2025, 10, 15),
            TimesheetId = 1
        };
        
        projectRepoMock
            .Setup(r => r.GetById(10))
            .ReturnsAsync(new Project { ProjectId = 10 });
        
        timeEntryRepoMock
            .Setup(r => r.Add(It.IsAny<TimeEntry>()))
            .ReturnsAsync((TimeEntry t) => t);
        
        auditServiceMock
            .Setup(a => a.Log(
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string>()))
            .ReturnsAsync(new AuditLog());

        var service = new TimeEntryService(
            timeEntryRepoMock.Object,
            projectRepoMock.Object,
            auditServiceMock.Object);

        // Act
        var result = await service.CreateTimeEntry(entry);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(8, result.Hours);
        Assert.Equal(10, result.ProjectId);
        
        projectRepoMock.Verify(r => r.GetById(10), Times.Once);
        timeEntryRepoMock.Verify(r => r.Add(It.IsAny<TimeEntry>()), Times.Once);
        auditServiceMock.Verify(a => a.Log(
            1,
            "TimeEntryCreated",
            "TimeEntry",
            1,
            It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task ReplaceTimeEntries_ValidInput_CallsDeleteAddSave()
    {
        // Arrange
        var timeEntryRepo = new Mock<ITimeEntryRepository>();
        var projectRepo = new Mock<IProjectRepository>();
        var auditService = new Mock<IAuditLogService>();

        var newEntries = new List<TimeEntry>
        {
            new TimeEntry { UserId = 1, ProjectId = 1, Hours = 8 },
            new TimeEntry { UserId = 1, ProjectId = 1, Hours = 7 }
        };

        timeEntryRepo
            .Setup(r => r.DeleteEntriesByTimesheetId(1))
            .Returns(Task.CompletedTask);

        timeEntryRepo
            .Setup(r => r.AddTimeEntries(It.IsAny<List<TimeEntry>>()))
            .Returns(Task.CompletedTask);

        timeEntryRepo
            .Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new TimeEntryService(
            timeEntryRepo.Object,
            projectRepo.Object,
            auditService.Object);

        // Act
        await service.ReplaceTimeEntries(1, newEntries);

        // Assert
        timeEntryRepo.Verify(r => r.DeleteEntriesByTimesheetId(1), Times.Once);
        timeEntryRepo.Verify(r => r.AddTimeEntries(newEntries), Times.Once);
        timeEntryRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        
        auditService.VerifyNoOtherCalls();
        projectRepo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReplaceTimeEntries_TimesheetId_Invalid_Throws()
    {
        // Arrange
        var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        var projectRepoMock = new Mock<IProjectRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimeEntryService(
            timeEntryRepoMock.Object,
            projectRepoMock.Object,
            auditServiceMock.Object);

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ReplaceTimeEntries(0, new List<TimeEntry>()));

        Assert.Equal("TimesheetId must be greater than zero", ex.Message);

        timeEntryRepoMock.VerifyNoOtherCalls();
        auditServiceMock.VerifyNoOtherCalls();
    }

    
    [Fact]
    public async Task GetTimeEntriesByUserWithDetails_UserIdInvalid_Throws()
    {
        // Arrange
        var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        var projectRepoMock = new Mock<IProjectRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimeEntryService(
            timeEntryRepoMock.Object,
            projectRepoMock.Object,
            auditServiceMock.Object);

        // Act + Assert
        var ex1 = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetTimeEntriesByUserWithDetails(0));
        Assert.Equal("UserId must be greater than zero", ex1.Message);

        var ex2 = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetTimeEntriesByUserWithDetails(-1));
        Assert.Equal("UserId must be greater than zero", ex2.Message);
        
        timeEntryRepoMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetTimeEntriesByUserWithDetails_RepoReturnsEmpty_ReturnsEmpty()
    {
        // Arrange
        var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        var projectRepoMock = new Mock<IProjectRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        timeEntryRepoMock
            .Setup(r => r.GetByUserWithDetails(1))
            .ReturnsAsync(Enumerable.Empty<TimeEntry>());

        var service = new TimeEntryService(
            timeEntryRepoMock.Object,
            projectRepoMock.Object,
            auditServiceMock.Object);

        // Act
        var result = await service.GetTimeEntriesByUserWithDetails(1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        timeEntryRepoMock.Verify(r => r.GetByUserWithDetails(1), Times.Once);
        auditServiceMock.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task GetTimeEntriesByUserWithDetails_ReturnsEntries()
    {
        // Arrange
        var timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        var projectRepoMock = new Mock<IProjectRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var entries = new List<TimeEntry>
        {
            new TimeEntry
            {
                TimeEntryId = 1,
                UserId = 1,
                ProjectId = 10,
                Date = new DateOnly(2025, 10, 15),
                Hours = 8
            },
            new TimeEntry
            {
                TimeEntryId = 2,
                UserId = 1,
                ProjectId = 10,
                Date = new DateOnly(2025, 10, 16),
                Hours = 7
            }
        };

        timeEntryRepoMock
            .Setup(r => r.GetByUserWithDetails(1))
            .ReturnsAsync(entries);

        var service = new TimeEntryService(
            timeEntryRepoMock.Object,
            projectRepoMock.Object,
            auditServiceMock.Object);

        // Act
        var result = (await service.GetTimeEntriesByUserWithDetails(1)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal(1, r.UserId));

        timeEntryRepoMock.Verify(r => r.GetByUserWithDetails(1), Times.Once);
        auditServiceMock.VerifyNoOtherCalls();
    }
}