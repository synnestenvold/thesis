using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace MeshTBrep
{
    public class MeshTBrepComponent : GH_Component
    {
        //Test
        public MeshTBrepComponent()
          : base("MeshTBrep", "MeshTBrep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("TBrep", "B", "Inupt twisted Brep as a cube", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U Count", "U", "Number of quads in U direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("V Count", "V", "Number of quads in V direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("W Count", "W", "Number of quads in W direction", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddIntegerParameter("Nodes", "N", "List of new node numbering for each cube", GH_ParamAccess.tree);
            pManager.AddPointParameter("Lengths", "P", "lx, ly and lz for the cubes", GH_ParamAccess.list);
            pManager.AddPointParameter("Lengths", "C", "lx, ly and lz for the cubes", GH_ParamAccess.list);
            pManager.AddNumberParameter("Lengths", "A", "lx, ly and lz for the cubes", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brp = new Brep();
            int u = 1;
            int v = 1;
            int w = 1;

            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;
            if (!DA.GetData(3, ref w)) return;

            if (u < 1 || v < 1 || w < 1) //None of the sides can be divided in less than one part
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the input parameters is less than one.");
                return;
            }

            //Finding the length of the new elements
            Point3d[] nodes = brp.DuplicateVertices();
            Curve[] edges = brp.DuplicateEdgeCurves();

            VolumeMassProperties vmp = VolumeMassProperties.Compute(brp);
            Point3d centroid = vmp.Centroid;


            Point3d[] sortedNodes = sortNodes(nodes, centroid);

            List<Point3d> points = CreateNewBreps(sortedNodes, u, v, w);


            DA.SetDataList(0, sortedNodes);
            DA.SetDataList(1, points);
            //DA.SetDataList(2, angles);

        }

        public Point3d[] sortNodes(Point3d[] nodes, Point3d centroid)
        {

            Point3d[] lowerNodes = new Point3d[4];
            Point3d[] upperNodes = new Point3d[4];

            double[] lowerAngles = new double[nodes.Length / 2];
            double[] upperAngles = new double[nodes.Length / 2];

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

            //IEnumerable<Point3d> sortedNodes = lowerNodes.Union(upperNodes);

            
            Point3d[] sortedNodes = new Point3d[8];
            
            sortedNodes[0] = lowerNodes[0];
            sortedNodes[1] = lowerNodes[1];
            sortedNodes[2] = lowerNodes[2];
            sortedNodes[3] = lowerNodes[3];
            sortedNodes[4] = upperNodes[0];
            sortedNodes[5] = upperNodes[1];
            sortedNodes[6] = upperNodes[2];
            sortedNodes[7] = upperNodes[3];
        

            return sortedNodes;
        }

        //private List<List<int>> CreateNewBreps(Point3d[] nodes, int u, int v, int w)
        private List<Point3d> CreateNewBreps(Point3d[] nodes, int u, int v, int w)
        {
            List<List<int>> global_numbering = new List<List<int>>();

            /*
            Line uLine1 = new Line(nodes[0], nodes[1]);
            Line uLine2 = new Line(nodes[2], nodes[3]);
            Line uLine3 = new Line(nodes[4], nodes[5]);
            Line uLine4 = new Line(nodes[6], nodes[7]);

            Line vLine1 = new Line(nodes[0], nodes[3]);
            Line vLine2 = new Line(nodes[1], nodes[2]);
            Line vLine3 = new Line(nodes[4], nodes[7]);
            Line vLine4 = new Line(nodes[5], nodes[6]);

            Line wLine1 = new Line(nodes[0], nodes[3]);
            Line wLine2 = new Line(nodes[1], nodes[4]);
            Line wLine3 = new Line(nodes[2], nodes[5]);
            Line wLine4 = new Line(nodes[3], nodes[7]);


            */

            /*

         double ly1_new = (nodes[0].DistanceTo(nodes[3])) / v;
         double ly2_new = (nodes[1].DistanceTo(nodes[2])) / v;
         double ly3_new = (nodes[4].DistanceTo(nodes[7])) / v;
         double ly4_new = (nodes[5].DistanceTo(nodes[6])) / v;

         double lz1_new = (nodes[0].DistanceTo(nodes[4])) / w;
         double lz2_new = (nodes[1].DistanceTo(nodes[5])) / w;
         double lz3_new = (nodes[2].DistanceTo(nodes[6])) / w;
         double lz4_new = (nodes[3].DistanceTo(nodes[7])) / w;

         */

            /*
            Vector3d vec_y1 = nodes[0] - nodes[3] / (nodes[0].DistanceTo(nodes[3]));
            Vector3d vec_y2 = nodes[1] - nodes[2] / (nodes[1].DistanceTo(nodes[2]));
            Vector3d vec_y3 = nodes[4] - nodes[7] / (nodes[4].DistanceTo(nodes[7]));
            Vector3d vec_y4 = nodes[5] - nodes[6] / (nodes[5].DistanceTo(nodes[6]));

            Vector3d vec_z1 = nodes[0] - nodes[4] / (nodes[0].DistanceTo(nodes[4]));
            Vector3d vec_z2 = nodes[1] - nodes[5] / (nodes[1].DistanceTo(nodes[5]));
            Vector3d vec_z3 = nodes[2] - nodes[6] / (nodes[2].DistanceTo(nodes[6]));
            Vector3d vec_z4 = nodes[3] - nodes[7] / (nodes[3].DistanceTo(nodes[7]));
            */


            /*
            List<Point3d> lx1 = new List<Point3d>(u + 1);
            List<Point3d> lx2 = new List<Point3d>(u + 1);
            List<Point3d> lx3 = new List<Point3d>(u + 1);
            List<Point3d> lx4 = new List<Point3d>(u + 1);

            List<Point3d> ly1 = new List<Point3d>(v + 1);
            List<Point3d> ly2 = new List<Point3d>(v + 1);
            List<Point3d> ly3 = new List<Point3d>(v + 1);
            List<Point3d> ly4 = new List<Point3d>(v + 1);

            List<Point3d> lz1 = new List<Point3d>(w + 1);
            List<Point3d> lz2 = new List<Point3d>(w + 1);
            List<Point3d> lz3 = new List<Point3d>(w + 1);
            List<Point3d> lz4 = new List<Point3d>(w + 1);
            */




            double lx1_new = (nodes[0].DistanceTo(nodes[1])) / u;
            double lx2_new = (nodes[3].DistanceTo(nodes[2])) / u;
            double lx3_new = (nodes[4].DistanceTo(nodes[5])) / u;
            double lx4_new = (nodes[7].DistanceTo(nodes[6])) / u;

         
            Vector3d vec_x1 = (nodes[1] - nodes[0]) / (nodes[0].DistanceTo(nodes[1]));
            Vector3d vec_x2 = (nodes[2] - nodes[3]) / (nodes[3].DistanceTo(nodes[2]));
            Vector3d vec_x3 = (nodes[5] - nodes[4]) / (nodes[4].DistanceTo(nodes[5]));
            Vector3d vec_x4 = (nodes[6] - nodes[7]) / (nodes[7].DistanceTo(nodes[6]));

            
            List<Point3d> points = new List<Point3d>();


            for(int i = 0; i < u+1; i++)
            {
                //Lager punkt i u-retning
                Point3d p1_u = new Point3d(nodes[0].X + lx1_new * i * vec_x1.X, nodes[0].Y + lx1_new * vec_x1.Y * i, nodes[0].Z + lx1_new * vec_x1.Z * i);
                Point3d p2_u = new Point3d(nodes[3].X + lx2_new * i * vec_x2.X, nodes[3].Y + lx2_new * vec_x2.Y * i, nodes[3].Z + lx2_new * vec_x2.Z * i);

                Point3d p3_u = new Point3d(nodes[4].X + lx3_new * i * vec_x3.X, nodes[4].Y + lx3_new * vec_x3.Y * i, nodes[4].Z + lx3_new * vec_x3.Z * i);
                Point3d p4_u = new Point3d(nodes[7].X + lx4_new * i * vec_x4.X, nodes[7].Y + lx4_new * vec_x4.Y * i, nodes[7].Z + lx4_new * vec_x4.Z * i);

                Vector3d vecV1 = (p2_u - p1_u) / (p1_u.DistanceTo(p2_u));
                Vector3d vecV2 = (p4_u - p3_u) / (p3_u.DistanceTo(p4_u));

                Double length_v1 = p1_u.DistanceTo(p2_u) / v;
                Double length_v2 = p3_u.DistanceTo(p4_u) / v;

                for (int j = 0; j < v+1; j++)
                {
                    Point3d p1_v = new Point3d(p1_u.X + length_v1 * j * vecV1.X, p1_u.Y + length_v1 * j * vecV1.Y, p1_u.Z + length_v1 * j * vecV1.Z);
                    Point3d p2_v = new Point3d(p3_u.X + length_v2 * j * vecV2.X, p3_u.Y + length_v2 * j * vecV2.Y, p3_u.Z + length_v2 * j * vecV2.Z);

                    Vector3d vecW1 = (p2_v - p1_v) / (p1_v.DistanceTo(p2_v));

                    Double length_W1 = p1_v.DistanceTo(p2_v) / w;


                    for (int k = 0; k < w+1; k++)
                    {
                        Point3d p1_w = new Point3d(p1_v.X + length_W1 * k * vecW1.X, p1_v.Y + length_W1 * k * vecW1.Y, p1_v.Z + length_W1 * k * vecW1.Z);
                        points.Add(p1_w);

                    }
                }
            }





            /*
            for (int k = 0; k <= u; k++)
            {
                for (int j = 0; j<= v; j++)
                {
                    for (int i = 0; i <= w; i++)
                    {
                        Point3d p1 = new Point3d(nodes[0].X + lx1_new * i * vec_x1.X, nodes[0].Y + lx1_new * vec_x1.Y * i, nodes[0].Z + lx1_new * vec_x1.Z * i);
                        Point3d p2 = new Point3d(nodes[0].X + lx1_new * (i+1) * vec_x1.X, nodes[0].Y + lx1_new * vec_x1.Y * (i+1), nodes[0].Z + lx1_new * vec_x1.Z * (i+1));
                        Point3d p4 = new Point3d(nodes[0].X + ly1_new * j
                    }
                }
            }


            for (int k = 0; k <= u; k++)
            {
                for (int j = 0; j <= v; j++)
                {
                    for (int i = 0; i <= w; i++)
                    {
                        if (i < w && j < v && k < u)
                        {

                            Point3d p1 = 



                            Interval x = new Interval(lx_new * i, lx_new * (i + 1));
                            Interval y = new Interval(ly_new * j, ly_new * (j + 1));
                            Interval z = new Interval(lz_new * k, lz_new * (k + 1));
                            Box box_new = new Box(Plane.WorldXY, x, y, z);
                            Brep brep_new = box_new.ToBrep();
                            brep_elem.Add(brep_new); //Adds the smaller breps to the list
                            brep.add()
                        }


                        Point3d node = new Point3d(lx_new * i, ly_new * j, lz_new * k);
                        all_nodes.Add(node); //Adds each point to the list of nodes
                    }
                }
            }
            */


            return points;
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
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

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7fb6bb89-8f98-422e-b178-9af798527794"); }
        }
    }
}
