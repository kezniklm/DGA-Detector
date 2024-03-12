﻿using BL.Models.Interfaces;
using MongoDB.Bson;

namespace BL.Models.Blacklist;

public record BlacklistDetailModel : IModel
{
    public required string DomainName { get; set; }
    public required DateTime Added { get; set; }
    public required ObjectId Id { get; set; }
}
