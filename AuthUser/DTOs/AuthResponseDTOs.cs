namespace AuthUser.DTOs
{
    public class AuthResponseDTOs
    {
        public bool Success { get; set; }=false;
        public string   Message { get; set; }=string.Empty;
        public string Token {  get; set; }=string.Empty ;
        public string RefreshToken { get; set; } =string.Empty ;
        public DateTime TokenExpires { get; set; }


    }
}
