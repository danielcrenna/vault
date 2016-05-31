using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace money.Tests
{
    [TestFixture]
    public class MoneyTests
    {
        [Test]
        public void Can_add_money()
        {
            const double left = 10.00;
            const double right = 20.00;

            Money total = left + right;
            Assert.AreEqual(30.00, (double) total);
        }

        [Test]
        public void Can_create_money_by_decimals()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Money money = 10.00;
            Assert.AreEqual(10.00, (double) money);
        }

        [Test]
        public void Can_create_money_by_units()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Money money = 1000;
            Assert.AreEqual(1000.00, (double) money);
        }

        [Test]
        public void Can_create_money_in_current_currency()
        {
            Money_with_current_culture_has_correct_currency_code("fr-FR", Currency.EUR);
            Money_with_current_culture_has_correct_currency_code("en-US", Currency.USD);
            Money_with_current_culture_has_correct_currency_code("en-CA", Currency.CAD);
            Money_with_current_culture_has_correct_currency_code("en-AU", Currency.AUD);
            Money_with_current_culture_has_correct_currency_code("en-GB", Currency.GBP);
            Money_with_current_culture_has_correct_currency_code("ja-JP", Currency.JPY);

            // Subsequent tests rely on en-US culture for currency rules
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        private static void Money_with_current_culture_has_correct_currency_code(string cultureName, Currency currency)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Money money = 1000;
            Assert.AreEqual(currency, money.CurrencyInfo.Code);

            Console.WriteLine(money.CurrencyInfo.Code);
            Console.WriteLine(money.CurrencyInfo.DisplayCulture);
            Console.WriteLine(money.CurrencyInfo.DisplayName);
            Console.WriteLine(money.CurrencyInfo.NativeRegion);
        }

        [Test]
        public void Can_determine_equality()
        {
            Money left = 456;
            Money right = 0.456;

            Assert.IsFalse(left == right);
            Assert.IsFalse(left.Equals(right));
            Assert.IsFalse((long) left == (long) right);
            Assert.IsFalse(left == (long) right);
            Assert.IsFalse((long) left == right);
        }

        [Test]
        public void Can_display_currency_in_given_culture_preserving_native_culture_info()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            Money expectedMoney = 1000;
            var expected = expectedMoney.ToString();
            Console.WriteLine(expected);

            // Display the fr-FR money in en-CA format
            var actual = expectedMoney.DisplayIn(new CultureInfo("en-CA"));
            Console.WriteLine(actual);
            Assert.AreNotEqual(expected, actual);
        }

        [Test]
        public void Can_display_currency_in_given_culture_with_disambiguation()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Money expectedMoney = 1000;
            var expected = expectedMoney.ToString();
            Console.WriteLine(expected);

            // Display the en-US money in en-CA format with "USD" disambiguation
            var actual = expectedMoney.DisplayIn(new CultureInfo("en-CA"));
            Console.WriteLine(actual);
            Assert.AreNotEqual(expected, actual);
        }

        [Test]
        public void Can_display_proper_culture_when_created_in_different_culture()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            var money = new Money(Currency.CAD, 1000);
            Assert.AreEqual(new CultureInfo("fr-CA"), money.CurrencyInfo.DisplayCulture);
            Console.WriteLine(money.ToString());
        }

        [Test]
        public void Can_divide_money()
        {
            const double left = 20.00;
            const double right = 2.00;

            Money total = left/right;
            Assert.AreEqual(10.00, (double) total);
        }

        [Test]
        public void Can_divide_money_with_decimals()
        {
            const decimal left = 20.00m;
            const decimal right = 2.00m;

            Money total = left / right;
            Assert.AreEqual(10.00m, total);
        }

        [Test]
        public void Can_divide_money_by_negative_identity()
        {
            var left = new Money(1);
            var right = new Money(-1);

            var total = right/left;
            Assert.AreEqual(-1, total);
        }

        [Test]
        public void Can_divide_money_by_negative_identity_with_decimals()
        {
            var left = new Money(1m);
            var right = new Money(-1m);

            var total = right / left;
            Assert.AreEqual(-1m, total);
        }

        [Test]
        public void Can_divide_money_by_positive_identity()
        {
            var left = new Money(1);
            var right = new Money(1);

            var total = right/left;
            Assert.AreEqual(1, total);
        }

        [Test]
        public void Can_divide_money_by_positive_identity_with_decimals()
        {
            var left = new Money(1m);
            var right = new Money(1m);

            var total = right / left;
            Assert.AreEqual(1m, total);
        }

        [Test]
        public void Can_handle_division_without_precision_loss()
        {
            Money left = 45;
            Money right = 13;

            var total = left/right; // 3.461538461538462

            Assert.AreEqual(3.46, (double) total);
        }

        [Test]
        public void Can_handle_division_without_precision_loss_with_decimals()
        {
            Money left = 45m;
            Money right = 13m;

            var total = left / right; // 3.461538461538462

            Assert.AreEqual(3.46m, total);
        }

        [Test]
        public void Can_handle_small_fractions()
        {
            Money total = 0.1;
            Assert.AreEqual(0.10, (double) total);
        }

        [Test]
        public void Can_handle_small_fractions_with_decimals()
        {
            Money total = 0.1m;
            Assert.AreEqual(0.10m, total);
        }

        [Test]
        public void Can_multiply_identity_without_casting()
        {
            var left = new Money(1.00);
            var right = new Money(1.00);

            var total = right*left;
            Assert.AreEqual(1.00, (double) total);
        }

        [Test]
        public void Can_multiply_identity_without_casting_with_decimals()
        {
            var left = new Money(1.00m);
            var right = new Money(1.00m);

            var total = right * left;
            Assert.AreEqual(1.00m, total);
        }

        [Test]
        public void Can_multiply_money()
        {
            var left = new Money(10.00);
            var right = new Money(20.00);

            var total = right*left;
            Assert.AreEqual(200.00, (double) total);
        }

        [Test]
        public void Can_multiply_money_with_decimals()
        {
            var left = new Money(10.00m);
            var right = new Money(20.00m);

            var total = right * left;
            Assert.AreEqual(200.00m, total);
        }

        [Test]
        public void Can_multiply_money_by_negative_identity()
        {
            const double left = 1.00;
            const double right = -1;

            Money total = right*left;
            Assert.AreEqual(-1, (double) total);
        }

        [Test]
        public void Can_multiply_money_by_positive_identity()
        {
            const double left = 1.00;
            const double right = 1;

            Money total = right*left;
            Assert.AreEqual(1, (double) total);
        }

        [Test]
        public void Can_multiply_non_identity_without_casting()
        {
            var left = new Money(4.00);
            var right = new Money(4.00);

            var total = right*left;
            Assert.AreEqual(16.00, (double) total);
        }

        [Test]
        public void Can_preserve_internal_precision()
        {
            Money total = 0.335678*345; // 115.80891

            // Loss of precision based on rounding rules
            Assert.AreEqual(115.81, (double) total);

            // Adding .005 to 115.81 would equal 115.82 
            // due to rounding if precision was lost
            total += 0.005; // 115.81391

            Assert.AreEqual(115.81, (double) total);
        }

        [Test]
        public void Can_preserve_internal_precision_with_decimals()
        {
            Money total = 0.335678m * 345m; // 115.80891

            // Loss of precision based on rounding rules
            Assert.AreEqual(115.81m, total);

            // Adding .005 to 115.81 would equal 115.82 
            // due to rounding if precision was lost
            total += 0.005m; // 115.81391

            Assert.AreEqual(115.81m, total);
        }

        [Test]
        public void Can_preserve_internal_rounding_against_larger_fractions()
        {
            Money total = 0.335678*345; // 115.80891

            // Loss of precision based on rounding rules
            Assert.AreEqual(115.81, (double) total);

            // This number has greater precision than the original
            total += .00082809; // 115.80973809

            Assert.AreEqual(115.81, (double) total);
        }

        [Test]
        public void Can_preserve_internal_rounding_against_larger_fractions_with_decimals()
        {
            Money total = 0.335678m * 345m; // 115.80891

            // Loss of precision based on rounding rules
            Assert.AreEqual(115.81m, total);

            // This number has greater precision than the original
            total += .00082809m; // 115.80973809

            Assert.AreEqual(115.81m, total);
        }

        [Test]
        public void Can_preserve_internal_rounding_against_smaller_fractions()
        {
            Money total = 0.335678*345; // 115.80891

            // Loss of precision based on rounding rules
            Assert.AreEqual(115.81, (double) total);

            // This number has lesser precision than the original
            total += .456; // 116.26491

            Assert.AreEqual(116.26, (double) total);
        }

        [Test]
        public void Can_preserve_internal_rounding_against_smaller_fractions_with_decimals()
        {
            Money total = 0.335678m * 345m; // 115.80891

            // Loss of precision based on rounding rules
            Assert.AreEqual(115.81m, total);

            // This number has lesser precision than the original
            total += .456m; // 116.26491

            Assert.AreEqual(116.26m, total);
        }

        [Test]
        public void Can_subtract_money()
        {
            const double left = 10.00;
            const double right = 20.00;

            Money total = right - left;
            Assert.AreEqual(10.00, (double) total);
        }

        [Test]
        public void Cannot_add_different_currencies()
        {
            var left = new Money(Currency.CAD, 10.00);
            var right = new Money(Currency.USD, 20.00);
            
            Assert.Throws(typeof (ArithmeticException), () =>
                                                            {
                                                                var total = left + right;
                                                                Console.WriteLine(total);
                                                            });
        }

        [Test]
        public void Can_sum()
        {
            var monies = new List<Money>();
            for (var i = 0; i < 10; i++)
            {
                monies.Add(new Money(1.45m));
            }
            var value = new Money(0);
            foreach (var money in monies)
            {
                value += money;
            }

            Console.WriteLine(value.ToString()); // Outputs 14, but is actually stored internally as $14.50
        }

    }
}