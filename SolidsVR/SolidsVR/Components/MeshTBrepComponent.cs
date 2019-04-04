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

namespace SolidsVR
{
    public class MeshTBrepComponent : GH_Component
    {
        
        public MeshTBrepComponent()
          : base("MeshTBrep", "MeshTBrep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Input cube as a (twisted) brep", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U count", "U", "Number of divisions in U direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("V count", "V", "Number of divisions in V direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("W count", "W", "Number of divisions in W direction", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddIntegerParameter("Nodes", "N", "List of new node numbering for each cube", GH_ParamAccess.tree);
            //pManager.AddIntegerParameter("Connectivity", "C", "Relationship between local and global numbering", GH_ParamAccess.tree);
            //pManager.AddPointParameter("Nodes", "N", "Coordinates for corner nodes in brep", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Mesh", "M", "Mesh of Brep", GH_ParamAccess.item);
            

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

            var tuple = CreateNewBreps(nodes, u, v, w); // Getting corner nodes and connectivity matrix

            List<List<Point3d>> elementPoints = tuple.Item1;
            List<List<int>> connectivity = tuple.Item2;
            int sizeOfMatrix = 3 * (u + 1) * (v + 1) * (w + 1);
            Point3d[] globalPoints = CreatePointList(connectivity, elementPoints, sizeOfMatrix);


            Mesh_class mesh = new Mesh_class(u, v, w);
            mesh.SetConnectivity(connectivity);
            mesh.SetElementPoints(elementPoints);
            mesh.SetSizeOfMatrix(sizeOfMatrix);
            mesh.SetGlobalPoints(globalPoints);


            DA.SetData(0, mesh);

        }

        
     
        private Tuple<List<List<Point3d>>, List<List<int>>> CreateNewBreps(Point3d[] nodes, int u, int v, int w)
        {

            List<List<int>> global_numbering = new List<List<int>>();
            List<List<Point3d>> points_brep = new List<List<Point3d>>();

            //Distances in w-direction after dividing in w elements
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
                //Creating points in w-directoin
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
                    //Creating points in v-direction
                    Point3d p1_v = new Point3d(p1_w.X + length_v1 * j * vecV1.X, p1_w.Y + length_v1 * j * vecV1.Y, p1_w.Z + length_v1 * j * vecV1.Z);
                    Point3d p2_v = new Point3d(p2_w.X + length_v2 * j * vecV2.X, p2_w.Y + length_v2 * j * vecV2.Y, p2_w.Z + length_v2 * j * vecV2.Z);

                    Vector3d vec_u1 = (p2_v - p1_v) / (p1_v.DistanceTo(p2_v));

                    Double length_u1 = p1_v.DistanceTo(p2_v) / u;


                    for (int k = 0; k <= u; k++)
                    {
                        //Creating points in u-direction and adding them to the global nodes.
                        Point3d p1_u = new Point3d(p1_v.X + length_u1 * k * vec_u1.X, p1_v.Y + length_u1 * k * vec_u1.Y, p1_v.Z + length_u1 * k * vec_u1.Z);
                        points.Add(p1_u);

                    }
                }
            }


            // Putting together the breps:

            //*******So much shitty code. Just trying to make it work:)))((:

            List<int> listJumpOne = new List<int>(); // List with points where it must move in v-direction
            List<int> listJumpUp = new List<int>(); // List with points where it must move upwards w-direction


            //Finding indexes for jumping in v-direction
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < v - 1; j++)
                {
                    listJumpOne.Add((u - 1) + j * (u + 1) + (u + 1) * (v + 1) * i);
                }

            }

            //Finding indexes for jumping in w-direction
            for (int i = 0; i < w; i++)
            {
                listJumpUp.Add((u + 1) * (v + 1) - (u + 1) - 2 + (u + 1) * (v + 1) * i);
            }

            int index = 0;
            
            for (int i = 0; i < u * v * w; i++) // Creating u*v*w new breps having the 8 corner points
            {
    
                List<Point3d> brp = new List<Point3d>();

                //Putting together the 8 points to make the brep
                brp.Add(points[index]);
                brp.Add(points[index + 1]);
                brp.Add(points[(u + 1) + (index + 1)]);
                brp.Add(points[(u + 1) + (index)]);
                brp.Add(points[(u + 1) * (v + 1) + index]);
                brp.Add(points[(u + 1) * (v + 1) + (index + 1)]);
                brp.Add(points[(u + 1) * (v + 1) + (u + 1) + (index + 1)]);
                brp.Add(points[(u + 1) * (v + 1) + (u + 1) + (index)]);

                points_brep.Add(brp);

                //Showing the connectivity between local and global nodes
                List<int> connectivity = new List<int>();
                connectivity.Add(index);
                connectivity.Add(index + 1);
                connectivity.Add((u + 1) + (index + 1));
                connectivity.Add((u + 1) + (index));
                connectivity.Add((u + 1) * (v + 1) + index);
                connectivity.Add((u + 1) * (v + 1) + (index + 1));
                connectivity.Add((u + 1) * (v + 1) + (u + 1) + (index + 1));
                connectivity.Add((u + 1) * (v + 1) + (u + 1) + (index));

                global_numbering.Add(connectivity);

                if (listJumpOne.Contains(index)) //Checking if we need to move to next row
                {
                    index += 1;
                }


                if (listJumpUp.Contains(index)) //Checking if we need to move to next level
                {
                    index += (u + 2);
                }

                index++;
            }

            return Tuple.Create(points_brep,global_numbering);
           
        }

        public Point3d[] CreatePointList(List<List<int>> treeConnectivity, List<List<Point3d>> treePoints, int sizeOfM)
        {
            Point3d[] pointList = new Point3d[sizeOfM / 3];


            for (int i = 0; i < treeConnectivity.Count; i++)
            {
                List<int> connectedNodes = treeConnectivity[i];
                List<Point3d> connectedPoints = treePoints[i];

                for (int j = 0; j < connectedNodes.Count; j++)
                {
                    pointList[connectedNodes[j]] = connectedPoints[j];
                }
            }
            return pointList;

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
