﻿using System;
using System.Threading.Tasks;
using GenericServices;
using MoneyFox.Foundation.Interfaces;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Parameters;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;

namespace MoneyFox.ServiceLayer.ViewModels
{
    public class EditCategoryViewModel : ModifyCategoryViewModel
    {
        private readonly ICrudServicesAsync crudServices;
        private readonly IDialogService dialogService;
        private readonly ISettingsFacade settingsFacade;
        private readonly IBackupManager backupManager;

        public EditCategoryViewModel(ICrudServicesAsync crudServices,
            IDialogService dialogService,
            ISettingsFacade settingsFacade,
            IBackupManager backupManager,
            IMvxLogProvider logProvider,
            IMvxNavigationService navigationService)
            : base(crudServices, dialogService, settingsFacade, backupManager, logProvider, navigationService)
        {
            this.crudServices = crudServices;
            this.dialogService = dialogService;
            this.settingsFacade = settingsFacade;
            this.backupManager = backupManager;
        }

        public override string Title => string.Format(Strings.EditCategoryTitle, SelectedCategory.Name);

        public override async void Prepare(ModifyCategoryParameter parameter)
        {
            SelectedCategory = await crudServices.ReadSingleAsync<CategoryViewModel>(CategoryId);
        }

        /// <summary>
        ///     Delete the selected CategoryViewModel from the database
        /// </summary>
        public MvxAsyncCommand DeleteCommand => new MvxAsyncCommand(DeleteCategory);

        protected override async Task SaveCategory()
        {
            await crudServices.UpdateAndSaveAsync(SelectedCategory, "ctor(2)");
            if (!crudServices.IsValid)
            {
                await dialogService.ShowMessage(Strings.GeneralErrorTitle, crudServices.GetAllErrors());
            }

            await NavigationService.Close(this);
        }

        private async Task DeleteCategory()
        {
            await crudServices.DeleteAndSaveAsync<AccountViewModel>(SelectedCategory.Id);
            settingsFacade.LastExecutionTimeStampSyncBackup = DateTime.Now;
            await backupManager.EnqueueBackupTask();
        }
    }
}