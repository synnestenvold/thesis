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
        public void StrainCalculations(List<Matrix<double>> B_e, List<Node> nodes_e)
        {

            List<Vector<double>> elementStrain = new List<Vector<double>>();
            Vector<double> u_e = Vector<double>.Build.Dense(24);

            //double g = Math.Sqrt(3);
            //List<List<double>> gaussPoints = new List<List<double>>()
            //{
            //    new List<double>() { -g, -g, -g },
            //    new List<double>() { g, -g, -g },
            //    new List<double>() { g, g, -g },
            //    new List<double>() { -g, g, -g },
            //    new List<double>() { -g, -g, g },
            //    new List<double>() { g, -g, g },
            //    new List<double>() { g, g, g },
            //    new List<double>() { -g, g, g },
            //};

            for (int i = 0; i < nodes_e.Count; i++)
            {
                List<double> deformations = nodes_e[i].GetDeformation();
                u_e[i * 3] = deformations[0];
                u_e[i * 3 + 1] = deformations[1];
                u_e[i * 3 + 2] = deformations[2];
            }

            for (int j=0; j< B_e.Count; j++)
            {
                Vector<double> nodeStrain = B_e[j].Multiply(u_e); /// IN GAUSS POINTS
                elementStrain.Add(nodeStrain);
                //nodes_e[j].SetStrain(nodeStrain);
            }
            
            for (int i = 0; i < elementStrain.Count; i++)
            {
                Vector<double> intStrain = InterpolateStrain(elementStrain);
                nodes_e[i].SetStrain(intStrain); //INTERPOLATED 
            }

        }

        public Vector<double> InterpolateStrain(List<Vector<double>> gaussStrain)
        {
            double k = -Math.Sqrt(3);
            double e = -Math.Sqrt(3);
            double z = -Math.Sqrt(3);

            List<double> shapeF = new List<double> {
            (double)1 / 8 * ((1 - k) * (1 - e) * (1 - z)),
            (double)1 / 8 * ((1 + k) * (1 - e) * (1 - z)),
            (double)1 / 8 * ((1 + k) * (1 + e) * (1 - z)),
            (double)1 / 8 * ((1 - k) * (1 + e) * (1 - z)),
            (double)1 / 8 * ((1 - k) * (1 - e) * (1 + z)),
            (double)1 / 8 * ((1 + k) * (1 - e) * (1 + z)),
            (double)1 / 8 * ((1 + k) * (1 + e) * (1 + z)),
            (double)1 / 8 * ((1 - k) * (1 + e) * (1 + z)),
            };
            Vector<double> strainNode = Vector<double>.Build.Dense(6);
           
            for (int i = 0; i <  strainNode.Count; i++)
            {
                for(int j = 0; j < shapeF.Count; j++)
                {
                    strainNode[i] += gaussStrain[j][i] * shapeF[j];
                }

            }

            return strainNode;

        }
    }
}
