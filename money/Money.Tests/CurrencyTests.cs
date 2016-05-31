using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace money.Tests
{
    [TestFixture]
    public class CurrencyTests
    {
        private CultureInfo _culture;

        [SetUp]
        public void SetUp()
        {
            _culture = Thread.CurrentThread.CurrentCulture;
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = _culture;
        }
        
        [Test]
        public void Can_create_currency_using_culture_info()
        {
            CurrencyInfo currencyInfo = new CultureInfo("fr-FR");
            Assert.IsNotNull(currencyInfo);
        }

        [Test]
        public void Can_create_currency_using_currency_code()
        {
            CurrencyInfo currencyInfo = Currency.NZD;
            Assert.IsNotNull(currencyInfo);
        }

        [Test]
        public void Can_create_currency_using_current_culture()
        {
            CurrencyInfo currencyInfo = CultureInfo.CurrentCulture;
            Assert.IsNotNull(currencyInfo);
        }

        [Test]
        public void Can_create_currency_using_region_info()
        {
            CurrencyInfo currencyInfo = new RegionInfo("CA");
            Assert.IsNotNull(currencyInfo);
        }

        [Test]
        public void Currency_creation_creates_meaningful_display_cultures()
        {
            // If I'm from France, and I reference Canadian Dollars,
            // then the default culture for CAD should be fr-CA
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            CurrencyInfo currencyInfo = Currency.CAD;
            Assert.AreEqual(currencyInfo.DisplayCulture, new CultureInfo("fr-CA"));

            // If I'm from England, and I reference Canadian Dollars,
            // then the default culture for CAD should be en-CA
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            currencyInfo = Currency.CAD;
            Assert.AreEqual(currencyInfo.DisplayCulture, new CultureInfo("en-CA"));

            // If I'm from Germany, and I reference Canadian Dollars,
            // then the default culture for CAD should be Canadian
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            currencyInfo = Currency.CAD;
            Assert.AreEqual(new CultureInfo("en-CA"), currencyInfo.DisplayCulture);

            // ... and it should not display as if it were in de currency!
            Money money = new Money(Currency.CAD, 1000);
            Assert.AreEqual("$1,000.00", money.DisplayNative());

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
		    money = new Money(1000);
		    var german = new CultureInfo("de-DE");
            Console.WriteLine(money.DisplayIn(german));  // Output: $1,000.00
        }

        [Test]
        public void Currency_creation_creates_meaningful_native_regions()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            CurrencyInfo currencyInfo = Currency.EUR;
            Assert.AreEqual(currencyInfo.NativeRegion, new RegionInfo("FR"));

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
            currencyInfo = Currency.CAD;
            Assert.AreEqual(currencyInfo.NativeRegion, new RegionInfo("CA"));
        }
    }
}