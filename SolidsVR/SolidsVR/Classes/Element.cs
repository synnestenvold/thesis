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
        List<Node> vertices = new List<Node>();
        int elementNr = 0;

        public Element (List<Node> _vertices, int _elementNr)
        {
            vertices = _vertices;
            elementNr = _elementNr;
        }

        public List<Node> GetVertices()
        {
            return vertices;
        }

        public double GetElementNr()
        {
            return elementNr;
        }

    }
}
