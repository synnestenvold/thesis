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
              "Category3", "Mesh")
        {
        }

        Node node = new Node(new Point3d(0, 0, 0),0);

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Input geometry as a curved brep", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Corner points in right order", GH_ParamAccess.list);
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

            Point3d[] cornerPoints = brp.DuplicateVertices();

            Curve[] edges = brp.DuplicateEdgeCurves();
            Curve[] sortedEdges = SortEdges(corners, edges);
            
            var tuple = CreateNewBreps(sortedEdges, u, v, w, cornerPoints); 

            List<List<Point3d>> elementPoints = tuple.Item1;
            List<List<int>> connectivity = tuple.Item2;
            List<List<Line>> edgeMesh = tuple.Item3;
            List<List<Brep>> surfacesMesh = tuple.Item4;
            List<Node> nodes = tuple.Item5;
            int sizeOfMatrix = 3 * (u + 1) * (v + 1) * (w + 1);
            Point3d[] globalPoints = CreatePointList(connectivity, elementPoints, sizeOfMatrix);

            //Setting values for Mesh class
            Mesh_class mesh = new Mesh_class(u, v, w);
            mesh.SetConnectivity(connectivity);
            mesh.SetElementPoints(elementPoints);
            mesh.SetEdgesMesh(edgeMesh);
            mesh.SetSurfacesMesh(surfacesMesh);
            mesh.SetSizeOfMatrix(sizeOfMatrix);
            mesh.SetGlobalPoints(globalPoints);
            mesh.SetNodeList(nodes);
            mesh.SetOrigBrep(brp);
            mesh.SetBrep(new Brep_class(brp));
            mesh.OrderSurfaces(corners);


            //---output---

            DA.SetData(0, mesh);
        }

        public Tuple<List<List<Point3d>>, List<List<int>>, List<List<Line>>, List<List<Brep>>, List<Node>> CreateNewBreps(Curve[] edges, int u, int v, int w, Point3d[] cornerNodes)
        {

            List<List<int>> global_numbering = new List<List<int>>();
            List<List<Point3d>> points_brep = new List<List<Point3d>>();
            List<List<Line>> edgeMesh = new List<List<Line>>();
            List <List<Brep>> surfacesMesh = new List<List<Brep>>();

            List<Node> nodes = new List<Node>();


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


            for (int i = 0; i <= w; i++)
            {
                //Creating points in w-directoin
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
                        Node node = new Node(p1_u, points.IndexOf(p1_u));

                        SetNodePosition(node, p1_u, cornerNodes, i, j, k, u, v, w);
                        SetNodeSurface(node, i, j, k, u, v, w);
                        nodes.Add(node);


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

                List<Line> edgesElement = CreateEdgesMesh(brp);
                List<Brep> surfaces = CreateSurfaceMesh(brp);

                edgeMesh.Add(edgesElement);
                surfacesMesh.Add(surfaces);
                
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

            

            return Tuple.Create(points_brep, global_numbering, edgeMesh, surfacesMesh, nodes);

        }

        public void SetNodeSurface(Node node, int i, int j, int k, int u, int v, int w)
        {
            if (j == 0) node.SetSurfaceNum(0);
            if (k == u) node.SetSurfaceNum(1);
            if (j == v) node.SetSurfaceNum(2);
            if (k == 0) node.SetSurfaceNum(3);
            if (i == 0) node.SetSurfaceNum(4);
            if (i == w) node.SetSurfaceNum(5);
        }

        public void SetNodePosition(Node node, Point3d p, Point3d[] cornerPoints, int i, int j, int k, int u, int v, int w)
        {
            if (cornerPoints.Contains(p)) node.SetIsCorner();
            else if (i == 0 && j == 0 || i == 0 && k == 0 || j == 0 && k ==0 ||  i == w && j == 0 || i == 0 && k == u ||  j == v && k == 0 || i == 0 && j == v || i == w && k == 0 || j == 0 && k == u || i == 0 && j == 0 || i == 0 && k == 0 || j == 0 && k == 0 || i == w && j == v || i == w && k == u || j == v && k == u) node.SetIsEdge();
            else node.SetIsMiddle();
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

        public List<Line> CreateEdgesMesh(List<Point3d> elementPoints)
        {
            List<Line> edges = new List<Line>
            {
                new Line(elementPoints[0], elementPoints[1]),
                new Line(elementPoints[1], elementPoints[2]),
                new Line(elementPoints[2], elementPoints[3]),
                new Line(elementPoints[3], elementPoints[0]),
                new Line(elementPoints[0], elementPoints[4]),
                new Line(elementPoints[1], elementPoints[5]),
                new Line(elementPoints[2], elementPoints[6]),
                new Line(elementPoints[3], elementPoints[7]),
                new Line(elementPoints[4], elementPoints[5]),
                new Line(elementPoints[5], elementPoints[6]),
                new Line(elementPoints[6], elementPoints[7]),
                new Line(elementPoints[7], elementPoints[4]),

            };

            return edges;
        }


        public List<Brep> CreateSurfaceMesh(List<Point3d> elementPoints)
        {

            Brep b1 = Brep.CreateFromCornerPoints(elementPoints[0], elementPoints[1], elementPoints[5], elementPoints[4], 0);
            Brep b2 = Brep.CreateFromCornerPoints(elementPoints[1], elementPoints[2], elementPoints[6], elementPoints[5], 0);
            Brep b3 = Brep.CreateFromCornerPoints(elementPoints[2], elementPoints[3], elementPoints[7], elementPoints[6], 0);
            Brep b4 = Brep.CreateFromCornerPoints(elementPoints[0], elementPoints[3], elementPoints[7], elementPoints[4], 0);
            Brep b5 = Brep.CreateFromCornerPoints(elementPoints[0], elementPoints[1], elementPoints[2], elementPoints[3], 0);
            Brep b6 = Brep.CreateFromCornerPoints(elementPoints[4], elementPoints[5], elementPoints[6], elementPoints[7], 0);
            

            List<Brep> surfaces = new List<Brep>()
            {
                b1,
                b2,
                b3,
                b4,
                b5,
                b6
            };

            return surfaces;
        }
        

        public Curve[] SortEdges(List<Point3d> corners, Curve[] edges)
        {
            Curve[] sortedEdges = new Curve[12];
            for (int i = 0; i < edges.Length; i++)
            {
                List<Point3d> tempP = new List<Point3d>() { edges[i].PointAtStart, edges[i].PointAtEnd };
                //u-dir
                if (tempP.Contains(corners[0]) && tempP.Contains(corners[1]))
                {
                    sortedEdges[0] = edges[i];
                    if (edges[i].PointAtEnd == corners[0]) { sortedEdges[0].Reverse(); }
                }
                else if (tempP.Contains(corners[3]) && tempP.Contains(corners[2]))
                {
                    sortedEdges[1] = edges[i];
                    if (edges[i].PointAtEnd == corners[3]) { sortedEdges[1].Reverse(); }
                }
                else if (tempP.Contains(corners[4]) && tempP.Contains(corners[5]))
                {
                    sortedEdges[2] = edges[i];
                    if (edges[i].PointAtEnd == corners[4]) { sortedEdges[2].Reverse(); }
                }
                else if (tempP.Contains(corners[7]) && tempP.Contains(corners[6]))
                {
                    sortedEdges[3] = edges[i];
                    if (edges[i].PointAtEnd == corners[7]) { sortedEdges[3].Reverse(); }
                }
                //v-dir
                else if (tempP.Contains(corners[0]) && tempP.Contains(corners[3]))
                {
                    sortedEdges[4] = edges[i];
                    if (edges[i].PointAtEnd == corners[0]) { sortedEdges[4].Reverse(); }
                }
                else if (tempP.Contains(corners[1]) && tempP.Contains(corners[2]))
                {
                    sortedEdges[5] = edges[i];
                    if (edges[i].PointAtEnd == corners[1]) { sortedEdges[5].Reverse(); }
                }
                else if (tempP.Contains(corners[5]) && tempP.Contains(corners[6]))
                {
                    sortedEdges[6] = edges[i];
                    if (edges[i].PointAtEnd == corners[5]) { sortedEdges[6].Reverse(); }
                }
                else if (tempP.Contains(corners[4]) && tempP.Contains(corners[7]))
                {
                    sortedEdges[7] = edges[i];
                    if (edges[i].PointAtEnd == corners[4]) { sortedEdges[7].Reverse(); }
                }
                //w-dir
                else if (tempP.Contains(corners[0]) && tempP.Contains(corners[4]))
                {
                    sortedEdges[8] = edges[i];
                    if (edges[i].PointAtEnd == corners[0]) { sortedEdges[8].Reverse(); }
                }
                else if (tempP.Contains(corners[1]) && tempP.Contains(corners[5]))
                {
                    sortedEdges[9] = edges[i];
                    if (edges[i].PointAtEnd == corners[1]) { sortedEdges[9].Reverse(); }
                }
                else if (tempP.Contains(corners[2]) && tempP.Contains(corners[6]))
                {
                    sortedEdges[10] = edges[i];
                    if (edges[i].PointAtEnd == corners[2]) { sortedEdges[10].Reverse(); }
                }
                else {
                    sortedEdges[11] = edges[i];
                    if (edges[i].PointAtEnd == corners[3]) { sortedEdges[11].Reverse(); }
                }
            }
            return sortedEdges;
        }

   
        public override Guid ComponentGuid
        {
            get { return new Guid("0f4702ea-a195-4b83-b3ae-5e067f56a73f"); }
        }
    }
}