using EPCSystemAPI.models;
using System.Threading.Tasks;

namespace EPCSystemAPI.Services
{
    public interface ITransactionManagementService
    {
        Task<TransactionResponse> SendPreCommit(TradeRequestDto tradeRequest);
        Task<TransactionResponse> CommitTransaction(TradeRequestDto tradeRequest);
        Task<TransactionResponse> AbortTransaction(TradeRequestDto tradeRequest);
    }
}
