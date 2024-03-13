﻿using DAL.Entities.Interfaces;
using MongoDB.Bson;

namespace DAL.Entities;

public record WhitelistEntity : IEntity
{
    public required DateTime Added { get; set; }
    public required string DomainName { get; set; }
    public required ObjectId Id { get; set; }
}
