using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class UserBalanceView
    {
        [Column("UserId")]
        public int UserId { get; set; }
        [Column("Username")]
        public string Username { get; set; }
        
        [Column("TotalBalance")]
        public decimal TotalTransactionAmount { get; set; }
    }
}
