using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Intermediate
{
    public class Label
    {
        private static int labelCount = 0;

        public Box<ushort> position = new Box<ushort> { data = 0 };
        public string rawLabel;

        public Label()
        {
            rawLabel = "L" + labelCount;
            ++labelCount;
        }

        public static Label Make(String suffix)
        {
            var l = new Label();
            l.rawLabel += suffix;
            return l;
        }

        public Label(String rl) { rawLabel = rl; }

        public override string ToString()
        {
            return rawLabel;
        }

    }
}
