using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace SolidsVR
{
    public class HeadLineComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the headLineComponent class.
        /// </summary>
        public HeadLineComponent()
          : base("HeadLine", "HeadLine",
              "Create headlines in VR",
              "SolidsVR", "VR Info")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "Curves for Surface for cross-section", GH_ParamAccess.list);
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh for Brep", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
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

            List<Curve> curves = new List<Curve>();
            MeshGeometry mesh = new MeshGeometry();

            //---input---

            if (!DA.GetDataList(0, curves)) return;
            if (!DA.GetData(1, ref mesh)) return;

            //---setup---
            Brep origBrep = mesh.GetOrigBrep();
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            var tuple = CreateTextAndPlane(curves, refLength);
            List<string> texts = tuple.Item1;

            Color colorText = Color.Orange;

            double size = (double)refLength / 7;

            List<Plane> planes = tuple.Item2;

            DA.SetDataList(0, texts);
            DA.SetData(1, size);
            DA.SetDataList(2, planes);
            DA.SetData(3, colorText);

        }

        public Tuple<List<string>, List<Plane>> CreateTextAndPlane(List<Curve> curves, double refLength)
        {
            List<string> text = new List<string>();
            List<Plane> planes = new List<Plane>();

            string mesh = "MESH";
            string bc = "BOUNDARY CONDITIONS";
            string load = "LOADS";

            Point3d mp1 = curves[0].PointAtStart;

            Point3d mplane0 = new Point3d(mp1.X+70, mp1.Y, mp1.Z + 150);
            Point3d mplane1 = Point3d.Add(mplane0, new Point3d(1, 0, 0));
            Point3d mplane2 = Point3d.Add(mplane1, new Point3d(0, 0, 1));

            Plane mplane = new Plane(mplane0, mplane1, mplane2);

            Point3d bp1 = curves[1].PointAtStart;

            Point3d bplane0 = new Point3d(bp1.X + refLength, bp1.Y, bp1.Z + 150);
            Point3d bplane1 = Point3d.Add(bplane0, new Point3d(1, 0, 0));
            Point3d bplane2 = Point3d.Add(bplane1, new Point3d(0, 0, 1));

            Plane bplane = new Plane(bplane0, bplane1, bplane2);

            Point3d lp1 = curves[2].PointAtStart;

            Point3d lplane0 = new Point3d(lp1.X + 70, lp1.Y, lp1.Z + 150);
            Point3d lplane1 = Point3d.Add(lplane0, new Point3d(1, 0, 0));
            Point3d lplane2 = Point3d.Add(lplane1, new Point3d(0, 0, 1));

            Plane lplane = new Plane(lplane0, lplane1, lplane2);


            planes.Add(mplane);
            planes.Add(bplane);
            planes.Add(lplane);

            text.Add(mesh);
            text.Add(bc);
            text.Add(load);


            return Tuple.Create(text, planes);
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
                return SolidsVR.Properties.Resource1.headline;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b9cf47a8-033a-4f65-bb3d-4b9291f75251"); }
        }
    }
}