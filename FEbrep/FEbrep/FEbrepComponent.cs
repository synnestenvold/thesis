using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace FEbrep
{
    public class FEbrepComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        /// test
        public FEbrepComponent()
          : base("Finite Element of brep", "FEbep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brepfs", "B", "Input Brep as a cube", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Node 1", "1", "Displacement, stress and strain for node 1", GH_ParamAccess.list);
            pManager.AddNumberParameter("Node 2", "2", "Displacement, stress and strain for node 2", GH_ParamAccess.item);
            pManager.AddNumberParameter("Node 3", "3", "Displacement, stress and strain for node 3", GH_ParamAccess.item);

        }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        /// 
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> listaCrv = new List<Curve>();

            Brep brp = new Brep();
            Point3d[] array = new Point3d[8];
       
            if (!DA.GetData(0, ref brp)) return;

            array = brp.DuplicateVertices();

            //Creates an array with the three lengths of the cube, in x y and z direction.
            double[] lengths = new double[3];
            lengths[0] = array[0].DistanceTo(array[1]);
            lengths[1] = array[0].DistanceTo(array[3]);
            lengths[2] = array[0].DistanceTo(array[4]);

            StiffnessMatrix K = new StiffnessMatrix(200000, 0.3, lengths[0], lengths[1], lengths[2]);
            Matrix<double> Ke = K.createMatrix(); //a dense matrix stored in an array, column major.
            Matrix<double> Ke_inverse = Ke.Inverse();
            double[] R_array = new double[] { 0,0,1,0,0,1,0,0,1,0,0,1,0,0,-1,0,0,-1,0,0,-1,0,0,-1 };
            var V = Vector<double>.Build;
            var R = V.DenseOfArray(R_array);
            Vector<double> u = Ke_inverse.Multiply(R);
            double[] u1 = new double[] { u[0],u[1],u[2] };
            //TODO: Fix tree structure, we want a list with 3 components: three deformations in a list, stress and strain.
            //List<List<double>> list = new List<List<double>> { u1, { 5 }, { 5 } };
            //DA.SetDataList(0, u1);
            DA.SetData(0, u[0]);
            DA.SetData(1, u[1]);
            DA.SetData(2, u[2]);
     
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

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("88eb5711-a923-44d9-af91-eb976d7b191e"); }
        }
    }
}
