using System.ComponentModel.DataAnnotations;

namespace EPCSystemAPI.models
{
    public class DeviceAmountDto
    {
        public int DeviceId { get; set; }
        public decimal Amount { get; set; }
    }
}
