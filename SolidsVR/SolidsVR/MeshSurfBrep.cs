using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;


namespace SolidsVR
{
    public class MeshSurfBrep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshSurfBrep class.
        /// </summary>
        public MeshSurfBrep()
          : base("Meshing brep", "Mesh crazy geometry",
              "Description",
              "Category3", "Mesh")
        {
        }

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

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of Brep", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve", "Curve", "Mesh of Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("Surface", "Surface", "Mesh of Brep", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            {
                // Define local variables to catch the incoming data from Grasshopper
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

                List<Surface> surfaces = new List<Surface>();

                foreach (Surface surf in brp.Surfaces)
                {
                    surfaces.Add(surf);
                }

                Surface[] sortedSurfaces = SortedSurfaces(corners, surfaces);

                Curve[] edges = brp.DuplicateEdgeCurves();
                Curve[] sortedEdges = SortEdges(corners, edges); //Index 3 and 7 gives null !!!

                var tuple = CreateNewBreps(brp, u, v, w, sortedEdges, sortedSurfaces);

                List<List<Point3d>> elementPoints = tuple.Item1;
                List<List<int>> connectivity = tuple.Item2;
                List<List<Line>> edgeMesh = tuple.Item3;
                List<List<Brep>> surfacesMesh = tuple.Item4;
                List<Node> nodes = tuple.Item5;
                List<Element> elements = tuple.Item6;
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
                mesh.SetElements(elements);
                mesh.SetOrigBrep(brp);
                mesh.SetBrep(new Brep_class(brp));
                mesh.OrderSurfaces(corners);


                //---output---

