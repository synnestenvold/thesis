using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;


namespace SolidsVR
{
    public class SetUniBCComponent : GH_Component
    {
        public SetUniBCComponent()
          : base("SetUniBC", "UniBC",
              "Uniform BC component for FEA",
              "SolidsVR", "BC")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Surface number", "S", "Surface number for BC (0-5)", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh", "Mesh", "MeshGeometry class", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Boundary conditions", "BC", "BC in point, (x,y,z);(Rx,Ry,Rz)", GH_ParamAccess.list);
            pManager.AddBrepParameter("Geometry", "Geometry", "Cones showing the boundary conditions", GH_ParamAccess.list);
            pManager.AddColourParameter("Color", "Color", "Coloring of cones", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---
            
            int surfNo = 0;
            string restrains = "0,0,0";
            MeshGeometry mesh = new MeshGeometry();

            //---input---

            if (!DA.GetData(0, ref surfNo)) return;
            if (!DA.GetData(1, ref mesh)) return;

            BrepGeometry brp = mesh.GetBrep();
            List<Node> nodes = mesh.GetNodeList();

            //---solve---

            List<string> pointsBC = FindBCPoints(surfNo, restrains, nodes);

            ///////FOR PREVIEWING OF BC///////

           
            double refLength = brp.GetRefLength();
            List<Brep> cones = DrawBC(pointsBC, refLength);
            Color color = Color.FromArgb(0, 100, 255);

            //---output---

            DA.SetDataList(0, pointsBC);
            DA.SetDataList(1, cones);
            DA.SetData(2, color);
        }

        public List<Brep> DrawBC(List<string> pointsBC, double refLength)
        {
            List<Line> arrows = new List<Line>();

            List<double> BCPoints = new List<double>();
            List<double> restrains = new List<double>();

            double height = (double)refLength / 25;
            double radius = (double)refLength / 25;

            List<Brep> bcCones = new List<Brep>();

            foreach (string s in pointsBC)
            {
                string coordinate = (s.Split(';'))[0];
                string iBC = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iBCs = (iBC.Split(','));


                double BCPoints1 = Math.Round(double.Parse(coord[0]), 8);
                double BCPoints2 = Math.Round(double.Parse(coord[1]), 8);
                double BCPoints3 = Math.Round(double.Parse(coord[2]), 8);

                double restrain1 = Math.Round(double.Parse(iBCs[0]), 8);
                double restrain2 = Math.Round(double.Parse(iBCs[1]), 8);
                double restrain3 = Math.Round(double.Parse(iBCs[2]), 8);

                Point3d p1 = new Point3d(BCPoints1, BCPoints2, BCPoints3);
                Point3d p2 = Point3d.Add(p1, new Point3d(0, 1, 0));
                Point3d p3 = Point3d.Add(p1, new Point3d(1, 0, 0));

                Plane bcPlane = new Plane(p1, p2, p3);

                Cone cone = new Cone(bcPlane, height, radius);

                Brep brep = cone.ToBrep(true);

                bcCones.Add(brep);

            }

            return bcCones;
        }

        public List<string> FindBCPoints (int surfNo, string restrains, List<Node> nodes)
        {
            List<string> pointsBC = new List<string>();
            for (int i = 0; i<nodes.Count; i++)
            {
                if (nodes[i].GetSurfaceNum().Contains(surfNo))
                {
                    Point3d node = nodes[i].GetCoord();
                    string pointString = node.X.ToString() + "," + node.Y.ToString() + "," + node.Z.ToString();
                    pointsBC.Add(pointString + ";" + restrains);
                    nodes[i].setRemovable(false);
                }
            }
            return pointsBC;
        }
        
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return SolidsVR.Properties.Resource1.bc;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("118f0ebe-f149-4000-8807-c54372838600"); }
        }
    }
}
