using System;
using System.Collections.Generic;
using System.Text;

namespace OneTime.Core.Models
{
    public class ProjectStatModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal TotalHours { get; set; }
        public List<ProjectMemberStat> Members { get; set; } = new();
    }
}
