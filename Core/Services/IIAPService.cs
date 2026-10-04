using System;

namespace Puzzle.Core.Services
{
    public interface IIAPService
    {
        bool IsInitialized { get; }
        bool CanMakePayments { get; }
        
        void Initialize(Action<bool> onComplete = null);
        void PurchaseProduct(string productId, Action<bool, string> onComplete);
        void RestorePurchases(Action<bool> onComplete);
        bool IsProductOwned(string productId);
    }
}
