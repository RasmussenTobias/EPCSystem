using EPCSystemAPI.models;

namespace EPCSystemAPI
{
    public interface ITransactionManagementService
    {
        Task<TransactionResponse> SendPreCommit(TradeRequestDto tradeRequest);
        Task<TransactionResponse> CommitTransaction(TradeRequestDto tradeRequest);
        Task<TransactionResponse> AbortTransaction(TradeRequestDto tradeRequest);
    }
    }
