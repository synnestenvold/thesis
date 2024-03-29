﻿using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Drawing;

namespace SolidsVR
{
    public class ViewDeformation : GH_Component
    {

        public ViewDeformation()
          : base("ViewDeformation", "ViewDef",
              "Display deformation in VR",
              "SolidsVR", "VR Preview")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh for Brep", GH_ParamAccess.item);
            pManager.AddNumberParameter("Displacement", "D", "Displacement in each dof", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scale", "Scale", "Scale factor for the view", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddGeometryParameter("Geometry solid", "Geometry solid", "3D Model of deformation with sphere", GH_ParamAccess.list);
            pManager.AddColourParameter("Color solid", "Color solid", "Colors for deformation", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Geometry edges", "Geometry edges", "Curves showing the original shape of solid", GH_ParamAccess.list);
            pManager.AddColourParameter("Color edges", "Color edges", "Colors for showing original shape", GH_ParamAccess.list);
            pManager.AddTextParameter("Text", "Text", "Text for max deformation", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Text size", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Color T", "Color T", "Colors for text", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //---variables---

            MeshGeometry mesh = new MeshGeometry();
            GH_Structure<GH_Number> treeDef = new GH_Structure<GH_Number>();
            double scale = 1;

            //---input---

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataTree(1, out treeDef)) return;
            if (!DA.GetData(2, ref scale)) return;

            //---setup---

            List<Brep> breps = new List<Brep>();
            BrepGeometry brp = mesh.GetBrep();
            Brep brep = brp.GetBrep();
            Point3d centroid = brp.GetCentroid();
            double refLength = brp.GetRefLength();
            double refSize = (double)(refLength / 10);
            double angle = 270*Math.PI/180;
            Point3d center = Point3d.Add(centroid, new Point3d(0, -refLength * 6.5, 0)); //Center for viewpoint

            //---solve---

            List<Element> elements = mesh.GetElements();
            List<Node> nodes = mesh.GetNodeList();

            breps = CreateDefBreps(elements, scale, angle, center);

            //Finding point with max deformation
            (double defMax, Node nodeMax) = GetMaxDeformation(nodes);

            Brep sphere = DrawSphere(nodeMax, angle, center, scale, refSize);
            Color colorSphere = Color.Orange;

            VolumeMassProperties vmpt = VolumeMassProperties.Compute(sphere);
            Point3d centroidt = vmpt.Centroid;

            //Creating text for displaying it for max value
            var tuple2 = CreateText(defMax, nodeMax, scale, refSize, angle, center);
            string textDef = tuple2.Item1;
            double textDefSize = tuple2.Item2;
            Plane textDefPlane = tuple2.Item3;
            Color textDefColor = tuple2.Item4;

            //Createing headline for area
            var tuple3 = CreateHeadline(centroid, angle, center, refLength);

            string headText = tuple3.Item1;
            double headSize = tuple3.Item2;
            Plane headPlane = tuple3.Item3;
            Color headColor = tuple3.Item4;

            //Adding geometry together for output
            List<Color> brepColors = AssignColors(breps);

            breps.Add(sphere);
            List<Brep> geoBreps = breps;

            brepColors.Add(colorSphere);
            List<Color> geoColor = brepColors;

            Curve[] lines = DrawOuterCurves(brep, angle, center);
            Color linesColor = Color.White;

            //Adding the different text components together to one output.

            List<string> text = new List<String>
            {
                textDef,
                headText,
            };

            List<double> textSizes = new List<double>
            {
                textDefSize,
                headSize,
            };

            List<Plane> textPlanes = new List<Plane>
            {
                textDefPlane,
                headPlane,
            };

            List<Color> textColors = new List<Color>
            {
                textDefColor,
                headColor,
            };

            //---output---

            //Geometry
            DA.SetDataList(0, geoBreps);
            DA.SetDataList(1, geoColor);
            DA.SetDataList(2, lines);
            DA.SetData(3, linesColor);
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

        public (double, Node) GetMaxDeformation(List<Node> nodes)
        {
            double defMax = -1;
            Node maxDefNode = new Node(new Point3d(0, 0, 0), 0);

            for (int i = 0; i < nodes.Count; i++)
            {
                Vector3d defVector = new Vector3d(nodes[i].GetDeformation()[0], nodes[i].GetDeformation()[1], nodes[i].GetDeformation()[2]);

                double def = defVector.Length;
                if (def > defMax)
                {
                    defMax = Math.Round(def, 6);
                    maxDefNode = nodes[i];
                }
            }

            return (defMax, maxDefNode);
        }

        public Brep DrawSphere(Node nodeMax, double angle, Point3d center, double scale, double refSize)
        {
            Vector3d def = new Vector3d(nodeMax.GetDeformation()[0], nodeMax.GetDeformation()[1], nodeMax.GetDeformation()[2]);

            Point3d newPoint = nodeMax.GetCoord() + def * scale;

            Point3d p0 = newPoint;
            Point3d p1 = Point3d.Add(p0, new Point3d(-1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane plane = new Plane(p0, p1, p2);

            plane.Rotate(angle, new Vector3d(0, 0, 1), center);

            Sphere sphere = new Sphere(plane, refSize);

            Brep brepSphere = sphere.ToBrep();

            return brepSphere;
        }

        public (string, double, Plane, Color) CreateText(double defMax, Node nodeMax, double scale, double refSize, double angle, Point3d center)
        {
            Vector3d def = new Vector3d(nodeMax.GetDeformation()[0], nodeMax.GetDeformation()[1], nodeMax.GetDeformation()[2]);

            Point3d newPoint = nodeMax.GetCoord() + def * scale;
            newPoint = Point3d.Add(newPoint, new Point3d(0, -1.5 * refSize, 0));
            defMax = Math.Round(defMax, 3);

            string text = defMax.ToString() + " mm";
            double textSize = refSize;

            Point3d p0 = newPoint;
            Point3d p1 = Point3d.Add(newPoint, new Point3d(-1, 0, 0));
            Point3d p2 = Point3d.Add(newPoint, new Point3d(0, 0, 1));

            Plane textplane = new Plane(p0, p1, p2);
            textplane.Rotate(angle, new Vector3d(0, 0, 1), center);
            textplane.Translate(new Vector3d(0, 0, 2 * refSize));

            Color textColor = Color.Orange;

            return (text, textSize, textplane, textColor);
        }


        public Tuple<string, double, Plane, Color> CreateHeadline(Point3d centroid, double angle, Point3d center, double refLength)
        {
            string headText = "Deformation";

            double headSize = (double)refLength / 1.5;

            Point3d p0 = centroid;
            Point3d p1 = Point3d.Add(p0, new Point3d(-1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane headPlane = new Plane(p0, p1, p2);

            headPlane.Translate(new Vector3d(0, -headSize, 3.5*refLength));
            headPlane.Rotate(angle, new Vector3d(0, 0, 1), center);
            

            Color headColor = Color.FromArgb(0, 100, 255);

            return Tuple.Create(headText, headSize, headPlane, headColor);
        }

        public List<Color> AssignColors(List<Brep> breps)
        {
            List<Color> colorBreps = new List<Color>();

            //Coloring
            Color color = Color.White;
            for (int i = 0; i < breps.Count; i++)
            {
                colorBreps.Add(color);
            }

            return colorBreps;
        }

        public Curve[] DrawOuterCurves(Brep brep, double angle, Point3d center)
        {

            brep.Rotate(angle, new Vector3d(0, 0, 1), center);

            Curve[] curves = brep.DuplicateEdgeCurves();

            brep.Rotate(-angle, new Vector3d(0, 0, 1), center);

            return curves;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return SolidsVR.Properties.Resource1.disp;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0391f003-161b-49e1-931b-71ca89c69aa6"); }
        }
    }
}