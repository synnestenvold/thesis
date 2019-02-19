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
            pManager.AddPointParameter("Node 1", "1", "Displacement, stress and strain for node 1", GH_ParamAccess.item);
            pManager.AddPointParameter("Node 2", "2", "Displacement, stress and strain for node 2", GH_ParamAccess.item);
            pManager.AddPointParameter("Node 3", "3", "Displacement, stress and strain for node 3", GH_ParamAccess.item);
            pManager.AddPointParameter("Node 4", "4", "Displacement, stress and strain for node 4", GH_ParamAccess.item);
            pManager.AddPointParameter("Node 5", "5", "Displacement, stress and strain for node 5", GH_ParamAccess.item);
            pManager.AddPointParameter("Node 6", "6", "Displacement, stress and strain for node 6", GH_ParamAccess.item);
            pManager.AddPointParameter("Node 7", "7", "Displacement, stress and strain for node 7", GH_ParamAccess.item);
            pManager.AddPointParameter("Node 8", "8", "Displacement, stress and strain for node 8", GH_ParamAccess.item);
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

            Console.WriteLine(array[0].DistanceTo(array[1]));

            //Creates an array with the three lengths of the cube, in x y and z direction.
            double[] lengths = new double[3];
            lengths[0] = array[0].DistanceTo(array[1]);
            lengths[1] = array[1].DistanceTo(array[2]);
            lengths[2] = array[2].DistanceTo(array[3]);

            DA.SetData(0, lengths[0]);
            DA.SetData(1, lengths[1]);
            DA.SetData(2, lengths[2]);
            DA.SetData(3, lengths[3]);

            /*
            DA.SetData(0, node1);
            DA.SetData(0, node2);
            DA.SetData(0, node3);
            DA.SetData(0, node4);
            DA.SetData(0, node5);
            DA.SetData(0, node6);
            DA.SetData(0, node7);
            DA.SetData(0, node8);
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
