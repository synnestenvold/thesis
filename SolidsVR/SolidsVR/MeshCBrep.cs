using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using Rhino.Geometry.Collections;

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
            pManager.AddPointParameter("Points", "P", "List of corner points in right order", GH_ParamAccess.list);
            pManager.AddIntegerParameter("U count", "U", "Number of divisions in U direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("V count", "V", "Number of divisions in V direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("W count", "W", "Number of divisions in W direction", GH_ParamAccess.item, 1);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of Brep", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // ---variables-- -

            Brep brp = new Brep();
            List<Point3d> corners = new List<Point3d>();
            int u = 1;
            int v = 1;
            int w = 1;

            // --- input ---

            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetDataList(1, corners)) return;
            if (!DA.GetData(2, ref u)) return;
            if (!DA.GetData(3, ref v)) return;
            if (!DA.GetData(4, ref w)) return;

            // --- solve ---

            if (u < 1 || v < 1 || w < 1) //None of the sides can be divided in less than one part
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the input parameters is less than one.");
                return;
            }

            Curve[] edges = brp.DuplicateEdgeCurves();
            Curve[] sortedEdges = SortEdges(corners, edges);

            //FrepFaceList face = brp.Faces;
            //Surface facetest = face[0];
            
            //var tuple = CreateNewBreps(sortedEdges, u, v, w); // Getting corner nodes and connectivity matrix

            //List<List<Point3d>> elementPoints = tuple.Item1;
            //List<List<int>> connectivity = tuple.Item2;
            int sizeOfMatrix = 3 * (u + 1) * (v + 1) * (w + 1);
            //Point3d[] globalPoints = CreatePointList(connectivity, elementPoints, sizeOfMatrix);

            //Setting values for Mesh class
            Mesh_class mesh = new Mesh_class(u, v, w);
            List<Point3d> points = CreateNewBreps(sortedEdges, u, v, w);
            //mesh.SetConnectivity(connectivity);
            //mesh.SetElementPoints(elementPoints);
            //mesh.SetSizeOfMatrix(sizeOfMatrix);
            //mesh.SetGlobalPoints(globalPoints);

            //---output---

            DA.SetDataList(0, points);
        }

        private List<Point3d> CreateNewBreps(Curve[] edges, int u, int v, int w)
        {
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

            return points;
        }

        public Curve[] SortEdges(List<Point3d> corners, Curve[] edges)
        {
            Curve[] sortedEdges = new Curve[12];
            for (int i = 0; i < edges.Length; i++)
            {
                List<Point3d> tempP = new List<Point3d>() { edges[i].PointAtStart, edges[i].PointAtEnd };
                if (tempP.Contains(corners[0]))
                {

                }
            }

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