using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;


namespace SolidsVR
{
    public class SetBCComponent : GH_Component
    {

        public SetBCComponent()
          : base("BC component for FEA", "BC",
              "Description",
              "Category3", "BC")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points for inserting BC", GH_ParamAccess.list);
            pManager.AddTextParameter("Restained translations", "BC", "Restained translation in the way (0,0,0)", GH_ParamAccess.item,"(0,0,0)");

        }

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

        public override Guid ComponentGuid
        {
            get { return new Guid("42b3f47f-ca8f-46f2-a18c-7999d897fb11"); }
        }
    }
}
