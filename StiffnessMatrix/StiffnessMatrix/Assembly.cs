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

        public Matrix<double> assemblyMatrix(Matrix<double> Ke)
        {
            //double[,] array = new double[6, 4];
            //array = fillZeros(array);

            Matrix<double> K = Matrix<double>.Build.Dense(81, 81);

            int [,] C = new int[,]
            
            {
                {1,2,9,8,19,11,18,17}

            };

            for (int i = 0; i<C.GetLength(1); i++)
            {
                for(int j = 0; j<C.GetLength(1); j++)
                {
                    //Console.WriteLine("i,j :" + i + " " + j);


                    int a = C[0, i] - 1;
                    int b = C[0, j] - 1;

                    //Console.WriteLine("a,b "+ a + " " + b);

                    for (int k = 0; k < 3; k++)
                    {
                        for (int e = 0; e<3; e++)
                        {
                            int c = 3 * a + k;
                            int d = 3 * b + e;

                            int f = 3 * i + k;
                            int g = 3 * j + e;

                            //Console.WriteLine("K :" + c + " " + d);
                            //Console.WriteLine("Ke :" + f+ " " +g) ;
                            //Console.WriteLine("");
                            K[3 * a + k, 3 * b + e] = Ke[3 * i + k, 3 * j + e];
                        }
                    }
                }
            }

            for (int i = 0; i < K.RowCount; i++)
            {
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
