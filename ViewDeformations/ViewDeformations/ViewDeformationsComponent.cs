using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;



namespace ViewDeformations
{
    public class ViewDeformationsComponent : GH_Component
    {
     
        public ViewDeformationsComponent()
          : base("ViewDeformations", "ViewDef",
              "Description",
              "Category3", "Preview")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points for Breps", "P", "Breps in coordinates", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.list);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points for new Breps", "P", "Breps in coordinates", GH_ParamAccess.tree);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Tar utgangspunkt i input-breps, legger til deformasjonen i riktige punkter, og gir ut nye breps. 
        
        }

 
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
            get { return new Guid("0391f003-161b-49e1-931b-71ca89c69aa6"); }
        }
    }
}
