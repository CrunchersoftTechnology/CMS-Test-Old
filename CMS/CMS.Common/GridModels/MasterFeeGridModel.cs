﻿using System;

namespace CMS.Common.GridModels
{
    public class MasterFeeGridModel
    {
        public int MasterFeeId { get; set; }

        public string Year { get; set; }

        public int SubjectId { get; set; }

        public string SubjectName { get; set; }

        public int ClassId { get; set; }

        public string ClassName { get; set; }

        public decimal Fee { get; set; }

        [Exclude]
        public string Action { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
