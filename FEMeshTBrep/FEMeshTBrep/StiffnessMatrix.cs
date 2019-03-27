using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using System.Linq;

namespace FEMeshTBrep
{
    class StiffnessMatrix
    {

        private double E = 0;
        private double nu = 0;

        public StiffnessMatrix(double _E, double _nu)
        {
            E = _E;
            nu = _nu;

        }

       

        public Tuple<Matrix<double>, List<Matrix<Double>>> CreateMatrix(List<GH_Point> pList)

        {
            //double[,] array = new double[6, 4];
            //array = fillZeros(array);

            //Matrix<double> Ke_test = Matrix<double>.Build.Sparse(24, 24);
            Matrix<double> Ke = Matrix<double>.Build.Dense(24, 24);
            List<Matrix<double>> Be = new List<Matrix<double>>();


            //3D Constitutive matrix: C
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


            //Gauss points
            Vector<double> gaussPoints = DenseVector.OfArray(new double[] { -1 / Math.Sqrt(3), 1 / Math.Sqrt(3) }); //Gauss points

            Point3d point = new Point3d(0, 0, 0);
            List<Point3d> pNatural = Enumerable.Repeat(point, 8).ToList();

            Point3d centroid = new Point3d(0, 0, 0);

            if (IsRectangle(pList))
            {
                centroid = FindCentroidRectangle(pList);
            }
            else
            {
                
                centroid = FindCentroidTwisted(pList);
            }
           
            

            for (int i = 0; i < pList.Count; i++)
            {
                Point3d pointA = new Point3d(pList[i].Value.X - centroid.X, pList[i].Value.Y - centroid.Y, pList[i].Value.Z - centroid.Z);
                pNatural[i] = pointA;

            }

            //Center points for each line
            Matrix<double> coordinates = DenseMatrix.OfArray(new double[,]
            {
                {pNatural[0].X,pNatural[0].Y , pNatural[0].Z},
                {pNatural[1].X,pNatural[1].Y , pNatural[1].Z},
                {pNatural[2].X,pNatural[2].Y , pNatural[2].Z},
                {pNatural[3].X,pNatural[3].Y , pNatural[3].Z},
                {pNatural[4].X,pNatural[4].Y , pNatural[4].Z},
                {pNatural[5].X,pNatural[5].Y , pNatural[5].Z},
                {pNatural[6].X,pNatural[6].Y , pNatural[6].Z},
                {pNatural[7].X,pNatural[7].Y , pNatural[7].Z},

            });


            //Numerical integration
            foreach (double g3 in gaussPoints)
            {
                foreach (double g2 in gaussPoints)
                {
                    foreach (double g1 in gaussPoints)
                    {

                        //Shape functions
                        Matrix<double> shapeF = DenseMatrix.OfArray(new double[,]
                       {
                            {-(1-g2)*(1-g3), (1-g2)*(1-g3), (1+g2)*(1-g3),-(1+g2)*(1-g3),-(1-g2)*(1+g3),(1-g2)*(1+g3),(1+g2)*(1+g3),-(1+g2)*(1+g3)},
                            {-(1-g1)*(1-g3), -(1+g1)*(1-g3), (1+g1)*(1-g3),(1-g1)*(1-g3),-(1-g1)*(1+g3),-(1+g1)*(1+g3),(1+g1)*(1+g3),(1-g1)*(1+g3)},
                            {-(1-g1)*(1-g2), -(1+g1)*(1-g2), -(1+g1)*(1+g2),-(1-g1)*(1+g2),(1-g1)*(1-g2),(1+g1)*(1-g2),(1+g1)*(1+g2),(1-g1)*(1+g2)},

                       });

                        shapeF = shapeF.Divide(8); //Divided by 8


                        //Jacobi Matrix

                        Matrix<double> JacobiMatrix = Matrix<double>.Build.Dense(3, 3);

                        JacobiMatrix = shapeF.Multiply(coordinates);

                        // Auxiliar matrix for assemblinng of B-matrix 
                        Matrix<double> auxiliar = Matrix<double>.Build.Dense(3, 8);

                        auxiliar = JacobiMatrix.Inverse().Multiply(shapeF);


                        // B matrix
                        Matrix<double> B = Matrix<double>.Build.Dense(6, 24);

                        //First three rows

                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j <= 7; j++)
                            {
                                B[i, 3 * j + 1 + (i - 1)] = auxiliar[i, j];
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
                            B[5, 3 * j + 2] = auxiliar[0, j];
                        }

                        
                        Be.Add(B);
                        
                        //Adding the stiffness matrix. Ke = Ke + B'*C*B*Det(JacobiMatrix)
                        Ke = Ke.Add(B.Transpose().Multiply(C).Multiply(B).Multiply(JacobiMatrix.Determinant()));
                       // Be = Be.Add(B);

                    }

                }
            }
            
