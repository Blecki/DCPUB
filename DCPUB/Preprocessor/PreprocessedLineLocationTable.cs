using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCPUB.Preprocessor
{
    public class PreprocessedLineLocationTable
    {
        public class LocationEntry
        {
            public String FileName;
            public int StartLine;       // The line in the final file where this file starts.
            public int OffsetLine;      // The offset into the original file for this chunk.
        }

        public List<LocationEntry> Locations = new List<LocationEntry>();

        public LocationEntry FindLocation(int line)
        {
            foreach (var entry in Locations)
                if (entry.StartLine < line) return entry;
            return null; // Not technically possible in a properly constructed file.
        }

        public Tuple<String, int> FindRealLocation(int line)
        {
            var loc = FindLocation(line);
            if (loc == null) return Tuple.Create("Unknown", 0);
            return Tuple.Create(loc.FileName, (line - loc.StartLine) + loc.OffsetLine);
        }

        public void AddLocation(String FileName, int StartLine, int OffsetLine)
        {
            Locations.Insert(0, new LocationEntry { FileName = FileName, StartLine = StartLine, OffsetLine = OffsetLine });
        }

        public void Merge(PreprocessedLineLocationTable Other, int Offset)
        {
            var localCopy = new List<LocationEntry>(Other.Locations);
            localCopy.Reverse();
            foreach (var entry in localCopy)
            {
                entry.StartLine += Offset;
                Locations.Insert(0, entry);
            }
        }
    }
}
