using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using PricingMonitor;

public partial class PricingMonitoring : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        btnPriceMonitoring.Click += new EventHandler(PriceMonitoring_Click);
    }

    void PriceMonitoring_Click(object sender, EventArgs e)
    {

        //test for WriteCasheToLog at  private bool MonitoringPrice() 
        //16.10.2012
      //  PriceMonitoring pm = new PriceMonitoring(0, 10, "109||255315||219307");
      //  PriceMonitoring pm = new PriceMonitoring(0, 10, "109||201954||219307");
      //  PriceMonitoring pm = new PriceMonitoring(0, 10, "109||111736||219307");


        //PriceMonitoring pm = new PriceMonitoring(Convert.ToInt32(TaskID.Text),Convert.ToInt32(IntervalInSec.Text), groupID.Text+"||"+UserGuid.Text +"||" + MediaFiles.Text);

        
        bool doTheTask = false;
       // doTheTask = pm.DoTheTask();
        //test until here


    //    PricingMonitor.PriceMonitoring pm = new PricingMonitor.PriceMonitoring(0, 10, "109||255315||219307");
    ////    pm.DoTheTask();
    //    API.API a = new API.API();
    //   API.MediaMarkObject test =  a.GetMediaMark("api_134", "11111", 172001, "256844");
        
    //   ca.module caModule = new ca.module();
    //    string[] subscriptioncode = {"299"};
    //    caModule.GetSubscriptionsPrices("conditionalaccess_144", "11111", subscriptioncode , "", "", "", "");
    //  //  caModule.
    }
}