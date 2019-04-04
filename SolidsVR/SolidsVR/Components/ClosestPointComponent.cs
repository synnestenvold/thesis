using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;
using System.Drawing;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace SolidsVR
{
    public class ClosestPointComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ClosestPointComponent()
          : base("Find the closest point to sphere", "Closest point",
              "Description",
              "Category3", "Closest point")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Sphere", "S", "Sphere to compare closest point", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Connectivity", "C", "Relationship between local and global numbering", GH_ParamAccess.tree);
            pManager.AddPointParameter("Nodes", "N", "Coordinates for corner nodes in brep", GH_ParamAccess.tree);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "P", "Closest point to sphere", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.item);
            pManager.AddColourParameter("Colors", "Color", "Colors for text", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Integer> treeConnectivity = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Point> treePoints = new GH_Structure<GH_Point>();

            Brep sphere = new Brep();

            if (!DA.GetData(0, ref sphere)) return;
            if (!DA.GetDataTree(1, out treeConnectivity)) return;
            if (!DA.GetDataTree(2, out treePoints)) return;

            // Temporary way of finding the size of stiffness matrix and B matrix
            int sizeOfM = FindSizeOfM(treeConnectivity);

            //List of global points with correct numbering
            Point3d[] globalPoints = CreatePointList(treeConnectivity, treePoints, sizeOfM);

            VolumeMassProperties vmp = VolumeMassProperties.Compute(sphere);
            Point3d centroid = vmp.Centroid;

            double refLength = 1;

            Point3d closestPoint = FindClosestPoint(globalPoints, centroid, refLength);

            String text = "Drag sphere to point";

            Plane textPlane = FindSpherePlane(centroid, refLength);

            Color color = Color.Red;

            DA.SetData(0, closestPoint);
            DA.SetData(1, text);
            DA.SetData(2, textPlane);
            DA.SetData(3, color);

        }

        public Plane FindSpherePlane(Point3d centroid, double refLength)
        {
            Point3d p0 = new Point3d(centroid.X + refLength * 2, centroid.Y, centroid.Z + refLength * 2);
            Point3d p1 = Point3d.Add(p0, new Point3d(1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane p = new Plane(p0, p1, p2);

            return p;
        }

        public int FindSizeOfM(GH_Structure<GH_Integer> treeConnectivity)
        {
            int max = 0;

            for (int i = 0; i < treeConnectivity.PathCount; i++)
            {
                List<GH_Integer> cNodes = (List<GH_Integer>)treeConnectivity.get_Branch(i);

                for (int j = 0; j < cNodes.Count; j++)
                {
                    if (cNodes[j].Value > max)
                    {
                        max = cNodes[j].Value;
                    }
                }
            }

            int sizeOfM = 3 * (max + 1);

            return sizeOfM;
        }


        public Point3d[] CreatePointList(GH_Structure<GH_Integer> treeConnectivity, GH_Structure<GH_Point> treePoints, int sizeOfM)
        {
            Point3d point = new Point3d(0, 0, 0);

            Point3d[] pointList = new Point3d[sizeOfM / 3];


            for (int i = 0; i < treeConnectivity.PathCount; i++)
            {
                List<GH_Integer> connectedNodes = (List<GH_Integer>)treeConnectivity.get_Branch(i);
                List<GH_Point> connectedPoints = (List<GH_Point>)treePoints.get_Branch(i);

                for (int j = 0; j < connectedNodes.Count; j++)
                {
                    pointList[connectedNodes[j].Value] = connectedPoints[j].Value;
                }
            }

            return pointList;

        }

        public Point3d FindClosestPoint(Point3d[] globalPoints, Point3d centroid, double refLength)
        {
            Point3d closestPoint = new Point3d(999.999, 999.999, 999.999);

            double length = double.PositiveInfinity;

            for (int i = 0; i < globalPoints.Length; i++)
            {
                double checkLength = globalPoints[i].DistanceTo(centroid);

                if (checkLength < length && checkLength < refLength)
                {
                    length = checkLength;
                    closestPoint = globalPoints[i];
                }
            }

            return closestPoint;
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
            get { return new Guid("6f3cb091-d8c1-47f2-907a-e991e673f70d"); }
        }
    }
}
