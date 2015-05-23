using System.Linq;

namespace Marketplace.Interview.Business.Basket
{
    public abstract class RuleCalculator
    {
        public abstract decimal Apply(Basket basket);
    }

    public class SameShippingOptionSupplierAndRegionRule : RuleCalculator
    {
        private decimal deductionValue;

        public SameShippingOptionSupplierAndRegionRule()
            : this(0m)
        {
        }

        public SameShippingOptionSupplierAndRegionRule(decimal value)
        {
            deductionValue = value;
        }

        public override decimal Apply(Basket basket)
        {
            foreach (var lineItem in basket.LineItems)
            {
                lineItem.ShippingAmount = lineItem.Shipping.GetAmount(lineItem, basket);
                lineItem.ShippingDescription = lineItem.Shipping.GetDescription(lineItem, basket);
            }

            var sum = basket.LineItems.Sum(li => li.ShippingAmount);

            var count = basket.LineItems.GroupBy(o => o.GetHashCode()).Max(p => p.Count());

            return count > 1 ? sum - deductionValue/100m : sum;
        }
    }
}
