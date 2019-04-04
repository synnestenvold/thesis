using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SolidsVR
{
    public class Cmatrix
    {
        private double E = 0;
        private double nu = 0;

        public Cmatrix(double _E, double _nu)
        {
            E = _E;
            nu = _nu;
        }
        public Matrix<double> CreateMatrix()
        {
            double value = E / ((1 + nu) * (1 - 2 * nu));
            Matrix<double> C = DenseMatrix.OfArray(new double[,]
            {
                {1-nu, nu, nu, 0, 0, 0},
                {nu, 1-nu, nu, 0, 0, 0},
                {nu, nu, 1-nu, 0, 0, 0},
                {0, 0, 0, (1-2*nu)/2, 0, 0},
                {0, 0, 0, 0, (1-2*nu)/2, 0},
                {0, 0, 0, 0, 0, (1-2*nu)/2},

            });
            C = C.Multiply(value);
            return C;
        }

    }
}
