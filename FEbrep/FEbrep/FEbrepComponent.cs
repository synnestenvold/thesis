using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using System.Linq;

//This solution is outdated. Use FEMeshedBrep instead.
namespace FEbrep
{
    public class FEbrepComponent : GH_Component
    {
        public FEbrepComponent()
          : base("Finite Element of brep", "FEbep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brepfs", "B", "Input Brep as a cube", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Boundary conditions", "BC", "Nodes that are constrained", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strain", "Strain", "Strain vector", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stress", "Stress", "Stress vector", GH_ParamAccess.list);

        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Brep brp = new Brep();
            Point3d point = new Point3d(0, 0, 0);


            
            List<GH_Integer> bcNodes = new List<GH_Integer>();
            Point3d[] array = new Point3d[8];
            List<Point3d> pList = Enumerable.Repeat(point, 8).ToList();

            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetDataList(1, bcNodes)) return;

            //Finding lengths of the Brep
            array = brp.DuplicateVertices();


            ///// Sorting points into similar for every brep taking in
            for (int i = 0; i < array.Length; i++)
            {
                pList[i] = array[i];
            }

            pList = sortList(pList);

            //Finding center of brep
            VolumeMassProperties vmp = VolumeMassProperties.Compute(brp);
            Point3d centroid = vmp.Centroid;

            double lx = pList[0].DistanceTo(pList[1]);
            double ly = pList[0].DistanceTo(pList[3]);
            double lz = pList[0].DistanceTo(pList[4]);


            //Creating K, using the StiffnessMatrix2 class, with the lengths as input
            
            //StiffnessMatrix K_new = new StiffnessMatrix(10, 0.3, lx, ly,lz);
            StiffnessMatrix2 K_new = new StiffnessMatrix2(210000, 0.3, lx, ly, lz);
            //StiffnessMatrix3 K_new = new StiffnessMatrix3(10, 0.3, pList,centroid);

            Matrix<double> Ke = K_new.CreateMatrix(); //A dense matrix stored in an array, column major.
            
            //Boundary condition
            //int[] bcNodes = new int[] { 0,1,2,3,4,5,6,7,8,9,10,11 };
            Ke = applyBC(Ke, bcNodes);

            Matrix<double> Ke_inverse = Ke.Inverse();

            //Force vector R
            double[] R_array = new double[] { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,-1,0,0,0,0,0,0,0,0,0 };
            var V = Vector<double>.Build;
            var R = V.DenseOfArray(R_array);
            

            //Caluculation of the displacement vector u
            Vector<double> u = Ke_inverse.Multiply(R);

            //Finding strain using the B matrix
            Bmatrix B_new = new Bmatrix(lx, ly, lz);
            Matrix<double> B = B_new.CreateMatrix();
            Vector<double> strain = B.Multiply(u);

            //Finding stress using the C matrix
            Cmatrix C_new = new Cmatrix(10, 0.3);
            Matrix<double> C = C_new.CreateMatrix();
            Vector<double> stress = C.Multiply(strain);

            DA.SetDataList(0, u);
            DA.SetDataList(1, strain);
            DA.SetDataList(2, stress);

        }

        public Matrix<double> applyBC(Matrix<double> K, List<GH_Integer> bcNodes)
        {
            for (int i = 0; i < bcNodes.Count; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    if (bcNodes[i].Value != j)
                    {
                        K[bcNodes[i].Value, j] = 0;
                    }

                }

                for (int j = 0; j < K.RowCount; j++)
                {
                    if (bcNodes[i].Value != j)
                    {
                        K[j, bcNodes[i].Value] = 0;
                    }

                }
            }

            return K;
        }

        public List<Point3d> sortList(List<Point3d> pList)
        {
            Point3d point = new Point3d(0, 0, 0);
            List<Point3d> pListCorrect = Enumerable.Repeat(point, 8).ToList();

            pList.Sort();

            pListCorrect[0] = pList[0];
            pListCorrect[1] = pList[4];
            pListCorrect[2] = pList[6];
            pListCorrect[3] = pList[2];
            pListCorrect[4] = pList[1];
            pListCorrect[5] = pList[5];
            pListCorrect[6] = pList[7];
            pListCorrect[7] = pList[3];

            return pListCorrect;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("88eb5711-a923-44d9-af91-eb976d7b191e"); }
        }
    }
}
