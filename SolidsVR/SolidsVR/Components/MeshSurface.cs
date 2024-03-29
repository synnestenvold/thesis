﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry.Collections;
using Rhino.Geometry;
using System.Linq;


namespace SolidsVR
{
    public class MeshSurface : GH_Component
    {
        public MeshSurface()
          : base("MeshSurface", "MeshS",
              "Mesh arbitrary surface",
              "SolidsVR", "Mesh")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Geometry", "G", "Input geometry as a curved brep", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Corner points in right order", GH_ParamAccess.list);
            pManager.AddIntegerParameter("U count", "u", "Number of divisions in U direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("V count", "v", "Number of divisions in V direction", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("W count", "w", "Number of divisions in W direction", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Volume to remove", "V", "Volume percentage to remove", GH_ParamAccess.item, 0);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of Brep", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            {
                //NOTE: This meshing class can mesh any type of geometry as long as it consist of 8 corners and no holes.
                //This is the meshing component for CASE 3.

                // ---variables-- -

                Brep brp = new Brep();
                List<Point3d> corners = new List<Point3d>();
                int u = 1;
                int v = 1;
                int w = 1;
                double removeVolume = 0;

                // --- input ---

                if (!DA.GetData(0, ref brp)) return;
                if (!DA.GetDataList(1, corners)) return;
                if (!DA.GetData(2, ref u)) return;
                if (!DA.GetData(3, ref v)) return;
                if (!DA.GetData(4, ref w)) return;
                if (!DA.GetData(5, ref removeVolume)) return;

                Curve[] edges = brp.DuplicateEdgeCurves();
                Curve[] sortedEdges = SortEdges(corners, edges);

                Surface[] surfaces = CreateSortedSurfaces(brp, edges, corners);

                var tuple = CreateMesh(brp, u, v, w, sortedEdges, surfaces);

