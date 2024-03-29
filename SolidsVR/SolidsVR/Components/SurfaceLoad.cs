﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;


namespace SolidsVR
{
    public class SurfaceLoad : GH_Component
    {
      
        public SurfaceLoad()
          : base("SurfaceLoad", "SurfaceLoad",
              "Uniform load component for FEA",
              "SolidsVR", "Load")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Surface number", "S", "Surface number for loading (0-5)", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load vector", "Load", "Direction and load amount in kN/m", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh", "Mesh", "MeshGeometry class", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Point loads", "PL", "Lumped load to points, (x,y,z);(Fx,Fy,Fz)", GH_ParamAccess.list);
            pManager.AddLineParameter("Geometry", "Geometry", "Arrows showing the load", GH_ParamAccess.list);
            pManager.AddColourParameter("Color", "Color", "Coloring of arrows", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //---variables---
            
            int surfNo = 0;
            Vector3d forceVec = new Vector3d();
            BrepGeometry brp = new BrepGeometry();
            MeshGeometry mesh = new MeshGeometry();

            //---input---

            if (!DA.GetData(0, ref surfNo)) return;
            if (!DA.GetData(1, ref forceVec)) return;
            if (!DA.GetData(2, ref mesh)) return;

            //---solve---

            brp = mesh.GetBrep();
            Brep surface = mesh.GetSurfaceAsBrep(surfNo);
            double area = surface.GetArea();
            List<Node> nodes = mesh.GetNodeList();
            (List<string> pointLoads, double maxLoad) = FindPointLoads(surfNo, area, forceVec, nodes, brp);

            // For previewing of loads

            double refLength = brp.GetRefLength();
            List<Line> arrows = DrawLoads(pointLoads, refLength, maxLoad);
            Color color = Color.FromArgb(135, 206, 255);

            //---output---

            DA.SetDataList(0, pointLoads);
            DA.SetDataList(1, arrows);
            DA.SetData(2, color);
        }

