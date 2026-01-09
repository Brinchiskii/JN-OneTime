using Moq;
using OneTime.Core.Models;
using OneTime.Core.Models.Enums;
using OneTime.Core.Services.Implementations;
using OneTime.Core.Services.Interfaces;
using Xunit;

namespace OneTime.Core.Tests.Unit;

public class TimesheetServiceUnitTests
{
    [Fact]
    public async Task CreateTimesheet_UserIdIsZero_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateTimesheet(0, DateOnly.MinValue, DateOnly.MinValue));

        timesheetRepositoryMock.VerifyNoOtherCalls();
        auditServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTimesheet_StartAfterEnd_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateTimesheet(1,
                new DateOnly(2025, 2, 1),
                new DateOnly(2025, 1, 1)));

        timesheetRepositoryMock.VerifyNoOtherCalls();
        auditServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTimesheet_AlreadyExists_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        timesheetRepositoryMock.Setup(r => r.ExistsForPeriod(
                1,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(true);

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateTimesheet(
                1,
                new DateOnly(2025, 1, 1),
                new DateOnly(2025, 1, 31)));

        timesheetRepositoryMock.Verify(r => r.ExistsForPeriod(
            1,
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>()), Times.Once);

        timesheetRepositoryMock.Verify(r => r.Add(It.IsAny<Timesheet>()), Times.Never);
        auditServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTimesheet_ValidInput_CallsRepoAndAudit()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        timesheetRepositoryMock.Setup(r => r.ExistsForPeriod(
                It.IsAny<int>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(false);

        timesheetRepositoryMock.Setup(r => r.Add(It.IsAny<Timesheet>()))
            .ReturnsAsync((Timesheet t) => t);

        auditServiceMock.Setup(a => a.Log(
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(new AuditLog());

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        var result = await service.CreateTimesheet(
            1,
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 1, 31));

        Assert.Equal((int)TimesheetStatus.Draft, result.Status);

        timesheetRepositoryMock.Verify(r => r.Add(It.IsAny<Timesheet>()), Times.Once);

        auditServiceMock.Verify(a => a.Log(
            1,
            "TimesheetCreated",
            "Timesheet",
            It.IsAny<int>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTimeSheet_IdIsZero_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.UpdateTimeSheet(0, 1, null));

        timesheetRepositoryMock.VerifyNoOtherCalls();
        auditServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateTimeSheet_NotFound_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        timesheetRepositoryMock.Setup(r => r.GetById(1))
            .ReturnsAsync((Timesheet?)null);

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateTimeSheet(1, 1, null));

        timesheetRepositoryMock.Verify(r => r.GetById(1), Times.Once);
        auditServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateTimeSheet_InvalidStatus_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        timesheetRepositoryMock.Setup(r => r.GetById(1))
            .ReturnsAsync(new Timesheet { Status = 0 });

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.UpdateTimeSheet(1, 99, null));

        auditServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateTimeSheet_Approved_UpdatesAndLogs()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var sheet = new Timesheet
        {
            TimesheetId = 1,
            Status = (int)TimesheetStatus.Pending,
            UserId = 2
        };

        timesheetRepositoryMock.Setup(r => r.GetById(1)).ReturnsAsync(sheet);
        timesheetRepositoryMock.Setup(r => r.Update(sheet)).Returns(Task.CompletedTask);

        auditServiceMock.Setup(a => a.Log(
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(new AuditLog());


        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        var result = await service.UpdateTimeSheet(1, 1, "ok", 5);

        Assert.Equal((int)TimesheetStatus.Approved, result.Status);
        Assert.Equal(5, result.DecidedByUserId);

        auditServiceMock.Verify(a => a.Log(
            5,
            "TimesheetStatusChanged",
            "Timesheet",
            1,
            It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task GetTimeentriesForPendingTimesheet_LeaderIdInvalid_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetTimeentriesForPendingTimesheet(
                0, DateOnly.MinValue, DateOnly.MinValue));
    }

    [Fact]
    public async Task GetTimeentriesForPendingTimesheet_RepoReturnsNull_ReturnsEmpty()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        timesheetRepositoryMock.Setup(r => r.GetTimeentriesForPendingTimesheet(
                1,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))!
            .ReturnsAsync((IEnumerable<TimeEntry>?)null);

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        var result = await service.GetTimeentriesForPendingTimesheet(
            1, DateOnly.MinValue, DateOnly.MaxValue);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task GetTimesheetByUserAndDate_UserIdInvalid_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetTimesheetByUserAndDate(
                0, DateOnly.MinValue, DateOnly.MinValue));
    }

    [Fact]
    public async Task GetTimesheetByUserAndDate_StartAfterEnd_Throws()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.GetTimesheetByUserAndDate(
                1,
                new DateOnly(2025, 2, 1),
                new DateOnly(2025, 1, 1)));
    }

    [Fact]
    public async Task GetTimesheetByUserAndDate_ReturnsTimesheet()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        var sheet = new Timesheet();

        timesheetRepositoryMock.Setup(r => r.GetTimesheetByUserAndDate(
                1,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(sheet);

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        var result = await service.GetTimesheetByUserAndDate(
            1, DateOnly.MinValue, DateOnly.MaxValue);

        Assert.Same(sheet, result);
    }

    [Fact]
    public async Task GetTimesheetByUserAndDate_NotFound_ReturnsNull()
    {
        var timesheetRepositoryMock = new Mock<ITimesheetRepository>();
        var auditServiceMock = new Mock<IAuditLogService>();

        timesheetRepositoryMock.Setup(r => r.GetTimesheetByUserAndDate(
                1,
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync((Timesheet?)null);

        var service = new TimesheetService(timesheetRepositoryMock.Object, auditServiceMock.Object);

        var result = await service.GetTimesheetByUserAndDate(
            1, DateOnly.MinValue, DateOnly.MaxValue);

        Assert.Null(result);
    }
}
