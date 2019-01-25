﻿using System;
using MoneyFox.ServiceLayer.ViewModels;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms.Xaml;

namespace MoneyFox.Presentation.Dialogs
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DateSelectionDialog
	{
		public DateSelectionDialog ()
		{
			InitializeComponent ();
		}

	    private async void Button_OnClicked(object sender, EventArgs e)
	    {
	        await Navigation.PopPopupAsync();
            (BindingContext as SelectDateRangeDialogViewModel)?.DoneCommand.Execute();
        }
	}
}