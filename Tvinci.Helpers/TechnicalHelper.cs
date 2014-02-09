using System.Web;

namespace Tvinci.Helpers
{
    /// <summary>
    /// Summary description for TechnicalManager
    /// </summary>
    public class TechnicalManager
    {
        #region Fields
        public static bool UserEditorialMode
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                    return false;
                else
                    return (HttpContext.Current.Session["IsEditorial"] != null);
            }
            set
            {
                if (value)
                {
                    HttpContext.Current.Session["IsEditorial"] = "yeaa";
                }
                else
                {
                    HttpContext.Current.Session["IsEditorial"] = null;
                }
            }
        }
        #endregion

        #region Properties
        public static bool IsUserEditorial()
        {            
            return UserEditorialMode;
        }

        #endregion
    }
}