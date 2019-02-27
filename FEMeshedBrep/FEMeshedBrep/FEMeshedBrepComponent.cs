﻿using System;
using System.Collections.Generic;
using System.Collections;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using Grasshopper;
using Grasshopper.Kernel.Data;
using System.Collections;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace FEMeshedBrep
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
            pManager.AddIntegerParameter("Nodes", "N", "List of new node numbering", GH_ParamAccess.tree);
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
        
            GH_Structure<GH_Integer> tree = new GH_Structure<GH_Integer>();
            List<double> lengths = new List<double>();

            if (!DA.GetDataTree(0, out tree)) return;
            if (!DA.GetDataList(1, lengths)) return;
            
            double lx = lengths[0];
            double ly = lengths[1];
            double lz = lengths[2];

            StiffnessMatrix2 sm = new StiffnessMatrix2(10, 0.3, lx, ly, lz);
            Bmatrix bm = new Bmatrix(lx, ly, lz);
            Assembly_StiffnessMatrix aSM = new Assembly_StiffnessMatrix();

            // Temporary way of finding the size of stiffness matrix and B matrix
            int max = 0;

            for (int i = 0; i < tree.PathCount; i++)
            {
                List<GH_Integer> cNodes = (List<GH_Integer>)tree.get_Branch(i);

                for (int j = 0; j < cNodes.Count; j++)
                {
                    if (cNodes[j].Value > max)
                    {
                        max = cNodes[j].Value;
                    }
                }

            }

            int sizeOfM = 3 * (max + 1);


            //Create K_tot
            Matrix<double> K_i = Matrix<double>.Build.Dense(sizeOfM, sizeOfM);
            Matrix<double> K_tot = Matrix<double>.Build.Dense(sizeOfM, sizeOfM);
            //Matrix<double> B_i = Matrix<double>.Build.Dense(6, sizeOfM);
            //Matrix<double> B_tot = Matrix<double>.Build.Dense(6, sizeOfM);

            Matrix<double> K_e = sm.CreateMatrix(); //a dense matrix stored in an array, column major.
            //Matrix<double> B_e = bm.CreateMatrix();

           

            for (int i = 0; i< tree.PathCount; i++)
            {
                List<GH_Integer> connectedNodes = (List<GH_Integer>)tree.get_Branch(i);

                K_i = aSM.assemblyMatrix(K_e, connectedNodes, sizeOfM);
                K_tot = K_tot + K_i;
                //B_i = aSM.assemblyMatrix(B_e, connectedNodes);
                //B_tot = B_tot + B_i;

            }

            Matrix<double> K_tot_inverse = K_tot.Inverse();
            //double[] R_array = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1, 0, 0, -1, 0, 0, -1 };
            double[] R_array = new double[sizeOfM];
            Array.Clear(R_array, 0, R_array.Length);

            R_array[0] = 1000;
            R_array[18] = -1000;


            var V = Vector<double>.Build;
            var R = V.DenseOfArray(R_array);
            Vector<double> u = K_tot_inverse.Multiply(R);

            for (int i = 0; i < tree.PathCount; i++)
            {
                List<GH_Integer> connectedNodes = (List<GH_Integer>)tree.get_Branch(i);

                //B_i = aSM.assemblyMatrix(B_e, connectedNodes);
                //B_tot = B_tot + B_i;

            }



            double[] u1 = new double[] { u[0], u[1], u[2] };

            //Finding strain
            
            //Vector<double> strain = B_tot.Multiply(u);

            //Finding stress
            Cmatrix C_new = new Cmatrix(10, 0.3);
            Matrix<double> C = C_new.CreateMatrix();
            //Vector<double> stress = C.Multiply(strain);
            //TODO: Fix tree structure, we want a list with 3 components: three deformations in a list, stress and strain.
            //List<List<double>> list = new List<List<double>> { u1, { 5 }, { 5 } };
            //DA.SetDataList(0, u1);
            DA.SetDataList(0, u);
            //DA.SetDataList(1, strain);
            //DA.SetDataList(2, stress);
  

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
