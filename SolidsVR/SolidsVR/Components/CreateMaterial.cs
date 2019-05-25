using System;
using Grasshopper.Kernel;

namespace SolidsVR
{
    public class CreateMaterial : GH_Component
    {
        public CreateMaterial()
          : base("CreateMaterial", "Material",
              "Create material",
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

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.material;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a7fa5dad-2d97-4290-96fb-8ee3f594cc8d"); }
        }
    }
}