using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace LoadSlider
{
    public class LoadSliderInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "LoadSlider";
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
                return new Guid("bf5b4e80-ca94-4d93-8038-39399db4f3d5");
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
