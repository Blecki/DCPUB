using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC
{
    public static class HelperExtensions
    {
        public static void Upsert<A, B>(this Dictionary<A, B> Dict, A _a, B _b)
        {
            if (Dict.ContainsKey(_a)) Dict[_a] = _b;
            else Dict.Add(_a, _b);
        }
    }
}
