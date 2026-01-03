using CommunityToolkit.Mvvm.Messaging.Messages;

namespace OCC.Client.ViewModels.Messages
{
    public class NavigationMessage : ValueChangedMessage<ViewModelBase>
    {
        public NavigationMessage(ViewModelBase value) : base(value)
        {
        }
    }
}
