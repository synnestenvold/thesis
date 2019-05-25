using System;
using System.Collections.Generic;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;


namespace SolidsVR
{
    public class Node
    {
        private Point3d coordinate;
        private int nodeNr = 0;

        private Boolean isCorner = false;
        private Boolean isMiddle = false;
        private Boolean isEdge = false;
        private Boolean removable = true;
        private List<int> partOfElement = new List<int>();
        private List<int> surfaceNum = new List<int>();
        private List<Vector<double>> stress = new List<Vector<double>>();
        private Vector<double> globalStrain = Vector<double>.Build.Dense(6);
        private Vector<double> globalStress = Vector<double>.Build.Dense(6);
        private List<double> deformation = new List<double>();


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

        public void CleanStress()
        {
            stress.Clear();
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

        public Boolean IsRemovable()
        {
            return removable;
        }

        public void SetRemovable(Boolean _removable)
        {
            removable = _removable;
        }
    }
}