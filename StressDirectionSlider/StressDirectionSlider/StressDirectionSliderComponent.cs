﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace StressDirectionSlider
{
    public class StressDirectionSliderComponent : GH_Component
    {
        Text3d text = new Text3d("");
        Text3d textValue = new Text3d("");
        int maxDir = 5;

        public StressDirectionSliderComponent()
          : base("StressDirectionSlider", "StressSlider",
              "Slider for what stress direction to show",
              "Category3", "SliderVR")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("SliderVR", "S", "Slider as curve", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Direction", "Dir", "Direction of stress (0-5)", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T", "Direction text", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Placement for text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            if (!DA.GetData(0, ref curve)) return;

            int dir = Convert.ToInt32(curve.GetLength());
            dir = dir > maxDir ? maxDir : dir;

            //Text start
            var tuple = CreateText(text, curve);
            string textOut = tuple.Item1;
            Plane plane = tuple.Item2;
            //Text on the other side shows the value of load
            var tupleValue = CreateValueText(textValue, curve, dir);
            string textValueOut = tupleValue.Item1;
            Plane planeValue = tupleValue.Item2;

            DA.SetData(0, dir);
            DA.SetData(1, textValueOut);
            DA.SetData(2, planeValue);

        }

        public Tuple<string, Plane> CreateText(Text3d text, Curve curve)
        {
            text.Text = "Adjust for direction to view";
            Point3d start = curve.PointAtStart;
            Point3d p0 = Point3d.Add(start, new Point3d(0, 0, 0.4));
            Point3d p1 = Point3d.Add(start, new Point3d(1, 0, 0.4));
            Point3d p2 = Point3d.Add(start, new Point3d(0, 0, 1.4));
            text.TextPlane = new Plane(p0, p1, p2);
            text.Height = 0.4;
            return Tuple.Create(text.Text, text.TextPlane);
        }

        public Tuple<string, Plane> CreateValueText(Text3d textValue, Curve curve, int dir)
        {
            string direction = "";
            if (dir < 1) direction = "σ,xx";
            else if (dir < 1) direction = "σ,yy";
            else if (dir < 1) direction = "σ,zz";
            else if (dir < 1) direction = "σ,yz?";
            else if (dir < 1) direction = "σ,xz?";
            else direction = "σ,xy?";
            textValue.Text = "Direction: " + direction;
            Point3d end = curve.PointAtEnd;
            Point3d p0 = Point3d.Add(end, new Point3d(0, 0, 0.4));
            Point3d p1 = Point3d.Add(end, new Point3d(1, 0, 0.4));
            Point3d p2 = Point3d.Add(end, new Point3d(0, 0, 1.4));
            textValue.TextPlane = new Plane(p0, p1, p2);
            textValue.Height = 0.6;
            return Tuple.Create(textValue.Text, textValue.TextPlane);
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
            get { return new Guid("db0c32b8-24e5-443c-8c3a-03bb239ba4ed"); }
        }
        public override void ExpireSolution(bool recompute)
        {
            base.ExpireSolution(recompute);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            args.Display.Draw3dText(text, Color.Red);
            args.Display.Draw3dText(textValue, Color.Red);
            //base.DrawViewportMeshes(args);
        }
    }
}
