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
                return new Guid("f531984b-94e6-4fa6-93d7-e05bb8989cb0");
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
