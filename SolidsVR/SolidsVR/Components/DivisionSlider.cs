﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR
{
    public class DivisionSlider : GH_Component
    {
        readonly int max = 15;

        public DivisionSlider()
          : base("DivisionSlider", "DivSlider",
              "Slider for division in VR",
              "SolidsVR", "VR Slider")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("SliderVR", "S", "Sliders as curves (u, v, w)", GH_ParamAccess.list);
            pManager.AddBrepParameter("Geometry", "G", "Geometry as reference size", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("u count", "u", "Number of divisions", GH_ParamAccess.item);
            pManager.AddIntegerParameter("v count", "v", "Number of divisions", GH_ParamAccess.item);
            pManager.AddIntegerParameter("w count", "w", "Number of divisions", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Division text", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Size for text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color", "Color for text and geometry", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geometry", "Sphere for place to drag line", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //---variables---

            List<Curve> curve = new List<Curve>();
            Brep brep = new Brep();

            //---input---

            if (!DA.GetDataList(0, curve)) return;
            if (!DA.GetData(1, ref brep)) return;

            //---setup---

            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(brep.GetVolume(), sqrt3);
            double adjustment = 8 / refLength; //the length should give 8

            //---solve---

            List<int> divAll = new List<int>();
            List<string> textAll = new List<string>();
            List<double> sizeAll = new List<double>();
            List<Plane> textPlaneAll = new List<Plane>();
            List<Sphere> sphereAll = new List<Sphere>();

            for (int i = 0; i < curve.Count; i++)
            {
                int div = Convert.ToInt32(curve[i].GetLength() * adjustment);
                div = div > max ? max : div;
                divAll.Add(div);

                (List<string> text, List<double> size, List<Plane> textPlane)= CreateText(curve[i], div, refLength, i);

                Sphere sphere = new Sphere(curve[i].PointAtEnd, (double)(size[0] / 2));

                textAll.AddRange(text);
                sizeAll.AddRange(size); 
                textPlaneAll.AddRange(textPlane);
                sphereAll.Add(sphere);
            }

            //---output---

            DA.SetData(0, divAll[0]);
            DA.SetData(1, divAll[1]);
            DA.SetData(2, divAll[2]);
            DA.SetDataList(3, textAll);
            DA.SetDataList(4, sizeAll);
            DA.SetDataList(5, textPlaneAll);
            DA.SetData(6, Color.White);
            DA.SetDataList(7, sphereAll);

        }

        public (List<string>, List<double>, List<Plane>) CreateText(Curve curve, double div, double refLength, int count)
        {
            List<string> text = new List<string>();
            string direction = "u";
            if (count == 1) { 
                direction = "v";
            }
            if (count == 2) {
                direction = "w";
            }
            text.Add(direction+"-direction: "+div.ToString());

            double refSize = (double)(refLength / 7);
            List<double> size = new List<double>() { refSize };

            List<Plane> textPlane = new List<Plane>();
            Point3d start = curve.PointAtStart;
            Point3d end = curve.PointAtEnd;
            Point3d p0 = Point3d.Add(end, new Point3d(0, 0, 2 * refSize));
            Point3d p1 = Point3d.Add(end, new Point3d(1, 0, 2 * refSize));
            Point3d p2 = Point3d.Add(end, new Point3d(0, 0, (1 + 2 * refSize)));
            textPlane.Add(new Plane(p0, p1, p2));

            double range = (double)(refLength / 8);
            for (int i = 1; i < 16; i++)
            {
                size.Add((double)(refSize / 2));
                string divRange = i.ToString();
                text.Add(divRange);
                Point3d p3 = Point3d.Add(start, new Point3d(-range + range * i, 0, -1 * refSize));
                Point3d p4 = Point3d.Add(start, new Point3d(1-range + range * i, 0, -1 * refSize));
                Point3d p5 = Point3d.Add(start, new Point3d(-range + range * i, 0, (1 - 1 * refSize)));
                Plane plane = new Plane(p3, p4, p5);
                textPlane.Add(plane);
            }
            return (text, size, textPlane);
        }
        
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.divSlider;
            }
        }

        public override Guid ComponentGuid
        { 
            get { return new Guid("f55bdd6e-1c71-44eb-b1d5-40d997763b3e"); }
        }
    }
}