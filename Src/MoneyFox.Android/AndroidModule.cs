﻿using Android.App;
using Autofac;
using MoneyFox.BusinessLogic.FileStore;
using MoneyFox.Droid.Manager;
using MoneyFox.Droid.Services;
using MoneyFox.Presentation;

namespace MoneyFox.Droid
{
    public class AndroidModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<PresentationModule>();

            builder.RegisterType<DialogService>().AsImplementedInterfaces();
            builder.RegisterType<DroidAppInformation>().AsImplementedInterfaces();
            builder.RegisterType<PlayStoreOperations>().AsImplementedInterfaces();
            builder.RegisterType<BackgroundTaskManager>().AsImplementedInterfaces();
            builder.RegisterType<NavigationService>().AsImplementedInterfaces();
            builder.RegisterType<ThemeSelectorAdapter>().AsImplementedInterfaces();
            builder.Register(c => new FileStoreIoBase(Application.Context.FilesDir.Path)).AsImplementedInterfaces();
        }
    }
}
