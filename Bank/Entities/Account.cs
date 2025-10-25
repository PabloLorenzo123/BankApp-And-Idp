namespace Bank.Entities
{
    public class Account
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public bool Deleted { get; set; }
    }
}
