using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Keysight.Tap;

namespace Tap.Plugins._5Genesis.Misc.Steps
{
    public abstract class MeasurementStepBase : TestStep
    {
        public enum WaitMode { Time, Children }

        [EnabledIf("HideMeasurement", false, HideIfDisabled = true)]
        [Display("Wait Mode", Group: "Measurement", Order: 50.0)]
        public WaitMode MeasurementMode { get; set; }

        [EnabledIf("HideMeasurement", false, HideIfDisabled = true)]
        [EnabledIf("MeasurementMode", WaitMode.Time, HideIfDisabled = true)]
        [Display("Time", Group: "Measurement", Order: 50.1)]
        [Unit("s")]
        public double MeasurementTime { get; set; }

        public abstract bool HideMeasurement { get; }

        public void MeasurementWait()
        {
            switch (MeasurementMode)
            {
                case WaitMode.Time:
                    TestPlan.Sleep((int)(MeasurementTime * 1000));
                    break;
                default:
                    RunChildSteps();
                    break;
            }
        }
    }
}
