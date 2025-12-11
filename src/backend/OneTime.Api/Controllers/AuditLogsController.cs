using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneTime.Api.Models.AuditLogsDto;
using OneTime.Core.Services.Interfaces;

namespace OneTime.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuditLogsController : ControllerBase
	{
		private readonly IAuditLogService _auditLogService;

		public AuditLogsController(IAuditLogService auditLogService)
		{
			_auditLogService = auditLogService;
		}

		// GET: api/AuditLogs?entityType=Timesheet&action=TimesheetStatusChanged&actorUserId=4
		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> Get([FromQuery] string? entityType,[FromQuery] string? action,[FromQuery] int? actorUserId,[FromQuery] DateTime? from,[FromQuery] DateTime? to)
		{
			var logs = await _auditLogService.Get(entityType, action, actorUserId, from, to);

			if (!logs.Any())
				return NoContent();

			var response = logs.Select(l => new AuditLogDto(
				l.AuditLogId,
				l.Timestamp,
				l.Action ?? string.Empty,
				l.EntityType ?? string.Empty,
				l.EntityId,
				l.ActorUserId,
				l.ActorUser?.Name,
				l.Details
			));

			return Ok(response);
		}

		[HttpGet("all")]
		[ProducesResponseType(200)]
		[ProducesResponseType(204)]
		public async Task<IActionResult> GetAll()
		{
			var logs = await _auditLogService.Get();

			if (!logs.Any())
				return NoContent();

			var response = logs.Select(l => new AuditLogDto(
				l.AuditLogId,
				l.Timestamp,
				l.Action ?? string.Empty,
				l.EntityType ?? string.Empty,
				l.EntityId,
				l.ActorUserId,
				l.ActorUser?.Name,
				l.Details
			));

			return Ok(response);
		}
	}
}
