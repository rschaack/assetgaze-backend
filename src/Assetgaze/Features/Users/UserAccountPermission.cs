using LinqToDB.Mapping;

namespace Assetgaze.Features.Users;

[Table("UserAccountPermissions")]
public class UserAccountPermission
{
    [Column("UserId"), NotNull] public Guid UserId { get; set; }
    [Column("AccountId"), NotNull] public Guid AccountId { get; set; }
}