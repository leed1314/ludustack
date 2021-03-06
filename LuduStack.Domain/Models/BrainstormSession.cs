﻿using LuduStack.Domain.Core.Enums;
using LuduStack.Domain.Core.Models;
using System;
using System.Collections.Generic;

namespace LuduStack.Domain.Models
{
    public class BrainstormSession : Entity
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public BrainstormSessionType Type { get; set; }

        public Guid? TargetContextId { get; set; }

        public virtual List<BrainstormIdea> Ideas { get; set; }
    }
}