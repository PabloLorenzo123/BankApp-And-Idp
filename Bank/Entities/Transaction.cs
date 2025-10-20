using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public int Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
