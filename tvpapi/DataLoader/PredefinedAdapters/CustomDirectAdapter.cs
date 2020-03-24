//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Tvinci.Data.DataLoader.PredefinedAdapters
//{
//    public sealed class CustomDirectAdapter<TExpectedResult> : CustomAdapter<TExpectedResult>
//    {
//        public delegate object ExecuteDelegate();

//        public ExecuteDelegate m_executeMethod { get; set; }

//        public CustomDirectAdapter(ExecuteDelegate executeMethod)
//        {
//            m_executeMethod = executeMethod;
//        }

//        protected internal override object Execute()
//        {
//            return m_executeMethod();
//        }

//        public override bool IsPersist()
//        {
//            return false;
//        }
//    }
//}