                DA.SetData(0, mesh);
                DA.SetDataList(1, sortedEdges);
                DA.SetDataList(2, sortedSurfaces);

            }
        }

        public Tuple<List<List<Point3d>>, List<List<int>>, List<List<Line>>, List<List<Brep>>, List<Node>, List<Element>> CreateNewBreps(Brep brep, int u, int v, int w, Curve[] edges, Surface [] oSurfaces)
        {
            List<Point3d> points = new List<Point3d>();
            List<List<int>> global_numbering = new List<List<int>>();
            List<List<Point3d>> points_brep = new List<List<Point3d>>();
            List<List<Line>> edgeMesh = new List<List<Line>>();
            List<List<Brep>> surfacesMesh = new List<List<Brep>>();

            List<Node> nodes = new List<Node>();
            List<Element> elements = new List<Element>();

            Point3d[] cornerPoints = brep.DuplicateVertices();

            Curve edge1 = edges[8];
            Curve edge2 = edges[9];
            Curve edge3 = edges[10];
            Curve edge4 = edges[11];

            /*
            edge1.Reverse();
            edge2.Reverse();
            edge3.Reverse();
            edge4.Reverse();
            */


            edge1.DivideByCount(w, true, out Point3d[] p1s);
            edge2.DivideByCount(w, true, out Point3d[] p2s);
            edge3.DivideByCount(w, true, out Point3d[] p3s);
            edge4.DivideByCount(w, true, out Point3d[] p4s);

            Surface surF1 = oSurfaces[0];
            Surface surF2 = oSurfaces[1];
            Surface surF3 = oSurfaces[2];
            Surface surF4 = oSurfaces[3];

            /*
            BrepFace face1 = brep.Faces[1];

            Surface surF1 = face1.DuplicateSurface();
            BrepFace face2 = brep.Faces[4];
            Surface surF2 = face2.DuplicateSurface();
            BrepFace face3 = brep.Faces[3];
            Surface surF3 = face3.DuplicateSurface();
            BrepFace face4 = brep.Faces[2];
            Surface surF4 = face4.DuplicateSurface();
            */

            List<NurbsCurve> curve = new List<NurbsCurve>();


            Interval dW = surF1.Domain(0);

            for (int i = 0; i <= w; i++)
            {
                double tw = dW.ParameterAt(i / (double)w);

                Point3d p_1 = p1s[i];
                Point3d p_2 = p2s[i];
                Point3d p_3 = p3s[i];
                Point3d p_4 = p4s[i];

                List<Point3d> ps1 = new List<Point3d> { p_1, p_2 };
                List<Point3d> ps2 = new List<Point3d> { p_2, p_3 };
                List<Point3d> ps3 = new List<Point3d> { p_3, p_4 };
                List<Point3d> ps4 = new List<Point3d> { p_4, p_1 };

                NurbsCurve c1 = surF1.InterpolatedCurveOnSurface(ps1, 0);
                NurbsCurve c2 = surF2.InterpolatedCurveOnSurface(ps2, 0);
                NurbsCurve c3 = surF3.InterpolatedCurveOnSurface(ps3, 0);
                NurbsCurve c4 = surF4.InterpolatedCurveOnSurface(ps4, 0);

                //c1.Reverse();
                //c2.Reverse();
                //c4.Reverse();



                curve = new List<NurbsCurve>() { c1, c2, c3, c4 };
                Brep brepSurf = Brep.CreateEdgeSurface(curve);

                Surface surface = null;


                foreach (BrepFace surf in brepSurf.Faces)
                {
                    surface = surf.DuplicateSurface();
                }

                List<Point3d> pointList = new List<Point3d>() { p_1, p_2, p_3, p_4 };

                Brep b = surface.ToBrep();
                Point3d[] vertices = b.DuplicateVertices();

                var tuple = CreatePoints(surface, u, v, w, i, cornerPoints, pointList);

                points.AddRange(tuple.Item1);
                nodes.AddRange(tuple.Item2);
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

                List<Node> elementNodes = new List<Node>();

                //Putting together the 8 points to make the brep
                elementNodes.Add(nodes[index]);
                elementNodes.Add(nodes[index + 1]);
                elementNodes.Add(nodes[(u + 1) + (index + 1)]);
                elementNodes.Add(nodes[(u + 1) + (index)]);
                elementNodes.Add(nodes[(u + 1) * (v + 1) + index]);
                elementNodes.Add(nodes[(u + 1) * (v + 1) + (index + 1)]);
                elementNodes.Add(nodes[(u + 1) * (v + 1) + (u + 1) + (index + 1)]);
                elementNodes.Add(nodes[(u + 1) * (v + 1) + (u + 1) + (index)]);

                nodes[index].AddElementNr(i);
                nodes[index + 1].AddElementNr(i);
                nodes[(u + 1) + (index + 1)].AddElementNr(i);
                nodes[(u + 1) + (index)].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + index].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (index + 1)].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (u + 1) + (index + 1)].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (u + 1) + (index)].AddElementNr(i);




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

                Element element = new Element(elementNodes, i, connectivity);

                elements.Add(element);

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
            
            return Tuple.Create(points_brep, global_numbering, edgeMesh, surfacesMesh, nodes, elements);
            //return points;
        }

        public Tuple<List<Point3d>, List<Node>> CreatePoints(Surface surface, int u, int v, int w, int i, Point3d[] cornerNodes, List<Point3d> pointList)
        {
            Interval domainU = surface.Domain(0);
            Interval domainV = surface.Domain(1);

            List<Node> nodes = new List<Node>();

            


            if (domainU[1] == 0) domainU.Swap();
            if (domainV[1] == 0) domainV.Swap();
         
            double tu0 = domainU.ParameterAt(0);
            double tu1 = domainU.ParameterAt(1);
            double tv0 = domainV.ParameterAt(0);
            double tv1 = domainV.ParameterAt(1);

            Point3d p_temp1 = surface.PointAt(tu0, tv0);
            Point3d p_temp2 = surface.PointAt(tu0, tv1);
            Point3d p_temp3 = surface.PointAt(tu1, tv0);
            Point3d p_temp4 = surface.PointAt(tu1, tv1);

            Boolean reverse = false;

            if (Math.Round(p_temp2.X,4) == Math.Round(pointList[3].X,4) && (Math.Round(p_temp2.Y, 4) == Math.Round(pointList[3].Y, 4)) && (Math.Round(p_temp2.Z, 4) == Math.Round(pointList[3].Z, 4))) reverse = true;

            List<Point3d> points = new List<Point3d>();
            List<Vector3d> vectors = new List<Vector3d>();

            if(reverse == false)
            {
                for (int j = 0; j <= v; j++)
                {
                    double tu = domainU.ParameterAt(j / (double)v);
                    for (int k = 0; k <= u; k++)
                    {
                        double tv = domainV.ParameterAt(k / (double)u);

                        Point3d p1 = surface.PointAt(tu, tv);

                        points.Add(p1);

                        Node node = new Node(p1, points.IndexOf(p1));

                        SetNodePosition(node, p1, cornerNodes, i, j, k, u, v, w);
                        SetNodeSurface(node, i, j, k, u, v, w);
                        nodes.Add(node);

                    }
                }
            }

            else
            {
                for (int j = 0; j <= v; j++)
                {
                    double tv = domainV.ParameterAt(j / (double)v);
                    for (int k = 0; k <= u; k++)
                    {
                        double tu = domainU.ParameterAt(k / (double)u);

                        Point3d p1 = surface.PointAt(tu, tv);

                        points.Add(p1);

                        Node node = new Node(p1, points.IndexOf(p1));

                        SetNodePosition(node, p1, cornerNodes, i, j, k, u, v, w);
                        SetNodeSurface(node, i, j, k, u, v, w);
                        nodes.Add(node);

                    }
                }
            }

            

            return Tuple.Create(points, nodes);
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
            else if (i == 0 && j == 0 || i == 0 && k == 0 || j == 0 && k == 0 || i == w && j == 0 || i == 0 && k == u || j == v && k == 0 || i == 0 && j == v || i == w && k == 0 || j == 0 && k == u || i == 0 && j == 0 || i == 0 && k == 0 || j == 0 && k == 0 || i == w && j == v || i == w && k == u || j == v && k == u) node.SetIsEdge();
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

        public Surface[] SortedSurfaces(List<Point3d> corners, List<Surface> surfaces)
        {
            Surface[] sortedSurfaces = new Surface[6];

            Point3d[] surf0 = new Point3d[] { corners[0], corners[1], corners[4], corners[5] };
            Point3d[] surf1 = new Point3d[] { corners[1], corners[2], corners[5], corners[6] };
            Point3d[] surf2 = new Point3d[] { corners[2], corners[3], corners[6], corners[7] };
            Point3d[] surf3 = new Point3d[] { corners[0], corners[3], corners[4], corners[7] };
            Point3d[] surf4 = new Point3d[] { corners[0], corners[1], corners[2], corners[3] };
            Point3d[] surf5 = new Point3d[] { corners[4], corners[5], corners[6], corners[7] };

            for (int i = 0; i < surfaces.Count; i++)
            {
                Brep surf = surfaces[i].ToBrep();
                Point3d[] cornerSurf = surf.DuplicateVertices();

                if (cornerSurf.All(surf0.Contains)) sortedSurfaces[0] = surfaces[i];
                if (cornerSurf.All(surf1.Contains)) sortedSurfaces[1] = surfaces[i];
                if (cornerSurf.All(surf2.Contains)) sortedSurfaces[2] = surfaces[i];
                if (cornerSurf.All(surf3.Contains)) sortedSurfaces[3] = surfaces[i];
                if (cornerSurf.All(surf4.Contains)) sortedSurfaces[4] = surfaces[i];
                if (cornerSurf.All(surf5.Contains)) sortedSurfaces[5] = surfaces[i];
            }


            return sortedSurfaces;
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
                else
                {
                    sortedEdges[11] = edges[i];
                    if (edges[i].PointAtEnd == corners[3]) { sortedEdges[11].Reverse(); }
                }
            }
            return sortedEdges;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1fd92232-70a8-4fc2-b2c0-144195bc7e29"); }
        }
    }
}