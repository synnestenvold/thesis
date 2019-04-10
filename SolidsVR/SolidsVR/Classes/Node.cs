using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;

namespace SolidsVR.Classes
{
    class Node
    {
        Point3d coordinate;

        public Node (Point3d _coordinate)
        {
           coordinate = _coordinate;
        }

        public Point3d Coordinate { get; set; }

        public void SetCoord(Point3d _coord)
        {
            coordinate = _coord;

        }

        public Point3d GetCoord()
        {
            return coordinate;
        }

    }
}
