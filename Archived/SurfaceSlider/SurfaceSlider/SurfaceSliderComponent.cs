﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;


namespace SurfaceSlider
{
    public class SurfaceSliderComponent : GH_Component
    {
        int max = 5;
       
        public SurfaceSliderComponent()
          : base("SurfaceSlider", "SurfSlider",
              "Description",
              "Category3", "SliderVR")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("SliderVR", "S", "Slider as curve", GH_ParamAccess.item);
            pManager.AddBrepParameter("Brep", "B", "Brep as reference size", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Text size", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Text placement", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color", "Color for text", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "Geometry", "Sphere on point to drag", GH_ParamAccess.item);
            
        }
        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            Brep brep = new Brep();
            if (!DA.GetData(0, ref curve)) return;
            if (!DA.GetData(1, ref brep)) return;
            
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(brep.GetVolume(), sqrt3);
            double adjustment = 5 / refLength; //the length should give 5

            int surface = Convert.ToInt32(curve.GetLength() * adjustment);
            surface = surface > max ? max : surface;
            Surface surf = brep.Surfaces[surface];

            var tuple = CreateText(curve, refLength);
            List<string> text = tuple.Item1;
            List<double> size = tuple.Item2;
            List<Plane> textPlane = tuple.Item3;
            Color color = tuple.Item4;

            Sphere sphere = new Sphere(curve.PointAtEnd, (double)(size[0] / 2));

            DA.SetData(0, surf);
            DA.SetDataList(1, text);
            DA.SetDataList(2, size);
            DA.SetDataList(3, textPlane);
            DA.SetData(4, color);
            DA.SetData(5, sphere);
            
        }

        public Tuple<List<string>, List<double>, List<Plane>, Color> CreateText(Curve curve, double refLength)
        {
            List<string> text = new List<string>();
            text.Add("Adjust to pick surface (1-6)");
            text.AddRange(new List<string>() { "1", "2", "3", "4", "5", "6" });
            double refSize = (double)(refLength / 10);
            List<double> size = new List<double>() { refSize, (double)(refSize / 2) };
            List<Plane> textPlane = new List<Plane>();
            Point3d start = curve.PointAtStart;
            Point3d end = curve.PointAtEnd;
            Point3d p0 = Point3d.Add(start, new Point3d(0, 0, 2 * refSize));
            Point3d p1 = Point3d.Add(start, new Point3d(1, 0, 2 * refSize));
            Point3d p2 = Point3d.Add(start, new Point3d(0, 0, (1 + 2 * refSize)));
            textPlane.Add(new Plane(p0, p1, p2));
            double range = (double)(refLength / 6);
            for (int i = 1; i < 7; i++)
            {
                string surfRange = i.ToString();
                text.Add(surfRange);
                Point3d p3 = Point3d.Add(start, new Point3d(0 + range * i, 0, -2 * refSize));
                Point3d p4 = Point3d.Add(start, new Point3d(1 + range * i, 0, -2 * refSize));
                Point3d p5 = Point3d.Add(start, new Point3d(0 + range * i, 0, (1 - 2 * refSize)));
                Plane plane = new Plane(p3, p4, p5);
                textPlane.Add(plane);
                
            }

            return Tuple.Create(text, size, textPlane, Color.White);
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

        public override Guid ComponentGuid
        {
            get { return new Guid("aad2b024-a051-47f4-8569-67ed6ce30a8b"); }
        }
    }
}
