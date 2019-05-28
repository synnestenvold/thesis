using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Drawing;
using System.Linq;

namespace SolidsVR
{
    public class ViewStresses : GH_Component
    {

        public ViewStresses()
          : base("ViewStresses", "ViewStress",
              "Display stress in VR",
              "SolidsVR", "VR Preview")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh for Brep", GH_ParamAccess.item);
            pManager.AddNumberParameter("Stress", "S", "Stress in each dof", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Stress direction", "S dir", "S11, S22, S33, S12, S13, S23, mises as 0, 1, 2, 3, 4, 5, 6", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scaling", "Scale", "Scale factor for the view", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Brep solid", "Solid", "3D Model of stresses", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color solid", "Colors for breps", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Stress legend", "Legend", "Breps for stress range", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color legend", "Colors for stress legend", GH_ParamAccess.list);
            pManager.AddTextParameter("Text", "Text", "Text for stress legend and headline", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Text size", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color T", "Colors for text", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---

            MeshGeometry mesh = new MeshGeometry();
            GH_Structure<GH_Number> treeStress = new GH_Structure<GH_Number>();
            int dir = new int();
            double scale = new double();
            

            //---input---

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataTree(1, out treeStress)) return;
            if (!DA.GetData(2, ref dir)) return;
            if (!DA.GetData(3, ref scale)) return;

            //---setup---

            BrepGeometry brp = mesh.GetBrep();
            Brep brep = brp.GetBrep();
            Point3d centroid = brp.GetCentroid();
            double refLength = brp.GetRefLength();
            Point3d center = Point3d.Add(centroid, new Point3d(0, -refLength * 6.5, 0));
            double angle = 90 * Math.PI / 180;

            //---solve---

            List<Element> elements = mesh.GetElements();
            List<Node> nodes = mesh.GetNodeList();

            List<Brep> breps = CreateDefBreps(elements, scale, angle, center);

            //Getting colors for each brep
            (List <Color> brepColors, List<string> rangeValues) = ColorBreps(elements, breps, dir);

            //Creating breps stress legend
            List<Brep> brepRanges = CreateBrepRanges(centroid, refLength, center, angle);

            //Getting colors stress legend
            List<Color> colorRange = CreateColorRange(); //Output brep colors legend

            (List <Plane> planeRanges, double textSize) = CreateTextPlanes(brepRanges, refLength);

            List<double> textSizeRange = Enumerable.Repeat(textSize, rangeValues.Count).ToList();  //Output text legend size
            List<Color> textColorRange = Enumerable.Repeat(Color.White, rangeValues.Count).ToList(); // Output text color legend

            //Createing headline for area
            var tuple3 = CreateHeadline(centroid, angle, center, refLength);

            string headText = tuple3.Item1;
            double headSize = tuple3.Item2;
            Plane headPlane = tuple3.Item3;
            Color headColor = tuple3.Item4;

            //Adding the different text components together to one output.

            rangeValues.Add(headText);
            List<string> text = rangeValues;

            textSizeRange.Add(headSize);
            List<double> textSizes = textSizeRange;

            planeRanges.Add(headPlane);
            List<Plane> textPlanes = planeRanges;

            textColorRange.Add(headColor);
            List<Color> textColors = textColorRange;

            //---output---

            DA.SetDataList(0, breps);
            DA.SetDataList(1, brepColors);
            DA.SetDataList(2, brepRanges);
            DA.SetDataList(3, colorRange);
            DA.SetDataList(4, text);
            DA.SetDataList(5, textSizes);
            DA.SetDataList(6, textPlanes);
            DA.SetDataList(7, textColors);
        }

        public List<Brep> CreateDefBreps(List<Element> elements, double scale, double angle, Point3d center)
        {
            List<Brep> breps = new List<Brep>();
            for (int j = 0; j < elements.Count; j++)
            {
                var mesh = new Mesh();
                List<Node> vertices = elements[j].GetVertices();

                for (int i = 0; i < vertices.Count; i++)
                {
                    Point3d p = vertices[i].GetCoord();
                    Vector3d defVector = new Vector3d(vertices[i].GetDeformation()[0], vertices[i].GetDeformation()[1], vertices[i].GetDeformation()[2]);
                    Point3d new_p = Point3d.Add(p, defVector * scale);
                    mesh.Vertices.Add(new_p);
                }
                mesh.Faces.AddFace(0, 1, 5, 4);
                mesh.Faces.AddFace(1, 2, 6, 5);
                mesh.Faces.AddFace(2, 3, 7, 6);
                mesh.Faces.AddFace(0, 3, 7, 4);
                mesh.Faces.AddFace(4, 5, 6, 7);
                mesh.Faces.AddFace(0, 1, 2, 3);

                Brep new_brep = Brep.CreateFromMesh(mesh, false);
                Vector3d vecAxis = new Vector3d(0, 0, 1);
                new_brep.Rotate(angle, vecAxis, center);
                breps.Add(new_brep);
            }
            return breps;
        }

