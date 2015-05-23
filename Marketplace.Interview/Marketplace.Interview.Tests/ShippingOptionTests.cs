using System.Collections.Generic;
using NUnit.Framework;
using Marketplace.Interview.Business.Basket;
using Marketplace.Interview.Business.Shipping;

namespace Marketplace.Interview.Tests
{
    [TestFixture]
    public class ShippingOptionTests
    {
        [Test]
        public void FlatRateShippingOptionTest()
        {
            var flatRateShippingOption = new FlatRateShipping {FlatRate = 1.5m};
            var shippingAmount = flatRateShippingOption.GetAmount(new LineItem(), new Basket());

            Assert.That(shippingAmount, Is.EqualTo(1.5m), "Flat rate shipping not correct.");
        }

        [Test]
        public void PerRegionShippingOptionTest()
        {
            var perRegionShippingOption = new PerRegionShipping()
                                              {
                                                  PerRegionCosts = new[]
                                                                       {
                                                                           new RegionShippingCost()
                                                                               {
                                                                                   DestinationRegion =
                                                                                       RegionShippingCost.Regions.UK,
                                                                                   Amount = .75m
                                                                               },
                                                                           new RegionShippingCost()
                                                                               {
                                                                                   DestinationRegion =
                                                                                       RegionShippingCost.Regions.Europe,
                                                                                   Amount = 1.5m
                                                                               }
                                                                       },
                                              };

            var shippingAmount = perRegionShippingOption.GetAmount(new LineItem() {DeliveryRegion = RegionShippingCost.Regions.Europe}, new Basket());
            Assert.That(shippingAmount, Is.EqualTo(1.5m));

            shippingAmount = perRegionShippingOption.GetAmount(new LineItem() { DeliveryRegion = RegionShippingCost.Regions.UK}, new Basket());
            Assert.That(shippingAmount, Is.EqualTo(.75m));
        }

        [Test]
        public void BasketShippingTotalTest()
        {
            var perRegionShippingOption = new PerRegionShipping()
            {
                PerRegionCosts = new[]
                                                                       {
                                                                           new RegionShippingCost()
                                                                               {
                                                                                   DestinationRegion =
                                                                                       RegionShippingCost.Regions.UK,
                                                                                   Amount = .75m
                                                                               },
                                                                           new RegionShippingCost()
                                                                               {
                                                                                   DestinationRegion =
                                                                                       RegionShippingCost.Regions.Europe,
                                                                                   Amount = 1.5m
                                                                               }
                                                                       },
            };

            var flatRateShippingOption = new FlatRateShipping {FlatRate = 1.1m};

            var basket = new Basket()
                             {
                                 LineItems = new List<LineItem>
                                                 {
                                                     new LineItem()
                                                         {
                                                             DeliveryRegion = RegionShippingCost.Regions.UK,
                                                             Shipping = perRegionShippingOption
                                                         },
                                                     new LineItem()
                                                         {
                                                             DeliveryRegion = RegionShippingCost.Regions.Europe,
                                                             Shipping = perRegionShippingOption
                                                         },
                                                     new LineItem() {Shipping = flatRateShippingOption},
                                                 }
                             };

            var calculator = new ShippingCalculator();

            decimal basketShipping = calculator.CalculateShipping(basket);

            Assert.That(basketShipping, Is.EqualTo(3.35m));
        }

        [Test]
        public void NewPerRegionShippingTest()
        {
            var newPerRegionShippingOption = new NewPerRegionShipping()
            {
                PerRegionCosts = new[] 
                {
                    new RegionShippingCost()
                    {
                        Amount = 2, // UK £ Pound
                        DestinationRegion = RegionShippingCost.Regions.UK
                    },
                    new RegionShippingCost()
                    {
                        Amount = 20, // UK £ Pound
                        DestinationRegion = RegionShippingCost.Regions.RestOfTheWorld
                    },
                    new RegionShippingCost()
                    {
                        Amount = 20, // UK £ Pound
                        DestinationRegion = RegionShippingCost.Regions.Europe
                    },
                }
            };

            var shippingAmount = newPerRegionShippingOption.GetAmount(new LineItem() { DeliveryRegion = RegionShippingCost.Regions.Europe }, new Basket());

            Assert.AreEqual(20, shippingAmount);

            shippingAmount = newPerRegionShippingOption.GetAmount(new LineItem() { DeliveryRegion = RegionShippingCost.Regions.UK }, new Basket());

            Assert.AreEqual(2, shippingAmount);

            shippingAmount = newPerRegionShippingOption.GetAmount(new LineItem() { DeliveryRegion = RegionShippingCost.Regions.RestOfTheWorld }, new Basket());

            Assert.AreEqual( 20, shippingAmount);
        }

        [Test]
        public void DeductShippingWhenThereIsAtLeastOneOtherItemInBasketWithTheSameShippingOptionSupplierAndRegionTest()
        {
            var newPerRegionShippingOption = new NewPerRegionShipping()
            {
                PerRegionCosts = new[] 
                {
                    new RegionShippingCost()
                    {
                        Amount = 30, // UK £ Pound
                        DestinationRegion = RegionShippingCost.Regions.UK
                    }
                }
            };

            var perRegionShippingOption = new PerRegionShipping()
            {
                PerRegionCosts = new[] 
                {
                    new RegionShippingCost()
                    {
                        Amount = 25, // UK £ Pound
                        DestinationRegion = RegionShippingCost.Regions.UK
                    }
                }
            };

            var basket = new Basket()
            {
                LineItems = new List<LineItem>()
                {
                    new LineItem()
                    {
                        ProductId = "P1",
                        SupplierId = 1,
                        DeliveryRegion = RegionShippingCost.Regions.UK,
                        Shipping = newPerRegionShippingOption
                    },
                    new LineItem()
                    {
                        ProductId = "P2",
                        SupplierId = 1,
                        DeliveryRegion = RegionShippingCost.Regions.UK,
                        Shipping = newPerRegionShippingOption // new option was added
                    },
                    new LineItem()
                    {
                        ProductId = "P3",
                        SupplierId = 3,
                        DeliveryRegion = RegionShippingCost.Regions.UK,
                        Shipping = perRegionShippingOption
                    }
                }
            };

            var rule = new SameShippingOptionSupplierAndRegionRule(50m); // 50pence

            var calculator = new ShippingCalculator();

            decimal basketShipping = calculator.CalculateShipping(basket, rule);

            Assert.AreEqual(84.5, basketShipping);
        }
    }
}
