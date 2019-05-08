using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using Grasshopper.Kernel.Data;


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
        List<Vector<double>> strain = new List<Vector<double>>();
        Vector<double> stress;
        Dictionary<int, int> elementNode = new Dictionary<int, int>();

        List<double> deformation = new List<double>();


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

        public void SetStrain(Vector<double> _strain)
        {
            strain.Add(_strain);
        }

        public List<Vector<double>> GetStrain()
        {
            return strain;
        }

        public void SetStress(Vector<double> _stress)
        {
            stress = _stress;
        }

        public Vector<double> GetStress()
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

        public int GetNodeNr()
        {
            return nodeNr;
        }

        public void SetDeformation(List<double> _deformation)
        {
            deformation = _deformation;
        }

        public List<double> GetDeformation()
        {
            return deformation;
        }

        public Vector<double> GetGlobalStrain()
        {
            double amount = strain.Count;
            Vector<double> globalStrain =  Vector<double>.Build.Dense(6);
            for (int i = 0; i < strain.Count; i++)
            {
                globalStrain[0] += strain[i][0];
                globalStrain[1] += strain[i][1];
                globalStrain[2] += strain[i][2];
                globalStrain[3] += strain[i][3];
                globalStrain[4] += strain[i][4];
                globalStrain[5] += strain[i][5];
            }

            for(int j = 0; j < globalStrain.Count; j++)
            {
                globalStrain[j] = (double)globalStrain[j] / amount;
            }

            return globalStrain;
        }
    }
}
