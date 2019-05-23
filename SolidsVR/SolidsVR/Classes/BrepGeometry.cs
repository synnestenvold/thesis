using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidsVR
{
    public class BrepGeometry
    {
        Brep brp = new Brep();
        private List<Brep> surfaces = new List<Brep>();
        Point3d centroid = new Point3d(0, 0, 0);

        public BrepGeometry(Brep _brp) {
            brp = _brp;
            VolumeMassProperties vmp = VolumeMassProperties.Compute(brp);
            centroid = vmp.Centroid;

        }

        public BrepGeometry() { }

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
        public Point3d GetCentroid()
        {
            return centroid;
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
    }
}
