using Android.Runtime;
using MoneyManager.Core.ViewModels;
using MvvmCross.Droid.Support.V7.Fragging.Attributes;

namespace MoneyManager.Droid.Fragments
{
    [MvxFragment(typeof(MainViewModel), Resource.Id.content_frame)]
    [Register("moneymanager.droid.fragments.StatisticCategorySummaryFragment")]
    public class StatisticCategorySummaryFragment : BaseFragment<StatisticCategorySummaryViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_category_summary;
    }
}