using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace BL.Models.User;

[CollectionName("Users")]
public class User : MongoIdentityUser<Guid>;
