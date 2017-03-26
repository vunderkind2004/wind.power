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

        public Communication(string baseUrl)
        {
            transport = new HttpTransport(baseUrl);
        }

        public async Task<MainSwitchViewModel> SendMainSwitchCommand(bool isTurnOn)
        {
            var parameters = new Dictionary<string, object> { { "turnOn", isTurnOn.ToString().ToLower() } };
            var result = await transport.Get<MainSwitchViewModel>(mainSwitchUrl, parameters);
            return result;
        }


        public async Task<MainSwitchViewModel> GetMainSwitchState()
        {
            var result = await transport.Get<MainSwitchViewModel>(mainSwitchUrl);
            return result;
        }

    }
}
