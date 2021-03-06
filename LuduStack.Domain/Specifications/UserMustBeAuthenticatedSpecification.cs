﻿using LuduStack.Domain.Core.Interfaces;
using System;

namespace LuduStack.Domain.Specifications
{
    public class UserMustBeAuthenticatedSpecification<T> : ISpecification<T> where T : IEntity
    {
        private Guid currentUserId;

        public UserMustBeAuthenticatedSpecification(Guid currentUserId)
        {
            this.currentUserId = currentUserId;
        }

        public string ErrorMessage => "You must be authenticated!";

        public bool IsSatisfied { get; private set; }

        public bool IsSatisfiedBy(T item)
        {
            IsSatisfied = currentUserId != Guid.Empty;

            return IsSatisfied;
        }
    }
}