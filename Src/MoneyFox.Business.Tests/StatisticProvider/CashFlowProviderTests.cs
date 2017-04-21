﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MoneyFox.Business.StatisticDataProvider;
using MoneyFox.DataAccess.Repositories;
using MoneyFox.Foundation;
using MoneyFox.Foundation.DataModels;
using MoneyFox.Foundation.Tests;
using MoneyFox.Service.DataServices;
using MoneyFox.Service.Pocos;
using Moq;
using Xunit;

namespace MoneyFox.Business.Tests.StatisticProvider
{
    public class CashFlowProviderTests
    {
        [Fact]
        public void Constructor_Null_NotNullObject()
        {
            new CashFlowDataProvider(null).ShouldNotBeNull();
        }

        [Fact]
        public async void GetValues_NullDependency_NullReferenceException()
        {
            await Assert.ThrowsAsync<NullReferenceException>(
                () => new CashFlowDataProvider(null).GetCashFlow(DateTime.Today, DateTime.Today));
        }

        [Fact]
        public async void GetValues_CorrectSums()
        {
            // Arrange
            var paymentRepoSetup = new Mock<IPaymentService>();
            paymentRepoSetup.Setup(x => x.GetPaymentsWithoutTransfer(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                            .Returns(Task.FromResult<IEnumerable<Payment>>(new List<Payment>
                            {
                                new Payment
                                {
                                    Data =
                                    {
                                        Id = 1,
                                        Type = PaymentType.Income,
                                        Date = DateTime.Today,
                                        Amount = 60
                                    }
                                },
                                new Payment
                                {
                                    Data =
                                    {
                                        Id = 3,
                                        Type = PaymentType.Income,
                                        Date = DateTime.Today,
                                        Amount = 70
                                    }
                                },
                                new Payment
                                {
                                    Data =
                                    {
                                        Id = 2,
                                        Type = PaymentType.Expense,
                                        Date = DateTime.Today,
                                        Amount = 50
                                    }
                                },
                                new Payment
                                {
                                    Data =
                                    {
                                        Id = 3,
                                        Type = PaymentType.Expense,
                                        Date = DateTime.Today,
                                        Amount = 40
                                    }
                                }
                            }));

            // Act
            var result = await new CashFlowDataProvider(paymentRepoSetup.Object)
                .GetCashFlow(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3));

            // Assert
            result[0].Value.ShouldBe(130);
            result[1].Value.ShouldBe(90);
            result[2].Value.ShouldBe(40);
        }
    }
}