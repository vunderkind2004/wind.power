using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wind.Power.App.ViewModels;

namespace Wind.Power.App.Services
{
    public class Communication
    {
        private HttpTransport transport;

        private string mainSwitchUrl = "/mainSwitch";
        private string pwmUrl = "/pwm";

        public Communication(string baseUrl)
        {
            transport = new HttpTransport(baseUrl);
        }

        public async Task<StateViewModel> SendMainSwitchCommand(bool isTurnOn)
        {
            var parameters = new Dictionary<string, object> { { "turnOn", isTurnOn.ToString().ToLower() } };
            var result = await transport.Get<StateViewModel>(mainSwitchUrl, parameters);
            return result;
        }


        public async Task<StateViewModel> GetMainSwitchState()
        {
            var result = await transport.Get<StateViewModel>(mainSwitchUrl);
            return result;
        }

        public async Task<StateViewModel> ChangePwmFrequency(int newValue)
        {
            var parameters = new Dictionary<string, object> { { "frequency", newValue } };
            var result = await transport.Get<StateViewModel>(pwmUrl, parameters);
            return result;
        }

        public async Task<StateViewModel> ChangePwmDuty(int newValue)
        {
            var parameters = new Dictionary<string, object> { { "duty", newValue } };
            var result = await transport.Get<StateViewModel>(pwmUrl, parameters);
            return result;
        }
    }
}
