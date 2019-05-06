// Author:      Bruno Garcia Garcia <bgarcia@lcc.uma.es>
// Copyright:   Copyright 2019-2020 Universidad de Málaga (University of Málaga), Spain
//
// This file is part of the 5GENESIS project. The 5GENESIS project is funded by the European Union’s
// Horizon 2020 research and innovation programme, grant agreement No 815178.
//
// This file cannot be modified or redistributed. This header cannot be removed.

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Tap.Plugins._5Genesis.Misc.Extensions
{
    public static class StringExtension
    {
        public static SecureString ToSecureString(this string value)
        {
            SecureString res = new SecureString();

            foreach (char c in value)
            {
                res.AppendChar(c);
            }

            return res;
        }
    }
}
