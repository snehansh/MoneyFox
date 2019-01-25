﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GenericServices;
using MoneyFox.Business.Manager;
using MoneyFox.Business.ViewModels;
using MoneyFox.Foundation.Groups;
using MoneyFox.Foundation.Interfaces;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Messages;
using MoneyFox.ServiceLayer.Parameters;
using MoneyFox.ServiceLayer.ViewModels.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;

namespace MoneyFox.ServiceLayer.ViewModels
{
    /// <summary>
    ///     Representation of the payment list view.
    /// </summary>
    public class PaymentListViewModel : MvxViewModel<PaymentListParameter>, IPaymentListViewModel
    {
        private readonly ICrudServicesAsync crudServices;
        private readonly IDialogService dialogService;
        private readonly ISettingsManager settingsManager;
        private readonly IBalanceCalculationService balanceCalculationService;
        private readonly IBackupManager backupManager;
        private readonly IMvxNavigationService navigationService;
        private readonly IMvxMessenger messenger;
        private readonly IMvxLogProvider logProvider;

        private ObservableCollection<DateListGroup<DateListGroup<PaymentViewModel>>> source;
        private ObservableCollection<DateListGroup<PaymentViewModel>> dailyList;
        private IBalanceViewModel balanceViewModel;
        private int accountId;
        private string title;
        private IPaymentListViewActionViewModel viewActionViewModel;

        //this token ensures that we will be notified when a message is sent.
        private readonly MvxSubscriptionToken token;

        /// <summary>
        ///     Default constructor
        /// </summary>
        public PaymentListViewModel(ICrudServicesAsync crudServices,
            IDialogService dialogService,
            ISettingsManager settingsManager,
            IBalanceCalculationService balanceCalculationService,
            IBackupManager backupManager,
            IMvxNavigationService navigationService,
            IMvxMessenger messenger, 
            IMvxLogProvider logProvider)
        {
            this.crudServices = crudServices;
            this.dialogService = dialogService;
            this.settingsManager = settingsManager;
            this.balanceCalculationService = balanceCalculationService;
            this.backupManager = backupManager;
            this.navigationService = navigationService;
            this.messenger = messenger;
            this.logProvider = logProvider;

            token = messenger.Subscribe<PaymentListFilterChangedMessage>(async message => await LoadPayments(message));
        }

        #region Properties

        /// <summary>
        ///     Indicator if there are payments or not.
        /// </summary>
        public bool IsPaymentsEmtpy => Source != null && !Source.Any();

