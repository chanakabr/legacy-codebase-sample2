using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class UIData
    {
        public string ColorCode { get; set; }
        public string picURL    { get; set; }
    }

    public class GroupOperator
    {
        public int ID               { get; set; }
        public string Name          { get; set; }
        public eOperatorType Type   { get; set; }
        public string CoGuid        { get; set; }
        public string LoginUrl      { get; set; }
        public int SubGroupID       { get; set; }
        public UIData UIData;
        public Scope[] Scopes       { get; set; }
        public String GroupUserName { get; set; }
        public string GroupPassword { get; set; }
        public string LogoutURL     { get; set; }
        public List<ApiObjects.KeyValuePair> Groups_operators_menus;
        public string AboutUs       { get; set; }
        public string ContactUs     { get; set; }
    }

    public enum eOperatorType
    {
        OAuth = 1,
        API = 2,
        TVinci = 3,
        SAML = 4
    }

    public class Scope
    {
        public string Name      { get; set; }
        public string LoginUrl  { get; set; }
        public string LogoutUrl { get; set; }
    }

    public class DisplayObj
    {
        public string Logo          { get; set; }
        public string ColorScheme   { get; set; }
    }
}
