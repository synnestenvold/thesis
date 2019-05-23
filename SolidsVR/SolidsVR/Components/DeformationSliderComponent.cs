using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;

namespace SolidsVR
{
    public class DeformationSliderComponent : GH_Component
    {
       
        public DeformationSliderComponent()
          : base("DeformationSlider", "DefSlider",
              "Slider for deformation scale in VR",
              "SolidsVR", "VR Slider")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("SliderVR", "S", "Slider as curve", GH_ParamAccess.item);
            pManager.AddBrepParameter("Geometry", "G", "Brep as reference size", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Scale", "Scale", "Scale value as length", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "Size", "Text size", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Text placement", GH_ParamAccess.item);
            pManager.AddColourParameter("Colors", "Color", "Color for text and geometry", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geometry", "Sphere as geometry", GH_ParamAccess.item);
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

            double sqrt3 = (double) 1 / 3;
            double refLength = Math.Pow(brep.GetVolume(), sqrt3);
            double adjustment = 200 / refLength; //5 times the length should give scale = 1000
            double scale = curve.GetLength()*adjustment;

            //---solve---

            var tuple = CreateText(curve, scale, refLength);
            string text = tuple.Item1;
            double refSize = tuple.Item2;
            Plane textPlane = tuple.Item3;
            Color color = tuple.Item4;

            Sphere sphere = new Sphere(curve.PointAtEnd, (double)(refSize/2));

            //---output---

            DA.SetData(0, scale);
            DA.SetData(1, text);
            DA.SetData(2, refSize);
            DA.SetData(3, textPlane);
            DA.SetData(4, color);
            DA.SetData(5, sphere);
        }
        public Tuple<string, double, Plane, Color> CreateText(Curve curve, double scale, double refLength)
        {
            string text = "Scale: " + Math.Round(scale).ToString();
            double refSize = (double)(refLength / 7); //text size and sphere size
            Point3d end = curve.PointAtEnd;
            Point3d p0 = Point3d.Add(end, new Point3d(0, 0, 2*refSize));
            Point3d p1 = Point3d.Add(end, new Point3d(0, 1, 2*refSize));
            Point3d p2 = Point3d.Add(end, new Point3d(0, 0, 1+2*refSize));
            Plane textPlane = new Plane(p0, p1, p2);

            return Tuple.Create(text, refSize, textPlane, Color.White);
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
                return SolidsVR.Properties.Resource1.scaleSlider;
            }
        }
        
        public override Guid ComponentGuid
        {
            get { return new Guid("8a4183c2-fdcf-4af1-93ca-8e411a84a3bc"); }
        }
        
    }
}
