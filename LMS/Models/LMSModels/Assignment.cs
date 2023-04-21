using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submissions = new HashSet<Submission>();
        }

        public string Name { get; set; } = null!;
        public uint MaxPoint { get; set; }
        public string Contents { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public uint CategoryNum { get; set; }
        public uint AssignmentNum { get; set; }

        public virtual AssignmentCategory CategoryNumNavigation { get; set; } = null!;
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
