﻿using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace FEbrep
{
    class StiffnessMatrix2
    {

        private double E = 0;
        private double nu = 0;
        private double lx = 0;
        private double ly = 0;
        private double lz = 0;

        public StiffnessMatrix2(double _E, double _nu, double _lx, double _ly, double _lz)
        {
            E = _E;
            nu = _nu;
            lx = _lx/2;
            ly = _ly/2;
            lz = _lz/2;
        }
        public Matrix<double> CreateMatrix()
        {
            //double[,] array = new double[6, 4];
            //array = fillZeros(array);

            double val = E / ((1 + nu) * (1 - 2 * nu));

            double d11 = val * (1 - nu);
            double d44 = val * (1 - 2 * nu) / 2;
            double d55 = d44;
            double d12 = val * nu;
            double d22 = d11;
            double d23 = d12;
            double d33 = d11;
            double d13 = d12;
            double d66 = d44;

            Matrix<double> Ke = DenseMatrix.OfArray(new double[,]
            {
                {lx*ly*lz*((2*d11)/(9*(lx*lx)) + (2*d44)/(9*(ly*ly)) + (2*d55)/(9*(lz*lz))), (lz*(d12 + d44))/6, (ly*(d13 + d55))/6, lx*ly*lz*(d44/(9*(ly*ly)) - (2*d11)/(9*(lx*lx)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/6, (ly*(d13 - d55))/6, -lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - d55/(18*(lz*lz))), -(lz*(d12 + d44))/6, (ly*(d13 - d55))/12, lx*ly*lz*(d11/(9*(lx*lx)) - (2*d44)/(9*(ly*ly)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/6, (ly*(d13 + d55))/12, lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - (2*d55)/(9*(lz*lz))), (lz*(d12 + d44))/12, -(ly*(d13 - d55))/6, -lx*ly*lz*(d11/(9*(lx*lx)) - d44/(18*(ly*ly)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/12, -(ly*(d13 + d55))/6, -lx*ly*lz*(d11/(18*(lx*lx)) + d44/(18*(ly*ly)) + d55/(18*(lz*lz))), -(lz*(d12 + d44))/12, -(ly*(d13 + d55))/12, -lx*ly*lz*(d44/(9*(ly*ly)) - d11/(18*(lx*lx)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/12, -(ly*(d13 - d55))/12},
                {(lz*(d12 + d44))/6, lx*ly*lz*((2*d44)/(9*(lx*lx)) + (2*d22)/(9*(ly*ly)) + (2*d66)/(9*(lz*lz))), (lx*(d23 + d66))/6, -(lz*(d12 - d44))/6, lx*ly*lz*(d22/(9*(ly*ly)) - (2*d44)/(9*(lx*lx)) + d66/(9*(lz*lz))), (lx*(d23 + d66))/12, -(lz*(d12 + d44))/6, -lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - d66/(18*(lz*lz))), (lx*(d23 - d66))/12, (lz*(d12 - d44))/6, lx*ly*lz*(d44/(9*(lx*lx)) - (2*d22)/(9*(ly*ly)) + d66/(9*(lz*lz))), (lx*(d23 - d66))/6, (lz*(d12 + d44))/12, lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - (2*d66)/(9*(lz*lz))), -(lx*(d23 - d66))/6, -(lz*(d12 - d44))/12, -lx*ly*lz*(d44/(9*(lx*lx)) - d22/(18*(ly*ly)) + d66/(9*(lz*lz))), -(lx*(d23 - d66))/12, -(lz*(d12 + d44))/12, -lx*ly*lz*(d44/(18*(lx*lx)) + d22/(18*(ly*ly)) + d66/(18*(lz*lz))), -(lx*(d23 + d66))/12, (lz*(d12 - d44))/12, -lx*ly*lz*(d22/(9*(ly*ly)) - d44/(18*(lx*lx)) + d66/(9*(lz*lz))), -(lx*(d23 + d66))/6},
                {(ly*(d12 + d55))/6, (lx*(d23 + d66))/6, lx*ly*lz*((2*d55)/(9*(lx*lx)) + (2*d66)/(9*(ly*ly)) + (2*d33)/(9*(lz*lz))), -(ly*(d12 - d55))/6, (lx*(d23 + d66))/12, lx*ly*lz*(d66/(9*(ly*ly)) - (2*d55)/(9*(lx*lx)) + d33/(9*(lz*lz))), -(ly*(d12 - d55))/12, -(lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - d33/(18*(lz*lz))), (ly*(d12 + d55))/12, -(lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) - (2*d66)/(9*(ly*ly)) + d33/(9*(lz*lz))), (ly*(d12 - d55))/6, (lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - (2*d33)/(9*(lz*lz))), -(ly*(d12 + d55))/6, (lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) - d66/(18*(ly*ly)) + d33/(9*(lz*lz))), -(ly*(d12 + d55))/12, -(lx*(d23 + d66))/12, -lx*ly*lz*(d55/(18*(lx*lx)) + d66/(18*(ly*ly)) + d33/(18*(lz*lz))), (ly*(d12 - d55))/12, -(lx*(d23 + d66))/6, -lx*ly*lz*(d66/(9*(ly*ly)) - d55/(18*(lx*lx)) + d33/(9*(lz*lz)))},
                {lx*ly*lz*(d44/(9*(ly*ly)) - (2*d11)/(9*(lx*lx)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/6, -(ly*(d13 - d55))/6, lx*ly*lz*((2*d11)/(9*(lx*lx)) + (2*d44)/(9*(ly*ly)) + (2*d55)/(9*(lz*lz))), -(lz*(d12 + d44))/6, -(ly*(d13 + d55))/6, lx*ly*lz*(d11/(9*(lx*lx)) - (2*d44)/(9*(ly*ly)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/6, -(ly*(d13 + d55))/12, -lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - d55/(18*(lz*lz))), (lz*(d12 + d44))/6, -(ly*(d13 - d55))/12, -lx*ly*lz*(d11/(9*(lx*lx)) - d44/(18*(ly*ly)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/12, (ly*(d13 + d55))/6, lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - (2*d55)/(9*(lz*lz))), -(lz*(d12 + d44))/12, (ly*(d13 - d55))/6, -lx*ly*lz*(d44/(9*(ly*ly)) - d11/(18*(lx*lx)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/12, (ly*(d13 - d55))/12, -lx*ly*lz*(d11/(18*(lx*lx)) + d44/(18*(ly*ly)) + d55/(18*(lz*lz))), (lz*(d12 + d44))/12, (ly*(d13 + d55))/12},
                {(lz*(d12 - d44))/6, lx*ly*lz*(d22/(9*(ly*ly)) - (2*d44)/(9*(lx*lx)) + d66/(9*(lz*lz))), (lx*(d23 + d66))/12, -(lz*(d12 + d44))/6, lx*ly*lz*((2*d44)/(9*(lx*lx)) + (2*d22)/(9*(ly*ly)) + (2*d66)/(9*(lz*lz))), (lx*(d23 + d66))/6, -(lz*(d12 - d44))/6, lx*ly*lz*(d44/(9*(lx*lx)) - (2*d22)/(9*(ly*ly)) + d66/(9*(lz*lz))), (lx*(d23 - d66))/6, (lz*(d12 + d44))/6, -lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - d66/(18*(lz*lz))), (lx*(d23 - d66))/12, (lz*(d12 - d44))/12, -lx*ly*lz*(d44/(9*(lx*lx)) - d22/(18*(ly*ly)) + d66/(9*(lz*lz))), -(lx*(d23 - d66))/12, -(lz*(d12 + d44))/12, lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - (2*d66)/(9*(lz*lz))), -(lx*(d23 - d66))/6, -(lz*(d12 - d44))/12, -lx*ly*lz*(d22/(9*(ly*ly)) - d44/(18*(lx*lx)) + d66/(9*(lz*lz))), -(lx*(d23 + d66))/6, (lz*(d12 + d44))/12, -lx*ly*lz*(d44/(18*(lx*lx)) + d22/(18*(ly*ly)) + d66/(18*(lz*lz))), -(lx*(d23 + d66))/12},
                {(ly*(d12 - d55))/6, (lx*(d23 + d66))/12, lx*ly*lz*(d66/(9*(ly*ly)) - (2*d55)/(9*(lx*lx)) + d33/(9*(lz*lz))), -(ly*(d12 + d55))/6, (lx*(d23 + d66))/6, lx*ly*lz*((2*d55)/(9*(lx*lx)) + (2*d66)/(9*(ly*ly)) + (2*d33)/(9*(lz*lz))), -(ly*(d12 + d55))/12, -(lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) - (2*d66)/(9*(ly*ly)) + d33/(9*(lz*lz))), (ly*(d12 - d55))/12, -(lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - d33/(18*(lz*lz))), (ly*(d12 + d55))/6, (lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) - d66/(18*(ly*ly)) + d33/(9*(lz*lz))), -(ly*(d12 - d55))/6, (lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - (2*d33)/(9*(lz*lz))), -(ly*(d12 - d55))/12, -(lx*(d23 + d66))/6, -lx*ly*lz*(d66/(9*(ly*ly)) - d55/(18*(lx*lx)) + d33/(9*(lz*lz))), (ly*(d12 + d55))/12, -(lx*(d23 + d66))/12, -lx*ly*lz*(d55/(18*(lx*lx)) + d66/(18*(ly*ly)) + d33/(18*(lz*lz)))},
                {-lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - d55/(18*(lz*lz))), -(lz*(d12 + d44))/6, -(ly*(d13 - d55))/12, lx*ly*lz*(d11/(9*(lx*lx)) - (2*d44)/(9*(ly*ly)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/6, -(ly*(d13 + d55))/12, lx*ly*lz*((2*d11)/(9*(lx*lx)) + (2*d44)/(9*(ly*ly)) + (2*d55)/(9*(lz*lz))), (lz*(d12 + d44))/6, -(ly*(d13 + d55))/6, lx*ly*lz*(d44/(9*(ly*ly)) - (2*d11)/(9*(lx*lx)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/6, -(ly*(d13 - d55))/6, -lx*ly*lz*(d11/(18*(lx*lx)) + d44/(18*(ly*ly)) + d55/(18*(lz*lz))), -(lz*(d12 + d44))/12, (ly*(d13 + d55))/12, -lx*ly*lz*(d44/(9*(ly*ly)) - d11/(18*(lx*lx)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/12, (ly*(d13 - d55))/12, lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - (2*d55)/(9*(lz*lz))), (lz*(d12 + d44))/12, (ly*(d13 - d55))/6, -lx*ly*lz*(d11/(9*(lx*lx)) - d44/(18*(ly*ly)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/12, (ly*(d13 + d55))/6},
                {-(lz*(d12 + d44))/6, -lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - d66/(18*(lz*lz))), -(lx*(d23 - d66))/12, (lz*(d12 - d44))/6, lx*ly*lz*(d44/(9*(lx*lx)) - (2*d22)/(9*(ly*ly)) + d66/(9*(lz*lz))), -(lx*(d23 - d66))/6, (lz*(d12 + d44))/6, lx*ly*lz*((2*d44)/(9*(lx*lx)) + (2*d22)/(9*(ly*ly)) + (2*d66)/(9*(lz*lz))), -(lx*(d23 + d66))/6, -(lz*(d12 - d44))/6, lx*ly*lz*(d22/(9*(ly*ly)) - (2*d44)/(9*(lx*lx)) + d66/(9*(lz*lz))), -(lx*(d23 + d66))/12, -(lz*(d12 + d44))/12, -lx*ly*lz*(d44/(18*(lx*lx)) + d22/(18*(ly*ly)) + d66/(18*(lz*lz))), (lx*(d23 + d66))/12, (lz*(d12 - d44))/12, -lx*ly*lz*(d22/(9*(ly*ly)) - d44/(18*(lx*lx)) + d66/(9*(lz*lz))), (lx*(d23 + d66))/6, (lz*(d12 + d44))/12, lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - (2*d66)/(9*(lz*lz))), (lx*(d23 - d66))/6, -(lz*(d12 - d44))/12, -lx*ly*lz*(d44/(9*(lx*lx)) - d22/(18*(ly*ly)) + d66/(9*(lz*lz))), (lx*(d23 - d66))/12},
                {(ly*(d12 - d55))/12, (lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - d33/(18*(lz*lz))), -(ly*(d12 + d55))/12, (lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) - (2*d66)/(9*(ly*ly)) + d33/(9*(lz*lz))), -(ly*(d12 + d55))/6, -(lx*(d23 + d66))/6, lx*ly*lz*((2*d55)/(9*(lx*lx)) + (2*d66)/(9*(ly*ly)) + (2*d33)/(9*(lz*lz))), (ly*(d12 - d55))/6, -(lx*(d23 + d66))/12, lx*ly*lz*(d66/(9*(ly*ly)) - (2*d55)/(9*(lx*lx)) + d33/(9*(lz*lz))), (ly*(d12 + d55))/12, (lx*(d23 + d66))/12, -lx*ly*lz*(d55/(18*(lx*lx)) + d66/(18*(ly*ly)) + d33/(18*(lz*lz))), -(ly*(d12 - d55))/12, (lx*(d23 + d66))/6, -lx*ly*lz*(d66/(9*(ly*ly)) - d55/(18*(lx*lx)) + d33/(9*(lz*lz))), -(ly*(d12 - d55))/6, -(lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - (2*d33)/(9*(lz*lz))), (ly*(d12 + d55))/6, -(lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) - d66/(18*(ly*ly)) + d33/(9*(lz*lz)))},
                {lx*ly*lz*(d11/(9*(lx*lx)) - (2*d44)/(9*(ly*ly)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/6, (ly*(d13 + d55))/12, -lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - d55/(18*(lz*lz))), (lz*(d12 + d44))/6, (ly*(d13 - d55))/12, lx*ly*lz*(d44/(9*(ly*ly)) - (2*d11)/(9*(lx*lx)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/6, (ly*(d13 - d55))/6, lx*ly*lz*((2*d11)/(9*(lx*lx)) + (2*d44)/(9*(ly*ly)) + (2*d55)/(9*(lz*lz))), -(lz*(d12 + d44))/6, (ly*(d13 + d55))/6, -lx*ly*lz*(d44/(9*(ly*ly)) - d11/(18*(lx*lx)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/12, -(ly*(d13 - d55))/12, -lx*ly*lz*(d11/(18*(lx*lx)) + d44/(18*(ly*ly)) + d55/(18*(lz*lz))), (lz*(d12 + d44))/12, -(ly*(d13 + d55))/12, -lx*ly*lz*(d11/(9*(lx*lx)) - d44/(18*(ly*ly)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/12, -(ly*(d13 + d55))/6, lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - (2*d55)/(9*(lz*lz))), -(lz*(d12 + d44))/12, -(ly*(d13 - d55))/6},
                {-(lz*(d12 - d44))/6, lx*ly*lz*(d44/(9*(lx*lx)) - (2*d22)/(9*(ly*ly)) + d66/(9*(lz*lz))), -(lx*(d23 - d66))/6, (lz*(d12 + d44))/6, -lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - d66/(18*(lz*lz))), -(lx*(d23 - d66))/12, (lz*(d12 - d44))/6, lx*ly*lz*(d22/(9*(ly*ly)) - (2*d44)/(9*(lx*lx)) + d66/(9*(lz*lz))), -(lx*(d23 + d66))/12, -(lz*(d12 + d44))/6, lx*ly*lz*((2*d44)/(9*(lx*lx)) + (2*d22)/(9*(ly*ly)) + (2*d66)/(9*(lz*lz))), -(lx*(d23 + d66))/6, -(lz*(d12 - d44))/12, -lx*ly*lz*(d22/(9*(ly*ly)) - d44/(18*(lx*lx)) + d66/(9*(lz*lz))), (lx*(d23 + d66))/6, (lz*(d12 + d44))/12, -lx*ly*lz*(d44/(18*(lx*lx)) + d22/(18*(ly*ly)) + d66/(18*(lz*lz))), (lx*(d23 + d66))/12, (lz*(d12 - d44))/12, -lx*ly*lz*(d44/(9*(lx*lx)) - d22/(18*(ly*ly)) + d66/(9*(lz*lz))), (lx*(d23 - d66))/12, -(lz*(d12 + d44))/12, lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - (2*d66)/(9*(lz*lz))), (lx*(d23 - d66))/6},
                {(ly*(d12 + d55))/12, (lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) - (2*d66)/(9*(ly*ly)) + d33/(9*(lz*lz))), -(ly*(d12 - d55))/12, (lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - d33/(18*(lz*lz))), -(ly*(d12 - d55))/6, -(lx*(d23 + d66))/12, lx*ly*lz*(d66/(9*(ly*ly)) - (2*d55)/(9*(lx*lx)) + d33/(9*(lz*lz))), (ly*(d12 + d55))/6, -(lx*(d23 + d66))/6, lx*ly*lz*((2*d55)/(9*(lx*lx)) + (2*d66)/(9*(ly*ly)) + (2*d33)/(9*(lz*lz))), (ly*(d12 - d55))/12, (lx*(d23 + d66))/6, -lx*ly*lz*(d66/(9*(ly*ly)) - d55/(18*(lx*lx)) + d33/(9*(lz*lz))), -(ly*(d12 + d55))/12, (lx*(d23 + d66))/12, -lx*ly*lz*(d55/(18*(lx*lx)) + d66/(18*(ly*ly)) + d33/(18*(lz*lz))), -(ly*(d12 + d55))/6, -(lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) - d66/(18*(ly*ly)) + d33/(9*(lz*lz))), (ly*(d12 - d55))/6, -(lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - (2*d33)/(9*(lz*lz)))},
                {lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - (2*d55)/(9*(lz*lz))), (lz*(d12 + d44))/12, (ly*(d13 - d55))/6, -lx*ly*lz*(d11/(9*(lx*lx)) - d44/(18*(ly*ly)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/12, (ly*(d13 + d55))/6, -lx*ly*lz*(d11/(18*(lx*lx)) + d44/(18*(ly*ly)) + d55/(18*(lz*lz))), -(lz*(d12 + d44))/12, (ly*(d13 + d55))/12, -lx*ly*lz*(d44/(9*(ly*ly)) - d11/(18*(lx*lx)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/12, (ly*(d13 - d55))/12, lx*ly*lz*((2*d11)/(9*(lx*lx)) + (2*d44)/(9*(ly*ly)) + (2*d55)/(9*(lz*lz))), (lz*(d12 + d44))/6, -(ly*(d13 + d55))/6, lx*ly*lz*(d44/(9*(ly*ly)) - (2*d11)/(9*(lx*lx)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/6, -(ly*(d13 - d55))/6, -lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - d55/(18*(lz*lz))), -(lz*(d12 + d44))/6, -(ly*(d13 - d55))/12, lx*ly*lz*(d11/(9*(lx*lx)) - (2*d44)/(9*(ly*ly)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/6, -(ly*(d13 + d55))/12},
                {(lz*(d12 + d44))/12, lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - (2*d66)/(9*(lz*lz))), (lx*(d23 - d66))/6, -(lz*(d12 - d44))/12, -lx*ly*lz*(d44/(9*(lx*lx)) - d22/(18*(ly*ly)) + d66/(9*(lz*lz))), (lx*(d23 - d66))/12, -(lz*(d12 + d44))/12, -lx*ly*lz*(d44/(18*(lx*lx)) + d22/(18*(ly*ly)) + d66/(18*(lz*lz))), (lx*(d23 + d66))/12, (lz*(d12 - d44))/12, -lx*ly*lz*(d22/(9*(ly*ly)) - d44/(18*(lx*lx)) + d66/(9*(lz*lz))), (lx*(d23 + d66))/6, (lz*(d12 + d44))/6, lx*ly*lz*((2*d44)/(9*(lx*lx)) + (2*d22)/(9*(ly*ly)) + (2*d66)/(9*(lz*lz))), -(lx*(d23 + d66))/6, -(lz*(d12 - d44))/6, lx*ly*lz*(d22/(9*(ly*ly)) - (2*d44)/(9*(lx*lx)) + d66/(9*(lz*lz))), -(lx*(d23 + d66))/12, -(lz*(d12 + d44))/6, -lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - d66/(18*(lz*lz))), -(lx*(d23 - d66))/12, (lz*(d12 - d44))/6, lx*ly*lz*(d44/(9*(lx*lx)) - (2*d22)/(9*(ly*ly)) + d66/(9*(lz*lz))), -(lx*(d23 - d66))/6},
                {-(ly*(d12 - d55))/6, -(lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - (2*d33)/(9*(lz*lz))), (ly*(d12 + d55))/6, -(lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) - d66/(18*(ly*ly)) + d33/(9*(lz*lz))), (ly*(d12 + d55))/12, (lx*(d23 + d66))/12, -lx*ly*lz*(d55/(18*(lx*lx)) + d66/(18*(ly*ly)) + d33/(18*(lz*lz))), -(ly*(d12 - d55))/12, (lx*(d23 + d66))/6, -lx*ly*lz*(d66/(9*(ly*ly)) - d55/(18*(lx*lx)) + d33/(9*(lz*lz))), -(ly*(d12 + d55))/6, -(lx*(d23 + d66))/6, lx*ly*lz*((2*d55)/(9*(lx*lx)) + (2*d66)/(9*(ly*ly)) + (2*d33)/(9*(lz*lz))), (ly*(d12 - d55))/6, -(lx*(d23 + d66))/12, lx*ly*lz*(d66/(9*(ly*ly)) - (2*d55)/(9*(lx*lx)) + d33/(9*(lz*lz))), (ly*(d12 - d55))/12, (lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - d33/(18*(lz*lz))), -(ly*(d12 + d55))/12, (lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) - (2*d66)/(9*(ly*ly)) + d33/(9*(lz*lz)))},
                {-lx*ly*lz*(d11/(9*(lx*lx)) - d44/(18*(ly*ly)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/12, -(ly*(d13 + d55))/6, lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - (2*d55)/(9*(lz*lz))), -(lz*(d12 + d44))/12, -(ly*(d13 - d55))/6, -lx*ly*lz*(d44/(9*(ly*ly)) - d11/(18*(lx*lx)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/12, -(ly*(d13 - d55))/12, -lx*ly*lz*(d11/(18*(lx*lx)) + d44/(18*(ly*ly)) + d55/(18*(lz*lz))), (lz*(d12 + d44))/12, -(ly*(d13 + d55))/12, lx*ly*lz*(d44/(9*(ly*ly)) - (2*d11)/(9*(lx*lx)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/6, (ly*(d13 - d55))/6, lx*ly*lz*((2*d11)/(9*(lx*lx)) + (2*d44)/(9*(ly*ly)) + (2*d55)/(9*(lz*lz))), -(lz*(d12 + d44))/6, (ly*(d13 + d55))/6, lx*ly*lz*(d11/(9*(lx*lx)) - (2*d44)/(9*(ly*ly)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/6, (ly*(d13 + d55))/12, -lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - d55/(18*(lz*lz))), (lz*(d12 + d44))/6, (ly*(d13 - d55))/12},
                {(lz*(d12 - d44))/12, -lx*ly*lz*(d44/(9*(lx*lx)) - d22/(18*(ly*ly)) + d66/(9*(lz*lz))), (lx*(d23 - d66))/12, -(lz*(d12 + d44))/12, lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - (2*d66)/(9*(lz*lz))), (lx*(d23 - d66))/6, -(lz*(d12 - d44))/12, -lx*ly*lz*(d22/(9*(ly*ly)) - d44/(18*(lx*lx)) + d66/(9*(lz*lz))), (lx*(d23 + d66))/6, (lz*(d12 + d44))/12, -lx*ly*lz*(d44/(18*(lx*lx)) + d22/(18*(ly*ly)) + d66/(18*(lz*lz))), (lx*(d23 + d66))/12, (lz*(d12 - d44))/6, lx*ly*lz*(d22/(9*(ly*ly)) - (2*d44)/(9*(lx*lx)) + d66/(9*(lz*lz))), -(lx*(d23 + d66))/12, -(lz*(d12 + d44))/6, lx*ly*lz*((2*d44)/(9*(lx*lx)) + (2*d22)/(9*(ly*ly)) + (2*d66)/(9*(lz*lz))), -(lx*(d23 + d66))/6, -(lz*(d12 - d44))/6, lx*ly*lz*(d44/(9*(lx*lx)) - (2*d22)/(9*(ly*ly)) + d66/(9*(lz*lz))), -(lx*(d23 - d66))/6, (lz*(d12 + d44))/6, -lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - d66/(18*(lz*lz))), -(lx*(d23 - d66))/12},
                {-(ly*(d12 + d55))/6, -(lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) - d66/(18*(ly*ly)) + d33/(9*(lz*lz))), (ly*(d12 - d55))/6, -(lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - (2*d33)/(9*(lz*lz))), (ly*(d12 - d55))/12, (lx*(d23 + d66))/6, -lx*ly*lz*(d66/(9*(ly*ly)) - d55/(18*(lx*lx)) + d33/(9*(lz*lz))), -(ly*(d12 + d55))/12, (lx*(d23 + d66))/12, -lx*ly*lz*(d55/(18*(lx*lx)) + d66/(18*(ly*ly)) + d33/(18*(lz*lz))), -(ly*(d12 - d55))/6, -(lx*(d23 + d66))/12, lx*ly*lz*(d66/(9*(ly*ly)) - (2*d55)/(9*(lx*lx)) + d33/(9*(lz*lz))), (ly*(d12 + d55))/6, -(lx*(d23 + d66))/6, lx*ly*lz*((2*d55)/(9*(lx*lx)) + (2*d66)/(9*(ly*ly)) + (2*d33)/(9*(lz*lz))), (ly*(d12 + d55))/12, (lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) - (2*d66)/(9*(ly*ly)) + d33/(9*(lz*lz))), -(ly*(d12 - d55))/12, (lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - d33/(18*(lz*lz)))},
                {-lx*ly*lz*(d11/(18*(lx*lx)) + d44/(18*(ly*ly)) + d55/(18*(lz*lz))), -(lz*(d12 + d44))/12, -(ly*(d13 + d55))/12, -lx*ly*lz*(d44/(9*(ly*ly)) - d11/(18*(lx*lx)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/12, -(ly*(d13 - d55))/12, lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - (2*d55)/(9*(lz*lz))), (lz*(d12 + d44))/12, -(ly*(d13 - d55))/6, -lx*ly*lz*(d11/(9*(lx*lx)) - d44/(18*(ly*ly)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/12, -(ly*(d13 + d55))/6, -lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - d55/(18*(lz*lz))), -(lz*(d12 + d44))/6, (ly*(d13 - d55))/12, lx*ly*lz*(d11/(9*(lx*lx)) - (2*d44)/(9*(ly*ly)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/6, (ly*(d13 + d55))/12, lx*ly*lz*((2*d11)/(9*(lx*lx)) + (2*d44)/(9*(ly*ly)) + (2*d55)/(9*(lz*lz))), (lz*(d12 + d44))/6, (ly*(d13 + d55))/6, lx*ly*lz*(d44/(9*(ly*ly)) - (2*d11)/(9*(lx*lx)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/6, (ly*(d13 - d55))/6},
                {-(lz*(d12 + d44))/12, -lx*ly*lz*(d44/(18*(lx*lx)) + d22/(18*(ly*ly)) + d66/(18*(lz*lz))), -(lx*(d23 + d66))/12, (lz*(d12 - d44))/12, -lx*ly*lz*(d22/(9*(ly*ly)) - d44/(18*(lx*lx)) + d66/(9*(lz*lz))), -(lx*(d23 + d66))/6, (lz*(d12 + d44))/12, lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - (2*d66)/(9*(lz*lz))), -(lx*(d23 - d66))/6, -(lz*(d12 - d44))/12, -lx*ly*lz*(d44/(9*(lx*lx)) - d22/(18*(ly*ly)) + d66/(9*(lz*lz))), -(lx*(d23 - d66))/12, -(lz*(d12 + d44))/6, -lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - d66/(18*(lz*lz))), (lx*(d23 - d66))/12, (lz*(d12 - d44))/6, lx*ly*lz*(d44/(9*(lx*lx)) - (2*d22)/(9*(ly*ly)) + d66/(9*(lz*lz))), (lx*(d23 - d66))/6, (lz*(d12 + d44))/6, lx*ly*lz*((2*d44)/(9*(lx*lx)) + (2*d22)/(9*(ly*ly)) + (2*d66)/(9*(lz*lz))), (lx*(d23 + d66))/6, -(lz*(d12 - d44))/6, lx*ly*lz*(d22/(9*(ly*ly)) - (2*d44)/(9*(lx*lx)) + d66/(9*(lz*lz))), (lx*(d23 + d66))/12},
                {-(ly*(d12 + d55))/12, -(lx*(d23 + d66))/12, -lx*ly*lz*(d55/(18*(lx*lx)) + d66/(18*(ly*ly)) + d33/(18*(lz*lz))), (ly*(d12 - d55))/12, -(lx*(d23 + d66))/6, -lx*ly*lz*(d66/(9*(ly*ly)) - d55/(18*(lx*lx)) + d33/(9*(lz*lz))), (ly*(d12 - d55))/6, (lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - (2*d33)/(9*(lz*lz))), -(ly*(d12 + d55))/6, (lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) - d66/(18*(ly*ly)) + d33/(9*(lz*lz))), -(ly*(d12 - d55))/12, -(lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - d33/(18*(lz*lz))), (ly*(d12 + d55))/12, -(lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) - (2*d66)/(9*(ly*ly)) + d33/(9*(lz*lz))), (ly*(d12 + d55))/6, (lx*(d23 + d66))/6, lx*ly*lz*((2*d55)/(9*(lx*lx)) + (2*d66)/(9*(ly*ly)) + (2*d33)/(9*(lz*lz))), -(ly*(d12 - d55))/6, (lx*(d23 + d66))/12, lx*ly*lz*(d66/(9*(ly*ly)) - (2*d55)/(9*(lx*lx)) + d33/(9*(lz*lz)))},
                {-lx*ly*lz*(d44/(9*(ly*ly)) - d11/(18*(lx*lx)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/12, (ly*(d13 - d55))/12, -lx*ly*lz*(d11/(18*(lx*lx)) + d44/(18*(ly*ly)) + d55/(18*(lz*lz))), (lz*(d12 + d44))/12, (ly*(d13 + d55))/12, -lx*ly*lz*(d11/(9*(lx*lx)) - d44/(18*(ly*ly)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/12, (ly*(d13 + d55))/6, lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - (2*d55)/(9*(lz*lz))), -(lz*(d12 + d44))/12, (ly*(d13 - d55))/6, lx*ly*lz*(d11/(9*(lx*lx)) - (2*d44)/(9*(ly*ly)) + d55/(9*(lz*lz))), (lz*(d12 - d44))/6, -(ly*(d13 + d55))/12, -lx*ly*lz*(d11/(9*(lx*lx)) + d44/(9*(ly*ly)) - d55/(18*(lz*lz))), (lz*(d12 + d44))/6, -(ly*(d13 - d55))/12, lx*ly*lz*(d44/(9*(ly*ly)) - (2*d11)/(9*(lx*lx)) + d55/(9*(lz*lz))), -(lz*(d12 - d44))/6, -(ly*(d13 - d55))/6, lx*ly*lz*((2*d11)/(9*(lx*lx)) + (2*d44)/(9*(ly*ly)) + (2*d55)/(9*(lz*lz))), -(lz*(d12 + d44))/6, -(ly*(d13 + d55))/6},
                {-(lz*(d12 - d44))/12, -lx*ly*lz*(d22/(9*(ly*ly)) - d44/(18*(lx*lx)) + d66/(9*(lz*lz))), -(lx*(d23 + d66))/6, (lz*(d12 + d44))/12, -lx*ly*lz*(d44/(18*(lx*lx)) + d22/(18*(ly*ly)) + d66/(18*(lz*lz))), -(lx*(d23 + d66))/12, (lz*(d12 - d44))/12, -lx*ly*lz*(d44/(9*(lx*lx)) - d22/(18*(ly*ly)) + d66/(9*(lz*lz))), -(lx*(d23 - d66))/12, -(lz*(d12 + d44))/12, lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - (2*d66)/(9*(lz*lz))), -(lx*(d23 - d66))/6, -(lz*(d12 - d44))/6, lx*ly*lz*(d44/(9*(lx*lx)) - (2*d22)/(9*(ly*ly)) + d66/(9*(lz*lz))), (lx*(d23 - d66))/6, (lz*(d12 + d44))/6, -lx*ly*lz*(d44/(9*(lx*lx)) + d22/(9*(ly*ly)) - d66/(18*(lz*lz))), (lx*(d23 - d66))/12, (lz*(d12 - d44))/6, lx*ly*lz*(d22/(9*(ly*ly)) - (2*d44)/(9*(lx*lx)) + d66/(9*(lz*lz))), (lx*(d23 + d66))/12, -(lz*(d12 + d44))/6, lx*ly*lz*((2*d44)/(9*(lx*lx)) + (2*d22)/(9*(ly*ly)) + (2*d66)/(9*(lz*lz))), (lx*(d23 + d66))/6},
                {-(ly*(d12 - d55))/12, -(lx*(d23 + d66))/6, -lx*ly*lz*(d66/(9*(ly*ly)) - d55/(18*(lx*lx)) + d33/(9*(lz*lz))), (ly*(d12 + d55))/12, -(lx*(d23 + d66))/12, -lx*ly*lz*(d55/(18*(lx*lx)) + d66/(18*(ly*ly)) + d33/(18*(lz*lz))), (ly*(d12 + d55))/6, (lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) - d66/(18*(ly*ly)) + d33/(9*(lz*lz))), -(ly*(d12 - d55))/6, (lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - (2*d33)/(9*(lz*lz))), -(ly*(d12 + d55))/12, -(lx*(d23 - d66))/6, lx*ly*lz*(d55/(9*(lx*lx)) - (2*d66)/(9*(ly*ly)) + d33/(9*(lz*lz))), (ly*(d12 - d55))/12, -(lx*(d23 - d66))/12, -lx*ly*lz*(d55/(9*(lx*lx)) + d66/(9*(ly*ly)) - d33/(18*(lz*lz))), (ly*(d12 - d55))/6, (lx*(d23 + d66))/12, lx*ly*lz*(d66/(9*(ly*ly)) - (2*d55)/(9*(lx*lx)) + d33/(9*(lz*lz))), -(ly*(d12 + d55))/6, (lx*(d23 + d66))/6, lx*ly*lz*((2*d55)/(9*(lx*lx)) + (2*d66)/(9*(ly*ly)) + (2*d33)/(9*(lz*lz)))},

            });
            
            return Ke;
        }

        public double[,] fillZeros(double[,] array)
        {
            Array.Clear(array, 0, array.Length);
            return array;
        }

        static void Main(string[] args)
        {
            StiffnessMatrix2 s = new StiffnessMatrix2(10, 0.3, 10, 10, 10);
            s.CreateMatrix();
        }


    }
}
