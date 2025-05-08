namespace atmnr_api.Models;
public class UserInfo
{
    public String? UserId { get; set; }
    public String? Name { get; set; }
    public String? Firstname { get; set; }
    public String? Lastname { get; set; }
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public String? Telephone { get; set; }
    public String? Email { get; set; }
    public Int16? Archived { get; set; }
    public ICollection<UserClaimInfo>? Claims { get; set; }
}

public class UserSummaryInfo
{
    public Int16 Archived { get; set; }
    public int Count { get; set; }
}

public class UserListInfo
{
    public String? UserId { get; set; }
    public String? Name { get; set; }
}

public class UserComboInfo
{
    public String? UserId { get; set; }
    public String? Name { get; set; }

}