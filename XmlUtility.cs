using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace System.XmlUtility
{
    public class XmlUtility
    {
        /// <summary>
        /// Checks if xElement and whether it contains other elements
        /// </summary>
        /// <param name="xElement">Element, which is verified</param>
        /// <returns></returns>
        public static bool IsExistElementsInXElement(XElement xElement)
        {
            if (xElement != null && xElement.Elements().Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if xElement and whether it contains attributes
        /// </summary>
        /// <param name="xElement">Element, which is verified</param>
        /// <returns></returns>
        public static bool IsExistAttributesInXElement(XElement xElement)
        {
            if (xElement != null && xElement.Attributes().Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


}