        public (List<string>, double) FindPointLoads(int surfNo, double area, Vector3d forceVec, List<Node> nodes, BrepGeometry brp)
        {
            List<string> pointLoads = new List<string>();
            Vector3d maxLoads = new Vector3d(0, 0, 0);
            double maxLoad = 0;
            List<string> centerPointsString = new List<string>();
            List<string> cornerPointsString = new List<string>();
            List<string> edgePointsString = new List<string>();
            forceVec = forceVec * area;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].GetSurfaceNum().Contains(surfNo))

                {
                    nodes[i].SetRemovable(false);
                    if (nodes[i].GetIsMiddle())
                    {
                        Point3d node = nodes[i].GetCoord();
                        string pointString = node.X.ToString() + "," + node.Y.ToString() + "," + node.Z.ToString();
                        centerPointsString.Add(pointString);

                    }
                    else if (nodes[i].GetIsCorner())
                    {
                        Point3d node = nodes[i].GetCoord();
                        string pointString = node.X.ToString() + "," + node.Y.ToString() + "," + node.Z.ToString();
                        cornerPointsString.Add(pointString);
                    }
                    else if (nodes[i].GetIsEdge())
                    {
                        Point3d node = nodes[i].GetCoord();
                        string pointString = node.X.ToString() + "," + node.Y.ToString() + "," + node.Z.ToString();
                        edgePointsString.Add(pointString);
                    }
                }
            }
            double pointsCount = 4 * centerPointsString.Count + cornerPointsString.Count + 2 * edgePointsString.Count;

            List<string> centerPointLoads = new List<string>();
            List<string> cornerPointLoads = new List<string>();
            List<string> edgePointLoads = new List<string>();
            string centerVector = 4 * (forceVec.X) / pointsCount + "," + 4 * (forceVec.Y) / pointsCount + "," + 4 * (forceVec.Z) / pointsCount;
            string cornerVector = (forceVec.X) / pointsCount + "," + (forceVec.Y) / pointsCount + "," + (forceVec.Z) / pointsCount;
            string edgeVector = 2 * (forceVec.X) / pointsCount + "," + 2 * (forceVec.Y) / pointsCount + "," + 2 * (forceVec.Z) / pointsCount;

            foreach (string s in centerPointsString)
            {
                centerPointLoads.Add(s + ";" + centerVector);

                string[] centerLoads = (centerVector.Split(','));

                double loadX = Math.Round(double.Parse(centerLoads[0]), 8);
                double loadY = Math.Round(double.Parse(centerLoads[1]), 8);
                double loadZ = Math.Round(double.Parse(centerLoads[2]), 8);
                Vector3d loads = new Vector3d(loadX, loadY, loadZ);

                if (loads.Length > maxLoads.Length)
                {
                    maxLoads = loads;
                    maxLoad = Math.Abs(maxLoads.MaximumCoordinate);
                }
            }

            foreach (string s in cornerPointsString)
            {
                cornerPointLoads.Add(s + ";" + cornerVector);
            }
            foreach (string s in edgePointsString)
            {
                edgePointLoads.Add(s + ";" + edgeVector);
            }

            pointLoads.AddRange(centerPointLoads);
            pointLoads.AddRange(edgePointLoads);
            pointLoads.AddRange(cornerPointLoads);

            return (pointLoads, maxLoad);
        }

        public List<Line> DrawLoads(List<string> pointLoads, double refLength, double maxLoad)
        {
            List<Line> arrows = new List<Line>();

            List<double> loadCoord = new List<double>();
            List<double> pointValues = new List<double>();

            double arrowRef = refLength / 50;
            double maxLength = refLength * 2;

            foreach (string s in pointLoads)
            {
                string coordinate = (s.Split(';'))[0];
                string iLoad = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iLoads = (iLoad.Split(','));

                double loadCoord1 = Math.Round(double.Parse(coord[0]), 8);
                double loadCoord2 = Math.Round(double.Parse(coord[1]), 8);
                double loadCoord3 = Math.Round(double.Parse(coord[2]), 8);

                double loadX = Math.Round(double.Parse(iLoads[0]), 8);
                double loadY = Math.Round(double.Parse(iLoads[1]), 8);
                double loadZ = Math.Round(double.Parse(iLoads[2]), 8);

                Point3d startPoint = new Point3d(loadCoord1, loadCoord2, loadCoord3);
                Point3d arrowPart1 = new Point3d(0, 0, 0);
                Point3d arrowPart2 = new Point3d(0, 0, 0);
                Point3d endPoint = new Point3d(0, 0, 0);

                if (loadX != 0)
                {
                    endPoint = new Point3d(loadCoord1 - loadX * maxLength/maxLoad, loadCoord2, loadCoord3);

                    if (loadX > 0)
                    {
                        arrowPart1 = new Point3d(loadCoord1 - arrowRef, loadCoord2 - arrowRef, loadCoord3);
                        arrowPart2 = new Point3d(loadCoord1 - arrowRef, loadCoord2 + arrowRef, loadCoord3);
                    }
                    else
                    {
                        arrowPart1 = new Point3d(loadCoord1 + arrowRef, loadCoord2 + arrowRef, loadCoord3);
                        arrowPart2 = new Point3d(loadCoord1 + arrowRef, loadCoord2 - arrowRef, loadCoord3);
                    }
                    arrows.Add(new Line(startPoint, endPoint));
                    arrows.Add(new Line(startPoint, arrowPart1));
                    arrows.Add(new Line(startPoint, arrowPart2));
                }

                if (loadY != 0)
                {
                    endPoint = new Point3d(loadCoord1, loadCoord2 - loadY *maxLength / maxLoad, loadCoord3);


                    if (loadY > 0)
                    {
                        arrowPart1 = new Point3d(loadCoord1 - arrowRef, loadCoord2 - arrowRef, loadCoord3);
                        arrowPart2 = new Point3d(loadCoord1 + arrowRef, loadCoord2 - arrowRef, loadCoord3);
                    }
                    else
                    {
                        arrowPart1 = new Point3d(loadCoord1 - arrowRef, loadCoord2 + arrowRef, loadCoord3);
                        arrowPart2 = new Point3d(loadCoord1 + arrowRef, loadCoord2 + arrowRef, loadCoord3);
                    }

                    arrows.Add(new Line(startPoint, endPoint));
                    arrows.Add(new Line(startPoint, arrowPart1));
                    arrows.Add(new Line(startPoint, arrowPart2));
                }

                if (loadZ != 0)
                {
                    endPoint = new Point3d(loadCoord1, loadCoord2, loadCoord3 - loadZ * maxLength / maxLoad);


                    if (loadZ > 0)
                    {
                        arrowPart1 = new Point3d(loadCoord1 + arrowRef, loadCoord2, loadCoord3 - arrowRef);
                        arrowPart2 = new Point3d(loadCoord1 - arrowRef, loadCoord2, loadCoord3 - arrowRef);
                    }
                    else
                    {
                        arrowPart1 = new Point3d(loadCoord1 + arrowRef, loadCoord2, loadCoord3 + arrowRef);
                        arrowPart2 = new Point3d(loadCoord1 - arrowRef, loadCoord2, loadCoord3 + arrowRef);
                    }

                    arrows.Add(new Line(startPoint, endPoint));
                    arrows.Add(new Line(startPoint, arrowPart1));
                    arrows.Add(new Line(startPoint, arrowPart2));
                }
            }
            return arrows;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.loads;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("9fa57f7d-4d01-4004-aa03-214e25f02b6a"); }
        }
    }
}