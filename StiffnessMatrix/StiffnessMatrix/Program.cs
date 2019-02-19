﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace StiffnessMatrix
{
    class StiffnessMatrix
    {

        private double E = 0;
        private double nu = 0;
        private double lx = 0;
        private double ly = 0;
        private double lz = 0;

        public StiffnessMatrix(double _E, double _nu, double _lx, double _ly, double _lz)
        {
            E = _E;
            nu = _nu;
            lx = _lx;
            ly = _ly;
            lz = _lz;
        }

        public void matrix()
        {
            double[,] array = new double[6, 4];
            array = fillZeros(array);

            Matrix<double> Ke = Matrix<double>.Build.Dense(24, 24);


            //3D Constitutive matrix
            double value = E / ((1 + nu) * (1 - 2 * nu));
            Matrix<double> C = DenseMatrix.OfArray(new double[,]
            {
                {1-nu, nu, nu, 0, 0, 0},
                {nu, 1-nu, nu, 0, 0, 0},
                {nu, nu, 1-nu, 0, 0, 0},
                {0, 0, 0, (1-2*nu)/2, 0, 0},
                {0, 0, 0, 0, (1-2*nu)/2, 0},
                {0, 0, 0, 0, 0, (1-2*nu)/2},

            });

           
            C = C.Multiply(value); //Constitutive matrix

            

            Vector<double> gaussPoints = DenseVector.OfArray(new double[] { -1 / Math.Sqrt(3), 1 / Math.Sqrt(3) }); //Gauss points

            Matrix<double> coordinates = DenseMatrix.OfArray(new double[,]
            {
                {-lx/2, -ly/2, -lz/2},
                {lx/2, -ly/2, -lz/2},
                {lx/2, ly/2, -lz/2},
                {-lx/2, ly/2, -lz/2},
                {-lx/2, -ly/2, lz/2},
                {lx/2, -ly/2, lz/2},
                {lx/2, ly/2, lz/2},
                {-lx/2, ly/2, lz/2},

            });

            
            
            
            
            foreach (double g1 in gaussPoints)
            {
                foreach (double g2 in gaussPoints)
                {
                    foreach (double g3 in gaussPoints)
                    {
                        Matrix<double> shapeF = DenseMatrix.OfArray(new double[,]
                       {
                            {-(1-g2)*(1-g3), (1-g2)*(1-g3), (1+g2)*(1-g3),-(1+g2)*(1-g3),-(1-g2)*(1+g3),(1-g2)*(1+g3),(1+g2)*(1+g3),-(1+g2)*(1+g3)},
                            {-(1-g1)*(1-g3), -(1+g1)*(1-g3), (1+g1)*(1-g3),(1-g1)*(1-g3),-(1-g1)*(1+g3),-(1+g1)*(1+g3),(1+g1)*(1+g3),(1-g1)*(1+g3)},
                            {-(1-g1)*(1-g2), -(1+g1)*(1-g2), -(1+g1)*(1+g2),-(1-g1)*(1+g2),(1-g1)*(1-g2),(1+g1)*(1-g2),(1+g1)*(1+g2),(1-g1)*(1+g2)},

                       });

                        shapeF = shapeF.Divide(8);
                        
                        
                        

                        

                        Matrix<double> JacobiMatrix = Matrix<double>.Build.Dense(3,3);

                        JacobiMatrix = shapeF.Multiply(coordinates);

                      

                        Matrix<double> auxiliar = Matrix<double>.Build.Dense(3, 8);

                        auxiliar = JacobiMatrix.Inverse().Multiply(shapeF);

                        
                        /*
                        for (int i = 0; i < auxiliar.RowCount; i++)
                        {
                            for (int j = 0; j < auxiliar.ColumnCount; j++)
                            {
                                Console.Write(auxiliar[i, j]);
                                Console.Write(" ");
                            }
                            Console.WriteLine();
                        }
                        Console.WriteLine();
                        */
                        

                        

                        Matrix<double> B = Matrix<double>.Build.Dense(6, 24);

                        //First three rows OK!

                        for (int i = 0; i<3; i++)
                        {
                            for(int j = 0; j<=7; j++)
                            {
                                B[i, 3 * j +1 + (i - 1)] = auxiliar[i, j];
                            }
                        }

                        

                        //Fourth row
                        for (int j = 0; j <= 7; j++)
                        {
                            B[3, 3 * j] = auxiliar[1, j];
                        }

                       
                        
                        for (int j = 0; j <= 7; j++)
                        {
                            B[3, 3 * j + 1] = auxiliar[0, j];
                        }

                        

                        //Fifth row
                        for (int j = 0; j <= 7; j++)
                       {
                           B[4, 3 * j + 2] = auxiliar[1, j];
                       }

                        for (int j = 0; j <= 7; j++)
                        {
                            B[4, 3 * j + 1] = auxiliar[2, j];
                        }
                        

                        
                        //Sixth row
                       for (int j = 0; j <= 7; j++)
                       {
                           B[5, 3 * j] = auxiliar[2, j];
                       }

                       for (int j = 0; j <= 7; j++)
                       {
                           B[5, 3 * j + 2] = auxiliar[0, j ];
                       }


                        /*
                          for (int i = 0; i < B.Transpose().RowCount; i++)
                          {
                              for (int j = 0; j < B.Transpose().ColumnCount; j++)
                              {
                                  Console.Write(B.Transpose()[i, j]);
                                  Console.Write("| ");
                              }
                              Console.WriteLine();
                          }
                          Console.WriteLine();

                          */






                        /*

                        Console.WriteLine(B.Inverse().RowCount);
                        Console.WriteLine(B.Inverse().ColumnCount);
                        Console.WriteLine(C.RowCount);
                        Console.WriteLine(C.ColumnCount);
                        */

                        


                        

                        
                        Ke = Ke.Add(B.Transpose().Multiply(C).Multiply(B).Multiply(JacobiMatrix.Determinant()));



                            

                            


                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            for (int i = 0; i < Ke.RowCount; i++)
            {
                for (int j = 0; j < Ke.ColumnCount; j++)
                {
                    Console.Write(Ke[i, j]);
                    Console.Write("| ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            /*
            for (int i = 0; i < C.RowCount; i++)
            {
                for (int j = 0; j < C.ColumnCount; j++)
                {
                    Console.Write(C[i, j]);
                }
                Console.WriteLine();
            }
           */

            Console.ReadKey();
        }

        public double[,] fillZeros(double[,] array)
        {
           
            Array.Clear(array, 0, array.Length);

            return array;
        }

        static void Main(string[] args)
        {
            StiffnessMatrix s = new StiffnessMatrix(10, 1, 5, 5, 5);
            s.matrix();

        }


    }
}
