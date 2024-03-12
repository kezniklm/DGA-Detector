using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDbGenericRepository.Attributes;

namespace BL.Models.User;

[CollectionName("Users")]
public class User : IdentityUser;
