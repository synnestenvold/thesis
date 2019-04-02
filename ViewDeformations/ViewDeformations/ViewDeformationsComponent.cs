using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Display;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Drawing;



namespace ViewDeformations
{
    public class ViewDeformationsComponent : GH_Component
    {

        public ViewDeformationsComponent()
          : base("ViewDeformations", "ViewDef",
              "Description",
              "Category3", "Preview")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Connectivity", "C", "", GH_ParamAccess.tree);
            pManager.AddPointParameter("Points for Breps", "N", "Breps in coordinates", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scaling", "Scale", "Scale factor for the view", GH_ParamAccess.item, 1);
            pManager.AddBrepParameter("Brep", "B", "Original brep for preview", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddGeometryParameter("Brep solid", "Solid", "3D Model of deformation with sphere", GH_ParamAccess.list);
            pManager.AddColourParameter("Color solid", "Color solid", "Colors for deformation", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Brep outer edges", "Edges", "Curves showing the original shape of solid", GH_ParamAccess.list);
            pManager.AddColourParameter("Color edges", "Color edges", "Colors for showing original shape", GH_ParamAccess.list);

            pManager.AddTextParameter("Text", "Text", "Text for max deformation", GH_ParamAccess.list);
            pManager.AddNumberParameter("Size", "Size", "Text size", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Plane", "Placement for text", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "Color T", "Colors for text", GH_ParamAccess.list);



        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Integer> treeConnect = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Point> treePoints = new GH_Structure<GH_Point>();
            GH_Structure<GH_Number> treeDef = new GH_Structure<GH_Number>();
            double scale = 1;
            Brep brep = new Brep();

            if (!DA.GetDataTree(0, out treeConnect)) return;
            if (!DA.GetDataTree(1, out treePoints)) return;
            if (!DA.GetDataTree(2, out treeDef)) return;
            if (!DA.GetData(3, ref scale)) return;
            if (!DA.GetData(4, ref brep)) return;


            List<Brep> breps = new List<Brep>();
            Dictionary<Brep, Color> tmpModels = new Dictionary<Brep, Color>();

            // Setting up values for refLength and angle for rotation of area
            VolumeMassProperties vmp = VolumeMassProperties.Compute(brep);
            Point3d centroid = vmp.Centroid;
            double volume = brep.GetVolume();
            double sqrt3 = (double)1 / 3;
            double refLength = Math.Pow(brep.GetVolume(), sqrt3);
            double refSize = (double)(refLength / 10);
            double angle = 270*Math.PI/180;
            Point3d center = Point3d.Add(centroid, new Point3d(0, -refLength * 2.5, 0)); //Center for viewpoint

            //Creating deformation vectors
            Vector3d[] defVectors = CreateVectors(treeDef);
            breps = CreateDefBreps(treePoints, treeConnect, defVectors, scale, angle, center);

            //Finding point with max deformation
            var tuple = GetMaxDeformation(defVectors, treePoints, treeConnect);
            double defMax = tuple.Item1; 
            Point3d pointMax = tuple.Item2;
            int nodeGlobalMax = tuple.Item3;

            Brep sphere = DrawSphere(pointMax, nodeGlobalMax, defVectors, angle, center, scale, refSize); //output geo
            Color colorSphere = Color.Red;

            //Creating text for displaying it for max value
            var tuple2 = CreateText(defMax, pointMax, refSize, angle, center);
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
            List<Color> brepColors = AssignColors(breps); //output geo

            breps.Add(sphere);
            List<Brep> geoBreps = breps;

            brepColors.Add(colorSphere);
            List<Color> geoColor = brepColors;

            Curve[] lines = DrawOuterCurves(brep, angle, center); //output geo
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

            //Geometry
            DA.SetDataList(0, geoBreps);
            DA.SetDataList(1, geoColor);
            DA.SetDataList(2, lines);
            DA.SetData(3, linesColor);

            //Text
            DA.SetDataList(4, text);
            DA.SetDataList(5, textSizes);
            DA.SetDataList(6, textPlanes);
            DA.SetDataList(7, textColors);

        }

        public Tuple<string, double, Plane, Color> CreateHeadline(Point3d centroid, double angle, Point3d center, double refLength)
        {
            string headText = "Deformation";

            double headSize = (double)refLength / 2;

            Point3d p0 = centroid;
            Point3d p1 = Point3d.Add(p0, new Point3d(1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(0, 0, 1));

            Plane headPlane = new Plane(p0, p1, p2);

            headPlane.Rotate(angle, new Vector3d(0, 0, 1), center);
            headPlane.Translate(new Vector3d(0, 0, refLength));

            Color headColor = Color.Pink;

            return Tuple.Create(headText, headSize, headPlane, headColor);
        }

        public List<Color> AssignColors(List<Brep> breps)
        {
            List<Color> colorBreps = new List<Color>();

            //Coloring
            Color color = Color.White;
            for (int i = 0; i < breps.Count; i++)
            {
                //if (defVectors[i].Length < limit) color = Color.Green;
                //else color = Color.Red;
                colorBreps.Add(color);
            }

            return colorBreps;
        }

        public Brep DrawSphere(Point3d pointMax, int nodeGlobalMax, Vector3d[] defVectors, double angle, Point3d center, double scale, double refSize)
        {
            Point3d newPoint = pointMax + defVectors[nodeGlobalMax]*scale;

            Point3d p0 = newPoint;
            Point3d p1 = Point3d.Add(p0, new Point3d(1, 0, 0));
            Point3d p2 = Point3d.Add(p0, new Point3d(1, 0, 1));

            Plane plane = new Plane(p0, p1, p2);

            plane.Rotate(angle, new Vector3d(0, 0, 1), center);

            Sphere sphere = new Sphere(plane, refSize);

            Brep brepSphere = sphere.ToBrep();

            return brepSphere;
        }

        public Curve[] DrawOuterCurves(Brep brep, double angle, Point3d center)
        {
            brep.Rotate(angle, new Vector3d(0, 0, 1), center);
            Curve[] curves = brep.DuplicateEdgeCurves();

            return curves;
        }

        public Vector3d[] CreateVectors(GH_Structure<GH_Number> treeDef)
        {
            int number = treeDef.PathCount;
            Vector3d[] vectors = new Vector3d[number];
            for (int i = 0; i< number; i++)
            {
                List<GH_Number> def = (List<GH_Number>)treeDef.get_Branch(i);
                Vector3d vector = new Vector3d((def[0].Value), (def[1].Value), (def[2].Value));
                vectors[i] = vector;
            }
            return vectors;
        }

        public Tuple<double, Point3d, int> GetMaxDeformation(Vector3d[] defVectors, GH_Structure<GH_Point> treePoints, GH_Structure<GH_Integer> treeConnect)
        {
            double defMax = -1;
            int nodeGlobalMax = new int();
            int nodeMax = new int();
            int elemMax = new int();
            Point3d pointMax = new Point3d();
            for (int i = 0; i < defVectors.Length; i++)
            {
                double def = defVectors[i].Length;
                if (def > defMax)
                {
                    defMax = Math.Round(def, 6);
                    nodeGlobalMax = i;
                }
            }
            for (int j = 0; j < treeConnect.PathCount; j++)
            {
                List<GH_Integer> connect = (List<GH_Integer>)treeConnect.get_Branch(j);
                for (int k = 0; k < connect.Count; k++)
                {
                    if (connect[k].Value == nodeGlobalMax)
                    {
                        nodeMax = k;
                        elemMax = j;
                    }
                }
                List<GH_Point> point = (List<GH_Point>)treePoints.get_Branch(elemMax);
                pointMax = point[nodeMax].Value;
            }


            return Tuple.Create(defMax, pointMax, nodeGlobalMax);
        }

        public List<Brep> CreateDefBreps(GH_Structure<GH_Point> treePoints, GH_Structure<GH_Integer> treeConnect, Vector3d[] defVectors, double scale, double angle, Point3d center)
        {
            List<Brep> breps = new List<Brep>();
            for (int j = 0; j < treePoints.PathCount; j++)
            {
                List<GH_Point> vertices = (List<GH_Point>)treePoints.get_Branch(j);
                var mesh = new Mesh();
                List<GH_Integer> connect = (List<GH_Integer>)treeConnect.get_Branch(j);

                for (int i = 0; i < vertices.Count; i++)
                {
                    GH_Point p = vertices[i];
                    Point3d new_p = Point3d.Add(p.Value, defVectors[connect[i].Value]*scale);
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

        public Tuple<string, double, Plane, Color> CreateText(double defMax, Point3d pointMax, double refSize, double angle, Point3d center)
        {
 
            string text = defMax.ToString();
            double textSize = refSize;

            Point3d p0 = Point3d.Add(pointMax, new Point3d(0, 0, 2*refSize));
            Point3d p1 = Point3d.Add(pointMax, new Point3d(0, 1, 2*refSize));
            Point3d p2 = Point3d.Add(pointMax, new Point3d(0, 0, (1+2*refSize)));

            Plane textplane = new Plane(p0, p1, p2);
            textplane.Rotate(angle, new Vector3d(0,0,1), center);

            Color textColor = Color.Red;
     
            return Tuple.Create(text, textSize, textplane, textColor);
        }

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
            get { return new Guid("0391f003-161b-49e1-931b-71ca89c69aa6"); }
        }

    }
}
