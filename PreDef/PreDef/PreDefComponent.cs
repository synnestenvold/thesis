using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace PreDef
{
    public class PreDefComponent : GH_Component
    {
 
        public PreDefComponent()
          : base("PreDef", "PreDef",
              "Prescribed deformation",
              "Category3", "Def")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points for deformation", GH_ParamAccess.list);
            pManager.AddTextParameter("Prescribed deformations", "PreDef", "Prescribed deformation in the way (0,0,0)", GH_ParamAccess.item, "(0,0,0)");

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Prescribed deformations", "PreDef", "Def in point, (x,y,z);(Tx,Ty,Tz)", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string def = "";
            List<Point3d> points = new List<Point3d>();

            List<string> pointsString = new List<string>();
            
            if (!DA.GetDataList(0, points)) return;
            if (!DA.GetData(1, ref def)) return;

            string pointString;

            foreach (Point3d p in points)
            {
                pointString = p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();

                pointsString.Add(pointString);
            }

            List<string> pointBC = new List<string>();

            foreach (string s in pointsString)
            {
                pointBC.Add(s + ";" + def);
            }

            DA.SetDataList(0, pointBC);
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
            get { return new Guid("25e13746-7e88-4749-a603-98926136ebb6"); }
        }
    }
}
