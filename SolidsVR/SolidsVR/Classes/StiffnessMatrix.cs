using System;
using System.Collections.Generic;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Linq;

namespace SolidsVR
{
    class StiffnessMatrix
    {
        private double E = 0;
        private double nu = 0;

        public StiffnessMatrix(double _E, double _nu)
        {
            E = _E;
            nu = _nu;
        }

        public (Matrix<double>, List<Matrix<Double>>) CreateMatrix(List<Node> nodeList)
        {
            Matrix<double> Ke = Matrix<double>.Build.Dense(24, 24);
            List<Matrix<double>> Be = new List<Matrix<double>>();

            //3D Constitutive matrix: C
            double value = (double) E / ((1 + nu) * (1 - 2 * nu));
            Matrix<double> C = DenseMatrix.OfArray(new double[,]
            {
                {1-nu, nu, nu, 0, 0, 0},
                {nu, 1-nu, nu, 0, 0, 0},
                {nu, nu, 1-nu, 0, 0, 0},
                {0, 0, 0, (1-2*nu)/2, 0, 0},
                {0, 0, 0, 0, (1-2*nu)/2, 0},
                {0, 0, 0, 0, 0, (1-2*nu)/2},
            });

            C = C.Multiply(value); //Constitutive matrix

            //Gauss points
            Vector<double> gaussPoints = DenseVector.OfArray(new double[] { -1 / Math.Sqrt(3), 1 / Math.Sqrt(3) }); //Gauss points

            Point3d point = new Point3d(0, 0, 0);
            List<Point3d> pNatural = Enumerable.Repeat(point, 8).ToList();

            for (int i = 0; i < nodeList.Count; i++)
            {
                pNatural[i] = nodeList[i].GetCoord();
            }

            Matrix<double> coordinates = DenseMatrix.OfArray(new double[,]
            {
                {pNatural[0].X,pNatural[0].Y , pNatural[0].Z},
                {pNatural[1].X,pNatural[1].Y , pNatural[1].Z},
                {pNatural[2].X,pNatural[2].Y , pNatural[2].Z},
                {pNatural[3].X,pNatural[3].Y , pNatural[3].Z},
                {pNatural[4].X,pNatural[4].Y , pNatural[4].Z},
                {pNatural[5].X,pNatural[5].Y , pNatural[5].Z},
                {pNatural[6].X,pNatural[6].Y , pNatural[6].Z},
                {pNatural[7].X,pNatural[7].Y , pNatural[7].Z},
            });

            //Numerical integration
            foreach (double g3 in gaussPoints)
            {
                foreach (double g2 in gaussPoints)
                {
                    foreach (double g1 in gaussPoints)
                    {
                        //Shape functions
                        Matrix<double> shapeF = DenseMatrix.OfArray(new double[,]
                       {
                            {-(1-g2)*(1-g3), (1-g2)*(1-g3), (1+g2)*(1-g3),-(1+g2)*(1-g3),-(1-g2)*(1+g3),(1-g2)*(1+g3),(1+g2)*(1+g3),-(1+g2)*(1+g3)},
                            {-(1-g1)*(1-g3), -(1+g1)*(1-g3), (1+g1)*(1-g3),(1-g1)*(1-g3),-(1-g1)*(1+g3),-(1+g1)*(1+g3),(1+g1)*(1+g3),(1-g1)*(1+g3)},
                            {-(1-g1)*(1-g2), -(1+g1)*(1-g2), -(1+g1)*(1+g2),-(1-g1)*(1+g2),(1-g1)*(1-g2),(1+g1)*(1-g2),(1+g1)*(1+g2),(1-g1)*(1+g2)},

                       });

                        shapeF = shapeF.Divide(8); //Divided by 8

                        //Jacobi Matrix

                        Matrix<double> JacobiMatrix = Matrix<double>.Build.Dense(3, 3);

                        JacobiMatrix = shapeF.Multiply(coordinates);

                        // Auxiliar matrix for assemblinng of B-matrix 
                        Matrix<double> auxiliar = Matrix<double>.Build.Dense(3, 8);

                        auxiliar = JacobiMatrix.Inverse().Multiply(shapeF);

                        // B matrix
                        Matrix<double> B = Matrix<double>.Build.Dense(6, 24);

                        //First three rows
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j <= 7; j++)
                            {
                                B[i, 3 * j + 1 + (i - 1)] = auxiliar[i, j];
                            }
                        }

                        //Fourth row
                        for (int j = 0; j <= 7; j++)
                        {
                            B[3, 3 * j] = auxiliar[1, j];
                        }

                        for (int j = 0; j <= 7; j++)
                        {
                            B[3, 3 * j + 1] = auxiliar[0, j];
                        }

                        //Fifth row
                        for (int j = 0; j <= 7; j++)
                        {
                            B[4, 3 * j + 2] = auxiliar[1, j];
                        }

                        for (int j = 0; j <= 7; j++)
                        {
                            B[4, 3 * j + 1] = auxiliar[2, j];
                        }

                        //Sixth row
                        for (int j = 0; j <= 7; j++)
                        {
                            B[5, 3 * j] = auxiliar[2, j];
                        }

                        for (int j = 0; j <= 7; j++)
                        {
                            B[5, 3 * j + 2] = auxiliar[0, j];
                        }

                        Be.Add(B);
                        
                        //Adding the stiffness matrix. Ke = Ke + B'*C*B*Det(JacobiMatrix)
                        Ke = Ke.Add(B.Transpose().Multiply(C).Multiply(B).Multiply(JacobiMatrix.Determinant()));
                    }
                }
            }
            
            //Changing order of Be to fit the global numbering
            Matrix<double> B_2 = Be[2];
            Be[2] = Be[3];
            Be[3] = B_2;
            Matrix<double> B_6 = Be[6];
            Be[6] = Be[7];
            Be[7] = B_6;

            return (Ke, Be);
        }
    }
}