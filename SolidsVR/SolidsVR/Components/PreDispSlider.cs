using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR.Components
{
    public class PreDispSlider : GH_Component
    {
        public PreDispSlider()
          : base("PreDispSlider", "PreDispSlider",
              "Prescribed displacement",
              "SolidsVR", "VR Slider")
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("SliderVR", "S", "Slider as curve", GH_ParamAccess.item);
            pManager.AddBrepParameter("Geoemtry", "G", "Brep as reference", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Prescribed displacement", "PreDisp", "Prescribed displacement", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Slider text", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Text size", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color", "Color for text and geometry", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geometry", "Sphere for place to drag line", GH_ParamAccess.item);
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //---variables---

            Curve curve = null;
            Brep brep = new Brep();

            //---input---

            if (!DA.GetData(0, ref curve)) return;
            if (!DA.GetData(1, ref brep)) return;

            //---setup---

            double volume = brep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(brep.GetVolume(), sqrt3);

            double adjustment = 2 / refLength; //the length should give 2
            Vector3d vectorRef = curve.PointAtEnd - curve.PointAtStart;
            Vector3d def = Vector3d.Multiply(adjustment, vectorRef);

            //---solve---

            var tuple = CreateText(curve, def, refLength);
            List<string> text = tuple.Item1;
            double refSize = tuple.Item2;
            List<Plane> textPlane = tuple.Item3;
            Color color = tuple.Item4;

            Sphere sphere = new Sphere(curve.PointAtEnd, (double)(refSize / 2));

            //---output---

            DA.SetData(0, def);
            DA.SetDataList(1, text);
            DA.SetData(2, refSize);
            DA.SetDataList(3, textPlane);
            DA.SetData(4, Color.White);
            DA.SetData(5, sphere);
        }

        public (List<string>, double, List<Plane>, Color) CreateText(Curve curve, Vector3d load, double refLength)
        {
            List<string> text = new List<string>();
            text.Add("Adjust for deformations in mm");
            text.Add("(" + Math.Round((load.X), 3).ToString() + ", " + Math.Round((load.Y), 3).ToString() + ", " + Math.Round((load.Z), 3).ToString() + ")");

            double refSize = (double)(refLength / 7);

            List<Plane> textPlane = new List<Plane>();
            Point3d start = curve.PointAtStart;
            Point3d p0 = Point3d.Add(start, new Point3d(0, 0, 2 * refSize));
            Point3d p1 = Point3d.Add(start, new Point3d(1, 0, 2 * refSize));
            Point3d p2 = Point3d.Add(start, new Point3d(0, 0, (1 + 2 * refSize)));
            textPlane.Add(new Plane(p0, p1, p2));

            Point3d end = curve.PointAtEnd;
            Point3d p3 = Point3d.Add(end, new Point3d(0, 0, -2 * refSize));
            Point3d p4 = Point3d.Add(end, new Point3d(1, 0, -2 * refSize));
            Point3d p5 = Point3d.Add(end, new Point3d(0, 0, (1 - 2 * refSize)));
            textPlane.Add(new Plane(p3, p4, p5));

            return (text, refSize, textPlane, Color.White);
        }
        
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.predefSlider;
            }
        }
        
        public override Guid ComponentGuid
        {
            get { return new Guid("5a6c91bb-d740-45fb-a939-5bc237dd7096"); }
        }
    }
}