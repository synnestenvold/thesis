using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace SetBC
{
    public class SetBCComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SetBCComponent()
          : base("Load component for FEA", "Loads",
              "Description",
              "Category3", "BC")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points for inserting BC", GH_ParamAccess.list);
            pManager.AddTextParameter("Restained translations", "BC", "Restained translation in the way (0,0,0)", GH_ParamAccess.item,"(0,0,0)");

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Boundary conditions", "BC", "BC in point, (x,y,z);(Tx,Ty,Tz)", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string restrains="";
            List<Point3d> points = new List<Point3d>();
            List<Point3d> AllPoints = new List<Point3d>();

            List<string> pointsString = new List<string>();

            
            if (!DA.GetDataList(0, points)) return;
            if (!DA.GetData(1, ref restrains)) return;

            string pointString;

            foreach (Point3d p in points)
            {
                pointString =  p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();

                pointsString.Add(pointString);
            }

            List<string> pointBC = new List<string>();

            foreach (string s in pointsString)
            {
                pointBC.Add(s + ";" + restrains);
            }

            DA.SetDataList(0, pointBC);
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
            get { return new Guid("42b3f47f-ca8f-46f2-a18c-7999d897fb11"); }
        }
    }
}
