﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MoneyFox.BusinessDbAccess.StatisticDataProvider;
using MoneyFox.BusinessLogic.StatisticDataProvider;
using MoneyFox.DataLayer.Entities;
using MoneyFox.Foundation;
using Moq;
using Should;
using Xunit;

namespace MoneyFox.BusinessLogic.Tests.StatisticDataProvider
{
    [ExcludeFromCodeCoverage]
    public class CashFlowProviderTests
    {
        [Fact]
        public async Task GetValues_NullDependency_NullReferenceException()
        {
            await Assert.ThrowsAsync<NullReferenceException>(
                                                             () =>
                                                                 new CashFlowDataProvider(null)
                                                                    .GetCashFlow(DateTime.Today, DateTime.Today));
        }

        [Fact]
        public async Task GetValues_CorrectSums()
        {
            // Arrange
            var statisticDbAccess = new Mock<IStatisticDbAccess>();
            statisticDbAccess.Setup(x => x.GetPaymentsWithoutTransfer(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                             .Returns(Task.FromResult(new List<Payment>
                                                      {
                                                          new Payment(DateTime.Today, 60, PaymentType.Income,
                                                                      new Account("Foo1")),
                                                          new Payment(DateTime.Today, 70, PaymentType.Income,
                                                                      new Account("Foo2")),
                                                          new Payment(DateTime.Today, 50, PaymentType.Expense,
                                                                      new Account("Foo3")),
                                                          new Payment(DateTime.Today, 40, PaymentType.Expense,
                                                                      new Account("Foo3"))
                                                      }));

            // Act
            var result = await new CashFlowDataProvider(statisticDbAccess.Object)
                            .GetCashFlow(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3));

            // Assert
            result[0].Value.ShouldEqual(130);
            result[1].Value.ShouldEqual(90);
            result[2].Value.ShouldEqual(40);
        }

        [Theory]
        [InlineData("de-CH", 3, '-')]
        public async Task GetValues_CorrectNegativeSign(string culture, int indexNegativeSign,
                                                        char expectedNegativeSign)
        {
            var cultureInfo = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // Arrange
            var statisticDbAccess = new Mock<IStatisticDbAccess>();
            statisticDbAccess.Setup(x => x.GetPaymentsWithoutTransfer(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                             .Returns(Task.FromResult(new List<Payment>
                                                      {
                                                          new Payment(DateTime.Today, 40, PaymentType.Expense,
                                                                      new Account("Foo3"))
                                                      }));

            // Act
            var result = await new CashFlowDataProvider(statisticDbAccess.Object)
                            .GetCashFlow(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3));

            // Assert
            result[2].ValueLabel[indexNegativeSign].ShouldEqual(expectedNegativeSign);
        }

        [Fact]
        public async Task GetValues_USVersion_CorrectNegativeSign()
        {
            var cultureInfo = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // Arrange
            var statisticDbAccess = new Mock<IStatisticDbAccess>();
            statisticDbAccess.Setup(x => x.GetPaymentsWithoutTransfer(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                             .Returns(Task.FromResult(new List<Payment>
                                                      {
                                                          new Payment(DateTime.Today, 40, PaymentType.Expense,
                                                                      new Account("Foo3"))
                                                      }));

            // Act
            var result = await new CashFlowDataProvider(statisticDbAccess.Object)
                            .GetCashFlow(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3));

            // Assert
            // We have to test here for Mac since they have a different standard sign than Windows.
            result[2].ValueLabel[0].ShouldEqual(RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? '-' : '(');
        }

        [Theory]
        [InlineData("en-US", 1, '$')]
        [InlineData("de-CH", 0, 'C')]
        public async Task GetValues_CorrectCurrency(string culture, int indexCurrencySign, char expectedCurrencySymbol)
        {
            var cultureInfo = new CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // Arrange
            var statisticDbAccess = new Mock<IStatisticDbAccess>();
            statisticDbAccess.Setup(x => x.GetPaymentsWithoutTransfer(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                             .Returns(Task.FromResult(new List<Payment>
                                                      {
                                                          new Payment(DateTime.Today, 30, PaymentType.Income,
                                                                      new Account("Foo3")),
                                                          new Payment(DateTime.Today, 40, PaymentType.Expense,
                                                                      new Account("Foo3"))
                                                      }));

            // Act
            var result = await new CashFlowDataProvider(statisticDbAccess.Object)
                            .GetCashFlow(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3));

            // Assert
            result[0].ValueLabel[0].ShouldEqual(expectedCurrencySymbol);
            result[1].ValueLabel[0].ShouldEqual(expectedCurrencySymbol);
            result[2].ValueLabel[indexCurrencySign].ShouldEqual(expectedCurrencySymbol);
        }
    }
}