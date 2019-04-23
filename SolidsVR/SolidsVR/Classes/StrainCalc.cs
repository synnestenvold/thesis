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
    class StrainCalc
    {
        public List<Vector<double>> StrainCalculations(List<Matrix<double>> B_e, Vector<double> u, List<int> c_e, List<Node> nodes_e)
        {
            
            List<Vector<double>> strain = new List<Vector<double>>();
            Vector<double> u_e = Vector<double>.Build.Dense(24);
           

            for (int i = 0; i < nodes_e.Count; i++)
            {
                List<double> deformations = nodes_e[i].GetDeformation();
                u_e[i * 3] = deformations[0];
                u_e[i * 3 + 1] = deformations[1];
                u_e[i * 3 + 2] = deformations[2];
            }


            for (int j=0; j< B_e.Count; j++)
            {
                Vector<double> nodeStrain = B_e[j].Multiply(u_e);
                nodes_e[j].SetStrain(nodeStrain);
                strain.Add(nodeStrain);
            }

            return strain;

        }
        
    }
}
