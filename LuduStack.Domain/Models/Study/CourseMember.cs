﻿using LuduStack.Domain.Core.Models;
using System;

namespace LuduStack.Domain.Models
{
    public class CourseMember : Entity
    {
        public Guid? PlanId { get; set; }

        public bool Accepted { get; set; }

        public decimal FinalScore { get; set; }

        public bool Passed { get; set; }

        public DateTime? ConclusionDate { get; set; }
    }
}