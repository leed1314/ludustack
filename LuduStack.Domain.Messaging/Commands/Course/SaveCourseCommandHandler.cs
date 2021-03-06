﻿using LuduStack.Domain.Interfaces;
using LuduStack.Domain.Interfaces.Repository;
using LuduStack.Domain.Interfaces.Services;
using LuduStack.Infra.CrossCutting.Messaging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LuduStack.Domain.Messaging
{
    public class SaveCourseCommandHandler : CommandHandler, IRequestHandler<SaveCourseCommand, CommandResult>
    {
        protected readonly IUnitOfWork unitOfWork;
        protected readonly IStudyCourseRepository studyCourseRepository;
        protected readonly IGamificationDomainService gamificationDomainService;

        public SaveCourseCommandHandler(IUnitOfWork unitOfWork, IStudyCourseRepository studyCourseRepository, IGamificationDomainService gamificationDomainService)
        {
            this.unitOfWork = unitOfWork;
            this.studyCourseRepository = studyCourseRepository;
            this.gamificationDomainService = gamificationDomainService;
        }

        public async Task<CommandResult> Handle(SaveCourseCommand request, CancellationToken cancellationToken)
        {
            var result = request.Result;

            if (!request.IsValid()) return request.Result;

            if (request.Course.Id == Guid.Empty)
            {
                studyCourseRepository.Add(request.Course);
            }
            else
            {
                studyCourseRepository.Update(request.Course);
            }

            var commitResult = await Commit(unitOfWork);

            result.Validation = commitResult;

            return result;
        }
    }
}