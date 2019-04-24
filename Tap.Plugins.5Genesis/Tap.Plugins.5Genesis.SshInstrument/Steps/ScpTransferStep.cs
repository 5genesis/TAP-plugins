// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using Keysight.Tap;
using System;

namespace Tap.Plugins._5Genesis.SshInstrument.Steps
{
    [Display("SCP transfer", Group: "5Genesis", Description: "Send/Retrieve files and folders through SCP")]
    public class ScpTransferStep : SshBaseStep
    {
        public enum DirectionEnum { Pull, Push }

        #region Settings

        [Display("Direction", Group: "Configuration", Order: 1.0)]
        public DirectionEnum Direction { get; set; }

        [Display("Transfer Folder", Group: "Configuration", Order: 1.1)]
        public bool IsFolder { get; set; }

        #region Pull

        [Display("Source", Group: "Configuration", Order: 1.2)]
        [EnabledIf("Direction", DirectionEnum.Pull, HideIfDisabled = true)]
        public string SourceD { get; set; }

        [Display("Target", Group: "Configuration", Order: 1.3)]
        [EnabledIf("Direction", DirectionEnum.Pull, HideIfDisabled = true)]
        [EnabledIf("IsFolder", true, HideIfDisabled = true)]
        [DirectoryPath]
        public string TargetDD { get; set; }

        [Display("Target", Group: "Configuration", Order: 1.3)]
        [EnabledIf("Direction", DirectionEnum.Pull, HideIfDisabled = true)]
        [EnabledIf("IsFolder", false, HideIfDisabled = true)]
        [FilePath(behavior: FilePathAttribute.BehaviorChoice.Open)]
        public string TargetDF { get; set; }

        #endregion

        #region Push

        [Display("Source", Group: "Configuration", Order: 1.2)]
        [EnabledIf("Direction", DirectionEnum.Push, HideIfDisabled = true)]
        [EnabledIf("IsFolder", true, HideIfDisabled = true)]
        [DirectoryPath]
        public string SourceUD { get; set; }

        [Display("Source", Group: "Configuration", Order: 1.2)]
        [EnabledIf("Direction", DirectionEnum.Push, HideIfDisabled = true)]
        [EnabledIf("IsFolder", false, HideIfDisabled = true)]
        [FilePath(behavior: FilePathAttribute.BehaviorChoice.Open)]
        public string SourceUF { get; set; }

        [Display("Target", Group: "Configuration", Order: 1.3)]
        [EnabledIf("Direction", DirectionEnum.Push, HideIfDisabled = true)]
        public string TargetU { get; set; }

        #endregion

        #endregion

        public string Source { get {
            switch (Direction)
            {
                case DirectionEnum.Pull: return SourceD;
                default: return IsFolder ? SourceUD : SourceUF;
            }
        } }

        public string Target { get {
            switch (Direction)
            {
                case DirectionEnum.Pull: return IsFolder ? TargetDD : TargetDF;
                default: return TargetU;
            }
        } }

        public ScpTransferStep()
        {
            IsFolder = false;
            Direction = DirectionEnum.Pull;

            string setSource = "Please select a valid source";
            string setTarget = "Please select a valid target";

            Rules.Add(() => Direction == DirectionEnum.Pull && !string.IsNullOrWhiteSpace(SourceD), setSource, "SourceD");
            Rules.Add(() => Direction == DirectionEnum.Pull && IsFolder && !string.IsNullOrWhiteSpace(TargetDD), setTarget, "TargetDD");
            Rules.Add(() => Direction == DirectionEnum.Pull && !IsFolder && !string.IsNullOrWhiteSpace(TargetDF), setTarget, "TargetDF");

            Rules.Add(() => Direction == DirectionEnum.Push && !string.IsNullOrWhiteSpace(TargetU), setTarget, "TargetU");
            Rules.Add(() => Direction == DirectionEnum.Push && IsFolder && !string.IsNullOrWhiteSpace(SourceUD), setSource, "SourceUD");
            Rules.Add(() => Direction == DirectionEnum.Push && !IsFolder && !string.IsNullOrWhiteSpace(SourceUF), setSource, "SourceUF");
        }

        public override void Run()
        {
            try
            {
                if (Direction == DirectionEnum.Pull)
                {
                    Instrument.Pull(Source, Target, IsFolder);
                }
                else
                {
                    Instrument.Push(Source, Target, IsFolder);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Exception while transferring: {e.Message}");
            }
        }
    }
}
