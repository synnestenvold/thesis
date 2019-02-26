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

        public Matrix<double> assemblyMatrix(Matrix<double> Ke, List<int> C)
        {
            
            Matrix<double> K = Matrix<double>.Build.Dense(81, 81);
            
            


            for (int i = 0; i<C.Count; i++)
            {
                for(int j = 0; j<C.Count; j++)
                {

                    int a = C[i];
                    int b = C[j];


                    //Inserting 3x3 stiffness matri
                    for (int k = 0; k < 3; k++)
                    {
                        for (int e = 0; e<3; e++)
                        {
                            K[3 * C[i] + k, 3 * C[j] + e] = Ke[3 * i + k, 3 * j + e];
                            Console.WriteLine("K: (" + (3 * a + k)+","+ (3 *b + e) +") = " +"K: (" + (3 * i + k) +","+(3 * j + e)+")"); 
                        }
                    }
                }
            }


            /*
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
            */

            Console.ReadKey();
            return K;
        
        
            

           
        }
    }
}
