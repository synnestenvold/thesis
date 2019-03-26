using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;


namespace FEMeshTBrep
{
    public class FEMeshTBrepComponent : GH_Component
    {

        double E = 210000;
        double nu = 0.3;

        public FEMeshTBrepComponent()
          : base("Finite Element of meshed TBrep", "FEMeshTBrep",
              "Description",
              "Category3", "Subcategory3")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Connectivity", "C", "Relationship between local and global numbering", GH_ParamAccess.tree);
            pManager.AddPointParameter("Nodes", "N", "Coordinates for corner nodes in brep", GH_ParamAccess.tree);
            pManager.AddTextParameter("Boundary conditions", "BC", "Nodes that are constrained", GH_ParamAccess.list);
            pManager.AddTextParameter("PointLoads", "PL", "Input loads", GH_ParamAccess.list);
            pManager.AddTextParameter("PreDeformations", "PD", "Input deformations", GH_ParamAccess.list);
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacement", "Disp", "Displacement in each dof", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Strain", "Strain", "Strain vector", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stress", "Stress", "Stress vector", GH_ParamAccess.tree);
            pManager.AddPointParameter("Nodes", "N", "Coordinates for corner nodes in brep", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {


            GH_Structure<GH_Integer> treeConnectivity = new GH_Structure<GH_Integer>();
            GH_Structure<GH_Point> treePoints = new GH_Structure<GH_Point>();
            List<string> bctxt = new List<string>();
            List<string> loadtxt = new List<string>();
            List<string> deftxt = new List<string>();

            if (!DA.GetDataTree(0, out treeConnectivity)) return;
            if (!DA.GetDataTree(1, out treePoints)) return;
            if (!DA.GetDataList(2, bctxt)) return;
            if (!DA.GetDataList(3, loadtxt)) return;
            if (!DA.GetDataList(4, deftxt)) return;


            // Temporary way of finding the size of stiffness matrix and B matrix
            int sizeOfM = FindSizeOfM(treeConnectivity);

            //List of global points with correct numbering
            Point3d[] globalPoints = CreatePointList(treeConnectivity, treePoints, sizeOfM);

            //Create K_tot
            var tuple = CreateGlobalStiffnessMatrix(treeConnectivity, treePoints, sizeOfM);
            Matrix<double> K_tot = tuple.Item1;

            Matrix<double> K_inv = K_tot.Inverse();

            //B_all
            List<List<Matrix<double>>> B_all = tuple.Item2;

            //Create boundary condition list AND predeformations
            var tupleBC = CreateBCList(bctxt, globalPoints);
            List<int> bcNodes = tupleBC.Item1;

            var tupleDef = CreateBCList(deftxt, globalPoints);
            List<int> predefNodes = tupleDef.Item1;

            List<double> predef = tupleDef.Item2;

            //Apply boundary condition and predeformations
            K_tot = ApplyBC(K_tot, bcNodes);
            K_tot = ApplyBC(K_tot, predefNodes);

            //Needs to take the predefs into account
            Vector<double> R_def = Vector<double>.Build.Dense(sizeOfM);

            if (deftxt.Any())
            {
                R_def = ApplyPreDef(K_tot, predefNodes, predef, sizeOfM);
            }

            //Inverting K matrix
            //Matrix<double> K_tot_inverse = K_tot.Inverse();

            //double[] R_array = SetLoads(sizeOfM, loadtxt);
            double[] R_array = AssignLoadsAndBC(loadtxt, bcNodes, globalPoints);
            var V = Vector<double>.Build;
            var R = (V.DenseOfArray(R_array)).Subtract(R_def);

            double[] R_array_def = new double[sizeOfM];
            for (int j = 0; j < sizeOfM; j++)
            {
                R_array_def[j] = R[j];
            }

            //Caluculation of the displacement vector u
            //Vector<double> u = K_tot_inverse.Multiply(R);

            //Trying with cholesky
            Deformations def = new Deformations(K_tot, R_array_def);
            List<double> u = def.Cholesky_Banachiewicz();

            DataTree<double> defTree = new DataTree<double>();
            int n = 0;
            for (int i = 0; i < u.Count; i += 3)
            {
                List<double> u_node = new List<double>(3);
                u_node.Add(u[i]);
                u_node.Add(u[i + 1]);
                u_node.Add(u[i + 2]);

                defTree.AddRange(u_node, new GH_Path(new int[] { 0, n }));
                n++;
            }

            //Calculatin strains for each node and stresses based on strain. 
            List<Matrix<double>> B_e = new List<Matrix<double>>();
            List<GH_Integer> c_e = new List<GH_Integer>();
            DataTree<double> strain_node = new DataTree<double>();

            Cmatrix C = new Cmatrix(E, nu);
            Matrix<double> C_matrix = C.CreateMatrix();

            List<List<Vector<double>>> strain = new List<List<Vector<double>>>();

            for (int i = 0; i < B_all.Count; i++)
            {
                B_e = B_all[i];
                c_e = (List<GH_Integer>)treeConnectivity.get_Branch(i);
                List<Vector<double>> calcedStrain = CalcStrain(c_e, u, B_e);

                strain.Add(calcedStrain);


            }

            DataTree<double> strainTree = new DataTree<double>();
            DataTree<double> stressTree = new DataTree<double>();

            List<List<double>> globalStrain = FindGlobalStrain(strain, treeConnectivity, sizeOfM);
            List<Vector<double>> globalStress = CalcStress(globalStrain, C_matrix);

            for (int i = 0; i < globalStrain.Count; i++)
            {
                strainTree.AddRange(globalStrain[i], new GH_Path(new int[] { 0, i }));
                stressTree.AddRange(globalStress[i], new GH_Path(new int[] { 0, i }));
            }

            DA.SetDataTree(0, defTree);
            DA.SetDataTree(1, strainTree);
            DA.SetDataTree(2, stressTree);
            DA.SetDataList(3, globalPoints);


            /*
            GH_Boolean => Boolean
            GH_Integer => int
            GH_Number => double
            GH_Vector => Vector3d
            GH_Matrix => Matrix
            GH_Surface => Brep
            */

        }

        public List<Vector<double>> CalcStress(List<List<double>> globalStrain, Matrix<double> Cmatrix)
        {
            List<Vector<double>> globalStress = new List<Vector<double>>();


            for (int i = 0; i < globalStrain.Count; i++)
            {
                Vector<double> strainVec = Vector<double>.Build.Dense(globalStrain[i].ToArray());
                globalStress.Add(Cmatrix.Multiply(strainVec));
            }

            return globalStress;
        }

        public List<List<double>> FindGlobalStrain(List<List<Vector<double>>> strain, GH_Structure<GH_Integer> treeConnectivity, int sizeOfM)
        {
            List<List<double>> globalStrain = new List<List<double>>();


            for (int i = 0; i < sizeOfM / 3; i++)
            {
                List<double> nodeStrain = Enumerable.Repeat(0d, 6).ToList();
                globalStrain.Add(nodeStrain);
            }

            for (int i = 0; i < treeConnectivity.PathCount; i++) //For each element
            {
                List<GH_Integer> cNodes = (List<GH_Integer>)treeConnectivity.get_Branch(i);
                for (int j = 0; j < cNodes.Count; j++)
                {
                    List<double> TList = globalStrain[cNodes[j].Value];
                    for (int k = 0; k < 6; k++)
                    {


                        if (globalStrain[cNodes[j].Value][k] == 0)
                        {
                            globalStrain[cNodes[j].Value][k] = strain[i][j][k];
                        }
                        else
                        {
                            globalStrain[cNodes[j].Value][k] = (globalStrain[cNodes[j].Value][k] + strain[i][j][k]) / 2;
                        }

                        //globalStrain[cNodes[j].Value][k] = strain[i][j][k];

                    }
                }
            }

            return globalStrain;
        }

        public Tuple<List<int>, List<double>> CreateBCList(List<string> bctxt, Point3d[] points)
        {
            List<int> BC = new List<int>();
            List<double> BCPoints = new List<double>();
            List<double> restrains = new List<double>();

            foreach (string s in bctxt)
            {
                string coordinate = (s.Split(';'))[0];
                string iBC = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iBCs = (iBC.Split(','));


                BCPoints.Add(Math.Round(double.Parse(coord[0]), 8));
                BCPoints.Add(Math.Round(double.Parse(coord[1]), 8));
                BCPoints.Add(Math.Round(double.Parse(coord[2]), 8));

                restrains.Add(Math.Round(double.Parse(iBCs[0]), 8));
                restrains.Add(Math.Round(double.Parse(iBCs[1]), 8));
                restrains.Add(Math.Round(double.Parse(iBCs[2]), 8));
            }

            int index = 0;


            foreach (Point3d p in points)
            {

                for (int j = 0; j < BCPoints.Count / 3; j++)
                {
                    if (BCPoints[3 * j] == Math.Round(p.X, 8) && BCPoints[3 * j + 1] == Math.Round(p.Y, 8) && BCPoints[3 * j + 2] == Math.Round(p.Z, 8))
                    {
                        BC.Add(index);
                        BC.Add(index + 1);
                        BC.Add(index + 2);

                    }
                }
                index += 3;
            }

            return Tuple.Create(BC, restrains);
        }

        public Matrix<double> ApplyBC(Matrix<double> K, List<int> bcNodes)
        {
            for (int i = 0; i < bcNodes.Count; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    if (bcNodes[i] != j)
                    {
                        K[bcNodes[i], j] = 0;
                    }

                }

                for (int j = 0; j < K.RowCount; j++)
                {
                    if (bcNodes[i] != j)
                    {
                        K[j, bcNodes[i]] = 0;
                    }

                }
            }

            return K;
        }

