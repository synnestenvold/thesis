using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;

namespace SolidsVR
{
    public class Node
    {
        Point3d coordinate;
        int nodeNr = 0;
        List<int> partOfElement = new List<int>();
        List<int> surfaceNum = new List<int>();
        Boolean isCorner = false;
        Boolean isMiddle = false;
        Boolean isEdge = false;
        List<double> strain = new List<double>();
        double stress = 0;
        Dictionary<int, int> elementNode = new Dictionary<int, int>();


        public Node (Point3d _coordinate, int _nodeNr)
        {
            coordinate = _coordinate;
            nodeNr = _nodeNr;
        }

        public void SetSurfaceNum(int i)
        {
            surfaceNum.Add(i);
        }

        public List<int> GetSurfaceNum()
        {
            return surfaceNum;
        }

        public void SetIsCorner()
        {
            isCorner = true;
        }

        public Boolean GetIsCorner()
        {
            return isCorner;
        }

        public void SetIsMiddle()
        {
            isMiddle = true;
        }

        public Boolean GetIsMiddle()
        {
            return isMiddle;
        }

        public void SetIsEdge()
        {
            isEdge = true;
        }

        public Boolean GetIsEdge()
        {
            return isEdge;
        }

        public void SetNewCoord(Point3d _coord)
        {
            coordinate = _coord;

        }

        public Point3d GetCoord()
        {
            return coordinate;
        }

        public void SetStrain(double _strain)
        {
            strain.Add(_strain);
        }

        public double GetStrain()
        {
            return strain.Average();
        }

        public void SetStress(double _stress)
        {
            stress = _stress;
        }

        public double GetStress()
        {
            return stress;
        }

        public void AddElementNr(int nr)
        {
            partOfElement.Add(nr);
        }

        public List<int> GetElementNr()
        {
            return partOfElement;
        }

    }
}
