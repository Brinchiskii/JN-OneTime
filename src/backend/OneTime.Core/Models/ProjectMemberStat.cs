using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Models
{
    public class ProjectMemberStat
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Hours { get; set; }
    }
}
