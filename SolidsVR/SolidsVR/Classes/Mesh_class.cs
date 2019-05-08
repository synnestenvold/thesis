using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;

namespace SolidsVR
{
    class Mesh_class
    {
        private int u = 1;
        private int v = 1;
        private int w = 1;

        List<List<Point3d>> elementPoints = new List<List<Point3d>>();
        List<List<int>> connectivity = new List<List<int>>();
        List<List<Line>> edgesMesh = new List<List<Line>>();
        List<List<Brep>> surfacesMesh = new List<List<Brep>>();
        List<Node> nodes = new List<Node>();
        Brep_class brp = new Brep_class();
        Brep origBrep = new Brep();
        List<Brep> surfaces = new List<Brep>();
        List<Element> elements = new List<Element>();

        Point3d[] globalPoints = null;
        int sizeOfMatrix = 0;

        public Mesh_class(int _u, int _v, int _w)
        {
            u = _u;
            v = _v;
            w = _w;
        }
        public Mesh_class() { }

        public int getU()
        {
            return u;
        }

        public int getV()
        {
            return v;
        }

        public int getW()
        {
            return w;
        }

        public List<List<Point3d>> GetElementPoints()
        {
            return elementPoints;
        }

        public void SetElementPoints(List<List<Point3d>> _elementPoints)
        {
            elementPoints = _elementPoints;
        }

        public List<List<int>> GetConnectivity()
        {
            return connectivity;
        }

        public void SetConnectivity(List<List<int>> _connectivity)
        {
            connectivity = _connectivity;
        }

   
        public Point3d[] GetGlobalPoints()
        {
            return globalPoints;
        }

        public void SetGlobalPoints(Point3d[] _globalPoints)
        {
            globalPoints = _globalPoints;
        }

        public int GetSizeOfMatrix()
        {
            return sizeOfMatrix;
        }

        public void SetSizeOfMatrix(int _sizeOfMatrix)
        {
            sizeOfMatrix = _sizeOfMatrix;
        }

        public void SetEdgesMesh(List<List<Line>> edges)
        {
            edgesMesh = edges;
        }

        public List<List<Line>> GetEdges()
        {
            return edgesMesh;
        }

        public void SetSurfacesMesh(List<List<Brep>> surfaces)
        {
            surfacesMesh = surfaces;
        }

        public List<List<Brep>> GetSurfaces()
        {
            return surfacesMesh;
        }

        public void SetNodeList(List<Node> _nodes)
        {
            nodes = _nodes;
        }

        public List<Node> GetNodeList()
        {
            return nodes;
        }

        public Brep_class GetBrep()
        {
            return brp;
        }

        public void SetBrep(Brep_class _brp)
        {
            brp = _brp;
        }

        public void SetOrigBrep(Brep _origBrep)
        {
            origBrep = _origBrep;
        }
        public Brep GetOrigBrep()
        {
            return origBrep;
        }
        public List<Brep> GetOrderedSurfaces()
        {
            return surfaces;
        }
        public Brep GetSurfaceAsBrep(int n)
        {
            return surfaces[n];
        }

        public void SetElements(List<Element> _elements)
        {
            elements = _elements;
        }

        public List<Element> GetElements()
        {
            return elements;
        }

        public void RemoveElements()
        {
            elements.RemoveAt(40);
            elements.RemoveAt(40);
            elements.RemoveAt(40);
            elements.RemoveAt(40);
            elements.RemoveAt(40);
            elements.RemoveAt(40);
            elements.RemoveAt(40);
            elements.RemoveAt(40);
            elements.RemoveAt(40);
            elements.RemoveAt(40);
        }

        public Tuple<double, int> RemoveOneElement()
        {
            double max = double.NegativeInfinity;
            double min = double.PositiveInfinity;
            int minElem = -1;
            for (int i=0; i<elements.Count; i++)
            {
                double mises = elements[i].GetAverageStressDir(6);
                if (mises < min)
                {
                    min = mises;
                    minElem = i;
                }
                if (mises >= max)
                {
                    max = mises;
                }
            }
            //if (minElem != -1 || max < 355)
            //{
            //    elements.RemoveAt(minElem);
            //}
            return Tuple.Create(max, minElem);
        }

        public void OrderSurfaces(List<Point3d> orderedPoints)
        {
            Brep[] orderedSurfaces = new Brep[6];
            //List<Brep> surfaces = new List<Brep>();
            List<int> surfaceNumber = new List<int>();

            foreach (BrepFace surf in origBrep.Faces)
            {
                Brep faceBrep = surf.DuplicateFace(true);
               
                Point3d[] vertices = faceBrep.DuplicateVertices();

                vertices = RoundPoints(vertices);

                int[] nodeIndex = new int[4];
                for (int i = 0; i < vertices.Length; i++)
                {
                    for (int j = 0; j < orderedPoints.Count; j++)
                    {
                        if (vertices[i] == orderedPoints[j])
                        {
                            nodeIndex[i] = j;
                        }
                    }
                }
                Array.Sort(nodeIndex);

                int number = 4;
                if (nodeIndex[0] == 0)
                {
                    if (nodeIndex[1] == 1)
                    {
                        if (nodeIndex[2] == 4)
                        {
                            number = 0;
                        }
                    }
                    else if (nodeIndex[1] == 3)
                    {
                        number = 3;
                    }
                    
                }
                else if (nodeIndex[0] == 1)
                {
                    if (nodeIndex[1] == 2)
                    {
                            number = 1;
                    }
                }
                else if (nodeIndex[0] == 2)
                {
                    number = 2;
                }
                else
                {
                    number = 5;
                }
                
                orderedSurfaces[number] = faceBrep;
            }
            surfaces = orderedSurfaces.ToList();
        }


        public Point3d[] RoundPoints(Point3d[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Point3d(Math.Round(vertices[i].X, 1), Math.Round(vertices[i].Y, 1), Math.Round(vertices[i].Z, 1));
            }

            return vertices;
        }

    }
}
