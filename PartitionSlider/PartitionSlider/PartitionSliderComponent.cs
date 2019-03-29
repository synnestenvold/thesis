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

namespace PartitionSlider
{
    

    public class PartitionSliderComponent : GH_Component
    {
        readonly int maxPartition = 10;
        readonly Text3d text = new Text3d("");
        readonly Text3d textValue = new Text3d("");

        public PartitionSliderComponent()
          : base("PartitionSlider", "PartSlider",
              "Slider for partition in VR",
              "Category3", "SliderVR")
        {
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("SliderVR", "S", "Slider as curve", GH_ParamAccess.item);
            pManager.AddBrepParameter("Brep", "B", "Brep as reference", GH_ParamAccess.item);
        }
        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Partitions", "P", "Number of partitions", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T", "Partition text", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Placement for text", GH_ParamAccess.item);
            pManager.AddColourParameter("Text colors", "C text", "Color for deformed text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            Brep brep = new Brep();
            if (!DA.GetData(0, ref curve)) return;
            if (!DA.GetData(1, ref brep)) return;
            double volume = brep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(brep.GetVolume(), sqrt3);
            double adjustment = 10 / refLength; //the length should give 10
           
            int parts = Convert.ToInt32(curve.GetLength()*adjustment);
            parts = parts > maxPartition ? maxPartition : parts;
            
            //Text start (not needed for partitions?)
            //var tuple = CreateText(text, curve);
            //string textOut = tuple.Item1;
            //Plane plane = tuple.Item2;
            //Text on the other side shows the number of partitions
            var tupleValue = CreateValueText(textValue, curve, parts);
            string textValueOut = tupleValue.Item1;
            Plane planeValue = tupleValue.Item2;

           

            DA.SetData(0, parts);
            DA.SetData(1, textValueOut);
            DA.SetData(2, planeValue);
            DA.SetData(3, Color.White);

        }

        public Tuple<string, Plane> CreateText(Text3d text, Curve curve)
        {
            text.Text = "Adjust for partitions";
            Point3d start = curve.PointAtStart;
            Point3d p0 = Point3d.Add(start, new Point3d(0, 0, 0.4));
            Point3d p1 = Point3d.Add(start, new Point3d(1, 0, 0.4));
            Point3d p2 = Point3d.Add(start, new Point3d(0, 0, 1.4));
            text.TextPlane = new Plane(p0, p1, p2);
            text.Height = 0.6;
            return Tuple.Create(text.Text, text.TextPlane);
        }

        public Tuple<string, Plane> CreateValueText(Text3d textValue, Curve curve, int parts)
        {
            textValue.Text = "Partitions: "+parts.ToString();
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
            get { return new Guid("f55bdd6e-1c71-44eb-b1d5-40d997763b3e"); }
        }

        public override void ExpireSolution(bool recompute)
        {
            base.ExpireSolution(recompute);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            //args.Display.Draw3dText(text, Color.White);
            args.Display.Draw3dText(textValue, Color.White);
            //base.DrawViewportMeshes(args);
        }
    }
}
