using System.Threading.Tasks;
using Wind.Power.App.ViewModels;

namespace Wind.Power.App.Services
{
    public class RemoteControl
    {
        private Communication communication;

        public delegate void MainSwitchStateUpdatedHandler(object sender, MainSwitchViewModel state);

        public event MainSwitchStateUpdatedHandler OnMainSwitchStateUpdated;

        public RemoteControl()
        {
            communication = new Communication(Configuration.BaseUrl);
        }

        public async Task Connect()
        {
            var state = await communication.GetMainSwitchState();
            RaiseOnMainSwitchStateUpdated(state);
        }

        public async Task TurnOn()
        {
            var state = await communication.SendMainSwitchCommand(isTurnOn: true);
            RaiseOnMainSwitchStateUpdated(state);
        }

        public async Task TurnOff()
        {
            var state = await communication.SendMainSwitchCommand(isTurnOn: false);
            RaiseOnMainSwitchStateUpdated(state);
        }

        protected virtual void RaiseOnMainSwitchStateUpdated(MainSwitchViewModel state)
        {
            var temp = OnMainSwitchStateUpdated;
            if (temp != null)
            {
                temp(this, state);
            }
        }
    }
}
