using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FEMeshedBrep
{
    public class Bmatrix
    {
        private double lx = 0;
        private double ly = 0;
        private double lz = 0;

        public Bmatrix(double _lx, double _ly, double _lz)
        {
            lx = _lx;
            ly = _ly;
            lz = _lz;
        }
        public Matrix<double> CreateMatrix()
        {
            Matrix<double> B = DenseMatrix.OfArray(new double[,]
            {
                {-1/lx, 0, 0, 1/lx, 0, 0, 1/lx, 0, 0, -1/lx, 0, 0, -1/lx, 0, 0, 1/lx, 0, 0, 1/lx, 0, 0, -1/lx, 0, 0},
                {0, -1/ly, 0, 0, -1/ly, 0, 0, 1/ly, 0, 0, 1/ly, 0, 0, -1/ly, 0, 0, -1/ly, 0, 0, 1/ly, 0, 0, 1/ly, 0},
                {0, 0, -1/lz, 0, 0, -1/lz, 0, 0, -1/lz, 0, 0, -1/lz, 0, 0, 1/lz, 0, 0, 1/lz, 0, 0, 1/lz, 0, 0, 1/lz},
                {-1/ly, -1/lx, 0, -1/ly, 1/lx, 0, 1/ly, 1/lx, 0, 1/ly, -1/lx, 0, -1/ly, -1/lx, 0, -1/ly, 1/lx, 0, 1/ly, 1/lx, 0, 1/ly, -1/lx, 0},
                {-1/lz, 0, -1/lx, -1/lz, 0, 1/lx, -1/lz, 0, 1/lx, -1/lz, 0, -1/lx, 1/lz, 0, -1/lx, 1/lz, 0, 1/lx, 1/lz, 0, 1/lx, 1/lz, 0, -1/lx},
                {0, -1/lz, -1/ly, 0, -1/lz, -1/ly, 0, -1/lz, 1/ly, 0, -1/lz, 1/ly, 0, 1/lz, -1/ly, 0, 1/lz, -1/ly, 0, 1/lz, 1/ly, 0, 1/lz, 1/ly},
            });
            return B;
        }

    }
}

