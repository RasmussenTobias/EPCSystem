using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class Bundle
    {
        [Key]
        public int BundleId { get; set; }
    }

}
