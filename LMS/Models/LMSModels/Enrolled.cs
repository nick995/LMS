using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrolled
    {
        public string UId { get; set; } = null!;
        public uint ClassNum { get; set; }
        public string Grade { get; set; } = null!;

        public virtual Class ClassNumNavigation { get; set; } = null!;
        public virtual Student UIdNavigation { get; set; } = null!;
    }
}
