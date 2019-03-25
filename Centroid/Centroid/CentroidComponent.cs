using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Centroid
{
    public class CentroidComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public CentroidComponent()
          : base("Centroid", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Points for Breps", "P", "Breps in coordinates", GH_ParamAccess.item);
            pManager.AddPointParameter("Points for Breps", "P", "Breps in coordinates", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> points = new List<Point3d>();

            points.Add(new Point3d(0, 0, 0));
            points.Add(new Point3d(5, 0, 0));
            points.Add(new Point3d(5, 5, 0));
            points.Add(new Point3d(0, 5, 0));
            points.Add(new Point3d(0, 0, 5));
            points.Add(new Point3d(5, 0, 5));
            points.Add(new Point3d(5, 5, 5));
            points.Add(new Point3d(0, 5, 5));

            Mesh mesh = new Mesh();

            foreach (Point3d p in points)
            {
                mesh.Vertices.Add(p);
            }





          
            mesh.Faces.AddFace(0, 1, 2, 3);
            mesh.Faces.AddFace(4, 5, 6, 7);

            mesh.Faces.AddFace(2, 3, 7, 6);
            mesh.Faces.AddFace(0, 1, 5, 4);
            mesh.Faces.AddFace(1, 2, 6, 5);

            mesh.Faces.AddFace(0, 3, 7, 4);


            Brep brep = Brep.CreateFromMesh(mesh, true);

            Box b = new Box(Plane.WorldXY, new Interval(0,10), new Interval(0, 10), new Interval(0, 10));
            Point3d[] pss = brep.DuplicateVertices();
     
            VolumeMassProperties vmp = VolumeMassProperties.Compute(brep);
            Point3d centroid = vmp.Centroid;

            DA.SetData(0, brep);
            DA.SetData(1, centroid);

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
            get { return new Guid("11d61234-6de4-4471-9f2a-af85b7bbf740"); }
        }
    }
}
