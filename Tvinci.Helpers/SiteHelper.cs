//using System.Web;

//namespace Tvinci.Helpers
//{
//    #region ApplicationContext
//    public class ApplicationContext<TLanguage>
//    {
//        public TLanguage TranslationSourceLanguageID { get; set; }
//        public TLanguage DefaultLanguageID { get; set; }
//    }
//    #endregion

//    #region SiteHelper
//    public abstract class SiteHelper<TLanguage> 
//    {
//        #region Constructor
//        protected SiteHelper()
//        {
//        }
//        #endregion

//        #region Fields
//        private ApplicationContext<TLanguage> contextInstance = new ApplicationContext<TLanguage>();
//        #endregion

//        #region Properties
//        public ApplicationContext<TLanguage> AppContext
//        {
//            get
//            {
//                return contextInstance;                                
//            }
//        }

//        public bool IsDefaultLanguage(TLanguage languageID)
//        {
//            return (languageID.Equals(AppContext.DefaultLanguageID));
//        }

//        public virtual TLanguage LanguageValue
//        {
//            get
//            {                
//                return (TLanguage)HttpContext.Current.Session["LanguageID"];
//            }
//            set
//            {
//                HttpContext.Current.Session["LanguageID"] = value;
//            }

//        }
//        #endregion

//        #region Public Methods
//        public bool IsDefaultLanguage()
//        {
//            return (LanguageValue.Equals(AppContext.DefaultLanguageID));
//        }
//        #endregion

//        #region Old Code
//        //public static SessionContext SessionContext
//        //{
//        //    get
//        //    {
//        //        SessionContext result = HttpContext.Current.Session["SessionContext"];
//        //        if (result == null)
//        //        {
//        //            result = HttpContext.Current.Session["SessionContext"];
//        //            lock (contextLock)
//        //            {
//        //                if (contextInstance == null)
//        //                {
//        //                    contextInstance = new ApplicationContext() { DefaultLanguageID = 2 };
//        //                }
//        //            }
//        //        }

//        //        return contextInstance;
//        //    }

//        //}
//        #endregion
//    }
//    #endregion

//    //public class SessionContext
//    //{

//    //}
//}
