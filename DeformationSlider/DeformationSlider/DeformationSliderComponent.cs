using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace DeformationSlider
{
    public class DeformationSliderComponent : GH_Component
    {
        readonly Text3d text = new Text3d("");
        readonly Text3d textValue = new Text3d("");
       
        public DeformationSliderComponent()
          : base("DeformationSlider", "DefSlider",
              "Slider for deformation scale in VR",
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
            pManager.AddNumberParameter("Scale", "Scale", "Scale value as length", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T", "Text", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Text placement", GH_ParamAccess.item);
            pManager.AddColourParameter("Text colors", "C text", "Color for deformed text", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            Brep brep = new Brep();
            if (!DA.GetData(0, ref curve)) return;
            if (!DA.GetData(1, ref brep)) return;
            double volume = brep.GetVolume();
            double sqrt3 = (double) 1 / 3;
            double refLength = Math.Pow(brep.GetVolume(), sqrt3);
            double refSize = (double)(refLength / 10);
            double adjustment = 200 / refLength; //5 times the length should give 1000
            double length = curve.GetLength()*adjustment;

            var tupleValue = CreateValueText(textValue, curve, length, refSize);
            string textValueOut = tupleValue.Item1;
            Plane planeValue = tupleValue.Item2;
            DA.SetData(0, length);
            DA.SetData(1, textValueOut);
            DA.SetData(2, planeValue);
            DA.SetData(3, Color.White);
        }
        public Tuple<string, Plane> CreateValueText(Text3d textValue, Curve curve, double length, double refSize)
        {
            textValue.Text = "Scale size: " + Math.Round(length).ToString();
            Point3d end = curve.PointAtEnd;
            Point3d p0 = Point3d.Add(end, new Point3d(0, 0, 0.4));
            Point3d p1 = Point3d.Add(end, new Point3d(0, -1, 0.4));
            Point3d p2 = Point3d.Add(end, new Point3d(0, 0, 1.4));
            textValue.TextPlane = new Plane(p0, p1, p2);
            textValue.Height = refSize;
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
            get { return new Guid("8a4183c2-fdcf-4af1-93ca-8e411a84a3bc"); }
        }
        public override void ExpireSolution(bool recompute)
        {
            base.ExpireSolution(recompute);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            //args.Display.Draw3dText(text, Color.Red);
            args.Display.Draw3dText(textValue, Color.White);
            //base.DrawViewportMeshes(args);
        }
    }
}
