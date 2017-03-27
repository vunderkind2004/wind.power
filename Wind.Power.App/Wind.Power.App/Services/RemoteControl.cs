using System;
using System.Threading.Tasks;
using Wind.Power.App.ViewModels;

namespace Wind.Power.App.Services
{
    public class RemoteControl
    {
        private Communication communication;

        public delegate void StateUpdatedHandler(object sender, StateViewModel state);

        public event StateUpdatedHandler OnStateUpdated;

        public RemoteControl()
        {
            communication = new Communication(Configuration.BaseUrl);
        }

        public async Task Connect()
        {
            var state = await communication.GetMainSwitchState();
            RaiseOnStateUpdated(state);
        }

        public async Task TurnOn()
        {
            var state = await communication.SendMainSwitchCommand(isTurnOn: true);
            RaiseOnStateUpdated(state);
        }

        public async Task TurnOff()
        {
            var state = await communication.SendMainSwitchCommand(isTurnOn: false);
            RaiseOnStateUpdated(state);
        }

        public async Task ChangePwmFrequency(int newValue)
        {
            var state = await communication.ChangePwmFrequency(newValue);
            RaiseOnStateUpdated(state);
        }

        public async Task ChangePwmDuty(int newValue)
        {
            var state = await communication.ChangePwmDuty(newValue);
            RaiseOnStateUpdated(state);
        }

        protected virtual void RaiseOnStateUpdated(StateViewModel state)
        {
            var temp = OnStateUpdated;
            if (temp != null)
            {
                temp(this, state);
            }
        }

        
    }
}
