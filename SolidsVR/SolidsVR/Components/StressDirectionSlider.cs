﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR
{
    public class StressDirectionSlider : GH_Component
    {
        private int max = 6;

        public StressDirectionSlider()
          : base("StressDirectionSlider", "StressSlider",
              "Slider for what stress direction to show",
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
            pManager.AddIntegerParameter("Stress direction", "S", "Direction of stress (0-5)", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Direction text", GH_ParamAccess.list);
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
            double adjustment = 4 / refLength; //the length should give 4

            //---solve---

            int dir = Convert.ToInt32(curve.GetLength()*adjustment);
            dir = dir > max ? max : dir;
            
            var tuple = CreateText(curve, dir, refLength);
            List<string> text = tuple.Item1;
            List<double> size = tuple.Item2;
            List<Plane> textPlane = tuple.Item3;
            Color color = tuple.Item4;
            Sphere sphere = new Sphere(curve.PointAtEnd, (double)(refSize / 2));


            //---output---

            DA.SetData(0, dir);
            DA.SetDataList(1, text);
            DA.SetDataList(2, size);
            DA.SetDataList(3, textPlane);
            DA.SetData(4, color);
            DA.SetData(5, sphere);

        }

        public (List<string>, List<double>, List<Plane>, Color) CreateText(Curve curve, double dir, double refLength)
        {
            List<string> text = new List<string>();
            string direction = "";
            if (dir < 1) direction = "S,xx";
            else if (dir < 2) direction = "S,yy";
            else if (dir < 3) direction = "S,zz";
            else if (dir < 4) direction = "S,xy";
            else if (dir < 5) direction = "S,xz";
            else if (dir < 6) direction = "S,yz";
            else direction = "Mises";

            text.Add("Stress direction: "+direction);
            text.AddRange(new List<string>() { "| S,xx", "| S,yy", "| S,zz", "| S,xy", "| S,xz", "| S,yz", "| Mises" });
                
            double refSize = (double)(refLength / 7);
            List<double> size = new List<double>() { refSize, (double)(refSize / 2) };
            List<Plane> textPlane = new List<Plane>();

            Point3d start = curve.PointAtStart;
            Point3d end = curve.PointAtEnd;
            Point3d p0 = Point3d.Add(end, new Point3d(0, 0, 2 * refSize));
            Point3d p1 = Point3d.Add(end, new Point3d(0, -1, 2 * refSize));
            Point3d p2 = Point3d.Add(end, new Point3d(0, 0, (1 + 2 * refSize)));
            textPlane.Add(new Plane(p0, p1, p2));

            double range = (double)(refLength / 4);
            for (int i = 0; i < 7; i++)
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
                return SolidsVR.Properties.Resource1.stressSlider;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("db0c32b8-24e5-443c-8c3a-03bb239ba4ed"); }
        }
    }
}