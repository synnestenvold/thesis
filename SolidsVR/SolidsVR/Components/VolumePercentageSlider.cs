using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR.Components
{
    public class VolumePercentageSlider : GH_Component
    {
        private int max = 30;

        public VolumePercentageSlider()
          : base("VolumePercentageSlider", "VolumeSlider",
              "Volume reduction in percentage",
              "SolidsVR", "VR Slider")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("SliderVR", "S", "Slider as curve", GH_ParamAccess.item);
            pManager.AddBrepParameter("Geometry", "G", "Geometry as reference", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Volume percentage", "V", "Volume (0%-30%)", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Volume text", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Size for text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Color", "Color", "Color for text and geometry", GH_ParamAccess.item);
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

            double refLength = Math.Pow(brep.GetVolume(), (double)1 / 3);
            double refSize = (double)(refLength / 7);
            double adjustment = 20 / refLength; //the length should give 20%

            //---solve---

            int volume = Convert.ToInt32(curve.GetLength() * adjustment)-10;
            volume = volume <= 0 ? 0 : volume; //from -10-0% the value is 0
            volume = volume > max ? max : volume; // from 10-40% the value is 0-30%

            var tuple = CreateText(curve, volume, refLength);
            List<string> text = tuple.Item1;
            List<double> size = tuple.Item2;
            List<Plane> textPlane = tuple.Item3;
            Color color = tuple.Item4;
            Sphere sphere = new Sphere(curve.PointAtEnd, (double)(refSize / 2));

            //---output---

            DA.SetData(0, volume);
            DA.SetDataList(1, text);
            DA.SetDataList(2, size);
            DA.SetDataList(3, textPlane);
            DA.SetData(4, color);
            DA.SetData(5, sphere);

        }

        public (List<string>, List<double>, List<Plane>, Color) CreateText(Curve curve, double volume, double refLength)
        {
            List<string> text = new List<string>();
            string volumeText = "";

            if (volume == 0) volumeText = "No reduction";
            else volumeText = volume.ToString()+" %";

            text.Add("Volume reduction: " + volumeText);
            text.AddRange(new List<string>() { "No reduction", "0%", "10%", "20%", "30%"});

            double refSize = (double)(refLength / 7);
            List<double> size = new List<double>() { refSize, (double)(refSize / 2) };

            List<Plane> textPlane = new List<Plane>();
            Point3d start = curve.PointAtStart;
            Point3d end = curve.PointAtEnd;
            Point3d p0 = Point3d.Add(end, new Point3d(0, 0, 2 * refSize));
            Point3d p1 = Point3d.Add(end, new Point3d(0, -1, 2 * refSize));
            Point3d p2 = Point3d.Add(end, new Point3d(0, 0, (1 + 2 * refSize)));

            textPlane.Add(new Plane(p0, p1, p2));

            double range = (double)(refLength / 2);

            for (int i = 0; i < 5; i++)
            {
                Point3d p3 = Point3d.Add(start, new Point3d(0, range * i, -2 * refSize));
                Point3d p4 = Point3d.Add(start, new Point3d(0, -1 + range * i, -2 * refSize));
                Point3d p5 = Point3d.Add(start, new Point3d(0, range * i, (1 - 2 * refSize)));
                textPlane.Add(new Plane(p3, p4, p5));
            }
            return (text, size, textPlane, Color.White);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.volumeSlider;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3145bd35-52c7-4733-bde1-bf49564849dd"); }
        }
    }
}