using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

public partial class import : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //string[] s = { "guy@tvinci.com" };
        //TelemundoDailyReport.DailyReport.DoTheJob(s);
        //TvinciImporter.ImporterImpl.DoTheWork(105, "http://www.tvinci.com/clients/novebox/nove_articles_prob.xml", "", 0);
        //MSNFeeder.Feeder.CatalogImport("eric@novebox.com", "VVRfqKs2", "es-xl", "https://catalog.video.msn.com/admin/services/videobytag.aspx?mk=es-xl&ns=VC_Source&tag=ESXL_Telemundo_Network%3aESXL_Telemundo_Clon&ipt=1&ps=1000", "Telemundo_Clon_es_xl.xml");
        //MSNFeeder.Feeder.CatalogImport("eric@novebox.com", "VVRfqKs2", "es-us", "https://catalog.video.msn.com/admin/services/videobytag.aspx?mk=es-us&ns=VC_Source&tag=ESXL_Telemundo_Network%3aESXL_Telemundo_Clon&ipt=1&ps=1000", "Telemundo_Clon_es_us.xml");
        string sFeedXML = "<feed><export><media  erase=\"false\"  co_guid=\"5e484982-2969-49c4-9dd3-aa46bb00f323\" action=\"insert\" is_active=\"false\"><basic><media_type>episode</media_type><name><value lang=\"spa\">En cuerpo ajeno</value></name><description><value lang=\"spa\">Sintiéndose  traicionada, la última de las amantes de Lalo decide vengarse. Con la ayuda de una bruja, quien realiza un eficaz hechizo, Lalo despierta transformado en una mujer.  ¿Qué camino tomara el seductor profesional?</value></description><rules><watch_per_rule>Parent allowed</watch_per_rule></rules><dates><start>05/10/2010 08:51:37</start><catalog_end>07/07/2015 23:31:20</catalog_end><final_end>17/07/2013 23:31:20</final_end></dates></basic><structure><strings><meta name=\"Msn ID number\" ml_handling=\"duplicate\"><value lang=\"spa\">5e484982-2969-49c4-9dd3-aa46bb00f323</value></meta></strings><doubles></doubles><booleans></booleans><metas></metas></structure><files><file handling_type=\"IMAGE\" type=\"POSTER\" quality=\"HIGH\" cdn_name=\"\" cdn_code=\"\"/><file handling_type=\"CLIP\" type=\"Main MSN Player\" billing_type=\"None\" quality=\"HIGH\" player_type=\"MSN\" cdn_name=\"MSN Flv Palyer\" cdn_code=\"&lt;set&gt;&lt;file mk=&quot;es-us&quot;&gt;58653fb9-ddcb-4858-94c2-e226bf7ca5e2&lt;/file&gt;&lt;file mk=&quot;es-xl&quot;&gt;5e484982-2969-49c4-9dd3-aa46bb00f323&lt;/file&gt;&lt;/set&gt;\"/></files></media></export></feed>";
        string sNotifyXML = "";
        TvinciImporter.ImporterImpl.DoTheWorkInner(sFeedXML, 104, "", ref sNotifyXML);
        //TvinciImporter.ImporterImpl.Do
    }
}