                List<List<Point3d>> elementPoints = tuple.Item1;
                List<List<int>> connectivity = tuple.Item2;
                List<List<Line>> edgeMesh = tuple.Item3;
                List<List<Brep>> surfacesMesh = tuple.Item4;
                List<Node> nodes = tuple.Item5;
                List<Element> elements = tuple.Item6;
                List<Point3d> globalPoints = tuple.Item7;
                int sizeOfMatrix = 3 * (u + 1) * (v + 1) * (w + 1);
                

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
        }

        public Curve[] SortEdges(List<Point3d> corners, Curve[] edges)
        {
            corners = RoundPointsList(corners);
            Curve[] sortedEdges = new Curve[12];
            for (int i = 0; i < edges.Length; i++)
            {
                Point3d start = new Point3d(Math.Round(edges[i].PointAtStart.X, 1), Math.Round(edges[i].PointAtStart.Y, 1), Math.Round(edges[i].PointAtStart.Z, 1));
                Point3d end = new Point3d(Math.Round(edges[i].PointAtEnd.X, 1), Math.Round(edges[i].PointAtEnd.Y, 1), Math.Round(edges[i].PointAtEnd.Z, 1));
                List<Point3d> tempP = new List<Point3d>() { start, end };
                //u-dir
                if (tempP.Contains(corners[0]) && tempP.Contains(corners[1]))
                {
                    sortedEdges[0] = edges[i];
                    if (end == corners[0]) { sortedEdges[0].Reverse(); }
                }
                else if (tempP.Contains(corners[3]) && tempP.Contains(corners[2]))
                {
                    sortedEdges[1] = edges[i];
                    if (end == corners[3]) { sortedEdges[1].Reverse(); }
                }
                else if (tempP.Contains(corners[4]) && tempP.Contains(corners[5]))
                {
                    sortedEdges[2] = edges[i];
                    if (end == corners[4]) { sortedEdges[2].Reverse(); }
                }
                else if (tempP.Contains(corners[7]) && tempP.Contains(corners[6]))
                {
                    sortedEdges[3] = edges[i];
                    if (end == corners[7]) { sortedEdges[3].Reverse(); }
                }
                //v-dir
                else if (tempP.Contains(corners[0]) && tempP.Contains(corners[3]))
                {
                    sortedEdges[4] = edges[i];
                    if (end == corners[0]) { sortedEdges[4].Reverse(); }
                }
                else if (tempP.Contains(corners[1]) && tempP.Contains(corners[2]))
                {
                    sortedEdges[5] = edges[i];
                    if (end == corners[1]) { sortedEdges[5].Reverse(); }
                }
                else if (tempP.Contains(corners[5]) && tempP.Contains(corners[6]))
                {
                    sortedEdges[6] = edges[i];
                    if (end == corners[5]) { sortedEdges[6].Reverse(); }
                }
                else if (tempP.Contains(corners[4]) && tempP.Contains(corners[7]))
                {
                    sortedEdges[7] = edges[i];
                    if (end == corners[4]) { sortedEdges[7].Reverse(); }
                }
                //w-dir
                else if (tempP.Contains(corners[0]) && tempP.Contains(corners[4]))
                {
                    sortedEdges[8] = edges[i];
                    if (end == corners[0]) { sortedEdges[8].Reverse(); }
                }
                else if (tempP.Contains(corners[1]) && tempP.Contains(corners[5]))
                {
                    sortedEdges[9] = edges[i];
                    if (end == corners[1]) { sortedEdges[9].Reverse(); }
                }
                else if (tempP.Contains(corners[2]) && tempP.Contains(corners[6]))
                {
                    sortedEdges[10] = edges[i];
                    if (end == corners[2]) { sortedEdges[10].Reverse(); }
                }
                else
                {
                    sortedEdges[11] = edges[i];
                    if (end == corners[3]) { sortedEdges[11].Reverse(); }
                }
            }
            return sortedEdges;
        }

        public Surface[] CreateSortedSurfaces (Brep brp, Curve[] sortedEdges, List<Point3d> corners)
        {
            Surface[] surfaces = new Surface[6];

            BrepFaceList faces = brp.Faces;

            surfaces[0] = faces[0].DuplicateSurface();
            surfaces[1] = faces[1].DuplicateSurface();
            surfaces[2] = faces[2].DuplicateSurface();
            surfaces[3] = faces[3].DuplicateSurface();
            surfaces[4] = faces[4].DuplicateSurface();
            surfaces[5] = faces[5].DuplicateSurface();

            surfaces = FindSurfaces(corners, surfaces);

            return surfaces;
        }

        public Surface[] FindSurfaces(List<Point3d> corners, Surface[] surfaces)
        {
            Surface[] sortedSurfaces = new Surface[6];

            corners = RoundPointsList(corners);

            Point3d[] surf0 = new Point3d[] { corners[0], corners[1], corners[4], corners[5] };
            Point3d[] surf1 = new Point3d[] { corners[1], corners[2], corners[5], corners[6] };
            Point3d[] surf2 = new Point3d[] { corners[2], corners[3], corners[6], corners[7] };
            Point3d[] surf3 = new Point3d[] { corners[0], corners[3], corners[4], corners[7] };

            for (int i = 0; i < surfaces.Length; i++)
            {
                Brep surf = surfaces[i].ToBrep();
                Point3d[] cornerSurf = surf.DuplicateVertices();

                cornerSurf = RoundPoints(cornerSurf);

                if (cornerSurf.All(surf0.Contains)) sortedSurfaces[0] = surfaces[i];
                if (cornerSurf.All(surf1.Contains)) sortedSurfaces[1] = surfaces[i];
                if (cornerSurf.All(surf2.Contains)) sortedSurfaces[2] = surfaces[i];
                if (cornerSurf.All(surf3.Contains)) sortedSurfaces[3] = surfaces[i];

                
            }

            return sortedSurfaces;
        }

        public (List<List<Point3d>>, List<List<int>>, List<List<Line>>, List<List<Brep>>, List<Node>, List<Element>, List<Point3d>) CreateMesh(Brep brep, int u, int v, int w, Curve[] edges, Surface [] oSurfaces)
        {
            List<Point3d> globalPoints = new List<Point3d>();
            List<List<int>> globalConnectivity = new List<List<int>>();
            List<List<Point3d>> hexPoints = new List<List<Point3d>>();
            List<List<Line>> edgeMesh = new List<List<Line>>();
            List<List<Brep>> surfacesMesh = new List<List<Brep>>();

            List<Node> nodes = new List<Node>();
            List<Element> elements = new List<Element>();

            Point3d[] cornerPoints = brep.DuplicateVertices();

            Curve edge1 = edges[8];
            Curve edge2 = edges[9];
            Curve edge3 = edges[10];
            Curve edge4 = edges[11];

            edge1.DivideByCount(w, true, out Point3d[] p1w);
            edge2.DivideByCount(w, true, out Point3d[] p2w);
            edge3.DivideByCount(w, true, out Point3d[] p3w);
            edge4.DivideByCount(w, true, out Point3d[] p4w);
            
            Surface surF1 = oSurfaces[0];
            Surface surF2 = oSurfaces[1];
            Surface surF3 = oSurfaces[2];
            Surface surF4 = oSurfaces[3];


            List<NurbsCurve> curve = new List<NurbsCurve>();
            List<NurbsCurve> curves = new List<NurbsCurve>();


            for (int i = 0; i <= w; i++)
            {
                Point3d p_1 = p1w[i];
                Point3d p_2 = p2w[i];
                Point3d p_3 = p3w[i];
                Point3d p_4 = p4w[i];

                List<Point3d> ps1 = new List<Point3d> { p_1, p_2 };
                List<Point3d> ps2 = new List<Point3d> { p_2, p_3 };
                List<Point3d> ps3 = new List<Point3d> { p_3, p_4 };
                List<Point3d> ps4 = new List<Point3d> { p_4, p_1 };

                NurbsCurve c1 = surF1.InterpolatedCurveOnSurface(ps1, 0);
                NurbsCurve c2 = surF2.InterpolatedCurveOnSurface(ps2, 0);
                NurbsCurve c3 = surF3.InterpolatedCurveOnSurface(ps3, 0);
                NurbsCurve c4 = surF4.InterpolatedCurveOnSurface(ps4, 0);

                curve = new List<NurbsCurve>() { c1, c2, c3, c4 };

                Brep brepSurf = Brep.CreateEdgeSurface(curve);

                Surface surface = brepSurf.Faces[0].DuplicateSurface();

                List<Point3d> pointList = new List<Point3d>() { p_1, p_2, p_3, p_4 };

                (globalPoints, nodes) = CreatePoints(surface, u, v, w, i, cornerPoints, pointList, globalPoints, nodes);
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
                    globalPoints[(u + 1) + (counter)],
                    globalPoints[(u + 1) + (counter + 1)],
                    globalPoints[counter + 1],
                    globalPoints[(u + 1) * (v + 1) + counter],
                    globalPoints[(u + 1) * (v + 1) + (u + 1) + (counter)],
                    globalPoints[(u + 1) * (v + 1) + (u + 1) + (counter + 1)],
                    globalPoints[(u + 1) * (v + 1) + (counter + 1)],

                };

                hexPoints.Add(hex);

                //Putting together 8 nodes to craete hex
                List<Node> hexNodes = new List<Node>()
                {
                    nodes[counter],
                    nodes[(u + 1) + (counter)],
                    nodes[(u + 1) + (counter + 1)],
                    nodes[counter + 1],
                    nodes[(u + 1) * (v + 1) + counter],
                    nodes[(u + 1) * (v + 1) + (u + 1) + (counter)],
                    nodes[(u + 1) * (v + 1) + (u + 1) + (counter + 1)],
                    nodes[(u + 1) * (v + 1) + (counter + 1)],
                };

                //Adding element number to each node which is part of the element
                nodes[counter].AddElementNr(i);
                nodes[(u + 1) + (counter)].AddElementNr(i);
                nodes[(u + 1) + (counter + 1)].AddElementNr(i);
                nodes[counter + 1].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + counter].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (u + 1) + (counter)].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (u + 1) + (counter + 1)].AddElementNr(i);
                nodes[(u + 1) * (v + 1) + (counter + 1)].AddElementNr(i);

                List<Line> edgesElement = CreateEdgesMesh(hex);
                List<Brep> surfacesElement = CreateSurfaceMesh(hex);

                edgeMesh.Add(edgesElement);
                surfacesMesh.Add(surfacesElement);

                //Showing the connectivity between local and global nodes
                List<int> connectivity = new List<int>()
                {
                    counter,
                    (u + 1) + (counter),
                    (u + 1) + (counter + 1),
                    counter + 1,
                    (u + 1) * (v + 1) + counter,
                    (u + 1) * (v + 1) + (u + 1) + (counter),
                    (u + 1) * (v + 1) + (u + 1) + (counter + 1),
                    (u + 1) * (v + 1) + (counter + 1),
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

        public (List<Point3d>, List<Node>) CreatePoints(Surface surface, int u, int v, int w, int i, Point3d[] cornerNodes, List<Point3d> pointList, List<Point3d> points, List<Node> nodes)
        {
            //Domain of surface
            Interval domainU = surface.Domain(0);
            Interval domainV = surface.Domain(1);

            double tu0 = domainU.ParameterAt(0);
            double tu1 = domainU.ParameterAt(1);
            double tv0 = domainV.ParameterAt(0);
            double tv1 = domainV.ParameterAt(1);

            List<Point3d> tempPoints = new List<Point3d>()
            {
                surface.PointAt(tu0, tv0),
                surface.PointAt(tu0, tv1),
                surface.PointAt(tu1, tv0),
                surface.PointAt(tu1, tv1),
            };

            pointList = RoundPointsList(pointList);
            tempPoints = RoundPointsList(tempPoints);

            Interval point1 = new Interval(0, 0);
            Interval point2 = new Interval(0, 0);
            Interval point3 = new Interval(0, 0);

            //Finding new domain starting in correct point.
            for (int k = 0; k < tempPoints.Count; k++)
            {
                double test = tempPoints[k].DistanceTo(pointList[0]);
                if (tempPoints[k].DistanceTo(pointList[0]) < 0.001)
                {
                    if (k == 0) point1 = new Interval(tu0, tv0);
                    if (k == 1) point1 = new Interval(tu0, tv1);
                    if (k == 2) point1 = new Interval(tu1, tv0);
                    if (k == 3) point1 = new Interval(tu1, tv1);

                }
                double test2 = tempPoints[k].DistanceTo(pointList[1]);
                if (tempPoints[k].DistanceTo(pointList[1]) < 0.001)
                {
                    if (k == 0) point2 = new Interval(tu0, tv0);
                    if (k == 1) point2 = new Interval(tu0, tv1);
                    if (k == 2) point2 = new Interval(tu1, tv0);
                    if (k == 3) point2 = new Interval(tu1, tv1);
                }

                double test3 = tempPoints[k].DistanceTo(pointList[3]);

                if (tempPoints[k].DistanceTo(pointList[3]) < 0.001)
                {
                    if (k == 0) point3 = new Interval(tu0, tv0);
                    if (k == 1) point3 = new Interval(tu0, tv1);
                    if (k == 2) point3 = new Interval(tu1, tv0);
                    if (k == 3) point3 = new Interval(tu1, tv1);
                }
            }

            double d11 = point1.ParameterAt(0);
            double d12 = point1.ParameterAt(1);

            double d21 = point2.ParameterAt(0);
            double d22 = point2.ParameterAt(1);

            double d31 = point3.ParameterAt(0);
            double d32 = point3.ParameterAt(1);

            double tu = 0;
            double tv = 0;

            //Populating surface with points
            for (int j = 0; j <= v; j++)
            {
                if ((d31 - d11) != 0) tu = (double)(d11 - j * (d11 - d31) / v);
                else tv = (double)(d12 - j * (d12 - d32) / v);

                for (int k = 0; k <= u; k++)
                {
                    if ((d21 - d11) != 0) tu = (double)(d11 - k * (d11 - d21) / u);
                    else tv = (double)(d12 - k * (d12 - d22) / u);

                    Point3d p1 = surface.PointAt(tu, tv);
                    points.Add(p1);

                    Node node = new Node(p1, points.IndexOf(p1));

                    SetNodePosition(node, p1, cornerNodes, i, j, k, u, v, w);
                    SetNodeSurface(node, i, j, k, u, v, w);
                    nodes.Add(node);
                }
            }

            return (points, nodes);
        }

        public void SetNodePosition(Node node, Point3d p, Point3d[] cornerPoints, int i, int j, int k, int u, int v, int w)
        {
            p = new Point3d(Math.Round(p.X, 1), Math.Round(p.Y, 1), Math.Round(p.Z, 1));

            cornerPoints = RoundPoints(cornerPoints);

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

        public Curve[] RoundEdgePoints(Curve[] sortedEdges)
        {
            for (int i = 0; i < sortedEdges.Length; i++)
            {
                sortedEdges[i].SetStartPoint(new Point3d(Math.Round(sortedEdges[i].PointAtStart.X, 1), Math.Round(sortedEdges[i].PointAtStart.Y, 1), Math.Round(sortedEdges[i].PointAtStart.Z, 1)));
                sortedEdges[i].SetEndPoint(new Point3d(Math.Round(sortedEdges[i].PointAtEnd.X, 1), Math.Round(sortedEdges[i].PointAtEnd.Y, 1), Math.Round(sortedEdges[i].PointAtEnd.Z, 1)));
            }
            return sortedEdges;
        }

        
        public List<Point3d> RoundPointsList(List<Point3d> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = new Point3d(Math.Round(vertices[i].X, 1), Math.Round(vertices[i].Y, 1), Math.Round(vertices[i].Z, 1));
            }

            return vertices;
        }

        public Point3d[] RoundPoints(Point3d[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Point3d(Math.Round(vertices[i].X, 1), Math.Round(vertices[i].Y, 1), Math.Round(vertices[i].Z, 1));
            }

            return vertices;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return SolidsVR.Properties.Resource1.meshS;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("1fd92232-70a8-4fc2-b2c0-144195bc7e29"); }
        }
    }
}