        public double[] AssignLoadsAndBC(List<string> pointLoads, List<int> bcNodes, Point3d[] points)
        {
            List<double> loadCoord = new List<double>();
            List<double> pointValues = new List<double>();

            double[] loads = new double[points.Length * 3];

            foreach (string s in pointLoads)
            {
                string coordinate = (s.Split(';'))[0];
                string iLoad = (s.Split(';'))[1];

                string[] coord = (coordinate.Split(','));
                string[] iLoads = (iLoad.Split(','));

                loadCoord.Add(Math.Round(double.Parse(coord[0]), 8));
                loadCoord.Add(Math.Round(double.Parse(coord[1]), 8));
                loadCoord.Add(Math.Round(double.Parse(coord[2]), 8));

                pointValues.Add(Math.Round(double.Parse(iLoads[0]), 8));
                pointValues.Add(Math.Round(double.Parse(iLoads[1]), 8));
                pointValues.Add(Math.Round(double.Parse(iLoads[2]), 8));
            }

            int index = 0;

            foreach (Point3d p in points)
            {

                for (int j = 0; j < loadCoord.Count / 3; j++)
                {
                    if (loadCoord[3 * j] == Math.Round(p.X, 8) && loadCoord[3 * j + 1] == Math.Round(p.Y, 8) && loadCoord[3 * j + 2] == Math.Round(p.Z, 8))
                    {
                        loads[index] = pointValues[3 * j];
                        loads[index + 1] = pointValues[3 * j + 1];
                        loads[index + 2] = pointValues[3 * j + 2];
                    }
                }
                index += 3;
            }
            //Corresponding value in R is set to 0 if it is a BC here. Ref page 309 in FEM book.
            foreach (int bc in bcNodes)
            {
                loads[bc] = 0;
            }

            return loads;
        }

