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

        public Matrix<double> assemblyMatrix(Matrix<double> Ke, List<GH_Integer> C)
        {

            Matrix<double> K = Matrix<double>.Build.Dense(81, 81);

            for (int i = 0; i < C.Count; i++)
            {
                for (int j = 0; j < C.Count; j++)
                {

                    //Inserting 3x3 stiffness matri
                    for (int k = 0; k < 3; k++)
                    {
                        for (int e = 0; e < 3; e++)
                        {
                            K[3 * C[i] + k, 3 * C[j] + e] = Ke[3 * i + k, 3 * j + e];
                             
                        }
                    }
                }
            }
            return K;
        }

    }
}
