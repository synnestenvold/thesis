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
            pManager.AddCurveParameter("V value", "V", "V number of panels", GH_ParamAccess.list);
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

                if (!DA.GetData(0, ref brep)) { return; }
                if (!DA.GetData(1, ref uCount)) { return; }
                if (!DA.GetData(2, ref vCount)) { return; }
                if (!DA.GetData(3, ref wCount)) { return; }
                if (!DA.GetDataList(4, curves)) return;

                Curve[] edges = brep.DuplicateEdgeCurves();

                Curve edge1 = edges[0];
                Curve edge2 = edges[1];

                BrepFace face = brep.Faces[1];
                Surface surF = face.DuplicateSurface();

                Interval dW = surF.Domain(0);

                List<Curve> curveEdges = new List<Curve>();

                Point3d p1 = new Point3d(0, 0, 0);
                Point3d p2 = new Point3d(0, 1, 0);


                for (int w= 0; w <= wCount; w++)
                {
                    double tw = dW.ParameterAt(w / (double)wCount);
                    Point3d p_1 = p1 + new Vector3d(tw*w, 0, 0);
                    Point3d p_2 = p2 + new Vector3d(tw * w, 0, 0);

                    List<Point3d> ps = new List<Point3d> {p_1, p_2};

                    Curve c = surF.InterpolatedCurveOnSurface(ps,100);

                    curveEdges.Add(c);
                    
                   
                }

                Brep brp = Brep.CreateEdgeSurface(curves);

                Surface surface = null;

                foreach (BrepFace surf in brp.Faces)
                {
                     surface = surf.DuplicateSurface();
                }


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

                //Surface surf = new Surface();

                // Populate outputs.
                DA.SetDataList(0, points);
                DA.SetDataList(1, vectors);
                DA.SetData(2, surface);
                DA.SetDataList(3, curveEdges);
            }
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