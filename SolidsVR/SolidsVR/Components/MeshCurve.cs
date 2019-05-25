using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;

namespace SolidsVR
{
    public class MeshCurve : GH_Component
    {

        public MeshCurve()
          : base("MeshCurve", "MeshC",
              "For meshing curve-formed geometry with straight edges cross-section",
              "SolidsVR", "Mesh")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Input geometry as a curved brep", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Corner points in right order", GH_ParamAccess.list);
            pManager.AddIntegerParameter("u count", "u", "Number of divisions in u direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("v count", "v", "Number of divisions in v direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("w count", "w", "Number of divisions in w direction", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Volume to remove", "V", "Volume percentage to remove", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of Brep", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //NOTE: This meshing class only mesh geometry with straight lines as cross-section,
            //but handle geometry which are curved between the cross-sections.
            //This is the meshing component for CASE 2.

            // ---variables-- -

            Brep brp = new Brep();
            List<Point3d> cornersList = new List<Point3d>();
            int u = 1;
            int v = 1;
            int w = 1;
            double removeVolume = 0;

            // --- input ---

            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetDataList(1, cornersList)) return;
            if (!DA.GetData(2, ref u)) return;
            if (!DA.GetData(3, ref v)) return;
            if (!DA.GetData(4, ref w)) return;
            if (!DA.GetData(5, ref removeVolume)) return;

            // --- setup ---

            if (u < 1 || v < 1 || w < 1) //None of the sides can be divided in less than one part
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the input parameters is less than one.");
                return;
            }

            Point3d[] cornerPoints = brp.DuplicateVertices();
            cornerPoints = RoundPoints(cornerPoints); //Round off to remove inaccuries

            Curve[] edges = brp.DuplicateEdgeCurves();
            edges= RoundEdgePoints(edges); //Round off to remove inaccuries

            List<Point3d> corners = RoundPointsList(cornersList);
            Curve[] sortedEdges = SortEdges(corners, edges); //Sort edges in correct order according to control points

            // --- solve ---

            var tuple = CreateMesh(sortedEdges, u, v, w, cornerPoints); 

            List<List<Point3d>> elementPoints = tuple.Item1;
            List<List<int>> connectivity = tuple.Item2;
            List<List<Line>> edgeMesh = tuple.Item3;
            List<List<Brep>> surfacesMesh = tuple.Item4;
            List<Node> nodes = tuple.Item5;
            List<Element> elements = tuple.Item6;
            List<Point3d> globalPoints = tuple.Item7;

            int sizeOfMatrix = 3 * (u + 1) * (v + 1) * (w + 1);
            //Point3d[] globalPoints = CreatePointList(connectivity, elementPoints, sizeOfMatrix);

            //Setting values for Mesh class
            MeshGeometry mesh = new MeshGeometry(u, v, w);
            mesh.SetConnectivity(connectivity);
            mesh.SetElementPoints(elementPoints);
            mesh.SetEdgesMesh(edgeMesh);
            mesh.SetSurfacesMesh(surfacesMesh);
            mesh.SetSizeOfMatrix(sizeOfMatrix);
            mesh.SetGlobalPoints(globalPoints);
            mesh.SetNodeList(nodes);
            mesh.SetElements(elements);
            mesh.SetOrigBrep(brp);
            mesh.SetBrep(new BrepGeometry(brp));
            mesh.OrderSurfaces(corners);
            mesh.SetOptVolume(removeVolume);

            //---output---

