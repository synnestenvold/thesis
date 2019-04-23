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
    public class Element
    {
        List<Node> vertices = new List<Node>();
        List<int> connectivity = new List<int>();
        int elementNr = 0;
        Matrix<double> K_e = Matrix<double>.Build.Dense(24, 24);
        List<Matrix<Double>> B_e = new List<Matrix<Double>>();

        public Element (List<Node> _vertices, int _elementNr, List<int> _connectivity)
        {
            vertices = _vertices;
            elementNr = _elementNr;
            connectivity = _connectivity;
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

        public List<Matrix<double>> GetBMatrixes()
        {
            return B_e;
        }

    }
}
