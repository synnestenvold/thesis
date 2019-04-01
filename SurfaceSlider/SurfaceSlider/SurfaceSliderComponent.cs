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

namespace SurfaceSlider
{
    public class SurfaceSliderComponent : GH_Component
    {
        Text3d text = new Text3d("");
        //Text3d textValue = new Text3d("");
        List<Text3d> textList = new List<Text3d>();
        int max = 5;
       
        public SurfaceSliderComponent()
          : base("SurfaceSlider", "SurfSlider",
              "Description",
              "Category3", "SliderVR")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("SliderVR", "S", "Slider as curve", GH_ParamAccess.item);
            pManager.AddBrepParameter("Brep", "B", "Brep as reference size", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "S", "Surface", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T", "Text", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Text placement", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T info", "Text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "P info", "Text placement", GH_ParamAccess.list);
            pManager.AddColourParameter("Text colors", "C text", "Color for deformed text", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            Brep brep = new Brep();
            if (!DA.GetData(0, ref curve)) return;
            if (!DA.GetData(1, ref brep)) return;
            
            double volume = brep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(brep.GetVolume(), sqrt3);
            double refSize = (double)(refLength / 10);
            double adjustment = 5 / refLength; //the length should give 5

            int surface = Convert.ToInt32(curve.GetLength() * adjustment);
            surface = surface > max ? max : surface;

            var tuple = CreateText(text, curve, refSize);
            string textOut = tuple.Item1;
            Plane plane = tuple.Item2;
            
            List<string> textListOut = new List<string>() { "1", "2", "3", "4", "5", "6" };
            List<Plane> planeList = CreateTextList(curve, refLength, refSize);

            Surface surf = brep.Surfaces[surface];
            DA.SetData(0, surf);
            DA.SetData(1, textOut);
            DA.SetData(2, plane);
            DA.SetDataList(3, textListOut);
            DA.SetDataList(4, planeList);
            DA.SetData(5, Color.White);
        }

        public Tuple<string, Plane> CreateText(Text3d text, Curve curve, double refSize)
        {
            text.Text = "Adjust to pick surface (1-6)";
            Point3d start = curve.PointAtStart;
            Point3d p0 = Point3d.Add(start, new Point3d(0, 0, 2 * refSize));
            Point3d p1 = Point3d.Add(start, new Point3d(1, 0, 2 * refSize));
            Point3d p2 = Point3d.Add(start, new Point3d(0, 0, (1 + 2 * refSize)));
            text.TextPlane = new Plane(p0, p1, p2);
            text.Height = refSize;
            return Tuple.Create(text.Text, text.TextPlane);
        }
        List<Plane> CreateTextList(Curve curve, double refLength, double refSize)
        {
            List<Plane> textPlaneList = new List<Plane>();

            double range = (double)(refLength / 6);
            for (int i = 1; i < 7; i++)
            {
                Text3d tall = new Text3d(i.ToString());
                textList.Add(tall);
                Point3d start = curve.PointAtStart;
                Point3d p0 = Point3d.Add(start, new Point3d(0 + range * i, 0, -2 * refSize));
                Point3d p1 = Point3d.Add(start, new Point3d(1 + range * i, 0, -2 * refSize));
                Point3d p2 = Point3d.Add(start, new Point3d(0 + range * i, 0, (1 - 2 * refSize)));
                Plane plane = new Plane(p0, p1, p2);
                textList[i - 1].TextPlane = plane;
                textList[i - 1].Height = (double)(refSize / 2);
                textPlaneList.Add(plane);

            }
            return textPlaneList;
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
            get { return new Guid("aad2b024-a051-47f4-8569-67ed6ce30a8b"); }
        }
        public override void ExpireSolution(bool recompute)
        {
            base.ExpireSolution(recompute);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            //args.Display.Draw3dText(text, Color.Red);
            args.Display.Draw3dText(text, Color.White);
            for (int i = 0; i < 6; i++)
            {
                args.Display.Draw3dText(textList[i], Color.White);
            }
            //base.DrawViewportMeshes(args);
        }
    }
}
