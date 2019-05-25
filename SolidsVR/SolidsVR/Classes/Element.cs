using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace SolidsVR
{
    public class Element
    {
        private List<Node> vertices = new List<Node>();
        private List<int> connectivity = new List<int>();
        private int elementNr = 0;

        private List<Matrix<Double>> B_e = new List<Matrix<Double>>();
        private List<double> averageStress = new List<double>();
        private Matrix<double> K_e = Matrix<double>.Build.Dense(24, 24);
        private Boolean removable = true;

        public Element (List<Node> _vertices, int _elementNr, List<int> _connectivity)
        {
            vertices = _vertices;
            connectivity = _connectivity;
            elementNr = _elementNr;
        }

        public List<Node> GetVertices()
        {
            return vertices;
        }

        public int GetElementNr()
        {
            return elementNr;
        }

        public List<int> GetConnectivity()
        {
            return connectivity;
        }

        public void SetStiffnessMatrix(Matrix<double> _K_e)
        {
            K_e = _K_e;
        }

        public Matrix<double> GetStiffnessMatrix()
        {
            return K_e;
        }

        public void SetBMatrices(List<Matrix<double>> _B_e)
        {
            B_e = _B_e;
        }

        public List<Matrix<double>> GetBMatrices()
        {
            return B_e;
        }

        public void SetAverageValuesStress()
        {
            averageStress = new List<double>();
            for (int dir = 0; dir < 7; dir++)
            {
                double average = 0;
                for (int j = 0; j < vertices.Count; j++)
                {
                    average += vertices[j].GetGlobalStress()[dir];
                }
                averageStress.Add(Math.Round(average / vertices.Count, 4));
            }
        }

        public double GetAverageStressDir(int dir)
        {
            return averageStress[dir];
        }

        public void SetRemovable(Boolean _removable)
        {
            removable = _removable;
        }

        public Boolean IsRemovable()
        {
            return removable;
        }
    }
}