﻿using LuduStack.Domain.Interfaces.Repository;
using LuduStack.Domain.Interfaces.Services;
using LuduStack.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LuduStack.Domain.Services
{
    public class GamificationLevelDomainService : BaseDomainMongoService<GamificationLevel, IGamificationLevelRepository>, IGamificationLevelDomainService
    {
        public GamificationLevelDomainService(IGamificationLevelRepository repository) : base(repository)
        {
        }

        public async Task<GamificationLevel> GenerateNew(Guid userId)
        {
            GamificationLevel model = new GamificationLevel
            {
                UserId = userId
            };

            System.Collections.Generic.IEnumerable<GamificationLevel> all = await repository.GetAll();

            int maxNumber = all.Max(x => x.Number);
            model.Number = maxNumber + 1;

            int maxXp = all.Max(x => x.XpToAchieve);
            model.XpToAchieve = maxXp * 2;

            return model;
        }

        public async Task<bool> ValidateXp(int xpToAchieve, Guid id)
        {
            System.Collections.Generic.IEnumerable<GamificationLevel> all = await repository.GetAll();

            int maxXp = all.Max(x => x.XpToAchieve);
            Guid? maxId = all.FirstOrDefault(x => x.XpToAchieve == maxXp)?.Id;

            if (id == Guid.Empty)
            {
                return xpToAchieve > maxXp;
            }
            else if (id == maxId)
            {
                return xpToAchieve >= maxXp;
            }
            else
            {
                return true;
            }
        }
    }
}