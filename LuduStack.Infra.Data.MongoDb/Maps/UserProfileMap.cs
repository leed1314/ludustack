﻿using LuduStack.Domain.Models;
using MongoDB.Bson.Serialization;

namespace LuduStack.Infra.Data.MongoDb.Maps
{
    public static class UserProfileMap
    {
        public static void Configure()
        {
            BsonClassMap.RegisterClassMap<UserProfile>(map =>
            {
                map.AutoMap();
                map.MapMember(x => x.Name).SetIsRequired(true);
            });
        }
    }
}