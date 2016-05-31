using System.Collections.Generic;

namespace money
{
    partial class CurrencyInfo
    {
        private static readonly IDictionary<Currency, CurrencyInfo> Currencies = new Dictionary<Currency, CurrencyInfo>(9)
        {
            {
                Currency.USD,
                new CurrencyInfo
                    {
                        DisplayName = "US Dollar",
                        Code = Currency.USD
                    }
                },
            {
                Currency.CAD,
                new CurrencyInfo
                    {
                        DisplayName = "Canadian Dollar",
                        Code = Currency.CAD
                    }
                },
            {
                Currency.EUR,
                new CurrencyInfo
                    {
                        DisplayName = "Euro",
                        Code = Currency.EUR
                    }
                },
            {
                Currency.GBP,
                new CurrencyInfo
                    {
                        DisplayName = "Pound Sterling",
                        Code = Currency.GBP
                    }
                },
            {
                Currency.JPY,
                new CurrencyInfo
                    {
                        DisplayName = "Yen",
                        Code = Currency.JPY
                    }
                },
            {
                Currency.CHF,
                new CurrencyInfo
                    {
                        DisplayName = "Swiss Franc",
                        Code = Currency.CHF
                    }
                },
            {
                Currency.AUD,
                new CurrencyInfo
                    {
                        DisplayName = "Australian Dollar",
                        Code = Currency.AUD
                    }
                },
            {
                Currency.NZD,
                new CurrencyInfo
                    {
                        DisplayName = "New Zealand Dollar",
                        Code = Currency.NZD
                    }
                },
            {
                Currency.INR,
                new CurrencyInfo
                    {
                        DisplayName = "Indian Rupee",
                        Code = Currency.INR
                    }
                },
            {
                Currency.DKK,
                new CurrencyInfo
                    {
                        DisplayName = "Danish Krone",
                        Code = Currency.DKK
                    }
                }
        };
    }
}