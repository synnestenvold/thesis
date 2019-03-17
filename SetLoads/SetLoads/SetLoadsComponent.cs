using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace SetLoads
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
              "Category3", "Loads")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Force vector", "V", "Direction and load amount in kN", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Points for loading", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "PALL", "Points for loading", GH_ParamAccess.list);
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
            Vector3d forceVec = new Vector3d();
            List<Point3d> points = new List<Point3d>();
            List<Point3d> AllPoints = new List<Point3d>();

            List<string> pointsString = new List<string>();

            if (!DA.GetData(0, ref forceVec)) return;
            if (!DA.GetDataList(1, points)) return;
            if (!DA.GetDataList(2, AllPoints)) return;

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

            List<double> output = AssignLoads(pointLoads, AllPoints);

            DA.SetDataList(0, output);
        }

        public List<double> AssignLoads(List<string> pointLoads, List<Point3d> points)
        {
            List<double> loadCoord = new List<double>();
            List<double> pointValues = new List<double>();
            List<double> loads = new List<double>(points.Count*3);

            for (int i = 0; i < points.Count*3; i++)
            {
                loads.Add(0);
            }

            List<double> pointList = Enumerable.Repeat(0d, points.Count*3).ToList();

            foreach (string s in pointLoads)
            {
                string coordinate = (s.Split(';'))[0];
                string iLoad = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iLoads = (iLoad.Split(','));

                //loadPoints.Add(new Point3d(Math.Round(double.Parse(coord[0]), 4), Math.Round(double.Parse(coord[1]), 4), Math.Round(double.Parse(coord[2]), 4)));                loadCoord.Add(Math.Round(double.Parse(coord[0])));                loadCoord.Add(Math.Round(double.Parse(coord[1])));                loadCoord.Add(Math.Round(double.Parse(coord[2])));                pointValues.Add(Math.Round(double.Parse(iLoads[0])));
                pointValues.Add(Math.Round(double.Parse(iLoads[1])));
                pointValues.Add(Math.Round(double.Parse(iLoads[2])));
            }

            int index = 0;

            foreach (Point3d p in points)
            {

                for(int j = 0; j <loadCoord.Count/3; j++)
                {
                    if(loadCoord[3*j] == p.X && loadCoord[3*j+1] == p.Y && loadCoord[3*j+2] == p.Z)
                    {
                        loads[index] = pointValues[3*j];
                        loads[index+1] = pointValues[3*j+1];
                        loads[index+2] = pointValues[3*j+2];
                    }
                }
                index += 3;
            }

            return loads;
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
