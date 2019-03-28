using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Material
{
    class Material
    {
        private double E = 0;
        private double nu = 0;


        public Material(double _E, double _nu)
        {
            E = _E;
            nu = _nu;
        }

        public double GetYoungs()
        {
            return E;
        }

        public double GetPoisson()
        {
            return nu;
        }
    }

}
