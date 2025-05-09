namespace api3.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Used { get; set; }
    }
}
