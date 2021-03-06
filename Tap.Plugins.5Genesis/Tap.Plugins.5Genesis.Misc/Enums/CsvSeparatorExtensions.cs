﻿// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

namespace Tap.Plugins._5Genesis.Misc.Enums
{
    public static class CsvSeparatorExtensions
    {
        public static string AsString(this CsvSeparator separator)
        {
            switch (separator)
            {
                case CsvSeparator.Comma: return ",";
                case CsvSeparator.SemiColon: return ";";
                default: return "\t";
            }
        }

        public static string DefaultReplacement(this CsvSeparator separator)
        {
            return separator == CsvSeparator.SemiColon ? "," : ";";
        }
    }
}
