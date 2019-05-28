using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace SolidsVR
{
    public class HeadLine : GH_Component
    {

        public HeadLine()
          : base("HeadLine", "HeadLine",
              "Create headlines in VR",
              "SolidsVR", "VR Info")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "Curves for mesh, bc and load", GH_ParamAccess.list);
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh for geometry", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Size for text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color", "Colors for text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---

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

            (List<string> texts, List <Plane> planes) = CreateTextAndPlane(curves, refLength);

            Color colorText = Color.Orange;

            double size = (double)refLength / 7;

            //---output---

            DA.SetDataList(0, texts);
            DA.SetData(1, size);
            DA.SetDataList(2, planes);
            DA.SetData(3, colorText);
        }

        public (List<string>, List<Plane>) CreateTextAndPlane(List<Curve> curves, double refLength)
        {
            List<string> text = new List<string>();
            List<Plane> planes = new List<Plane>();

            string mesh = "MESH";
            string bc = "BOUNDARY CONDITIONS";
            string load = "LOADS";

            //Plane for mesh headline
            Point3d mp1 = curves[0].PointAtStart;

            Point3d mplane0 = new Point3d(mp1.X + (double)refLength/3.5, mp1.Y, mp1.Z + (double)refLength/1.5);
            Point3d mplane1 = Point3d.Add(mplane0, new Point3d(1, 0, 0));
            Point3d mplane2 = Point3d.Add(mplane1, new Point3d(0, 0, 1));

            Plane mplane = new Plane(mplane0, mplane1, mplane2);

            //Plane for bc headline
            Point3d bp1 = curves[1].PointAtStart;

            Point3d bplane0 = new Point3d(bp1.X + refLength, bp1.Y, bp1.Z + (double)refLength/1.5);
            Point3d bplane1 = Point3d.Add(bplane0, new Point3d(1, 0, 0));
            Point3d bplane2 = Point3d.Add(bplane1, new Point3d(0, 0, 1));

            Plane bplane = new Plane(bplane0, bplane1, bplane2);

            //Plane for load headline
            Point3d lp1 = curves[2].PointAtStart;

            Point3d lplane0 = new Point3d(lp1.X + (double) refLength/3.5, lp1.Y, lp1.Z + (double) refLength/1.5);
            Point3d lplane1 = Point3d.Add(lplane0, new Point3d(1, 0, 0));
            Point3d lplane2 = Point3d.Add(lplane1, new Point3d(0, 0, 1));

            Plane lplane = new Plane(lplane0, lplane1, lplane2);

            planes.Add(mplane);
            planes.Add(bplane);
            planes.Add(lplane);

            text.Add(mesh);
            text.Add(bc);
            text.Add(load);

            return (text, planes);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.headline;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b9cf47a8-033a-4f65-bb3d-4b9291f75251"); }
        }
    }
}