using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Entities
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
