﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;


namespace SolidsVR
{
    public class NodeInfo : GH_Component
    {

        public NodeInfo()
          : base("NodeInfo", "NodeInfo",
              "Get information in node",
              "SolidsVR", "VR Info")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Sphere", "S", "Sphere for finding closest point", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh for Brep", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {;
            pManager.AddTextParameter("Text", "Text", "Text", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Size for text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color", "Colors for text", GH_ParamAccess.list);
            pManager.AddTextParameter("Text Sphere", "Text", "Text for sphere", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "Size", "Size for text", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane Sphere", "Plane", "Placement for text", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---

            Brep sphere = new Brep();
            MeshGeometry mesh = new MeshGeometry();

            //---input---

            if (!DA.GetData(0, ref sphere)) return;
            if (!DA.GetData(1, ref mesh)) return;

            //---setup---

            //Setting up values for reflength and angle for rotation of area
            Brep origBrep = mesh.GetOrigBrep();
            VolumeMassProperties vmp = VolumeMassProperties.Compute(origBrep);
            Point3d centroid = vmp.Centroid;
            double volume = origBrep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(volume, sqrt3);

            //---solve---

            //List of global points with correct numbering
            List<Point3d> globalPoints = mesh.GetGlobalPoints();

            VolumeMassProperties vmpSphere = VolumeMassProperties.Compute(sphere);
            Point3d centroidSphere = vmpSphere.Centroid;

            Point3d point = FindClosestPoint(globalPoints, centroidSphere, refLength);

            String textSphere = "Drag sphere to point";

            Plane textPlaneSphere = FindSpherePlane(centroidSphere, refLength);

            List<Node> nodeList = mesh.GetNodeList();
            Node chosenNode = FindNode(nodeList, point);

            List<string> text = new List<string>();

            if (chosenNode == null)
            {
                text.Add("No node is chosen");
            }
            else
            {
                text = CreateText(chosenNode);
            }

            List<Plane> textPlane = CreateTextPlane(centroid, refLength, text);
            List<Color> colors = new List<Color>();

            colors.Add(Color.Orange);
            colors.Add(Color.Orange);
            colors.Add(Color.White);

            double size = (double)refLength / 7;

            //---output---

            DA.SetDataList(0, text);
            DA.SetData(1, size);
            DA.SetDataList(2, textPlane);
            DA.SetDataList(3, colors);
            DA.SetData(4, textSphere);
            DA.SetData(5, size);
            DA.SetData(6, textPlaneSphere);
        }

        public Point3d FindClosestPoint(List<Point3d> globalPoints, Point3d centroid, double refLength)
        {
            Point3d closestPoint = new Point3d(999.999, 999.999, 999.999);

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
            Point3d p0 = new Point3d(centroid.X, centroid.Y, centroid.Z + refLength / 5);
            Point3d p1 = Point3d.Add(p0, new Point3d(1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane p = new Plane(p0, p1, p2);

            return p;
        }

        public Node FindNode(List<Node> nodeList, Point3d point)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                Node node = nodeList[i];
                Point3d nodeCoord = node.GetCoord();
                if (Math.Round(nodeCoord.X, 4) == Math.Round(point.X, 4) && Math.Round(nodeCoord.Y, 4) == Math.Round(point.Y, 4) && Math.Round(nodeCoord.Z, 4) == Math.Round(point.Z, 4))
                {
                    return node;
                }
            }
            return null;
        }

        public List<Plane> CreateTextPlane(Point3d centroid, double refLength, List<string> text)
        {
            List<Plane> planes = new List<Plane>();

            for (int i = 0; i < text.Count; i++)
            {
                double zValue = centroid.Z + refLength * 2.5;

                double z = (double)(zValue - i * refLength / 7);

                if (i == 1) z = (double)(z + refLength / 12);

                Point3d p0 = new Point3d(centroid.X - refLength * 3, centroid.Y, z);
                Point3d p1 = Point3d.Add(p0, new Point3d(1, 0, 0));
                Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

                Plane p = new Plane(p0, p1, p2);

                planes.Add(p);
            }

            return planes;
        }

        public List<string> CreateText(Node node)
        {
            List<string> text = new List<string>();

            string header = "NODE INFORMATION:";
            string underScore = "________________________";
            string nodeCoord = "Coord: (" + Math.Round(node.GetCoord().X,2) + ", "+Math.Round(node.GetCoord().Y, 2) +", " + Math.Round(node.GetCoord().Z, 2) +") [mm]";
            string def = "Def: " + Math.Round(node.GetDeformation()[0], 3).ToString() + ", " + Math.Round(node.GetDeformation()[1], 3) + ", " + Math.Round(node.GetDeformation()[2], 3) + " [mm]";

            string stress = "von Mises: " + Math.Round(node.GetGlobalStress()[6],3) + " [MPa]";

            text.Add(header);
            text.Add(underScore);
            text.Add(nodeCoord);
            text.Add(def);
            text.Add(stress);

            return text;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.nodeInfo;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a72dcbc5-1b53-4f7f-8706-e25e383256f3"); }
        }
    }
}