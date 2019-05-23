using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR
{
    public class CreateMaterialComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateMaterial class.
        /// </summary>
        public CreateMaterialComponent()
          : base("CreateMaterial", "Material",
              "Create material with Young's modulus and Poisson's ratio",
              "SolidsVR", "Material")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("E-modulus", "E", "Young's modulus", GH_ParamAccess.item);
            pManager.AddNumberParameter("P-ratio", "nu", "Poisson's ratio", GH_ParamAccess.item);
            pManager.AddNumberParameter("Yield stress", "Y", "Yield stress", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Material", GH_ParamAccess.item);
        }

       
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            double E = 210000;
            double nu = 0.3;
            double Y = 355;

            // --- inputs ---
            if (!DA.GetData(0, ref E)) return;
            if (!DA.GetData(1, ref nu)) return;
            if (!DA.GetData(2, ref Y)) return;

            // --- solve ---
            Material material = new Material(E, nu, Y);

            // --- output ---
            DA.SetData(0, material);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return SolidsVR.Properties.Resource1.material;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a7fa5dad-2d97-4290-96fb-8ee3f594cc8d"); }
        }
    }
}