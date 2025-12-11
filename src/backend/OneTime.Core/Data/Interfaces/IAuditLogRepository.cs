using OneTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Data.Interfaces
{
	public interface IAuditLogRepository
	{
		Task<AuditLog> Add(AuditLog log);

		Task<IEnumerable<AuditLog>> Get(string? entityType = null, string? action = null, int? actorUserId = null, DateTime? from = null,DateTime? to = null);
	}
}
