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



    }
}