        /// <summary>
        ///     Id for the current account.
        /// </summary>
        public int AccountId
        {
            get => accountId;
            private set
            {
                accountId = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///      View Model for the balance subview.
        /// </summary>
        public IBalanceViewModel BalanceViewModel
        {
            get => balanceViewModel;
            private set
            {
                balanceViewModel = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     View Model for the global actions on the view.
        /// </summary>
        public IPaymentListViewActionViewModel ViewActionViewModel
        {
            get => viewActionViewModel;
            private set {
                if (viewActionViewModel == value) return;
                viewActionViewModel = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        ///     Returns groupped related payments
        /// </summary>
        public ObservableCollection<DateListGroup<DateListGroup<PaymentViewModel>>> Source
        {
            get => source;
            set
            {
                source = value;
                RaisePropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(IsPaymentsEmtpy));
            }
        }

        /// <summary>
        ///     Returns daily groupped related payments
        /// </summary>
        public ObservableCollection<DateListGroup<PaymentViewModel>> DailyList
        {
            get => dailyList;
            set
            {
                dailyList = value;
                RaisePropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(IsPaymentsEmtpy));
            }
        }

        /// <summary>
        ///     Returns the name of the account title for the current page
        /// </summary>
        public string Title
        {
            get => title;
            private set
            {
                if (title == value) return;
                title = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        ///     Opens the Edit Dialog for the passed Payment
        /// </summary>
        public MvxAsyncCommand<PaymentViewModel> EditPaymentCommand => new MvxAsyncCommand<PaymentViewModel>(EditPayment);

        /// <summary>
        ///     Deletes the passed PaymentViewModel.
        /// </summary>
        public MvxAsyncCommand<PaymentViewModel> DeletePaymentCommand => new MvxAsyncCommand<PaymentViewModel>(DeletePayment);

        #endregion

        /// <inheritdoc />
        public override void Prepare(PaymentListParameter parameter)
        {
            AccountId = parameter.AccountId;
        }

        /// <inheritdoc />
        public override async Task Initialize()
        {
            Title = await accountService.GetAccountName(AccountId);

            BalanceViewModel = new PaymentListBalanceViewModel(accountService, balanceCalculationManager, AccountId, logProvider, navigationService);
            ViewActionViewModel = new PaymentListViewActionViewModel(accountService,
                                                                     settingsManager,
                                                                     dialogService,
                                                                     BalanceViewModel,
                                                                     messenger,
                                                                     AccountId,
                                                                     logProvider,
                                                                     navigationService);
        }

        /// <inheritdoc />
        public override async void ViewAppearing()
        {
            dialogService.ShowLoadingDialog();
            await Task.Run(async () => await Load());
            dialogService.HideLoadingDialog();
        }

        private async Task Load()
        {
            await LoadPayments(new PaymentListFilterChangedMessage(this));

            //Refresh balance control with the current account
            await BalanceViewModel.UpdateBalanceCommand.ExecuteAsync();
        }

        private async Task LoadPayments(PaymentListFilterChangedMessage filterMessage)
        {
            var paymentQuery = await paymentService.GetPaymentsByAccountId(AccountId);

            if (filterMessage.IsClearedFilterActive)
            {
                paymentQuery = paymentQuery.Where(x => x.Data.IsCleared);
            }
            if (filterMessage.IsRecurringFilterActive)
            {
                paymentQuery = paymentQuery.Where(x => x.Data.IsRecurring);
            }

            paymentQuery = paymentQuery.Where(x => x.Data.Date >= filterMessage.TimeRangeStart);
            paymentQuery = paymentQuery.Where(x => x.Data.Date <= filterMessage.TimeRangeEnd);

            var loadedPayments = new List<PaymentViewModel>(
                paymentQuery
                    .OrderByDescending(x => x.Data.Date)
                    .Select(x => new PaymentViewModel(x)));


            foreach (PaymentViewModel payment in loadedPayments)
            {
                payment.CurrentAccountId = AccountId;
            }

            List<DateListGroup<PaymentViewModel>> dailyItems = DateListGroup<PaymentViewModel>
                .CreateGroups(loadedPayments,
                              s => s.Date.ToString("D", CultureInfo.CurrentUICulture),
                              s => s.Date,
                              itemClickCommand: EditPaymentCommand);

            DailyList = new ObservableCollection<DateListGroup<PaymentViewModel>>(dailyItems);

            Source = new ObservableCollection<DateListGroup<DateListGroup<PaymentViewModel>>>(
                DateListGroup<DateListGroup<PaymentViewModel>>
                    .CreateGroups(dailyItems,
                                  s =>
                                  {
                                      var date = Convert.ToDateTime(s.Key);
                                      return date.ToString("MMMM", CultureInfo.CurrentUICulture) + " " + date.Year;
                                  },
                                  s => Convert.ToDateTime(s.Key)));
        }

        private async Task EditPayment(PaymentViewModel payment)
        {
            await navigationService.Navigate<ModifyPaymentViewModel, ModifyPaymentParameter>(
                new ModifyPaymentParameter(payment.Id));
        }

        private async Task DeletePayment(PaymentViewModel payment)
        {
            if (!await dialogService
                .ShowConfirmMessage(Strings.DeleteTitle, Strings.DeletePaymentConfirmationMessage)) return;

            var paymentToDelete = await paymentService.GetById(payment.Id);

            if (paymentToDelete.Data.IsRecurring 
                && await dialogService.ShowConfirmMessage(Strings.DeleteRecurringPaymentTitle, Strings.DeleteRecurringPaymentMessage))
            {
                await recurringPaymentService.DeletePayment(paymentToDelete.Data.RecurringPayment);
            }

            await paymentService.DeletePayment(paymentToDelete);

            settingsManager.LastDatabaseUpdate = DateTime.Now;
#pragma warning disable 4014
            backupManager.EnqueueBackupTask();
#pragma warning restore 4014
            await Load();
        }
    }
}