using OneTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Services.Interfaces
{
	public interface IAuditLogService
	{
		Task<AuditLog> Log(int? actorUserId,string action,string entityType,int? entityId = null,string? details = null);

		Task<IEnumerable<AuditLog>> Get(string? entityType = null,string? action = null,int? actorUserId = null,DateTime? from = null,DateTime? to = null);
	}
}
