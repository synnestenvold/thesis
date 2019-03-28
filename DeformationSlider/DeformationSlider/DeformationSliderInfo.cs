using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace DeformationSlider
{
    public class DeformationSliderInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "DeformationSlider";
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
                return new Guid("d7b09d45-5100-4786-a9bc-76983219d0eb");
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
