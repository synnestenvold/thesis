﻿using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ViewDeformations
{
    public class ViewDeformationsInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ViewDeformations";
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
                return new Guid("27dddaee-b084-400e-85d3-7af3909305fa");
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
