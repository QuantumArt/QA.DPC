using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.DPC.UI
{
    public class PathSegment
    {
        public readonly string Name;
        public readonly PathType PathType;
        public readonly bool Latest;

        public PathSegment(string x, PathType pathType, bool latest)
        {
            // TODO: Complete member initialization
            this.Name = x;
            this.PathType = pathType;
            this.Latest = latest;
        }
    }
}
