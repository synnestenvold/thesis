using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace SolidsVR
{
    public class CrossSection : GH_Component
    {

        public CrossSection()
          : base("CrossSection", "CrossSec",
              "Get information of CrossSec",
              "SolidsVR", "VR Info")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "Curves for Surface for cross-section", GH_ParamAccess.list);
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh for Brep", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "P", "Closest point to sphere", GH_ParamAccess.list);
            pManager.AddColourParameter("Color", "Color", "Colors for points", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Size for text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Color", "Color", "Colors for text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---

            List<Curve> curves = new List<Curve>();
            MeshGeometry mesh = new MeshGeometry();

            //---input---

            if (!DA.GetDataList(0, curves)) return;
            if (!DA.GetData(1, ref mesh)) return;

            //---setup---

            int u = mesh.GetU();
            int v = mesh.GetV();
            Brep origBrep = mesh.GetOrigBrep();
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);
            Brep brepSurface = Brep.CreateEdgeSurface(curves);
            Point3d[] vertices = brepSurface.DuplicateVertices();
            Surface surface = brepSurface.Faces[0];

            // --- solve ---

            List<Point3d> points= CreatePoints(surface, u, v);

            Color pointsColor = Color.FromArgb(135, 206, 255);

            (List<string> texts, List <Plane> planes) = CreateTextAndPlane(curves);

            double size = (double)refLength / 7;

            Color colorText = Color.Orange;

            // --- output ---

            DA.SetDataList(0, points);
            DA.SetData(1, pointsColor);
            DA.SetDataList(2, texts);
            DA.SetData(3, size);
            DA.SetDataList(4, planes);
            DA.SetData(5, colorText);
        }

        public List<Point3d> CreatePoints(Surface surface, int u, int v)
        {
            List<Point3d> points = new List<Point3d>();

            Interval domainU = surface.Domain(0);
            Interval domainV = surface.Domain(1);

            domainU.Swap();

            for (int i = 0; i <= v; i++)
            {
                //Get domain values in v-direction
                double tv = domainV.ParameterAt(i / (double)v);
                for (int j = 0; j <= u; j++)
                {
                    //Get domain values in u-direction
                    double tu = domainU.ParameterAt(j / (double)u);
                    Point3d p1 = surface.PointAt(tu, tv); //Creating points in domain
                    points.Add(p1);
                }
            }
            return points;
        }

        public (List<string>,List<Plane>) CreateTextAndPlane(List<Curve> curves)
        {
            List<string> text = new List<string>();
            List<Plane> planes = new List<Plane>();

            //Text that will be added
            string header = "CROSS-SECTION";
            string uDir = "u-direction";
            string vDir = "v-direction";

            double refLength = curves[0].GetLength();

            Point3d hp1 = curves[0].PointAtEnd;
            Point3d hp2 = curves[0].PointAtStart;
            Vector3d headerVec = (hp2 - hp1)/2;
            Point3d headPoint = Point3d.Add(hp1, headerVec);

            //Creating plane for header
            Point3d hplane0 = new Point3d(headPoint.X, headPoint.Y, headPoint.Z + refLength);
            Point3d hplane1 = Point3d.Add(hplane0, new Point3d(1, 0, 0));
            Point3d hplane2 = Point3d.Add(hplane1, new Point3d(0, 0, 1));

            Plane hplane = new Plane(hplane0, hplane1, hplane2);

            planes.Add(hplane);

            //Creating plane for text in v-direction
            Point3d vplane0 = new Point3d(headPoint.X, headPoint.Y, headPoint.Z + refLength / 3 + 10);
            Point3d vplane1 = Point3d.Add(vplane0, new Point3d(1, 0, 0));
            Point3d vplane2 = Point3d.Add(vplane1, new Point3d(0, 0, 1));

            Plane vplane = new Plane(vplane0, vplane1, vplane2);

            planes.Add(vplane);

            //Creating plane for text in u-direction
            Point3d up1 = curves[1].PointAtEnd;
            Point3d up2 = curves[1].PointAtStart;
            Vector3d uVec = (up2 - up1)/2;
            Point3d uPoint = Point3d.Add(up1, uVec);

            Point3d uplane0 = new Point3d(uPoint.X+120, uPoint.Y, uPoint.Z);
            Point3d uplane1 = Point3d.Add(uplane0, new Point3d(1, 0, 0));
            Point3d uplane2 = Point3d.Add(uplane1, new Point3d(0, 0, 1));

            Plane uplane = new Plane(uplane0, uplane1, uplane2);

            planes.Add(uplane);

            text.Add(header);
            text.Add(vDir);
            text.Add(uDir);

            return (text, planes);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.crossSec;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("78178f4e-a6ff-4af9-95c7-7147908582ca"); }
        }
    }
}