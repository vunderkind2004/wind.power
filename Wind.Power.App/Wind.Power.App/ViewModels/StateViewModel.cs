using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wind.Power.App.ViewModels
{
    public class StateViewModel
    {
        public bool IsTurnOn { get; set; }
        public int PwmFrequency { get; set; }
        public int PwmDuty { get; set; }
    }
}
