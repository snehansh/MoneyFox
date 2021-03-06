﻿using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using MoneyFox.Foundation.Models;

namespace MoneyFox.Presentation.ViewModels.Statistic
{
    public interface IStatisticSelectorViewModel : IBaseViewModel
    {
        List<StatisticSelectorType> StatisticItems { get; }

        RelayCommand<StatisticSelectorType> GoToStatisticCommand { get; }
    }
}
