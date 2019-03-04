using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace StiffnessMatrix
{
    class Assembly
    {


        public Matrix<double> assemblyMatrix(Matrix<double> K_e, int[] connectivity, int sizeOfM)
        {

            Matrix<double> K = Matrix<double>.Build.Dense(sizeOfM, sizeOfM);

            for (int i = 0; i < connectivity.Length; i++)
            {
                for (int j = 0; j < connectivity.Length; j++)
                {

                    //Inserting 3x3 stiffness matrix
                    for (int k = 0; k < 3; k++)
                    {
                        for (int e = 0; e < 3; e++)
                        {
                            K[3 * connectivity[i] + k, 3 * connectivity[j] + e] = K_e[3 * i + k, 3 * j + e];

                        }
                    }
                }
            }
            return K;
        }
    }
}
