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

            List<List<Point3d>> points = CreateNewBreps(sortedNodes, u, v, w);
            //List < Point3d > points = CreateNewBreps(sortedNodes, u, v, w);

            DataTree<Point3d> tree = new DataTree<Point3d>();
            int i = 0;

            
            //Create a tree structure of the list of new brep-nodes
            foreach (List<Point3d> innerList in points)
            {
                tree.AddRange(innerList, new GH_Path(new int[] { 0, i }));
                i++;
            }
            


            DA.SetDataList(0, sortedNodes);
            DA.SetDataTree(1, tree);
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

        private List<List<Point3d>> CreateNewBreps(Point3d[] nodes, int u, int v, int w)
        //private List<Point3d> CreateNewBreps(Point3d[] nodes, int u, int v, int w)
        {


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



            /*

            double lx1_new = (nodes[0].DistanceTo(nodes[1])) / u;
            double lx2_new = (nodes[3].DistanceTo(nodes[2])) / u;
            double lx3_new = (nodes[4].DistanceTo(nodes[5])) / u;
            double lx4_new = (nodes[7].DistanceTo(nodes[6])) / u;

         
            Vector3d vec_x1 = (nodes[1] - nodes[0]) / (nodes[0].DistanceTo(nodes[1]));
            Vector3d vec_x2 = (nodes[2] - nodes[3]) / (nodes[3].DistanceTo(nodes[2]));
            Vector3d vec_x3 = (nodes[5] - nodes[4]) / (nodes[4].DistanceTo(nodes[5]));
            Vector3d vec_x4 = (nodes[6] - nodes[7]) / (nodes[7].DistanceTo(nodes[6]));

            */

            double lz1_new = (nodes[0].DistanceTo(nodes[4])) / w;
            double lz2_new = (nodes[1].DistanceTo(nodes[5])) / w;
            double lz3_new = (nodes[2].DistanceTo(nodes[6])) / w;
            double lz4_new = (nodes[3].DistanceTo(nodes[7])) / w;

            Vector3d vec_z1 = (nodes[4] - nodes[0]) / (nodes[0].DistanceTo(nodes[4]));
            Vector3d vec_z2 = (nodes[5] - nodes[1]) / (nodes[1].DistanceTo(nodes[5]));
            Vector3d vec_z3 = (nodes[6] - nodes[2]) / (nodes[2].DistanceTo(nodes[6]));
            Vector3d vec_z4 = (nodes[7] - nodes[3]) / (nodes[3].DistanceTo(nodes[7]));

            List<Point3d> points = new List<Point3d>();


            for (int i = 0; i <= w; i++)
            {
                //Lager punkt i u-retning
                Point3d p1_w = new Point3d(nodes[0].X + lz1_new * i * vec_z1.X, nodes[0].Y + lz1_new * vec_z1.Y * i, nodes[0].Z + lz1_new * vec_z1.Z * i);
                Point3d p2_w = new Point3d(nodes[1].X + lz2_new * i * vec_z2.X, nodes[1].Y + lz2_new * vec_z2.Y * i, nodes[1].Z + lz2_new * vec_z2.Z * i);

                Point3d p3_w = new Point3d(nodes[2].X + lz3_new * i * vec_z3.X, nodes[2].Y + lz3_new * vec_z3.Y * i, nodes[2].Z + lz3_new * vec_z3.Z * i);
                Point3d p4_w = new Point3d(nodes[3].X + lz4_new * i * vec_z4.X, nodes[3].Y + lz4_new * vec_z4.Y * i, nodes[3].Z + lz4_new * vec_z4.Z * i);

                Vector3d vecV1 = (p4_w - p1_w) / (p1_w.DistanceTo(p4_w));
                Vector3d vecV2 = (p3_w - p2_w) / (p2_w.DistanceTo(p3_w));

                Double length_v1 = p1_w.DistanceTo(p4_w) / v;
                Double length_v2 = p2_w.DistanceTo(p3_w) / v;

                for (int j = 0; j <= v; j++)
                {
                    Point3d p1_v = new Point3d(p1_w.X + length_v1 * j * vecV1.X, p1_w.Y + length_v1 * j * vecV1.Y, p1_w.Z + length_v1 * j * vecV1.Z);
                    Point3d p2_v = new Point3d(p2_w.X + length_v2 * j * vecV2.X, p2_w.Y + length_v2 * j * vecV2.Y, p2_w.Z + length_v2 * j * vecV2.Z);

                    Vector3d vec_u1 = (p2_v - p1_v) / (p1_v.DistanceTo(p2_v));

                    Double length_u1 = p1_v.DistanceTo(p2_v) / u;


                    for (int k = 0; k <= u; k++)
                    {
                        Point3d p1_u = new Point3d(p1_v.X + length_u1 * k * vec_u1.X, p1_v.Y + length_u1 * k * vec_u1.Y, p1_v.Z + length_u1 * k * vec_u1.Z);
                        points.Add(p1_u);

                    }
                }
            }

            List<List<int>> global_numbering = new List<List<int>>();

            List<List<Point3d>> points_brep = new List<List<Point3d>>();


            //So much shitty code. Just trying to make it work:)))((:

            List<int> listJumpOne = new List<int>();
            List<int> listJumpUp = new List<int>();

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < v - 1; j++)
                {
                    listJumpOne.Add((u - 1) + j * (u + 1) + (u + 1) * (v + 1) * i);
                }

            }


            for (int i = 0; i < w; i++)
            {
                listJumpUp.Add(u * v + (u - 2) + (u + 1) * (v + 1) * i);
            }



            int c = 0;

            for (int j = 0; j < u * v * w; j++)
            {
                Console.WriteLine("CUBE: " + (j + 1));

                List<Point3d> brp = new List<Point3d>();
                brp.Add(points[c]);
                brp.Add(points[c + 1]);
                brp.Add(points[(u + 1) + (c + 1)]);
                brp.Add(points[(u + 1) + (c)]);
                brp.Add(points[(u + 1) * (v + 1) + c]);
                brp.Add(points[(u + 1) * (v + 1) + (c + 1)]);
                brp.Add(points[(u + 1) * (v + 1) + (u + 1) + (c + 1)]);
                brp.Add(points[(u + 1) * (v + 1) + (u + 1) + (c)]);

                points_brep.Add(brp);



                if (listJumpOne.Contains(c))
                {
                    c += 1;
                }


                if (listJumpUp.Contains(c))
                {
                    c += (u + 2);
                }

                c++;
            }




            return points_brep;
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
