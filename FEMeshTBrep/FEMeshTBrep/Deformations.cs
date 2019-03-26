using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace FEMeshTBrep
{
    class Deformations
    {
        private double[,] m;
        private List<double> load = new List<double>();
        private int rows = 0;
        private int cols = 0;
        Matrix<double> mt;

        public Deformations(Matrix<double> _m, double[] _load)
        {
            //må gjøre om fra Matrix<double> til double[,]
            rows = _m.RowCount;
            cols = _m.ColumnCount;
            mt = _m;
            /*
            m = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    m[i, j] = _m[i, j];
                }
            }
            */
            //må gjøre om fra double[] til List<double>
            for (int k = 0; k < _load.Length; k++)
            {
                load.Add(_load[k]);
            }

        }
        public List<double> Cholesky_Banachiewicz()
        {
            //double[,] A = mt;

            Matrix<double> A = mt;
            List<double> load1 = load;
            //Cholesky only works for square, symmetric and positive
            //Square matrix is guaranteed because of how matrix is constructed, but symmetry is checked
            if (IsSymmetric(A))
            {
                //preallocating
                double[,] L = new double[A.RowCount, A.ColumnCount];
                double[,] L_T = new double[A.RowCount, A.ColumnCount];

                //creation of L and L_transposed matrices
                for (int i = 0; i < L.GetLength(0); i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        double L_sum = 0;
                        if (i == j)
                        {
                            for (int k = 0; k < j; k++)
                            {
                                L_sum += L[i, k] * L[i, k];
                            }
                            L[i, i] = Math.Sqrt(A[i, j] - L_sum);
                            L_T[i, i] = L[i, i];
                        }
                        else
                        {
                            for (int k = 0; k < j; k++)
                            {
                                L_sum += L[i, k] * L[j, k];
                            }
                            L[i, j] = (1 / L[j, j]) * (A[i, j] - L_sum);
                            L_T[j, i] = L[i, j];
                        }
                    }
                }
                //Solving L*y=load1 for temporary variable y
                List<double> y = ForwardsSubstitution(load1, L);

                //Solving LˆT*x = y for deformations x
                List<double> x = BackwardsSubstitution(load1, L_T, y);

                return x;
            }
            else //K-matrix is not symmetric
            {
                //throw new RuntimeException("Matrix is not symmetric");
                System.Diagnostics.Debug.WriteLine("Matrix is notnsymmetric(ERROR!)");
                return null;
            }
        }

        private List<double> ForwardsSubstitution(List<double> load1, double[,] L)
        {
            List<double> y = new List<double>();
            for (int i = 0; i < L.GetLength(1); i++)
            {
                double L_prev = 0;

                for (int j = 0; j < i; j++)
                {
                    L_prev += L[i, j] * y[j];
                }
                y.Add((load1[i] - L_prev) / L[i, i]);
            }
            return y;
        }

        private List<double> BackwardsSubstitution(List<double> load1, double[,] L_T, List<double> y)
        {
            var x = new List<double>(new double[load1.Count]);
            for (int i = L_T.GetLength(1) - 1; i > -1; i--)
            {
                double L_prev = 0;
                for (int j = L_T.GetLength(1) - 1; j > i; j--)
                {
                    L_prev += L_T[i, j] * x[j];
                }
                x[i] = ((y[i] - L_prev) / L_T[i, i]);
            }
            return x;
        }
        private static bool IsSymmetric(Matrix<double> A)
        {
            int rowCount = A.RowCount;
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (A[i, j] != A[j, i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}