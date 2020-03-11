using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Gateways_ximon_init : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(@"{
    ""initObj"": {
        ""Locale"": {
            ""LocaleLanguage"": """",
            ""LocaleCountry"": """",
            ""LocaleDevice"": """",
            ""LocaleUserState"": ""Unknown""
        },
        ""Platform"": ""ConnectedTV"",
        ""SiteGuid"": ""0"",
        ""DomainID"": 0,
        ""UDID"": """",
        ""ApiUser"": ""tvpapi_109"",
        ""ApiPass"": ""11111""
    },
    ""GatewayURL"": ""https://tvpapi.tvinci.com/gateways/jsonpostgw.aspx"",
    ""home"": {
        ""newest"": ""1250"",
        ""genres"": [
            {
                ""id"": ""Drama"",
                ""title"": ""Drama"",
                ""thumb"": ""http://""
            },
            {
                ""id"": ""Spanning"",
                ""title"": ""Spanning"",
                ""thumb"": ""http://""
            },
            {
                ""id"": ""Komedie"",
                ""title"": ""Komedie"",
                ""thumb"": ""http://""
            },
            {
                ""id"": ""Kids"",
                ""title"": ""Kids"",
                ""thumb"": ""http://""
            },
            {
                ""id"": ""Documentaires"",
                ""title"": ""Documentaires"",
                ""thumb"": ""http://""
            },
            {
                ""id"": ""Actie"",
                ""title"": ""ActieFilms"",
                ""thumb"": ""http://""
            }
        ],
        ""tryout"": ""1251""
    },
    ""info"": [
        {
            ""samsung"": {
                ""title"": ""wat is ximon?"",
                ""thumb"": ""http://""
            },
            ""lg"": {
                ""title"": ""ximon plus"",
                ""thumb"": ""http://""
            },
            ""sony"": {
                ""title"": ""contact opnemen"",
                ""thumb"": ""http://""
            },
            ""philips"": {
                ""title"": ""contact opnemen"",
                ""thumb"": ""http://""
            }
        }
    ],
    ""filters"": {
        ""Genre"": [
            ""Drama"",
            ""Spanning"",
            ""Komedie"",
            ""Actie""
        ],
        ""Subgenre"": [
            ""Animatie"",
            ""Arthouse"",
            ""Avontuur"",
            ""Biografie""
        ],
        ""Production country"": [
            ""Nederland"",
            ""Belgie"",
            ""Frankrijk""
        ]
    },
    ""myximon"": {
        ""samsung"": {
            ""price_thumb"": ""http://""
        },
        ""lg"": {
            ""price_thumb"": ""http://""
        },
        ""sony"": {
            ""price_thumb"": ""http://""
        },
        ""philips"": {
            ""price_thumb"": ""http://""
        }
    },
    ""error"": {
        ""samsung"": {
            ""no_subscription_thumb"": ""http://""
        },
        ""lg"": {
            ""no_subscription_thumb"": ""http://""
        },
        ""sony"": {
            ""no_subscription_thumb"": ""http://""
        },
        ""philips"": {
            ""no_subscription_thumb"": ""http://""
        }
    }
}");
    }
}