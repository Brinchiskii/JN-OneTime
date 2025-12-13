using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OneTime.Core.Models
{
	public partial class AuditLog
	{
		[Key]
		public int AuditLogId { get; set; }

		public int? ActorUserId { get; set; }

		[Precision(0)]
		public DateTime Timestamp { get; set; }

		[Required]
		[StringLength(100)]
		public string? Action { get; set; }

		[Required]
		[StringLength(100)]
		public string? EntityType { get; set; }

		public int? EntityId { get; set; }

		public string? Details { get; set; }

		[ForeignKey("ActorUserId")]
		public virtual JNUser? ActorUser { get; set; }
	}
}
