using System.Linq;

namespace Marketplace.Interview.Business.Basket
{
    public interface IShippingCalculator
    {
        decimal CalculateShipping(Basket basket);
        decimal CalculateShipping(Basket basket, RuleCalculator rule); 
    }

    public class ShippingCalculator : IShippingCalculator
    {
        public decimal CalculateShipping(Basket basket)
        {
            foreach (var lineItem in basket.LineItems)
            {
                lineItem.ShippingAmount = lineItem.Shipping.GetAmount(lineItem, basket);
                lineItem.ShippingDescription = lineItem.Shipping.GetDescription(lineItem, basket);
            }

            return basket.LineItems.Sum(li => li.ShippingAmount);
        }

        public decimal CalculateShipping(Basket basket, RuleCalculator rule)
        {
            return rule.Apply(basket);
        }
    }
}
