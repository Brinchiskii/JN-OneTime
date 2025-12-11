using Microsoft.EntityFrameworkCore;
using OneTime.Core.Data.Interfaces;
using OneTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Data.Repository
{
	public class AuditLogRepository : IAuditLogRepository
	{
		private readonly OneTimeContext _context;

		public AuditLogRepository(OneTimeContext context)
		{
			_context = context;
		}

		public async Task<AuditLog> Add(AuditLog log)
		{
			_context.AuditLogs.Add(log);
			await _context.SaveChangesAsync();
			return log;
		}

		public async Task<IEnumerable<AuditLog>> Get(
			string? entityType = null,
			string? action = null,
			int? actorUserId = null,
			DateTime? from = null,
			DateTime? to = null)
		{
			var query = _context.AuditLogs.Include(a => a.ActorUser).AsQueryable();

			if (!string.IsNullOrWhiteSpace(entityType))
				query = query.Where(a => a.EntityType == entityType);

			if (!string.IsNullOrWhiteSpace(action))
				query = query.Where(a => a.Action == action);

			if (actorUserId.HasValue)
				query = query.Where(a => a.ActorUserId == actorUserId.Value);

			if (from.HasValue)
				query = query.Where(a => a.Timestamp >= from.Value);

			if (to.HasValue)
				query = query.Where(a => a.Timestamp <= to.Value);

			return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
		}
	}
}
