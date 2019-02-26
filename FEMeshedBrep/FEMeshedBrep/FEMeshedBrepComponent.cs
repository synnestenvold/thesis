﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Collections;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace FEbrep
{
    public class FEbrepComponent : GH_Component
    {
        public FEbrepComponent()
          : base("Finite Element of meshed Brep", "FEMeshedBrep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Breps from MeshBox", "B", "Input Brep as a cube", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Lengths", "L", "lx, ly and lz for each cube", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strain", "Strain", "Strain vector", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stress", "Stress", "Stress vector", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        
            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number> tree = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Number>();
            List<double> lengths = new List<double>();

            if (!DA.GetDataTree(0, out tree)) return;
            if (!DA.GetDataList(1, lengths)) return;
            
            double lx = lengths[0];
            double ly = lengths[1];
            double lz = lengths[2];

            //Create K_tot

            for (int i = 0; i< tree.PathCount; i++)
            {
                IList node_list = tree.get_Branch(i);


                //TODO: Move all that you need into this for-loop
                //K_tot = K_tot + Ke.....
                
            }
            

            StiffnessMatrix2 K_new = new StiffnessMatrix2(10, 0.3, lx, ly, lz);
            Matrix<double> Ke = K_new.CreateMatrix(); //a dense matrix stored in an array, column major.
            Matrix<double> Ke_inverse = Ke.Inverse();
            double[] R_array = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, -1, 0, 0, -1 };
            var V = Vector<double>.Build;
            var R = V.DenseOfArray(R_array);
            Vector<double> u = Ke_inverse.Multiply(R);
            double[] u1 = new double[] { u[0], u[1], u[2] };

            //Finding strain
            Bmatrix B_new = new Bmatrix(lx, ly, lz);
            Matrix<double> B = B_new.CreateMatrix();
            Vector<double> strain = B.Multiply(u);

            //Finding stress
            Cmatrix C_new = new Cmatrix(10, 0.3);
            Matrix<double> C = C_new.CreateMatrix();
            Vector<double> stress = C.Multiply(strain);
            //TODO: Fix tree structure, we want a list with 3 components: three deformations in a list, stress and strain.
            //List<List<double>> list = new List<List<double>> { u1, { 5 }, { 5 } };
            //DA.SetDataList(0, u1);
            DA.SetDataList(0, u);
            DA.SetDataList(1, strain);
            DA.SetDataList(2, stress);

            /*
            GH_Boolean => Boolean
            GH_Integer => int
            GH_Number => double
            GH_Vector => Vector3d
            GH_Matrix => Matrix
            GH_Surface => Brep
            */
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("bf579881-ce88-48cf-9d76-3329422f8a25"); }
        }
    }
}