        public (List<Color>, List<string>) ColorBreps(List<Element> elements, List<Brep> breps, int dir)
        {
            List<Color> brepColors = new List<Color>();
            Color color = Color.White;
            List<string> rangeValues = new List<String>();

            double[] averageValues = AverageValuesStress(elements, dir);
            double max = averageValues.Max();
            double min = averageValues.Min();
            double range = (max - min) / 13;

            //Adding values to stress range
            for (int i = 0; i <= 13; i++)
            {
                rangeValues.Add((Math.Round(min + i * range, 4)).ToString() + "_");
            }

            //Creating header
            if (dir == 0) rangeValues.Add("S,xx [MPa]");
            else if (dir == 1) rangeValues.Add("S,yy [Mpa]");
            else if (dir == 2) rangeValues.Add("S,zz [Mpa]");
            else if (dir == 3) rangeValues.Add("S,yz [Mpa]");
            else if (dir == 4) rangeValues.Add("S,xz [Mpa]");
            else if (dir == 5) rangeValues.Add("S,zy [Mpa]");
            else if (dir == 6) rangeValues.Add("von Mises [Mpa]");

            for (int i = 0; i < breps.Count; i++)
            {
                if (averageValues[i] < min + range) color = Color.Blue;
                else if (averageValues[i] < min + 2 * range) color = Color.RoyalBlue;
                else if (averageValues[i] < min + 3 * range) color = Color.DeepSkyBlue;
                else if (averageValues[i] < min + 4 * range) color = Color.Cyan;
                else if (averageValues[i] < min + 5 * range) color = Color.PaleGreen;
                else if (averageValues[i] < min + 6 * range) color = Color.LimeGreen;
                else if (averageValues[i] < min + 7 * range) color = Color.Lime;
                else if (averageValues[i] < min + 8 * range) color = Color.Lime;
                else if (averageValues[i] < min + 9 * range) color = Color.GreenYellow;
                else if (averageValues[i] < min + 10 * range) color = Color.Yellow;
                else if (averageValues[i] < min + 11 * range) color = Color.Orange;
                else if (averageValues[i] < min + 12 * range) color = Color.OrangeRed;
                else color = Color.Red;
                brepColors.Add(color);
            }

            return (brepColors, rangeValues);
        }

        public double[] AverageValuesStress(List<Element> elements, int dir)
        {
            double[] averageList = new double[elements.Count];

            for (int i = 0; i < elements.Count; i++)
            {
                averageList[i] = elements[i].GetAverageStressDir(dir);

            }
            return averageList;
        }

        public List<Brep> CreateBrepRanges(Point3d centroid, double refLength, Point3d center, double angle)
        {
            List<Brep> brepRanges = new List<Brep>();
            List<Plane> planeRanges = new List<Plane>();
            Brep brep_new = new Brep();

            int ranges = 13;
            double totLength = refLength * 1.5;
            double rangeHeight = totLength / ranges;

            Vector3d rangePos = new Vector3d(refLength, 0, refLength);
            Point3d startRanges = centroid + rangePos;

            Vector3d vecBrep = new Vector3d(0, 0, 1);

            Plane plane = new Plane(new Point3d(0, 0, 0), vecBrep);

            for (int i = 0; i < ranges; i++)
            {
                Interval x = new Interval(startRanges.X, startRanges.X + refLength * 0.8);
                Interval y = new Interval(startRanges.Y, startRanges.Y + refLength * 0.2);
                Interval z = new Interval(startRanges.Z + rangeHeight * i, startRanges.Z + rangeHeight * (i + 1));
                Box box_new = new Box(Plane.WorldXY, x, y, z);
                brep_new = box_new.ToBrep();

                Vector3d vecAxis = new Vector3d(0, 0, 1);
                brep_new.Rotate(angle, vecAxis, center);

                brepRanges.Add(brep_new);
            }

            return brepRanges;
        }

        public List<Color> CreateColorRange()
        {
            List<Color> colorRange = new List<Color>
            {
                Color.Blue,
                Color.RoyalBlue,
                Color.DeepSkyBlue,
                Color.Cyan,
                Color.PaleGreen,
                Color.LimeGreen,
                Color.Lime,
                Color.Lime,
                Color.GreenYellow,
                Color.Yellow,
                Color.Orange,
                Color.OrangeRed,
                Color.Red,
            };

            return colorRange;
        }

        public (List<Plane>, double) CreateTextPlanes(List<Brep> brepRanges, double refLength)
        {
            int ranges = 13;
            double totLength = refLength * 1.2;
            double rangeHeight = totLength / ranges;
            double textSize = rangeHeight * 0.8;

            Vector3d textVector = new Vector3d(0, 0, textSize / 2);

            List<Plane> planeRanges = new List<Plane>();

            Vector3d vecBrep = new Vector3d(0, 0, 0);
            Point3d p = new Point3d(0, 0, 0);
            Plane plane = new Plane(p, p, p);

            for (int i = 0; i < brepRanges.Count; i++)
            {
                Point3d[] points = brepRanges[i].DuplicateVertices();

                vecBrep = points[1] - points[0];

                Point3d p0 = points[0];
                Point3d p1 = points[0] - vecBrep;
                Point3d p2 = points[4];

                plane = new Plane(p0, p1, p2);

                plane.Translate(textVector);
                plane.Translate(-vecBrep * 0.3);

                planeRanges.Add(plane);
            }

            plane.Translate(new Vector3d(0, 0, rangeHeight));

            planeRanges.Add(plane);

            plane.Translate(new Vector3d(0, 0, rangeHeight));
            plane.Translate(vecBrep);

            planeRanges.Add(plane);

            return (planeRanges, textSize);
        }


        public (string, double, Plane, Color) CreateHeadline(Point3d centroid, double angle, Point3d center, double refLength)
        {
            string headText = "Stress analysis";

            double headSize = (double)refLength / 1.5;

            Point3d p0 = centroid;
            Point3d p1 = Point3d.Add(p0, new Point3d(-1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane headPlane = new Plane(p0, p1, p2);
            
            headPlane.Translate(new Vector3d(0, -headSize, 3.5*refLength));
            headPlane.Rotate(angle, new Vector3d(0, 0, 1), center);

            Color headColor = Color.FromArgb(0, 100, 255);

            return (headText, headSize, headPlane, headColor);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.stress;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0f728642-ce7d-4508-95dd-4304ceb4d175"); }
        }
    }
}