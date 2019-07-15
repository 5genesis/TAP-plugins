using System.Xml.Serialization;
using Keysight.Tap;

namespace Tap.Plugins._5Genesis.Misc.ResultListeners
{
    public class ConfigurableResultListenerBase: ResultListener
    {
        [Display("Set Experiment ID", Group: "Metadata", Order: 99.0,
            Description: "Add an extra 'ExperimentId' identifier to the results. The value for\n" +
                         "this identifier must be set by the 'Set Experiment ID' step at some point\n" +
                         "before the end of the testplan run.")]
        public bool SetExperimentId { get; set; }

        [Display("Add Iteration Number", Group: "Metadata", Order: 99.1,
            Description: "Add an '_iteration_' identifier to the results. The value for\n" +
                         "this identifier will be set by the 'Mark Start of Iteration' step,\n" +
                         "the iteration value will automatically increase every time this step\n" +
                         "is run.")]
        public bool AddIteration { get; set; }

        [XmlIgnore]
        public string ExperimentId { get; set; }

        [XmlIgnore]
        public int Iteration { get; set; }
    }
}
