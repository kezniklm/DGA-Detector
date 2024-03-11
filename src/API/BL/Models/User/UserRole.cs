using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace BL.Models.User;

[CollectionName("Roles")]
public class UserRole : MongoIdentityRole<Guid>;
