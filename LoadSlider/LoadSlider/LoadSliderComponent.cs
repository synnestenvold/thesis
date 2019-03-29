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

namespace LoadSlider
{
    public class LoadSliderComponent : GH_Component
    {
        Text3d text = new Text3d("");
        Text3d textValue = new Text3d("");

        public LoadSliderComponent()
          : base("LoadSlider", "LoadSlider",
              "Load slider for VR",
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

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Load", "L", "Load vector and value", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T", "Slider text", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Placement for text", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T2", "Slider text", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P2", "Placement for text", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
        
            Curve curve = null;
            if (!DA.GetData(0, ref curve)) return;
            Vector3d vector = curve.PointAtEnd - curve.PointAtStart;
            //Text on start of curve
            var tuple = CreateText(text, curve);
            string textOut = tuple.Item1;
            Plane plane = tuple.Item2;
            //Text on the other side shows the value of load
            var tupleValue = CreateTextValue(textValue, curve, vector);
            string textValueOut = tupleValue.Item1;
            Plane planeValue = tupleValue.Item2;

            DA.SetData(0, vector);
            DA.SetData(1, textOut);
            DA.SetData(2, plane);
            DA.SetData(3, textValueOut);
            DA.SetData(4, planeValue);

        }

       
        public Tuple<string, Plane> CreateText(Text3d text, Curve curve)
        {
            text.Text = "Adjust for load in kN/m^2";
            Point3d start = curve.PointAtStart;
            Point3d p0 = Point3d.Add(start, new Point3d(0, 0, 0.4));
            Point3d p1 = Point3d.Add(start, new Point3d(1, 0, 0.4));
            Point3d p2 = Point3d.Add(start, new Point3d(0, 0, 1.4));
            text.TextPlane = new Plane(p0, p1, p2);
            text.Height = 0.6;
            return Tuple.Create(text.Text, text.TextPlane);
        }

        public Tuple<string, Plane> CreateTextValue(Text3d textValue, Curve curve, Vector3d vector)
        {
            textValue.Text = "("+Math.Round((vector.X),3).ToString()+", "+Math.Round((vector.Y),3).ToString()+", "+Math.Round((vector.Z),3).ToString()+")";
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
        
        public override Guid ComponentGuid
        {
            get { return new Guid("74238820-07e9-4490-966e-8a8dcfe33ca8"); }
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
