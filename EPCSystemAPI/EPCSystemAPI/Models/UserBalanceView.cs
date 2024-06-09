using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class UserBalanceView
    {
        public int UserId { get; set; }
        public int EnergyProductionId { get; set; }
        public decimal Balance { get; set; }
    }
}
