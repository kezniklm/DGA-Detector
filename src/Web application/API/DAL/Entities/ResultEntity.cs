﻿using DAL.Entities.Interfaces;
using MongoDB.Bson;

namespace DAL.Entities;

public record ResultEntity : IEntity
{
    public DateTime Detected { get; set; }
    public bool DidBlacklistHit { get; set; }
    public double DangerousProbabilityValue { get; set; }
    public bool DangerousBoolValue { get; set; }
    public required string DomainName { get; set; }
    public required ObjectId Id { get; set; }
}
