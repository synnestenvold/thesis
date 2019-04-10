using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace SolidsVR
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
            //pManager.AddSurfaceParameter("Surface", "Surface", "Surface for BC", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Surface number", "Surface number", "Surface number for BC (0-5)", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("U count", "U", "Number of divisions in U direction", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("V count", "V", "Number of divisions in V direction", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("W count", "W", "Number of divisions in W direction", GH_ParamAccess.item);
            //pManager.AddBrepParameter("Brep", "B", "Brep as a reference size", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh class", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("BC points", "BC", "BC in point, (x,y,z);(Fx,Fy,Fz)", GH_ParamAccess.list);
            pManager.AddBrepParameter("BC cones", "Geometry", "Cones showing the boundary conditions", GH_ParamAccess.list);
            pManager.AddColourParameter("Coloring for BC-cones", "Color", "Coloring of cones", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---

            //Surface surface = null;
            int surfNo = 0;
            string restrains = "0,0,0";
            int u = 1;
            int v = 1;
            int w = 1;
            //Brep origBrep = new Brep();
            Brep_class brp = new Brep_class();
            Mesh_class mesh = new Mesh_class();

            //---input---

            if (!DA.GetData(0, ref surfNo)) return;
            if (!DA.GetData(1, ref mesh)) return;
            //if (!DA.GetData(1, ref u)) return;
            //if (!DA.GetData(2, ref v)) return;
            //if (!DA.GetData(3, ref w)) return;
            //if (!DA.GetData(4, ref origBrep)) return;

            u = mesh.getU();
            v = mesh.getV();
            w = mesh.getW();
            brp = mesh.GetBrep();
            List<Node> nodes = mesh.GetNodeList();

            //---solve---

            //List<string> pointsBCold = FindBCPointsOld(surface, restrains, u, v, w, origBrep);
            List<string> pointsBC = FindBCPoints(surfNo, restrains, nodes);

            ///////FOR PREVIEWING OF BC///////

           
            double refLength = brp.GetRefLength();

            List<Brep> cones = DrawBC(pointsBC, refLength);
            Color color = Color.Green;

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
            //List<string> pointsString = new List<string>();
            List<string> pointsBC = new List<string>();
            for (int i = 0; i<nodes.Count; i++)
            {
                if (nodes[i].GetSurfaceNum().Contains(surfNo))
                {
                    Point3d node = nodes[i].GetCoord();
                    string pointString = node.X.ToString() + "," + node.Y.ToString() + "," + node.Z.ToString();
                    pointsBC.Add(pointString + ";" + restrains);
                }
            }
            return pointsBC;
        }

        public List<string> FindBCPointsOld (Surface surface, string restrains, int u, int v, int w, Brep brp)
        {
            List<Point3d> points = new List<Point3d>();
            List<string> pointsString = new List<string>();
            Brep surfaceBrep = surface.ToBrep();
            Point3d[] vertices = surfaceBrep.DuplicateVertices();
            Point3d[] nodesAll = brp.DuplicateVertices();
            
            

            //Finding all points 
            points.Add(vertices[0]);
            points.Add(vertices[1]);
            points.Add(vertices[2]);
            points.Add(vertices[3]);
            
            int relativeU = u;
            int relativeV = v;
            
            int[] nodeIndex = new int[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                for (int j = 0; j < nodesAll.Length; j++)
                {
                    if (vertices[i] == nodesAll[j])
                    {
                        nodeIndex[i] = j;
                    }
                }
            }
            Array.Sort(nodeIndex);

            Vector3d vec_u1 = (nodesAll[1] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[1]);
            Vector3d vec_u2 = (nodesAll[2] - nodesAll[3]) / nodesAll[3].DistanceTo(nodesAll[2]);
            Vector3d vec_v1 = (nodesAll[3] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[3]);
            Vector3d vec_v2 = (nodesAll[2] - nodesAll[1]) / nodesAll[1].DistanceTo(nodesAll[2]);

            double l_u1 = nodesAll[0].DistanceTo(nodesAll[1]) / relativeU; 
            double l_u2 = nodesAll[3].DistanceTo(nodesAll[2]) / relativeU;
            double l_v1 = nodesAll[0].DistanceTo(nodesAll[3]) / relativeV;
            double l_v2 = nodesAll[1].DistanceTo(nodesAll[2]) / relativeV;

            vertices[0] = nodesAll[0];
            vertices[1] = nodesAll[3];
            vertices[2] = nodesAll[0];
            vertices[3] = nodesAll[1];

            if (nodeIndex[0] == 0)
            {
                if (nodeIndex[1] == 1)
                {
                    if (nodeIndex[2] == 4)
                    {
                        relativeV = w;
                        vec_u2 = (nodesAll[5] - nodesAll[4]) / nodesAll[4].DistanceTo(nodesAll[5]);
                        vec_v1 = (nodesAll[4] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[4]);
                        vec_v2 = (nodesAll[5] - nodesAll[1]) / nodesAll[1].DistanceTo(nodesAll[5]);
                        
                        l_u2 = nodesAll[4].DistanceTo(nodesAll[5]) / relativeU;
                        l_v1 = nodesAll[0].DistanceTo(nodesAll[4]) / relativeV;
                        l_v2 = nodesAll[1].DistanceTo(nodesAll[5]) / relativeV;

                        vertices[1] = nodesAll[4];
                    }
                }
                else if (nodeIndex[1] == 3)
                {
                    {
                        relativeU = v;
                        relativeV = w;
                        vec_u1 = (nodesAll[3] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[3]);
                        vec_u2 = (nodesAll[7] - nodesAll[4]) / nodesAll[4].DistanceTo(nodesAll[7]);
                        vec_v1 = (nodesAll[4] - nodesAll[0]) / nodesAll[0].DistanceTo(nodesAll[4]);
                        vec_v2 = (nodesAll[7] - nodesAll[3]) / nodesAll[3].DistanceTo(nodesAll[7]);
                        l_u1 = nodesAll[0].DistanceTo(nodesAll[3]) / relativeU;
                        l_u2 = nodesAll[4].DistanceTo(nodesAll[7]) / relativeU;
                        l_v1 = nodesAll[0].DistanceTo(nodesAll[4]) / relativeV;
                        l_v2 = nodesAll[3].DistanceTo(nodesAll[7]) / relativeV;
                        vertices[1] = nodesAll[4];
                        vertices[3] = nodesAll[3];
                    }
                }
            }
            else if (nodeIndex[0] == 1)
            {
                if (nodeIndex[1] == 2)
                {
                    relativeU = v;
                    relativeV = w;
                    vec_u1 = (nodesAll[2] - nodesAll[1]) / nodesAll[1].DistanceTo(nodesAll[2]);
                    vec_u2 = (nodesAll[6] - nodesAll[5]) / nodesAll[5].DistanceTo(nodesAll[6]);
                    vec_v1 = (nodesAll[5] - nodesAll[1]) / nodesAll[1].DistanceTo(nodesAll[5]);
                    vec_v2 = (nodesAll[6] - nodesAll[2]) / nodesAll[2].DistanceTo(nodesAll[6]);
                    l_u1 = nodesAll[1].DistanceTo(nodesAll[2]) / relativeU;
                    l_u2 = nodesAll[5].DistanceTo(nodesAll[6]) / relativeU;
                    l_v1 = nodesAll[1].DistanceTo(nodesAll[5]) / relativeV;
                    l_v2 = nodesAll[2].DistanceTo(nodesAll[6]) / relativeV;
                    vertices[0] = nodesAll[1];
                    vertices[1] = nodesAll[5];
                    vertices[2] = nodesAll[1];
                    vertices[3] = nodesAll[2];
                }
            }
            else if (nodeIndex[0] == 2)
            {
                relativeV = w;
                vec_u1 = (nodesAll[2] - nodesAll[3]) / nodesAll[3].DistanceTo(nodesAll[2]);
                vec_u2 = (nodesAll[6] - nodesAll[7]) / nodesAll[7].DistanceTo(nodesAll[6]);
                vec_v1 = (nodesAll[7] - nodesAll[3]) / nodesAll[3].DistanceTo(nodesAll[7]);
                vec_v2 = (nodesAll[6] - nodesAll[2]) / nodesAll[2].DistanceTo(nodesAll[6]);
                l_u1 = nodesAll[3].DistanceTo(nodesAll[2]) / relativeU;
                l_u2 = nodesAll[7].DistanceTo(nodesAll[6]) / relativeU;
                l_v1 = nodesAll[3].DistanceTo(nodesAll[7]) / relativeV;
                l_v2 = nodesAll[2].DistanceTo(nodesAll[6]) / relativeV;
                vertices[0] = nodesAll[3];
                vertices[1] = nodesAll[7];
                vertices[2] = nodesAll[3];
                vertices[3] = nodesAll[2];
            }
            else
            {
                vec_u1 = (nodesAll[5] - nodesAll[4]) / nodesAll[4].DistanceTo(nodesAll[5]);
                vec_u2 = (nodesAll[6] - nodesAll[7]) / nodesAll[7].DistanceTo(nodesAll[6]);
                vec_v1 = (nodesAll[7] - nodesAll[4]) / nodesAll[4].DistanceTo(nodesAll[7]);
                vec_v2 = (nodesAll[6] - nodesAll[5]) / nodesAll[5].DistanceTo(nodesAll[6]);
                l_u1 = nodesAll[4].DistanceTo(nodesAll[5]) / relativeU;
                l_u2 = nodesAll[7].DistanceTo(nodesAll[6]) / relativeU;
                l_v1 = nodesAll[4].DistanceTo(nodesAll[7]) / relativeV;
                l_v2 = nodesAll[5].DistanceTo(nodesAll[6]) / relativeV;
                vertices[0] = nodesAll[4];
                vertices[1] = nodesAll[7];
                vertices[2] = nodesAll[4];
                vertices[3] = nodesAll[5];
            }
          
            
            for (int i = 1; i < relativeU; i++)
            {
                Point3d p1_u = new Point3d(vertices[0].X + l_u1 * i * vec_u1.X, vertices[0].Y + l_u1 * vec_u1.Y * i, vertices[0].Z + l_u1 * vec_u1.Z * i);
                Point3d p2_u = new Point3d(vertices[1].X + l_u2 * i * vec_u2.X, vertices[1].Y + l_u2 * vec_u2.Y * i, vertices[1].Z + l_u2 * vec_u2.Z * i);
                points.Add(p1_u);
                points.Add(p2_u);
                Vector3d vec_u = (p2_u - p1_u) / (p1_u.DistanceTo(p2_u));

                double length_v1 = p1_u.DistanceTo(p2_u) / relativeV;

                for (int j = 1; j < relativeV; j++)
                {
                    Point3d p1_v = new Point3d(p1_u.X + length_v1 * j * vec_u.X, p1_u.Y + length_v1 * j * vec_u.Y, p1_u.Z + length_v1 * j * vec_u.Z);
                    points.Add(p1_v);
                }
            }

            for (int i = 1; i < relativeV; i++)
            {
                Point3d p1 = new Point3d(vertices[2].X + l_v1 * i * vec_v1.X, vertices[2].Y + l_v1 * vec_v1.Y * i, vertices[2].Z + l_v1 * vec_v1.Z * i);
                points.Add(p1);
                Point3d p2 = new Point3d(vertices[3].X + l_v2 * i * vec_v2.X, vertices[3].Y + l_v2 * vec_v2.Y * i, vertices[3].Z + l_v2 * vec_v2.Z * i);
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

        public Point3d[] SortNodes(Point3d[] nodes, Point3d centroid)
        {

            Point3d[] lowerNodes = new Point3d[4];
            Point3d[] upperNodes = new Point3d[4];

            double[] lowerAngles = new double[nodes.Length / 2];
            double[] upperAngles = new double[nodes.Length / 2];

            //Dividing in lower and upper nodes.
            for (int i = 0; i < nodes.Length / 2; i++)
            {
                lowerNodes[i] = nodes[i];
                upperNodes[i] = nodes[nodes.Length / 2 + i];
            }


            for (int i = 0; i < nodes.Length / 2; i++)
            {
                lowerAngles[i] = (180 / Math.PI) * Math.Atan2(lowerNodes[i].Y - centroid.Y, lowerNodes[i].X - centroid.X);
            }

            Array.Sort(lowerAngles, lowerNodes);



            for (int i = 0; i < nodes.Length / 2; i++)
            {
                upperAngles[i] = (180 / Math.PI) * Math.Atan2(upperNodes[i].Y - centroid.Y, upperNodes[i].X - centroid.X);
            }

            Array.Sort(upperAngles, upperNodes);

            Point3d[] nodesAll = new Point3d[8];

            nodesAll[0] = lowerNodes[0];
            nodesAll[1] = lowerNodes[1];
            nodesAll[2] = lowerNodes[2];
            nodesAll[3] = lowerNodes[3];
            nodesAll[4] = upperNodes[0];
            nodesAll[5] = upperNodes[1];
            nodesAll[6] = upperNodes[2];
            nodesAll[7] = upperNodes[3];


            return nodesAll;
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
