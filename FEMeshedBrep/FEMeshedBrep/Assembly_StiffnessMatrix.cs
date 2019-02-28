using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Grasshopper.Kernel.Types;

namespace FEMeshedBrep
{
    class Assembly_StiffnessMatrix
    {

        public Matrix<double> assemblyMatrix(Matrix<double> K_e, List<GH_Integer> connectivity, int sizeOfM)
        {

            Matrix<double> K = Matrix<double>.Build.Dense(sizeOfM, sizeOfM);

            for (int i = 0; i < connectivity.Count; i++)
            {
                for (int j = 0; j < connectivity.Count; j++)
                {

                    //Inserting 3x3 stiffness matrix
                    for (int k = 0; k < 3; k++)
                    {
                        for (int e = 0; e < 3; e++)
                        {
                            K[3 * connectivity[i].Value + k, 3 * connectivity[j].Value + e] = K_e[3 * i + k, 3 * j + e];
                             
                        }
                    }
                }
            }
            return K;
        }

    }
}
