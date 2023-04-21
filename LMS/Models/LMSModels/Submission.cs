using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public string UId { get; set; } = null!;
        public uint AssignmentNum { get; set; }
        public uint? Score { get; set; }
        public DateTime Time { get; set; }
        public string Contents { get; set; } = null!;

        public virtual Assignment AssignmentNumNavigation { get; set; } = null!;
        public virtual Student UIdNavigation { get; set; } = null!;
    }
}
