using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidsVR
{
    public class MeshSurfBrep : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshSurfBrep class.
        /// </summary>
        public MeshSurfBrep()
          : base("Meshing brep", "Mesh crazy geometry",
              "Description",
              "Category3", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Glazing Surface", "GS", "The surface to create Louvers on", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U value", "U", "U number of panels", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("V value", "V", "V number of panels", GH_ParamAccess.item, 10);
            pManager.AddIntegerParameter("W value", "W", "W number of panels", GH_ParamAccess.item, 10);
            pManager.AddCurveParameter("Curve value", "C", "V number of panels", GH_ParamAccess.list);
            pManager.AddPointParameter("Curve value", "C", "V number of panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Pt", "Points", "Surface points", GH_ParamAccess.list);
            pManager.AddVectorParameter("Nr", "Normals", "Surface normal vectors", GH_ParamAccess.list);
            pManager.AddBrepParameter("Nr", "Normals", "Surface normal vectors", GH_ParamAccess.item);
            pManager.AddCurveParameter("C", "Normals", "Surface normal vectors", GH_ParamAccess.list);
            pManager.AddPointParameter("Pt", "Points", "Surface points", GH_ParamAccess.list);
            pManager.AddPointParameter("Pt", "Points", "Surface points", GH_ParamAccess.list);
            pManager.AddPointParameter("Pt", "Points", "Surface points", GH_ParamAccess.list);
            pManager.AddPointParameter("Pt", "Points", "Surface points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            {
                // Define local variables to catch the incoming data from Grasshopper
                Surface surface3 = null;
                Brep brep = new Brep();
                int uCount = 0;
                int vCount = 0;
                int wCount = 0;
                List<Curve> curves = new List<Curve>();
                List<Point3d> startP = new List<Point3d>();

                if (!DA.GetData(0, ref brep)) { return; }
                if (!DA.GetData(1, ref uCount)) { return; }
                if (!DA.GetData(2, ref vCount)) { return; }
                if (!DA.GetData(3, ref wCount)) { return; }
                if (!DA.GetDataList(4, curves)) return;
                if (!DA.GetDataList(5, startP)) return;

                Curve[] edges = brep.DuplicateEdgeCurves();

          

                Brep brp3 = Brep.CreateEdgeSurface(curves);

                Point3d[] test3 = brp3.DuplicateVertices();

                Curve edge1 = edges[0];
                Curve edge2 = edges[11];
                Curve edge3 = edges[10];
                Curve edge4 = edges[9];

                edge1.Reverse();
                edge2.Reverse();
                edge3.Reverse();
                edge4.Reverse();


                edge1.DivideByCount(wCount, true, out Point3d[] p1s);
                edge2.DivideByCount(wCount, true, out Point3d[] p2s);
                edge3.DivideByCount(wCount, true, out Point3d[] p3s);
                edge4.DivideByCount(wCount, true, out Point3d[] p4s);

                BrepFace face1 = brep.Faces[1];
                Surface surF1 = face1.DuplicateSurface();
                BrepFace face2 = brep.Faces[4];
                Surface surF2 = face2.DuplicateSurface();
                BrepFace face3 = brep.Faces[3];
                Surface surF3 = face3.DuplicateSurface();
                BrepFace face4 = brep.Faces[2];
                Surface surF4 = face4.DuplicateSurface();


                List<NurbsCurve> curveEdges = new List<NurbsCurve>();

                //NurbsCurve c = surF.InterpolatedCurveOnSurface(startP, 0);

                //curveEdges.Add(c);

                List<NurbsCurve> curve = new List<NurbsCurve>();

                List<Point3d> meshPoints = new List<Point3d>();
   

                Interval dW = surF1.Domain(0);

                for (int w= 0; w <= wCount; w++)
                {
                    double tw = dW.ParameterAt(w / (double)wCount);

                    Point3d p_1 = p1s[w];
                    Point3d p_2 = p2s[w];
                    Point3d p_3 = p3s[w];
                    Point3d p_4 = p4s[w];

                    List<Point3d> ps1 = new List<Point3d> {p_1, p_2};
                    List<Point3d> ps2 = new List<Point3d> {p_2, p_3};
                    List<Point3d> ps3 = new List<Point3d> {p_3, p_4};
                    List<Point3d> ps4 = new List<Point3d> {p_4, p_1};

                    NurbsCurve c1 = surF1.InterpolatedCurveOnSurface(ps1, 0);
                    NurbsCurve c2 = surF2.InterpolatedCurveOnSurface(ps2, 0);
                    NurbsCurve c3 = surF3.InterpolatedCurveOnSurface(ps3, 0);
                    NurbsCurve c4 = surF4.InterpolatedCurveOnSurface(ps4, 0);

                    curveEdges.Add(c1);
                    curveEdges.Add(c2);
                    curveEdges.Add(c3);
                    curveEdges.Add(c4);

                    curve = new List<NurbsCurve>() { c1, c2, c3, c4 };
                    Brep brp = Brep.CreateEdgeSurface(curve);

                    Brep brp2 = Brep.CreateEdgeSurface(curveEdges);

                    Point3d[] test2 = brp2.DuplicateVertices();

                    Point3d[] test = brp.DuplicateVertices();

                    Surface surface = null;

                    foreach (BrepFace surf in brp.Faces)
                    {
                        surface = surf.DuplicateSurface();
                    }

                    meshPoints.AddRange(CreatePoints(surface, uCount, vCount));

                }
              
                
                

                //List<Point3d> points = CreatePoints(surface, uCount, vCount);

                

                //Surface surf = new Surface();

                // Populate outputs.
                DA.SetDataList(0, meshPoints);
                //DA.SetDataList(1, vectors);
                //DA.SetData(2, surface);
                DA.SetDataList(3, curveEdges);
                DA.SetDataList(4, p1s);
                DA.SetDataList(5, p2s);
                DA.SetDataList(6, p3s);
                DA.SetDataList(7, p4s);
            }
        }

        public List<Point3d> CreatePoints(Surface surface, int uCount, int vCount)
        {
            Interval domainU = surface.Domain(0);
            Interval domainV = surface.Domain(1);

            List<Point3d> points = new List<Point3d>();
            List<Vector3d> vectors = new List<Vector3d>();


            for (int u = 0; u <= vCount; u++)
            {
                double tu = domainU.ParameterAt(u / (double)vCount);
                for (int v = 0; v <= uCount; v++)
                {
                    double tv = domainV.ParameterAt(v / (double)uCount);
                    points.Add(surface.PointAt(tu, tv));
                    vectors.Add(surface.NormalAt(tu, tv));
                }
            }

            return points;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1fd92232-70a8-4fc2-b2c0-144195bc7e29"); }
        }
    }
}