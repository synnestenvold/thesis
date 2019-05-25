using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace SolidsVR
{
    public class PreDef : GH_Component
    {
 
        public PreDef()
          : base("PreDef", "PreDef",
              "Prescribed deformation in nodes",
              "SolidsVR", "Load")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Sphere", "S", "Sphere for finding point", GH_ParamAccess.list);
            pManager.AddVectorParameter("Prescribed deformations", "PreDef", "Prescribed deformation as a vector", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh of geometry", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Prescribed deformations", "PreDef", "Def in point, (x,y,z);(Tx,Ty,Tz)", GH_ParamAccess.list);
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "Size", "Size", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color", "Colors for text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //---variables---

            List<Brep> spheres = new List<Brep>();
            Vector3d def = new Vector3d(0, 0, 0);
            MeshGeometry mesh = new MeshGeometry();

            List<string> pointsString = new List<string>();

            //---input---

            if (!DA.GetDataList(0, spheres)) return;
            if (!DA.GetData(1, ref def)) return;
            if (!DA.GetData(2, ref mesh)) return;

            //---setup---

            //Setting up values for reflength and angle for rotation of area
            Brep origBrep = mesh.GetOrigBrep();
            VolumeMassProperties vmp = VolumeMassProperties.Compute(origBrep);
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            //---solve---

            List<Point3d> points = new List<Point3d>();
            string text = "";
            List<Plane> planeSphere = new List<Plane>();
            

            for (int i = 0; i < spheres.Count; i++)
            {
                Brep sphere = spheres[i];
                //List of global points with correct numbering
                List<Point3d> globalPoints = mesh.GetGlobalPoints();

                VolumeMassProperties vmpSphere = VolumeMassProperties.Compute(sphere);
                Point3d centroidSphere = vmpSphere.Centroid;


                points.Add(FindClosestPoint(globalPoints, centroidSphere, refLength));

                text = "Drag sphere to point";

                planeSphere.Add(FindSpherePlane(centroidSphere, refLength));
            }

            string pointString;
            foreach (Point3d p in points)
            {
                pointString = p.X.ToString() + "," + p.Y.ToString() + "," + p.Z.ToString();
                pointsString.Add(pointString);
            }

            List<string> pointDef = new List<string>();
            foreach (string s in pointsString)
            {
                pointDef.Add(s + ";" + def.ToString());
            }

            double size = (double)refLength / 7;

            Color color = Color.Orange;

            //---output---

            DA.SetDataList(0, pointDef);
            DA.SetData(1, text);
            DA.SetData(2, size);
            DA.SetDataList(3, planeSphere);
            DA.SetData(4, color);
        }

        public Point3d FindClosestPoint(List<Point3d> globalPoints, Point3d centroid, double refLength)
        {
            Point3d closestPoint = new Point3d(999999, 999999, 999999);

            double length = double.PositiveInfinity;

            for (int i = 0; i < globalPoints.Count; i++)
            {
                double checkLength = globalPoints[i].DistanceTo(centroid);

                if (checkLength < length && checkLength < refLength / 2)
                {
                    length = checkLength;
                    closestPoint = globalPoints[i];
                }
            }

            return closestPoint;
        }

        public Plane FindSpherePlane(Point3d centroid, double refLength)
        {
            Point3d p0 = new Point3d(centroid.X, centroid.Y, centroid.Z + refLength/5);
            Point3d p1 = Point3d.Add(p0, new Point3d(1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane p = new Plane(p0, p1, p2);

            return p;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.predef;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("25e13746-7e88-4749-a603-98926136ebb6"); }
        }
    }
}