using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;

namespace SolidsVR.Classes
{
    class Element
    {
        List<Point3d> vertices = new List<Point3d>();

        public Element (List<Point3d> _vertices)
        {
            vertices = _vertices;
        }
    }
}
