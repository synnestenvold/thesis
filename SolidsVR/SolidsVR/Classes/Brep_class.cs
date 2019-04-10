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
        Brep brp = new Brep();
        private List<Brep> surfaces = new List<Brep>();

        public Brep_class(Brep _brp) {
            brp = _brp;
            
        }

        public Brep_class() { }

        public void SetBrep(Brep _brp)
        {
            brp = _brp;
        }
        public Brep GetBrep()
        {
            return brp;
        }
        
        public double GetVolume()
        {
            return brp.GetVolume();
        }
        public void SetSurfaces(List<Brep> _surf)
        {
            surfaces = _surf;
        }
        public List<Brep> GetSurfaces()
        {
            return surfaces;
        }

        public double GetRefLength()
        {
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(brp.GetVolume(), sqrt3);
            return refLength;
        }


        public Brep GetSurfaceAsBrep(int surfNo)
        {
            return surfaces[surfNo];
        }
    }
}
