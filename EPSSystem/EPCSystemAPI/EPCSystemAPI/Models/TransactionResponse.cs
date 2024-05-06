using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPCSystemAPI.models
{
    public class TransactionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

}
