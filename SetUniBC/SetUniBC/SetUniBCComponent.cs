using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace SetUniBC
{
    public class SetUniBCComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SetUniBCComponent()
          : base("Uniform BC component for FEA", "UniBC",
              "Description",
              "Category3", "BC")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Surface", "S", "Surface for BC", GH_ParamAccess.item);
            pManager.AddTextParameter("Restained translations", "BC", "Restained translation in the way (0,0,0)", GH_ParamAccess.item, "0,0,0");
            pManager.AddIntegerParameter("u-divisions", "U", "U-division", GH_ParamAccess.item);
            pManager.AddIntegerParameter("v-divisions", "V", "V-division", GH_ParamAccess.item);
            pManager.AddBrepParameter("Brep", "B", "Brep as a reference size", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BC points", "BC", "BC in point, (x,y,z);(Fx,Fy,Fz)", GH_ParamAccess.list);
            pManager.AddBrepParameter("BC-cones", "Geometry", "Cones showing the boundary conditions", GH_ParamAccess.list);
            pManager.AddColourParameter("Coloring for BC-cones", "Color", "Coloring of cones", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep surface = new Brep();
            string restrains = "";
            int u = 2;
            int v = 2;
            Brep origBrep = new Brep();


            if (!DA.GetData(0, ref surface)) return;
            if (!DA.GetData(1, ref restrains)) return;
            if (!DA.GetData(2, ref u)) return;
            if (!DA.GetData(3, ref v)) return;
            if (!DA.GetData(4, ref origBrep)) return;


            List<string> pointsBC = FindBCPoints(surface, restrains, u, v);

            ///////FOR PREVIEWING OF BC///////

            //Setting up values for reflength and angle for rotation of area
            VolumeMassProperties vmp = VolumeMassProperties.Compute(origBrep);
            Point3d centroid = vmp.Centroid;
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            List<Brep> cones = DrawBC(pointsBC, refLength);

            Color color = Color.Green;

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

        public List<string> FindBCPoints (Brep surface, string restrains, int u, int v)
        {
            List<Point3d> points = new List<Point3d>();
            List<string> pointsString = new List<string>();
            Point3d[] vertices = surface.DuplicateVertices();

            //Finding all points
            points.Add(vertices[0]);
            points.Add(vertices[1]);
            points.Add(vertices[2]);
            points.Add(vertices[3]);

            double l_u1 = vertices[0].DistanceTo(vertices[1]) / u;
            double l_u2 = vertices[3].DistanceTo(vertices[2]) / u;
            double l_v1 = vertices[0].DistanceTo(vertices[3]) / v;
            double l_v2 = vertices[1].DistanceTo(vertices[2]) / v;

            Vector3d vec_u1 = (vertices[1] - vertices[0]) / vertices[0].DistanceTo(vertices[1]);
            Vector3d vec_u2 = (vertices[2] - vertices[3]) / vertices[3].DistanceTo(vertices[2]);
            Vector3d vec_v1 = (vertices[3] - vertices[0]) / vertices[0].DistanceTo(vertices[3]);
            Vector3d vec_v2 = (vertices[2] - vertices[1]) / vertices[1].DistanceTo(vertices[2]);


            for (int i = 1; i < u; i++)
            {
                Point3d p1 = new Point3d(vertices[0].X + l_u1 * i * vec_u1.X, vertices[0].Y + l_u1 * vec_u1.Y * i, vertices[0].Z + l_u1 * vec_u1.Z * i);
                points.Add(p1);
                Point3d p2 = new Point3d(vertices[3].X + l_u2 * i * vec_u2.X, vertices[3].Y + l_u2 * vec_u2.Y * i, vertices[3].Z + l_u2 * vec_u2.Z * i);
                points.Add(p2);

                Point3d p1_u = new Point3d(vertices[0].X + l_u1 * i * vec_u1.X, vertices[0].Y + l_u1 * vec_u1.Y * i, vertices[0].Z + l_u1 * vec_u1.Z * i);
                Point3d p2_u = new Point3d(vertices[3].X + l_u2 * i * vec_u2.X, vertices[3].Y + l_u2 * vec_u2.Y * i, vertices[3].Z + l_u2 * vec_u2.Z * i);

                Vector3d vec_u = (p2_u - p1_u) / (p1_u.DistanceTo(p2_u));

                Double length_u1 = p1_u.DistanceTo(p2_u) / u;

                for (int j = 1; j < v; j++)
                {
                    Point3d p1_v = new Point3d(p1_u.X + length_u1 * j * vec_u.X, p1_u.Y + length_u1 * j * vec_u.Y, p1_u.Z + length_u1 * j * vec_u.Z);
                    points.Add(p1_v);
                }
            }

            for (int i = 1; i < v; i++)
            {
                Point3d p1 = new Point3d(vertices[0].X + l_v1 * i * vec_v1.X, vertices[0].Y + l_v1 * vec_v1.Y * i, vertices[0].Z + l_v1 * vec_v1.Z * i);
                points.Add(p1);
                Point3d p2 = new Point3d(vertices[1].X + l_v2 * i * vec_v2.X, vertices[1].Y + l_v2 * vec_v2.Y * i, vertices[1].Z + l_v2 * vec_v2.Z * i);
                points.Add(p2);
            }

            string pointString;

            foreach (Point3d p in points)
            {
                pointString = p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();
                pointsString.Add(pointString);
            }

            List<string> pointsBC = new List<string>();

            foreach (string s in pointsString)
            {
                pointsBC.Add(s + ";" + restrains);
            }

            return pointsBC;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("118f0ebe-f149-4000-8807-c54372838600"); }
        }
    }
}
