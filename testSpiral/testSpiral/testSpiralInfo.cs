using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace testSpiral
{
    public class testSpiralInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "testSpiral";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("57c2193b-9950-4ea2-9120-248f7fb5a8cb");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
