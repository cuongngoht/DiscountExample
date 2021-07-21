using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscountExample
{
    class Program
    {
        // Your task is to program a software which can calculate prices for articles in a shop.
        //An article will have at least a name, a slogan, a net price, a sales price and a VAT ratio.From time to time some special
        //discounts are available for some articles. Discounts are valid only for a certain period of time.When a discount applies,
        //the price presented to the customer has to be the discounted value. Discounted value should never be lower than the net
        //price in order to prevent a loss on the article. Each article can have multiple discount definitions, but only one can be
        //applicable at a time.
        //The program should be able to store an unlimited number of articles and to output the price of each article given a
        //specific date
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var discountes = new List<Discount>() {
                            new Discount1(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(1), 10),
                            new Discount1(DateTime.UtcNow, DateTime.UtcNow.AddDays(2), 10)
            };

            List<Article> articles = new List<Article>();
            for (int i = 0; i < 10; i++)
            {
                var article = new Article($"Product {i}", $"Product {i} Sologan", 1000, 10, discountes, 1200);
                article.PrepairPriceDiscount();
                article.ApplyPriceDiscount();
                articles.Add(article);
            }
            var articlesByDate = articles.Where(x => x.PriceDates.ContainsKey(DateTime.UtcNow.Date)).ToList();
        }


        class Article
        {
            public Article(string name, string slogan, double netPrice, int vatRation, List<Discount> discounts, double price)
            {
                Name = name;
                Slogan = slogan;
                NetPrice = netPrice;
                VatRation = vatRation;
                Discounts = discounts;
                Price = price;
                OldPrice = price;
            }

            public string Name { get; private set; }
            public string Slogan { get; private set; }
            public double NetPrice { get; private set; }
            public double Price { get; private set; }
            public double OldPrice { get; private set; }
            public int VatRation { get; private set; }
            public List<Discount> Discounts { get; private set; }
            public Dictionary<DateTime, (double oldPrice, double price)> PriceDates { get; private set; }
            public void PrepairPriceDiscount()
            {
                if (!CheckValidateDiscount()) return;
                foreach (var discount in Discounts)
                {
                    if (PriceDates == null)
                    {
                        discount.DiscountMethod(NetPrice, Price);
                    }
                    else
                    {
                        var priceDateMax = PriceDates.FirstOrDefault(x => x.Key == PriceDates.Max(date => date.Key));
                        discount.DiscountMethod(NetPrice, priceDateMax.Value.price);
                    }
                }
            }
            private bool CheckValidateDiscount()
            {
                if (Discounts == null || !Discounts.Any())
                {
                    Console.WriteLine("Don't have any discount");
                    return false;
                }
                return true;
            }
            public void ApplyPriceDiscount()
            {
                if (!CheckValidateDiscount()) return;
                var dateTimeNow = DateTime.Now;
                var discountApply = Discounts.FirstOrDefault(x => x.IsApplicable
                                                && x.DiscountRatio == Discounts.Max(discount => discount.DiscountRatio)
                                                && x.StartDate <= dateTimeNow && x.EndDate >= dateTimeNow);
                if (discountApply == null)
                {
                    Console.WriteLine("Don't have any discount appropriate for article");
                    return;
                }
                if (PriceDates == default)
                {
                    PriceDates = new Dictionary<DateTime, (double oldPrice, double price)>();
                }
                PriceDates.Add(DateTime.UtcNow.Date, (Price, discountApply.NewPrice));
            }

        }

        public abstract class Discount
        {
            public Discount(DateTime startDate, DateTime endDate, int discountRatio)
            {
                StartDate = startDate;
                EndDate = endDate;
                DiscountRatio = discountRatio;
            }
            public DateTime StartDate { get; private set; }
            public DateTime EndDate { get; private set; }
            public bool IsApplicable { get; protected set; }
            public int DiscountRatio { get; private set; }
            public double NewPrice { get; protected set; }
            public abstract void DiscountMethod(double netPrice, double price);
        }

        public class Discount1 : Discount
        {
            public Discount1(DateTime startDate, DateTime endDate, int discountRatio) : base(startDate, endDate, discountRatio)
            {

            }

            public override void DiscountMethod(double netPrice, double price)
            {
                if (StartDate >= EndDate)
                {
                    Console.WriteLine("Time to end date is equal or less start date");
                    IsApplicable = false;
                }

                var discountPrice = price - ((DiscountRatio / 100.0) * price);
                if (discountPrice < netPrice)
                {
                    Console.WriteLine("Discount Price to end date is less Net Price");
                    IsApplicable = false;
                }
                IsApplicable = true;
                NewPrice = discountPrice;
            }
        }
    }
}
