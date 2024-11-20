using LibationUiBase;
using LibationUiBase.ViewModels.Player;

namespace LibationAvalonia.ViewModels
{
    public class SidebarViewModel
    {
        public PlayerViewModel Player { get; } = ServiceLocator.Get<PlayerViewModel>();
        public ProcessQueueViewModel ProcessQueue { get; } = ServiceLocator.Get<ProcessQueueViewModel>();
    }
}
