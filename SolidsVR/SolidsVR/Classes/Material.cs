using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidsVR
{
    public class Material
    {
        private double E;
        private double nu;

        public Material(double _E, double _nu)
        {
            E = _E;
            nu = _nu;
        }
        public Material() { }

        public double GetE()
        {
            return E;
        }

        public double GetNu()
        {
            return nu;
        }

        public void SetE(double _E)
        {
            E = _E;
        }

        public void SetNu(double _nu)
        {
            nu = _nu;
        }
    }

    
}
