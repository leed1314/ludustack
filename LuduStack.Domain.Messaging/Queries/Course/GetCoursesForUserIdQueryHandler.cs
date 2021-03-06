﻿using LuduStack.Domain.Interfaces.Repository;
using LuduStack.Domain.Messaging.Queries.Course;
using LuduStack.Domain.ValueObjects.Study;
using LuduStack.Infra.CrossCutting.Messaging;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LuduStack.Domain.Messaging
{
    public class GetCoursesForUserIdQueryHandler : QueryHandler, IRequestHandler<GetCoursesForUserIdQuery, StudyCoursesOfUserVo>
    {
        protected readonly IStudyCourseRepository studyCourseRepository;

        public GetCoursesForUserIdQueryHandler(IStudyCourseRepository studyCourseRepository)
        {
            this.studyCourseRepository = studyCourseRepository;
        }

        public async Task<StudyCoursesOfUserVo> Handle(GetCoursesForUserIdQuery message, CancellationToken cancellationToken)
        {
            List<UserCourseVo> objs = studyCourseRepository.Get().Where(x => x.Members.Any(y => y.UserId == message.UserId)).Select(x => new UserCourseVo
            {
                CourseId = x.Id,
                CourseName = x.Name
            }).ToList();

            StudyCoursesOfUserVo result = new StudyCoursesOfUserVo
            {
                UserId = message.UserId
            };

            if (objs.Any())
            {
                result.Courses = objs;
            }

            return result;
        }
    }
}