﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace SetUniLoad
{
    public class SetUniLoadComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SetUniLoadComponent()
          : base("Uniform load component for FEA", "UniLoads",
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
            pManager.AddBrepParameter("Surface", "S", "Surface for loading", GH_ParamAccess.item);
            pManager.AddIntegerParameter("u-divisions", "U", "U-division", GH_ParamAccess.item);
            pManager.AddIntegerParameter("v-divisions", "V", "V-division", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddTextParameter("Uniformloads", "UL", "Loads in point, (x,y,z);(Fx,Fy,Fz)", GH_ParamAccess.list);
            pManager.AddPointParameter("v-divisions", "V", "V-division", GH_ParamAccess.list);
            pManager.AddPointParameter("v-divisions", "V", "V-division", GH_ParamAccess.list);
            pManager.AddPointParameter("v-divisions", "V", "V-division", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Vector3d forceVec = new Vector3d();
            Brep brep = new Brep();
            int u = 2;
            int v = 2;

            Point3d[] vertices = new Point3d[4];

            List<string> pointsString = new List<string>();

            if (!DA.GetData(0, ref forceVec)) return;
            if (!DA.GetData(1, ref brep)) return;
            if (!DA.GetData(2, ref u)) return;
            if (!DA.GetData(3, ref v)) return;

            vertices = brep.DuplicateVertices();

            //FINDING CORNER POINTS
            List<Point3d> cornerPoints = new List<Point3d>();
            cornerPoints.Add(vertices[0]);
            cornerPoints.Add(vertices[1]);
            cornerPoints.Add(vertices[2]);
            cornerPoints.Add(vertices[3]);

            double l_u1 = vertices[0].DistanceTo(vertices[1])/u;
            double l_u2 = vertices[3].DistanceTo(vertices[2])/u;

            double l_v1 = vertices[0].DistanceTo(vertices[3])/v;
            double l_v2 = vertices[1].DistanceTo(vertices[2])/v;

            Vector3d vec_u1 = (vertices[1] - vertices[0]) / vertices[0].DistanceTo(vertices[1]);
            Vector3d vec_u2 = (vertices[2] - vertices[3]) / vertices[3].DistanceTo(vertices[2]);

            Vector3d vec_v1 = (vertices[3] - vertices[0]) / vertices[0].DistanceTo(vertices[3]);
            Vector3d vec_v2 = (vertices[2] - vertices[1]) / vertices[1].DistanceTo(vertices[2]);


            //FINDING LINE POINTS
            List<Point3d> linePoints = new List<Point3d>();

            for(int i = 1; i < u; i++)
            {
                Point3d p1 = new Point3d(vertices[0].X + l_u1 * i * vec_u1.X, vertices[0].Y + l_u1 * vec_u1.Y * i, vertices[0].Z + l_u1 * vec_u1.Z * i);
                linePoints.Add(p1);
                Point3d p2 = new Point3d(vertices[3].X + l_u2 * i * vec_u2.X, vertices[3].Y + l_u2 * vec_u2.Y * i, vertices[3].Z + l_u2 * vec_u2.Z * i);
                
                linePoints.Add(p2);
            }

            for (int i = 1; i < v; i++)
            {
                Point3d p1 = new Point3d(vertices[0].X + l_v1 * i * vec_v1.X, vertices[0].Y + l_v1 * vec_v1.Y * i, vertices[0].Z + l_v1 * vec_v1.Z * i);
                linePoints.Add(p1);
                Point3d p2 = new Point3d(vertices[1].X + l_v2 * i * vec_v2.X, vertices[1].Y + l_v2 * vec_v2.Y * i, vertices[1].Z + l_v2 * vec_v2.Z * i);
                linePoints.Add(p2);
            }


            //FINDING CENTER POINTS
            List<Point3d> centerPoints = new List<Point3d>();

            for(int i = 1; i <u; i++)
            {
                Point3d p1_u = new Point3d(vertices[0].X + l_u1 * i * vec_u1.X, vertices[0].Y + l_u1 * vec_u1.Y * i, vertices[0].Z + l_u1 * vec_u1.Z * i);
                Point3d p2_u = new Point3d(vertices[3].X + l_u2 * i * vec_u2.X, vertices[3].Y + l_u2 * vec_u2.Y * i, vertices[3].Z + l_u2 * vec_u2.Z * i);

                Vector3d vec_u = (p2_u - p1_u) / (p1_u.DistanceTo(p2_u));

                Double length_u1 = p1_u.DistanceTo(p2_u) / u;

                for (int j = 1; j < v; j++)
                {
                    Point3d p1_v = new Point3d(p1_u.X + length_u1 * j * vec_u.X, p1_u.Y + length_u1 * j * vec_u.Y, p1_u.Z + length_u1 * j * vec_u.Z);
                    centerPoints.Add(p1_v);
                }
            }


            //DA.SetDataList(0, loads);
            DA.SetDataList(0, centerPoints);
            DA.SetDataList(1, linePoints);
            DA.SetDataList(2, vertices);


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
            get { return new Guid("9fa57f7d-4d01-4004-aa03-214e25f02b6a"); }
        }
    }
}
