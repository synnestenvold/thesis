using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace MeshBox
{
    public class MeshBoxComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MeshBoxComponent()
          : base("MeshBox", "MeshB",
              "Description",
              "Category3", "Subcategory3")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Inupt Brep as a cube", GH_ParamAccess.item);
            pManager.AddNumberParameter("U Count", "U", "Number of quads in U direction", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("V Count", "V", "Number of quads in V direction", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("W Count", "W", "Number of quads in W direction", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Breps", "B", "List of new Breps", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            Brep brp = new Brep();
            double u = 1.0;
            double v = 1.0;
            double w = 1.0;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref brp)) return;
            if (!DA.GetData(1, ref u)) return;
            if (!DA.GetData(2, ref v)) return;
            if (!DA.GetData(3, ref w)) return;

            // We should now validate the data and warn the user if invalid data is supplied.
            if (u < 1.0 || v < 1.0 || w < 1.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the input parameters is less than one.");
                return;
            }

            Brep brp = new Brep();

            Curve spiral = CreateSpiral(plane, radius0, radius1, turns);

            // Finally assign the spiral to the output parameter.
            DA.SetData(0, spiral);
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
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
            get { return new Guid("b0c2516b-1b1a-4e27-a2e5-6c4e01655683"); }
        }
    }
}
