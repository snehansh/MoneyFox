﻿using System.Threading.Tasks;
using GenericServices;
using MoneyFox.Presentation.Services;

namespace MoneyFox.Presentation.ViewModels
{
    /// <summary>
    ///     This ViewModel is for the usage in the payment list when a concrete account is selected
    /// </summary>
    public class PaymentListBalanceViewModel : BalanceViewModel
    {
        private readonly int accountId;
        private readonly IBalanceCalculationService balanceCalculationService;
        private readonly ICrudServicesAsync crudServices;

        /// <summary>
        ///     Constructor
        /// </summary>
        public PaymentListBalanceViewModel(ICrudServicesAsync crudServices,
                                           IBalanceCalculationService balanceCalculationService,
                                           int accountId) : base(balanceCalculationService)
        {
            this.crudServices = crudServices;
            this.balanceCalculationService = balanceCalculationService;
            this.accountId = accountId;
        }

        /// <summary>
        ///     Calculates the sum of all accounts at the current moment.
        /// </summary>
        /// <returns>Sum of the balance of all accounts.</returns>
        protected override async Task<double> CalculateTotalBalance()
        {
            AccountViewModel account = await crudServices.ReadSingleAsync<AccountViewModel>(accountId);
            return account.CurrentBalance;
        }

        /// <summary>
        ///     Calculates the sum of the selected account at the end of the month.
        ///     This includes all payments coming until the end of month.
        /// </summary>
        /// <returns>Balance of the selected account including all payments to come till end of month.</returns>
        protected override async Task<double> GetEndOfMonthValue()
        {
            AccountViewModel account = await crudServices.ReadSingleAsync<AccountViewModel>(accountId)
                ;
            return await balanceCalculationService.GetEndOfMonthBalanceForAccount(account);
        }
    }
}
