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
    
    [Column("CreatedDate"), NotNull]
    public DateTime CreatedDate { get; set; }

    [Column("LastLoginDate")]
    public DateTime? LastLoginDate { get; set; } // Nullable: new user hasn't logged in

    [Column("LastPasswordChangeDate")]
    public DateTime? LastPasswordChangeDate { get; set; } // Nullable: can be set at creation

    [Column("FailedLoginAttempts"), NotNull]
    public int FailedLoginAttempts { get; set; }

    [Column("LoginCount"), NotNull]
    public int LoginCount { get; set; }
    
    [Column("LockoutEndDateUtc")]
    public DateTime? LockoutEndDateUtc { get; set; } // Nullable: null means not locked
}
