using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Web.Controls.Infrastructure
{
    public enum eUIDirection
        {
            LTR,
            RTL
        }

    public delegate eUIDirection GetDirectionDelegate();

    public static class TvinciControlHelper
    {
        public static GetDirectionDelegate GetUserLanguageDirection { private get; set; }

        public static eUIDirection? GetUIDirection(eUIDirection defaultValue)
        {
            if (GetUserLanguageDirection != null)
            {
                return GetUserLanguageDirection();
            }

            return null;
        }
    }
   
}
