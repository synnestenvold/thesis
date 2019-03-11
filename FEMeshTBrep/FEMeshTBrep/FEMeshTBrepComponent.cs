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

namespace FEMeshTBrep
{
    public class FEMeshTBrepComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        /// 

        double E = 10;
        double nu = 0.3;

        public FEMeshTBrepComponent()
          : base("Finite Element of meshed TBrep", "FEMeshTBrep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Connectivity", "C", "Relationship between local and global numbering", GH_ParamAccess.tree);
            pManager.AddPointParameter("Nodes", "N", "Coordinates for corner nodes in brep", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Boundary conditions", "BC", "Nodes that are constrained", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strain", "Strain", "Strain vector", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stress", "Stress", "Stress vector", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Integer> treeConnectivity = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Integer> treePoints = new GH_Structure<GH_Integer>();
            List<GH_Integer> bcNodes = new List<GH_Integer>();

            if (!DA.GetDataTree(0, out treeConnectivity)) return;
            if (!DA.GetDataTree(1, out treePoints)) return;
            if (!DA.GetDataList(2, bcNodes)) return;


            //Finding stiffness matrix for all the small elements, assuming E=10 and nu=0.3
            StiffnessMatrix sm = new StiffnessMatrix(E, nu);
            //Bmatrix bm = new Bmatrix(lx, ly, lz);
            Assembly_StiffnessMatrix aSM = new Assembly_StiffnessMatrix();

            // Temporary way of finding the size of stiffness matrix and B matrix
            int max = 0;

            for (int i = 0; i < treeConnectivity.PathCount; i++)
            {
                List<GH_Integer> cNodes = (List<GH_Integer>)treeConnectivity.get_Branch(i);

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

             //a dense matrix stored in an array, column major.
            //Matrix<double> B_e = bm.CreateMatrix();



            for (int i = 0; i < treeConnectivity.PathCount; i++)
            {
                List<GH_Integer> connectedNodes = (List<GH_Integer>)treeConnectivity.get_Branch(i);
                List<Point3d> connectedPoints = (List<Point3d>)treePoints.get_Branch(i);

                Point3d centroid = something;

                Matrix<double> K_e = sm.CreateMatrix(connectedPoints, centroid);

                K_i = aSM.assemblyMatrix(K_e, connectedNodes, sizeOfM);
                K_tot = K_tot + K_i;

            }
            //Boundary condition

            K_tot = applyBC(K_tot, bcNodes);

            Matrix<double> K_tot_inverse = K_tot.Inverse();

            //Force vector R
            double[] R_array = new double[sizeOfM];
            R_array[12] = 10;
            var V = Vector<double>.Build;
            var R = V.DenseOfArray(R_array);


            //Caluculation of the displacement vector u
            Vector<double> u = K_tot_inverse.Multiply(R);


            Cmatrix C_new = new Cmatrix(E, nu);
            Matrix<double> C = C_new.CreateMatrix();

            StrainCalc sC = new StrainCalc();
            Vector<double> strain = Vector<double>.Build.Dense(6);
            Vector<double> stress = Vector<double>.Build.Dense(6);

            DataTree<double> treeStrain = new DataTree<double>();
            DataTree<double> treeStress = new DataTree<double>();

            /*
            //For calculating the strains and stress
            for (int i = 0; i < tree.PathCount; i++)
            {
                List<GH_Integer> connectedNodes = (List<GH_Integer>)tree.get_Branch(i);

                strain = sC.calcStrain(B_e, u, connectedNodes);
                treeStrain.AddRange(strain, new GH_Path(new int[] { 0, i }));

                stress = C.Multiply(strain);
                treeStress.AddRange(stress, new GH_Path(new int[] { 0, i }));

            }
            //Finding strain
            //Vector<double> strain = B_tot.Multiply(u);

            //Finding stress
            //Vector<double> stress = C.Multiply(strain);
            */

            DA.SetDataList(0, u);
            //DA.SetDataTree(1, treeStrain);
            //DA.SetDataTree(2, treeStress);


            /*
            GH_Boolean => Boolean
            GH_Integer => int
            GH_Number => double
            GH_Vector => Vector3d
            GH_Matrix => Matrix
            GH_Surface => Brep
            */

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

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cec65985-58a5-4ff1-a238-e4761e0abbeb"); }
        }
    }
}
