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

        //Point3d[] vertices;
        
        public Brep_class(Brep _brp) {
            brp = _brp;
            //Point3d[] vertices = brp.DuplicateVertices();
        }

        public Brep_class() { }

        public Brep GetBrep()
        {
            return brp;
        }
        public void SetSurfaces(List<Brep> _surf)
        {
            surfaces = _surf;
        }
        public List<Brep> GetSurfaces()
        {
            return surfaces;
        }
        public void SetBrep(Brep _brp)
        {
            brp = _brp;
        }

        public Brep GetSurfaceAsBrep(int surfNo)
        {
            return surfaces[surfNo];
        }
    }
}
