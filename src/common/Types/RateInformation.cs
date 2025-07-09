using System;

namespace Fortium.Types
{
    public class RateInformation
    {
        public decimal RatePerMinute { get; set; }
        public decimal MinimumCharge { get; set; }
        public int MinimumMinutes { get; set; }
        public int BillingIncrementMinutes { get; set; } = 1; // Default to 1-minute increments
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        
        public decimal CalculateCost(DateTime startTime, DateTime endTime)
        {
            var durationMinutes = (int)Math.Ceiling((endTime - startTime).TotalMinutes);
            
            // Ensure minimum duration is met
            durationMinutes = Math.Max(durationMinutes, MinimumMinutes);
            
            // Round up to billing increment
            var billableMinutes = (int)Math.Ceiling(
                durationMinutes / (double)BillingIncrementMinutes
            ) * BillingIncrementMinutes;
            
            var cost = billableMinutes * RatePerMinute;
            
            // Apply minimum charge if applicable
            return Math.Max(cost, MinimumCharge);
        }
    }
}
