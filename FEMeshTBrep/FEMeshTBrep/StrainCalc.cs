using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Grasshopper.Kernel.Types;

namespace FEMeshTBrep
{
    class StrainCalc
    {
        public List<Vector<double>> calcStrain(List<Matrix<double>> B_e, Vector<double> u, List<GH_Integer> c_e)
        {
            
            List<Vector<double>> strain = new List<Vector<double>>();
            Vector<double> u_e = Vector<double>.Build.Dense(24);


            for (int i = 0; i < c_e.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    u_e[3 * i + j] = u[c_e[i].Value * 3 + j];
                }
            }

            for (int j=0; j< B_e.Count; j++)
            {
                strain.Add(B_e[j].Multiply(u_e));
            }

            return strain;

        }
        
    }
}
