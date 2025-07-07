namespace Assetgaze.Features.Users;

using LinqToDB.Mapping;


[Table("Users")]
public class User
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Column("Email"), NotNull]
    public string Email { get; set; } = string.Empty;

    [Column("PasswordHash"), NotNull]
    public string PasswordHash { get; set; } = string.Empty;
}