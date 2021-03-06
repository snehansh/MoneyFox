﻿using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Microsoft.Identity.Client;
using MoneyFox.BusinessLogic.Adapters;
using MoneyFox.BusinessLogic.Backup;
using MoneyFox.Foundation.Constants;
using MoneyFox.Presentation.Facades;
using MoneyFox.Presentation.Services;
using MoneyFox.Uwp.Business;
using MoneyFox.Foundation;

namespace MoneyFox.Uwp.Tasks
{
    /// <inheritdoc />
    /// <summary>
    ///     Background task to sync the backup with OneDrive.
    /// </summary>
    public sealed class SyncBackupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            Debug.WriteLine("Sync Backup started.");
            ExecutingPlatform.Current = AppPlatform.UWP;

            var settingsFacade = new SettingsFacade(new SettingsAdapter());
            if (!settingsFacade.IsBackupAutouploadEnabled || !settingsFacade.IsLoggedInToBackupService) return;

            try
            {
                var pca = PublicClientApplicationBuilder
                    .Create(ServiceConstants.MSAL_APPLICATION_ID)
                    .WithRedirectUri($"msal{ServiceConstants.MSAL_APPLICATION_ID}://auth")
                    .Build();

                var backupManager = new BackupManager(
                    new OneDriveService(pca),
                    new WindowsFileStore(),
                    new ConnectivityAdapter());

                var backupService = new BackupService(backupManager,settingsFacade);

                var backupDate = await backupService.GetBackupDate();
                if (settingsFacade.LastDatabaseUpdate > backupDate) return;

                await backupService.RestoreBackup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.WriteLine("Sync Backup failed.");
            }
            finally
            {
                settingsFacade.LastExecutionTimeStampSyncBackup = DateTime.Now;
                Debug.WriteLine("Sync Backup finished.");
                deferral.Complete();
            }
        }
    }
}
