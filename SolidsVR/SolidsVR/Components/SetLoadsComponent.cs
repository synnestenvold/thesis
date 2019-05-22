using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace SolidsVR
{
    public class SetLoadsComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SetLoadsComponent()
          : base("Load component for FEA", "Loads",
              "Description",
              "SolidsVR", "Loads")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Force vector", "V", "Direction and load amount in kN", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Points for loading", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("PointLoads", "PL", "Loads in point, (x,y,z);(Fx,Fy,Fz)", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //---variables---

            Vector3d forceVec = new Vector3d();
            List<Point3d> points = new List<Point3d>();
            List<Point3d> AllPoints = new List<Point3d>();

            List<string> pointsString = new List<string>();

            //---input---

            if (!DA.GetData(0, ref forceVec)) return;
            if (!DA.GetDataList(1, points)) return;

            //---solve---

            string pointString;

            foreach(Point3d p in points)
            {
                pointString = p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();

                pointsString.Add(pointString);
            }

            string vector = forceVec.X + "," + forceVec.Y + "," + forceVec.Z;

            List<string> pointLoads = new List<string>();

            foreach (string s in pointsString)
            {
                pointLoads.Add(s + ";" + vector);
            }

            //List<double> output = AssignLoads(pointLoads, AllPoints);

            //---output---

            DA.SetDataList(0, pointLoads);
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
            get { return new Guid("01429895-7c9d-4396-839e-6b93ec7f1dcd"); }
        }
    }
}
