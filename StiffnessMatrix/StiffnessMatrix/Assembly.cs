using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace StiffnessMatrix
{
    class Assembly
    {

        public Matrix<double> assemblyMatrix(Matrix<double> Ke, int[] C_)
        {
           
            Matrix<double> K = Matrix<double>.Build.Dense(81, 81);
            int[] C = C_;


            for (int i = 0; i<C.Length; i++)
            {
                for(int j = 0; j<C.Length; j++)
                {

                    //int a = C[i];
                    //int b = C[j];

                    //Console.WriteLine("Ci: " + a + " Cj: " + b);

                    //Inserting 3x3 stiffness matri
                    for (int k = 0; k < 3; k++)
                    {
                        for (int e = 0; e<3; e++)
                        {
                            K[3 * C[i] + k, 3 * C[j] + e] = Ke[3 * i + k, 3 * j + e];
                            //Console.WriteLine("Ki: " + (3 * a + k) + " Kj: " + (3 *b + e) + " Kei: " + (3 * i + k) + " Kej: " + (3 * j + e)); 
                        }
                    }
                }
            }


            
            for (int i = 0; i < K.RowCount; i++)

            {
                
                Console.WriteLine("Row: " + (i+1));
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    Console.Write("{0:0.00}", K[i, j]);
                    Console.Write("| ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.ReadKey();
            return K;
        }

    }
}
