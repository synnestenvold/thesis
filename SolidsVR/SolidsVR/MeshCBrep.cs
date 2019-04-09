using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;

namespace SolidsVR
{
    public class MeshCBrep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshCBrep class.
        /// </summary>
        public MeshCBrep()
          : base("MeshCBrep", "MeshCBrep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Input geometry as a curved brep", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U count", "U", "Number of divisions in U direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("V count", "V", "Number of divisions in V direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("W count", "W", "Number of divisions in W direction", GH_ParamAccess.item, 1);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of Brep", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Connectivity", "C", "Relationship between local and global numbering", GH_ParamAccess.tree);
            pManager.AddPointParameter("Nodes", "N", "Coordinates for corner nodes in brep", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // ---variables-- -

            Brep brp = new Brep();
            int u = 1;
            int v = 1;
            int w = 1;

            // --- input ---

            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;
            if (!DA.GetData(3, ref w)) return;

            // --- solve ---

            if (u < 1 || v < 1 || w < 1) //None of the sides can be divided in less than one part
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the input parameters is less than one.");
                return;
            }

            Curve[] edges = brp.DuplicateEdgeCurves();

            Curve[] sortedEdges = SortEdges(edges);

            //FrepFaceList face = brp.Faces;
            //Surface facetest = face[0];
            

            var tuple = CreateNewBreps(sortedEdges, u, v, w); // Getting corner nodes and connectivity matrix

            List<List<Point3d>> elementPoints = tuple.Item1;
            List<List<int>> connectivity = tuple.Item2;
            int sizeOfMatrix = 3 * (u + 1) * (v + 1) * (w + 1);
            //Point3d[] globalPoints = CreatePointList(connectivity, elementPoints, sizeOfMatrix);

            //Setting values for Mesh class
            Mesh_class mesh = new Mesh_class(u, v, w);
            //List<Point3d> points = CreateNewBreps(sortedEdges, u, v, w);
            //mesh.SetConnectivity(connectivity);
            //mesh.SetElementPoints(elementPoints);
            //mesh.SetSizeOfMatrix(sizeOfMatrix);
            //mesh.SetGlobalPoints(globalPoints);

            //---output---


            DataTree<Point3d> treePoints = new DataTree<Point3d>();
            DataTree<int> treeIndexes = new DataTree<int>();

            int i = 0;
            //Create a tree structure of the list of new brep-nodes with cartesian coordinates
            foreach (List<Point3d> innerList in elementPoints)
            {
                treePoints.AddRange(innerList, new GH_Path(new int[] { 0, i }));
                i++;
            }

            i = 0;

            //Create a tree structure of the list of new brep-nodes with indexes
            foreach (List<int> innerList in connectivity)
            {
                treeIndexes.AddRange(innerList, new GH_Path(new int[] { 0, i }));
                i++;
            }

            DA.SetDataTree(0, treeIndexes);
            DA.SetDataTree(1, treePoints);

            //DA.SetDataList(0, points);
        }

        public Tuple<List<List<Point3d>>, List<List<int>>> CreateNewBreps(Curve[] edges, int u, int v, int w)
        {

            List<List<int>> global_numbering = new List<List<int>>();
            List<List<Point3d>> points_brep = new List<List<Point3d>>();
            List<Point3d> points = new List<Point3d>();
            List<List<Point3d>> uDiv = new List<List<Point3d>>();
            List<List<Point3d>> vDiv = new List<List<Point3d>>();
            List<List<Point3d>> wDiv = new List<List<Point3d>>();
            
            for (int i=0; i<4; i++)
            {
                Point3d[] uP = new Point3d[u+1];
                Point3d[] vP = new Point3d[v+1];
                Point3d[] wP = new Point3d[w+1];
                edges[i].DivideByCount(u, true, out uP);
                edges[i+4].DivideByCount(v, true, out vP);
                edges[i+8].DivideByCount(w, true, out wP);
                //List<Point2> lst = ints.OfType<int>().ToList();
                
                uDiv.Add(uP.ToList());
                vDiv.Add(vP.ToList());
                wDiv.Add(wP.ToList());

            }

            List<Vector3d> vecV1_ = new List<Vector3d>();
            List<Vector3d> vecV2_ = new List<Vector3d>();
            List<Vector3d> vecU_ = new List<Vector3d>();
            //List<List<Vector3d>> vecV = new List<List<Vector3d>>();

            for (int j = 0; j < (v + 1)-1; j++)
            {
                vecV1_.Add(vDiv[0][j+1] - vDiv[0][j]);
                vecV2_.Add(vDiv[1][j+1] - vDiv[1][j]);
            }

            for (int j = 0; j < (u + 1) - 1; j++)
            {
                vecU_.Add(uDiv[0][j + 1] - uDiv[0][j]);
            }

            //W-dir

            for (int i = 0; i <= w; i++)
            {
                //Creating points in w-directoin
                //Point3d p1_w = new Point3d(nodes[0].X + lz1_new * i * vec_z1.X, nodes[0].Y + lz1_new * vec_z1.Y * i, nodes[0].Z + lz1_new * vec_z1.Z * i);
                //Point3d p2_w = new Point3d(nodes[1].X + lz2_new * i * vec_z2.X, nodes[1].Y + lz2_new * vec_z2.Y * i, nodes[1].Z + lz2_new * vec_z2.Z * i);

                //Point3d p3_w = new Point3d(nodes[2].X + lz3_new * i * vec_z3.X, nodes[2].Y + lz3_new * vec_z3.Y * i, nodes[2].Z + lz3_new * vec_z3.Z * i);
                //Point3d p4_w = new Point3d(nodes[3].X + lz4_new * i * vec_z4.X, nodes[3].Y + lz4_new * vec_z4.Y * i, nodes[3].Z + lz4_new * vec_z4.Z * i);
                Point3d p1_w = wDiv[0][i];
                Point3d p2_w = wDiv[1][i];
                Point3d p3_w = wDiv[2][i];
                Point3d p4_w = wDiv[3][i];
            
                

                Vector3d vecV1 = (p4_w - p1_w) / (p1_w.DistanceTo(p4_w));
                Vector3d vecV2 = (p3_w - p2_w) / (p2_w.DistanceTo(p3_w));

                Double length_v1 = p1_w.DistanceTo(p4_w) / v;
                Double length_v2 = p2_w.DistanceTo(p3_w) / v;
                
                

                for (int j = 0; j <= v; j++)
                {
                    //Creating points in v-direction
                    Point3d p1_v = new Point3d(p1_w.X + length_v1 * j * vecV1.X, p1_w.Y + length_v1 * j * vecV1.Y, p1_w.Z + length_v1 * j * vecV1.Z);
                    Point3d p2_v = new Point3d(p2_w.X + length_v2 * j * vecV2.X, p2_w.Y + length_v2 * j * vecV2.Y, p2_w.Z + length_v2 * j * vecV2.Z);

                    //Point3d p1_v = Point3d.Add(p1_w + vecV1_[j]);

                    //Point3d p1_v = p1_w + vecV1_[j];
                   // Point3d p2_v = p2_w + vecV2_[j];



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

                //
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

            return Tuple.Create(points_brep, global_numbering);

        }
    

        public Curve[] SortEdges(Curve[] edges)
        {
            Curve[] sortedEdges = new Curve[12];

            sortedEdges[0] = edges[7]; //u-dir
            sortedEdges[0].Reverse();
            sortedEdges[1] = edges[5];
            sortedEdges[2] = edges[1];
            sortedEdges[3] = edges[3];
            sortedEdges[2].Reverse();
            sortedEdges[4] = edges[4]; //v-dir
            sortedEdges[5] = edges[6];
            sortedEdges[5].Reverse();
            sortedEdges[6] = edges[2];
            sortedEdges[7] = edges[0];
            sortedEdges[7].Reverse();
            sortedEdges[8] = edges[8]; //w-dir
            sortedEdges[8].Reverse();
            sortedEdges[9] = edges[9];
            sortedEdges[9].Reverse();
            sortedEdges[10] = edges[10];
            sortedEdges[10].Reverse();
            sortedEdges[11] = edges[11];
            sortedEdges[11].Reverse();

            return sortedEdges;
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0f4702ea-a195-4b83-b3ae-5e067f56a73f"); }
        }
    }
}