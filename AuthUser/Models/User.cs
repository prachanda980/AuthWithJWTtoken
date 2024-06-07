namespace AuthUser.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }= new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
        public string RefreshToken { get; set; }
        public DateTime TokenCreated {  get; set; }
        public DateTime TokenExpires{ get; set; }
        public string UserRole {  get; set; }=string.Empty;

    }
}
