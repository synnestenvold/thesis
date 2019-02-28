using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

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
            Point3d[] array = new Point3d[8];
       
            if (!DA.GetData(0, ref brp)) return;

            //Finding lengths of the Brep
            array = brp.DuplicateVertices();
            double lx = array[0].DistanceTo(array[1]);
            double ly = array[0].DistanceTo(array[3]);
            double lz = array[0].DistanceTo(array[4]);

            //Creating K, using the StiffnessMatrix2 class, with the lengths as input
            StiffnessMatrix2 K_new = new StiffnessMatrix2(10, 0.3, lx, ly, lz);
            Matrix<double> Ke = K_new.CreateMatrix(); //A dense matrix stored in an array, column major.
            Matrix<double> Ke_inverse = Ke.Inverse();

            //Force vector R
            double[] R_array = new double[] { 10,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };
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
