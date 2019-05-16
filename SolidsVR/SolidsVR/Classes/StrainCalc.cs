﻿using System;
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
        public void StrainCalculations(List<Matrix<double>> B_e, List<Node> nodes_e, Matrix<double> C_Matrix)
        {

            List<Vector<double>> elementStrain = new List<Vector<double>>();
            List<Vector<double>> elementStress= new List<Vector<double>>();
            Vector<double> u_e = Vector<double>.Build.Dense(24);

            
            double g = Math.Sqrt(3);
            List<List<double>> gaussPoints = new List<List<double>>()
            {
                new List<double>() { -g, -g, -g },
                new List<double>() { g, -g, -g },
                new List<double>() { g, g, -g },
                new List<double>() { -g, g, -g },
                new List<double>() { -g, -g, g },
                new List<double>() { g, -g, g },
                new List<double>() { g, g, g },
                new List<double>() { -g, g, g },
            };
            

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
                Vector<double> nodeStress= C_Matrix.Multiply(nodeStrain); /// IN GAUSS POINTS
                elementStrain.Add(nodeStrain);
                elementStress.Add(nodeStress);
                nodes_e[j].SetStrain(nodeStrain);
                //nodes_e[j].SetStress(nodeStress);
            }
            
            
            for (int i = 0; i < elementStress.Count; i++)
            {
                Vector<double> intStress = InterpolateStress(elementStress, gaussPoints[i]);

                nodes_e[i].SetStress(intStress); //INTERPOLATED TO NODES
            }
        }

        public Vector<double> InterpolateStress(List<Vector<double>> gaussStress, List<double> gaussPoints)
        {
            double r = gaussPoints[0];
            double s = gaussPoints[1];
            double t = gaussPoints[2];

            List<double> shapeF = new List<double> {
            (double)1 / 8 * ((1 - r) * (1 - s) * (1 - t)),
            (double)1 / 8 * ((1 + r) * (1 - s) * (1 - t)),
            (double)1 / 8 * ((1 + r) * (1 + s) * (1 - t)),
            (double)1 / 8 * ((1 - r) * (1 + s) * (1 - t)),
            (double)1 / 8 * ((1 - r) * (1 - s) * (1 + t)),
            (double)1 / 8 * ((1 + r) * (1 - s) * (1 + t)),
            (double)1 / 8 * ((1 + r) * (1 + s) * (1 + t)),
            (double)1 / 8 * ((1 - r) * (1 + s) * (1 + t)),
            };
            Vector<double> stressNode = Vector<double>.Build.Dense(6);
           
            for (int i = 0; i <  stressNode.Count; i++)
            {
                for(int j = 0; j < shapeF.Count; j++)
                {
                    stressNode[i] += gaussStress[j][i] * shapeF[j];
                }

            }
            return stressNode;

        }
    }
}
