using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Grasshopper.Kernel.Types;

namespace SolidsVR
{
    class Assembly_StiffnessMatrix
    {

        public Matrix<double> AssemblyMatrix(Matrix<double> K_tot, Matrix<double> K_e, List<int> connectivity, int sizeOfM)
        {

            //Matrix<double> K = Matrix<double>.Build.Sparse(sizeOfM, sizeOfM);

            for (int i = 0; i < connectivity.Count; i++)
            {
                for (int j = 0; j < connectivity.Count; j++)
                {

                    //Inserting 3x3 stiffness matrix
                    for (int k = 0; k < 3; k++)
                    {
                        for (int e = 0; e < 3; e++)
                        {
                            K_tot[3 * connectivity[i] + k, 3 * connectivity[j] + e] = K_tot[3 * connectivity[i] + k, 3 * connectivity[j] + e] + Math.Round(K_e[3 * i + k, 3 * j + e], 8);

                        }
                    }
                }
            }
            return K_tot;
        }
    }
}