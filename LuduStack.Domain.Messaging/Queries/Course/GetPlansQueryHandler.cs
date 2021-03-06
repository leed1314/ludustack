﻿using LuduStack.Domain.Interfaces.Repository;
using LuduStack.Domain.Messaging.Queries.Course;
using LuduStack.Domain.Models;
using LuduStack.Infra.CrossCutting.Messaging;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LuduStack.Domain.Messaging
{
    public class GetPlansQueryHandler : QueryHandler, IRequestHandler<GetPlansQuery, List<StudyPlan>>
    {
        protected readonly IStudyCourseRepository studyCourseRepository;

        public GetPlansQueryHandler(IStudyCourseRepository studyCourseRepository)
        {
            this.studyCourseRepository = studyCourseRepository;
        }

        public async Task<List<StudyPlan>> Handle(GetPlansQuery message, CancellationToken cancellationToken)
        {
            List<StudyPlan> entries = await studyCourseRepository.GetPlans(message.CourseId);

            return entries;
        }
    }
}