using System;
using System.Collections.Generic;
using Puzzle.Core.Logging;

namespace Puzzle.Core.Services.Mocks
{
    /// <summary>
    /// Resilient mock implementation of IIAPService for testing and development.
    /// Simulates immediate purchases and tracks owned non-consumable products.
    /// </summary>
    public class MockIAPService : IIAPService
    {
        private readonly HashSet<string> _ownedProducts = new HashSet<string>();
        public bool IsInitialized { get; private set; } = true;
        public bool CanMakePayments { get; set; } = true;

        public bool FailNextPurchase { get; set; } = false;

        public void Initialize(Action<bool> onComplete = null)
        {
            IsInitialized = true;
            onComplete?.Invoke(true);
        }

        public void PurchaseProduct(string productId, Action<bool, string> onComplete)
        {
            if (FailNextPurchase)
            {
                FailNextPurchase = false;
                CoreLogger.LogWarning($"[MockIAPService] Simulated purchase failure for: {productId}");
                onComplete?.Invoke(false, "Simulated purchase failure");
                return;
            }

            _ownedProducts.Add(productId);
            CoreLogger.Log($"[MockIAPService] Purchased product: {productId}");
            onComplete?.Invoke(true, null);
        }

        public void RestorePurchases(Action<bool> onComplete)
        {
            CoreLogger.Log("[MockIAPService] Purchases restored.");
            onComplete?.Invoke(true);
        }

        public bool IsProductOwned(string productId)
        {
            return _ownedProducts.Contains(productId);
        }
    }
}
