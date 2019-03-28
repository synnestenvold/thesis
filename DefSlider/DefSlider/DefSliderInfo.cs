using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace DefSlider
{
    public class DefSliderInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "DefSlider";
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
                return new Guid("f864b943-06a6-4cc5-86e7-99cd0e9b8164");
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