        public Vector<double> ApplyPreDef(Matrix<double> K_tot, List<int> predefNodes, List<double> predef, int sizeOfM)
        {
            //Pick the parts of K that are prescribed a deformation
            Matrix<double> K_red = Matrix<double>.Build.Dense(sizeOfM, predefNodes.Count);
            int n = 0;
            foreach (int dof in predefNodes)
            {
                for (int j = 0; j < sizeOfM; j++)
                {
                    K_red[j, n] = K_tot[j, dof];
                }

                n++;
            }

            //Create a vector of the deformations
            Vector<double> d = Vector<double>.Build.Dense(predefNodes.Count);
            for (int i = 0; i < predefNodes.Count; i++)
            {
                d[i] = predef[i];
            }

            //Multiply this with K_red
            Vector<double> R_def = K_red.Multiply(d);

            return R_def;
        }

        public Point3d[] CreatePointList(GH_Structure<GH_Integer> treeConnectivity, GH_Structure<GH_Point> treePoints, int sizeOfM)
        {
            Point3d point = new Point3d(0, 0, 0);

            Point3d[] pointList = new Point3d[sizeOfM / 3];


            for (int i = 0; i < treeConnectivity.PathCount; i++)
            {
                List<GH_Integer> connectedNodes = (List<GH_Integer>)treeConnectivity.get_Branch(i);
                List<GH_Point> connectedPoints = (List<GH_Point>)treePoints.get_Branch(i);

                for (int j = 0; j < connectedNodes.Count; j++)
                {
                    pointList[connectedNodes[j].Value] = connectedPoints[j].Value;
                }
            }

            return pointList;

        }

