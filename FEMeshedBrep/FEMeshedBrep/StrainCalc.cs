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
    class StrainCalc
    {
        public Vector<double> calcStrain(Matrix<double> B_e, Vector<double> u, List<GH_Integer> connectivity)
        {

            Vector <double> strain = Vector<double>.Build.Dense(6);
            Vector<double> u_e = Vector<double>.Build.Dense(24);
     

            for (int i = 0; i < connectivity.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    u_e[3 * i + j] = u[connectivity[i].Value * 3 + j];
                }

            }

            strain = B_e.Multiply(u_e);

            return strain;
            
        }
        

    }
}