            DA.SetData(0, mesh);
        }

    public (List<List<Point3d>>, List<List<int>>, List<List<Line>>, List<List<Brep>>, List<Node>, List<Element>, List<Point3d>) CreateMesh(Curve[] edges, int u, int v, int w, Point3d[] cornerNodes)
        {
            //Output-lists
            List<List<int>> globalConnectivity = new List<List<int>>();
            List<List<Point3d>> hexPoints = new List<List<Point3d>>();
            List<List<Line>> edgeMesh = new List<List<Line>>();
            List <List<Brep>> surfacesMesh = new List<List<Brep>>();
            List<Node> nodes = new List<Node>();
            List<Element> elements = new List<Element>();
            List<Point3d> globalPoints = new List<Point3d>();

            List<List<Point3d>> wDiv = new List<List<Point3d>>();

            //Finding points in w-direction
            for (int i=0; i<4; i++)
            {
                Point3d[] wP = new Point3d[w+1];
                edges[i+8].DivideByCount(w, true, out wP);
                wDiv.Add(wP.ToList());

            }

            for (int i = 0; i <= w; i++)
            {
                //Creating points in w-directoin
                Point3d p1_w = wDiv[0][i];
                Point3d p2_w = wDiv[1][i];
                Point3d p3_w = wDiv[2][i];
                Point3d p4_w = wDiv[3][i];
            
                //Vectors in v-direction
                Vector3d vecV1 = (p4_w - p1_w) / (p1_w.DistanceTo(p4_w));
                Vector3d vecV2 = (p3_w - p2_w) / (p2_w.DistanceTo(p3_w));

                Double length_v1 = p1_w.DistanceTo(p4_w) / v;
                Double length_v2 = p2_w.DistanceTo(p3_w) / v;

                for (int j = 0; j <= v; j++)
                {
                    //Creating points in v-direction
                    Point3d p1_v = new Point3d(p1_w.X + length_v1 * j * vecV1.X, p1_w.Y + length_v1 * j * vecV1.Y, p1_w.Z + length_v1 * j * vecV1.Z);
                    Point3d p2_v = new Point3d(p2_w.X + length_v2 * j * vecV2.X, p2_w.Y + length_v2 * j * vecV2.Y, p2_w.Z + length_v2 * j * vecV2.Z);

                    //Vector in u-direction
                    Vector3d vec_u1 = (p2_v - p1_v) / (p1_v.DistanceTo(p2_v));

                    Double length_u1 = p1_v.DistanceTo(p2_v) / u;

                    for (int k = 0; k <= u; k++)
                    {
                        //Creating points in u-direction and adding them to the global nodes.
                        Point3d p1_u = new Point3d(p1_v.X + length_u1 * k * vec_u1.X, p1_v.Y + length_u1 * k * vec_u1.Y, p1_v.Z + length_u1 * k * vec_u1.Z);
                        globalPoints.Add(p1_u);

                        //Create node instance of point created
                        Node node = new Node(p1_u, globalPoints.IndexOf(p1_u));

                        SetNodePosition(node, p1_u, cornerNodes, i, j, k, u, v, w);
                        SetNodeSurface(node, i, j, k, u, v, w);
                        nodes.Add(node);
                    }
                }
            }

            // Putting together Hex elements:

            List<int> jumpOne = new List<int>(); // List with points where it must move in v-direction
            List<int> jumpUp = new List<int>(); // List with points where it must move upwards w-direction


            //Finding indexes for jumping in v-direction
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < v - 1; j++)
                {
                    jumpOne.Add((u - 1) + j * (u + 1) + (u + 1) * (v + 1) * i);
                }

            }

            //Finding indexes for jumping in w-direction
            for (int i = 0; i < w; i++)
            {
                jumpUp.Add((u + 1) * (v + 1) - (u + 1) - 2 + (u + 1) * (v + 1) * i);
            }

            int counter = 0;

            for (int i = 0; i < u * v * w; i++) // Creating u*v*w hexahedron having the 8 corner points
            {
                //Putting together 8 points to create hex
                List<Point3d> hex = new List<Point3d>()
                {
                    globalPoints[counter],
                    globalPoints[counter + 1],
                    globalPoints[(u + 1) + (counter + 1)],
                    globalPoints[(u + 1) + (counter)],
                    globalPoints[(u + 1) * (v + 1) + counter],
                    globalPoints[(u + 1) * (v + 1) + (counter + 1)],
                    globalPoints[(u + 1) * (v + 1) + (u + 1) + (counter + 1)],
                    globalPoints[(u + 1) * (v + 1) + (u + 1) + (counter)]

                };

                hexPoints.Add(hex);

                //Putting together 8 nodes to craete hex
                List<Node> hexNodes = new List<Node>()
                {
                    nodes[counter],
                    nodes[counter + 1],
                    nodes[(u + 1) + (counter + 1)],
                    nodes[(u + 1) + (counter)],
                    nodes[(u + 1) * (v + 1) + counter],
                    nodes[(u + 1) * (v + 1) + (counter + 1)],
                    nodes[(u + 1) * (v + 1) + (u + 1) + (counter + 1)],
                    nodes[(u + 1) * (v + 1) + (u + 1) + (counter)],
                };

                //Adding element number to each node which is part of the element
                nodes[counter].AddElementNr(i);
                nodes[counter + 1].AddElementNr(i);
                nodes[(u + 1) + (counter + 1)].AddElementNr(i);
                nodes[(u + 1) + (counter)].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + counter].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (counter + 1)].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (u + 1) + (counter + 1)].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (u + 1) + (counter)].AddElementNr(i);

                List<Line> edgesElement = CreateEdgesMesh(hex);
                List<Brep> surfacesElement = CreateSurfaceMesh(hex);

                edgeMesh.Add(edgesElement);
                surfacesMesh.Add(surfacesElement);

                //Showing the connectivity between local and global nodes
                List<int> connectivity = new List<int>()
                {
                    counter,
                    counter + 1,
                    (u + 1) + (counter + 1),
                    (u + 1) + (counter),
                    (u + 1) * (v + 1) + counter,
                    (u + 1) * (v + 1) + (counter + 1),
                    (u + 1) * (v + 1) + (u + 1) + (counter + 1),
                    (u + 1) * (v + 1) + (u + 1) + (counter),
                };

                Element element = new Element(hexNodes, i, connectivity);
                elements.Add(element);

                globalConnectivity.Add(connectivity);

                //Checking if we need to move to next row
                if (jumpOne.Contains(counter)) counter += 1;


                //Checking if we need to move to next level
                if (jumpUp.Contains(counter)) counter += (u + 2);

                counter++;
            }

            return (hexPoints, globalConnectivity, edgeMesh, surfacesMesh, nodes, elements, globalPoints);
        }

        public void SetNodePosition(Node node, Point3d p, Point3d[] cornerPoints, int i, int j, int k, int u, int v, int w)
        {
            p = new Point3d(Math.Round(p.X, 1), Math.Round(p.Y, 1), Math.Round(p.Z, 1));

            if (cornerPoints.Contains(p)) node.SetIsCorner();
            else if (i == 0 && j == 0 || i == 0 && k == 0 || j == 0 && k == 0 || i == w && j == 0 || i == 0 && k == u || j == v && k == 0 || i == 0 && j == v || i == w && k == 0 || j == 0 && k == u || i == 0 && j == 0 || i == 0 && k == 0 || j == 0 && k == 0 || i == w && j == v || i == w && k == u || j == v && k == u) node.SetIsEdge();
            else node.SetIsMiddle();
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
         
            edges[0].SetStartPoint(new Point3d(Math.Round(edges[0].PointAtStart.X, 1), Math.Round(edges[0].PointAtStart.Y, 1), Math.Round(edges[0].PointAtStart.Z, 1))); ;
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

        public Point3d[] RoundPoints(Point3d[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Point3d(Math.Round(vertices[i].X, 1), Math.Round(vertices[i].Y, 1), Math.Round(vertices[i].Z, 1));
            }

            return vertices;
        }

        public List<Point3d> RoundPointsList(List<Point3d> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = new Point3d(Math.Round(vertices[i].X, 1), Math.Round(vertices[i].Y, 1), Math.Round(vertices[i].Z, 1));
            }

            return vertices;
        }

        public Curve[] RoundEdgePoints(Curve[] sortedEdges)
        {
            for (int i = 0; i < sortedEdges.Length; i++)
            {
                sortedEdges[i].SetStartPoint(new Point3d(Math.Round(sortedEdges[i].PointAtStart.X, 1), Math.Round(sortedEdges[i].PointAtStart.Y, 1), Math.Round(sortedEdges[i].PointAtStart.Z, 1)));
                sortedEdges[i].SetEndPoint(new Point3d(Math.Round(sortedEdges[i].PointAtEnd.X, 1), Math.Round(sortedEdges[i].PointAtEnd.Y, 1), Math.Round(sortedEdges[i].PointAtEnd.Z, 1)));
            }
            return sortedEdges;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.meshC;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("0f4702ea-a195-4b83-b3ae-5e067f56a73f"); }
        }
    }
}