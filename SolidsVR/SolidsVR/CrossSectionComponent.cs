using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace SolidsVR
{
    public class CrossSectionComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CrossSectionComponent class.
        /// </summary>
        public CrossSectionComponent()
          : base("Get information of CrossSec", "CrossSec",
              "Description",
              "Category3", "CrossSec")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "Curves for Surface for cross-section", GH_ParamAccess.list);
            pManager.AddGenericParameter("Mesh", "M", "Mesh for Brep", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "P", "Closest point to sphere", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color", "Colors for points", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Size for text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color", "Colors for text", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---


            List<Curve> curves = new List<Curve>();
            Mesh_class mesh = new Mesh_class();

            //---input---

            if (!DA.GetDataList(0, curves)) return;
            if (!DA.GetData(1, ref mesh)) return;

            //---setup---
            int u = mesh.getU();
            int v = mesh.getV();
            Brep origBrep = mesh.GetOrigBrep();
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            Brep brepSurface = Brep.CreateEdgeSurface(curves);

            Point3d[] vertices = brepSurface.DuplicateVertices();

            Surface surface = brepSurface.Faces[0];

            List <Point3d> points= CreatePoints(surface, u, v);

            Color pointsColor = Color.Black;

            var tuple = CreateTextAndPlane(curves);
            List<string> texts = tuple.Item1;

            Color colorText = Color.Orange;

            double size = (double)refLength / 10;

            List<Plane> planes = tuple.Item2;

            DA.SetDataList(0, points);
            DA.SetData(1, pointsColor);
            DA.SetDataList(2, texts);
            DA.SetData(3, size);
            DA.SetDataList(4, planes);
            DA.SetData(5, colorText);

        }

        public Tuple<List<string>,List<Plane>> CreateTextAndPlane(List<Curve> curves)
        {
            List<string> text = new List<string>();
            List<Plane> planes = new List<Plane>();

            string header = "CROSS-SECTION";
            string uDir = "u-direction";
            string vDir = "v-direction";

            Point3d hp1 = curves[0].PointAtEnd;
            Point3d hp2 = curves[0].PointAtStart;

            double refLength = curves[0].GetLength();

            Vector3d headerVec = (hp2 - hp1)/2;

            Point3d headPoint = Point3d.Add(hp1, headerVec);

            Point3d hplane0 = new Point3d(headPoint.X, headPoint.Y, headPoint.Z + refLength);
            Point3d hplane1 = Point3d.Add(hplane0, new Point3d(1, 0, 0));
            Point3d hplane2 = Point3d.Add(hplane1, new Point3d(0, 0, 1));

            Plane hplane = new Plane(hplane0, hplane1, hplane2);

            planes.Add(hplane);

            Point3d vplane0 = new Point3d(headPoint.X, headPoint.Y, headPoint.Z + refLength / 3);
            Point3d vplane1 = Point3d.Add(vplane0, new Point3d(1, 0, 0));
            Point3d vplane2 = Point3d.Add(vplane1, new Point3d(0, 0, 1));

            Plane vplane = new Plane(vplane0, vplane1, vplane2);

            planes.Add(vplane);

            Point3d up1 = curves[1].PointAtEnd;
            Point3d up2 = curves[1].PointAtStart;

            Vector3d uVec = (up2 - up1)/2;

            Point3d uPoint = Point3d.Add(up1, uVec);

            Point3d uplane0 = new Point3d(uPoint.X+100, uPoint.Y, uPoint.Z);
            Point3d uplane1 = Point3d.Add(uplane0, new Point3d(1, 0, 0));
            Point3d uplane2 = Point3d.Add(uplane1, new Point3d(0, 0, 1));

            Plane uplane = new Plane(uplane0, uplane1, uplane2);

            planes.Add(uplane);

            text.Add(header);
            text.Add(vDir);
            text.Add(uDir);


            return Tuple.Create(text, planes);
        }

        public List<Point3d> CreatePoints(Surface surface, int u, int v)
        {
            List<Point3d> points = new List<Point3d>();

            Interval domainU = surface.Domain(0);
            Interval domainV = surface.Domain(1);

            domainU.Swap();

            for (int i = 0; i <= v; i++)
            {
                double tv = domainV.ParameterAt(i / (double)v);
                for (int j = 0; j <= u; j++)
                {
                    double tu = domainU.ParameterAt(j / (double)u);
                    Point3d p1 = surface.PointAt(tu, tv);
                    points.Add(p1);
                }
            }
            return points;
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("78178f4e-a6ff-4af9-95c7-7147908582ca"); }
        }
    }
}