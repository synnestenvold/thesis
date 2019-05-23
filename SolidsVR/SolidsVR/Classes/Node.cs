using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;


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
        List<Vector<double>> stress = new List<Vector<double>>();
        Vector<double> globalStrain = Vector<double>.Build.Dense(6);
        Vector<double> globalStress = Vector<double>.Build.Dense(6);
        Boolean removable = true;

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

        public void CleanStressAndStrain()
        {
            strain.Clear();
            stress.Clear();
        }

        public List<Vector<double>> GetStrain()
        {
            return strain;
        }

        public void SetStress(Vector<double> _stress)
        {
            stress.Add(_stress);
        }

        public List<Vector<double>> GetStress()
        {
            return stress;
        }


        public void SetGlobalStress(Vector<double> _globalStress)
        {
            globalStress = _globalStress;
        }

        public Vector<double> GetGlobalStress()
        {
            return globalStress;
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

        public void SetGlobalStrain(Vector<double> _globalStrain)
        {
            globalStrain = _globalStrain;
        }

        public Vector<double> GetGlobalStrain()
        {
            return globalStrain;
        }
        public Boolean isRemovable()
        {
            return removable;
        }
        public void setRemovable(Boolean _removable)
        {
            removable = _removable;
        }
    }
}
