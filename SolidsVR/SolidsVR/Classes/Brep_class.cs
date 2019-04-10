using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidsVR
{
    public class Brep_class
    {
        private Brep brp = new Brep();
        private List<Surface> surfaces = new List<Surface>();

        public Brep_class(Brep _brp) {
            brp = _brp;
        }

        public Brep_class() { }

        public Brep GetBrep()
        {
            return brp;
        }
        public void SetBrep(Brep _brp)
        {
            brp = _brp;
        }

        public Brep GetSurfaceAsBrep(int s)
        {
            //hent ut faces??
            return surfaces[s].ToBrep();
        }
    }
}
