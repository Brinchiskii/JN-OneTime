using OneTime.Core.Data.Interfaces;
using OneTime.Core.Models;
using OneTime.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Implementations
{
	public class AuditLogService : IAuditLogService
	{
		private readonly IAuditLogRepository _repo;

		public AuditLogService(IAuditLogRepository repo)
		{
			_repo = repo;
		}

		public async Task<AuditLog> Log(int? actorUserId,string action,string entityType,int? entityId = null,string? details = null)
		{
			var log = new AuditLog
			{
				ActorUserId = actorUserId,
				Action = action,
				EntityType = entityType,
				EntityId = entityId,
				Details = details,
				Timestamp = DateTime.Now
			};

			return await _repo.Add(log);
		}

		public Task<IEnumerable<AuditLog>> Get(string? entityType = null,string? action = null,int? actorUserId = null,DateTime? from = null,DateTime? to = null)
		{
			return _repo.Get(entityType, action, actorUserId, from, to);
		}
	}
}
