﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUC.Assembly
{
    public class Annotation : Node
    {
        public String comment;

        public Annotation(String annotation) { this.comment = annotation; }

        public override void Emit(EmissionStream stream)
        {
            var commentLines = comment.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in commentLines)
                stream.WriteLine("; " + line);
        }
    }
}