            Matrix<double> B_2 = Be[2];
            Be[2] = Be[3];
            Be[3] = B_2;
            Matrix<double> B_6 = Be[6];
            Be[6] = Be[7];
            Be[7] = B_6;
            

            return Tuple.Create(Ke, Be);

        }

        public Boolean IsRectangle(List<GH_Point> pList)
        {
            double lx1 = pList[0].Value.DistanceTo(pList[1].Value);
            double lx2 = pList[3].Value.DistanceTo(pList[2].Value);
            double lx3 = pList[4].Value.DistanceTo(pList[5].Value);
            double lx4 = pList[7].Value.DistanceTo(pList[6].Value);

            double ly1 = pList[0].Value.DistanceTo(pList[3].Value);
            double ly2 = pList[1].Value.DistanceTo(pList[2].Value);
            double ly3 = pList[4].Value.DistanceTo(pList[7].Value);
            double ly4 = pList[5].Value.DistanceTo(pList[6].Value);

            double lz1 = pList[0].Value.DistanceTo(pList[4].Value);
            double lz2 = pList[1].Value.DistanceTo(pList[5].Value);
            double lz3 = pList[2].Value.DistanceTo(pList[6].Value);
            double lz4 = pList[3].Value.DistanceTo(pList[7].Value);

            if(lx1==lx2 && lx3 == lx4 && ly1==ly2 && ly3==ly4 && lz1 == lz4 && lz2 == lz3)
            {
                return true;
            }

            return false;
        }

        public Point3d FindCentroidTwisted(List<GH_Point> pList)
        {

            Mesh mesh = new Mesh();

            for (int i = 0; i < pList.Count; i++)
            {
                mesh.Vertices.Add(pList[i].Value);
            }

            mesh.Faces.AddFace(0, 3, 2, 1); //Bottom
            mesh.Faces.AddFace(4, 5, 6, 7); //Top
            mesh.Faces.AddFace(2, 3, 7, 6); //Back
            mesh.Faces.AddFace(0, 1, 5, 4); //Front
            mesh.Faces.AddFace(1, 2, 6, 5); //Right
            mesh.Faces.AddFace(0, 4, 7, 3); //Left

            Brep brep = Brep.CreateFromMesh(mesh, true);
            VolumeMassProperties vmp = VolumeMassProperties.Compute(brep);
            Point3d centroid = vmp.Centroid;

            return centroid;
        }

        public Point3d FindCentroidRectangle(List<GH_Point> pList)
        {

            double c_x = 0;
            double c_y = 0;
            double c_z = 0;

            foreach (GH_Point p in pList)
            {
                c_x += p.Value.X;
                c_y += p.Value.Y;
                c_z += p.Value.Z;
            }

            Point3d centroid = new Point3d(c_x / 8, c_y / 8, c_z / 8);

            return centroid;
        }

        static void Main(string[] args)
        {
            //StiffnessMatrix s = new StiffnessMatrix(10, 10, 10, 10, 10);

        }


    }
}