        private static bool IsSymmetric(Matrix<Double> K)
        {

            for (int i = 0; i < K.RowCount; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (K[i, j] != K[j, i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public int FindSizeOfM(GH_Structure<GH_Integer> treeConnectivity)
        {
            int max = 0;

            for (int i = 0; i < treeConnectivity.PathCount; i++)
            {
                List<GH_Integer> cNodes = (List<GH_Integer>)treeConnectivity.get_Branch(i);

                for (int j = 0; j < cNodes.Count; j++)
                {
                    if (cNodes[j].Value > max)
                    {
                        max = cNodes[j].Value;
                    }
                }
            }

            int sizeOfM = 3 * (max + 1);

            return sizeOfM;
        }

        public Tuple<Matrix<double>, List<List<Matrix<Double>>>> CreateGlobalStiffnessMatrix(GH_Structure<GH_Integer> treeConnectivity, GH_Structure<GH_Point> treePoints, int sizeOfM)
        {
            Matrix<double> K_i = Matrix<double>.Build.Sparse(sizeOfM, sizeOfM);
            Matrix<double> K_tot = Matrix<double>.Build.Sparse(sizeOfM, sizeOfM);
            List<Matrix<Double>> B_e = new List<Matrix<Double>>();
            List<List<Matrix<double>>> B_all = new List<List<Matrix<double>>>();
            StiffnessMatrix sm = new StiffnessMatrix(E, nu);
            Assembly_StiffnessMatrix aSM = new Assembly_StiffnessMatrix();

            for (int i = 0; i < treeConnectivity.PathCount; i++)
            {
                List<GH_Integer> connectedNodes = (List<GH_Integer>)treeConnectivity.get_Branch(i);
                List<GH_Point> connectedPoints = (List<GH_Point>)treePoints.get_Branch(i);

                var tuple = sm.CreateMatrix(connectedPoints);
                Matrix<double> K_e = tuple.Item1;
                B_e = tuple.Item2;
                B_all.Add(B_e);
                K_tot = aSM.assemblyMatrix(K_tot, K_e, connectedNodes, sizeOfM);
                //K_tot = K_tot + K_i;

            }

            //Check if stiffness matrix is symmetric
            //if (!IsSymmetric(K_tot)) return null; // Some error thing.

            return Tuple.Create(K_tot, B_all);
        }

        public List<Vector<double>> CalcStrain(List<GH_Integer> c_e, List<double> u, List<Matrix<Double>> B_e)
        {
            DataTree<double> treeStrain = new DataTree<double>();

            Cmatrix C_new = new Cmatrix(E, nu);
            Matrix<double> C = C_new.CreateMatrix();

            StrainCalc sC = new StrainCalc();
            List<Vector<double>> strain = new List<Vector<double>>();
            //Vector<double> stress = Vector<double>.Build.Dense(6);

            //For calculating the strains and stress
            strain = sC.calcStrain(B_e, u, c_e);

            for (int i = 0; i < strain.Count; i++)
            {
                treeStrain.AddRange(strain[i], new GH_Path(new int[] { 0, i }));
            }

            //stress = C.Multiply(strain);
            //treeStress.AddRange(stress, new GH_Path(new int[] { 0, i }));

            return strain;
        }

        public List<Vector<double>> CalcStress(List<Vector<double>> calcedStrain, Matrix<double> C_matrix)
        {

            DataTree<double> treeStress = new DataTree<double>();
            List<Vector<double>> calcedStress = new List<Vector<double>>();
            for (int i = 0; i < calcedStrain.Count; i++)
            {
                calcedStress.Add(C_matrix.Multiply(calcedStrain[i]));

            }

            return calcedStress;
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
            get { return new Guid("cec65985-58a5-4ff1-a238-e4761e0abbeb"); }
        }

    }
}

    