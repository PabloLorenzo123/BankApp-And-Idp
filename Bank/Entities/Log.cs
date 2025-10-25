namespace Bank.Entities
{
    public class Log
    {
        public int LogId { get; set; }
        public int AccountId { get; set; }
        public string Information { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
