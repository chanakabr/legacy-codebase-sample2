using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class UsersOffline : System.Web.UI.Page
{
    //protected override void OnInit(EventArgs e)
    //{
    //    base.OnInit(e);
    //    btnAddOfflineItem.Click += new EventHandler(btnAddOfflineItem_Click);
    //    btnRemoveOfflineItem.Click += new EventHandler(btnRemoveOfflineItem_Click);
    //    btmClearOfflineItem.Click += new EventHandler(btmClearOfflineItem_Click);
    //    btnGetOfflineItem.Click += new EventHandler(btnGetOfflineItem_Click);
    //    repeaterOfflineREsault.ItemDataBound += new RepeaterItemEventHandler(repeaterOfflineREsault_ItemDataBound);
    //    btnGetItemLeftViewLifeCycle.Click += new EventHandler(btnGetItemLeftViewLifeCycle_Click);
    //    btnAllGetOfflineItem.Click += new EventHandler(btnAllGetOfflineItem_Click);
    //    repeaterAllOfflineREsault.ItemDataBound += new RepeaterItemEventHandler(repeaterAllOfflineREsault_ItemDataBound);
    //    btnGetLicensedLink.Click += new EventHandler(btnGetLicensedLink_Click);
    //    btnGetsubscriptionprice.Click += new EventHandler(btnGetsubscriptionprice_Click);
    //}

    //void btnGetsubscriptionprice_Click(object sender, EventArgs e)
    //{
    //    ca.module c = new ca.module();
    //    string[] str = {"288"};
    //    ca.SubscriptionsPricesContainer[] res = c.GetSubscriptionsPrices("conditionalaccess_144", "11111", str, "", "", "", "");
       
    //}

    //void btnGetLicensedLink_Click(object sender, EventArgs e)
    //{

    //    ca.module f = new ca.module();
    //    int[] mediafile = {269011};
    //    string res = f.GetLicensedLink("conditionalaccess_134","11111", "256844", 269011, "http://192.168.16.150:54321/mp4/2.eny", "", "", "", "", "");
        
    //    lblGetLicensedLink.Text = res;
        
        
    //}

    //void repeaterAllOfflineREsault_ItemDataBound(object sender, RepeaterItemEventArgs e)
    //{
    //    if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
    //    {

    //        us.UserOfflineObject row = (us.UserOfflineObject)e.Item.DataItem;
    //        HtmlGenericControl ltr1 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr2 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr3 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr4 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr5 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr6 = new HtmlGenericControl("div");

    //        ltr1.InnerText = string.Format("- Group ID : {0}", row.m_GroupID.ToString());
    //        ltr2.InnerText = string.Format("- Site User GUID : {0}", row.m_SiteUserGUID);
    //        ltr3.InnerText = string.Format("- Media ID : {0}", row.m_MediaID);
    //        //ltr4.InnerText = string.Format("- Media File ID : {0}", row.m_MediaFileID);
    //        ltr5.InnerText = string.Format("- Create Date : {0}", row.m_CreateDate);
    //        ltr6.InnerText = string.Format("- Update Date : {0}", row.m_UpdateDate);

    //        e.Item.Controls.Add(ltr1);
    //        e.Item.Controls.Add(ltr2);
    //        e.Item.Controls.Add(ltr3);
    //        e.Item.Controls.Add(ltr4);
    //        e.Item.Controls.Add(ltr5);
    //        e.Item.Controls.Add(ltr6);

    //    }
    //}

    //void btnAllGetOfflineItem_Click(object sender, EventArgs e)
    //{
    //    us.UsersService f = new us.UsersService();
    //    us.UserOfflineObject[] arr = f.GetAllUserOfflineAssets("users_134", "11111", "256844");
    //    lblAllGetOfflineItem.Text = arr.Length.ToString();

    //    repeaterAllOfflineREsault.DataSource = arr;
    //    repeaterAllOfflineREsault.DataBind();
    //}

    //void btnGetItemLeftViewLifeCycle_Click(object sender, EventArgs e)
    //{
    //    ca.module c = new ca.module();
    //    //ConditionalAccess.BaseConditionalAccess t = null;
    //    //Int32 nGroupID = ConditionalAccess.Utils.GetGroupID("conditionalaccess_134", "11111", "GetLicensedLink", ref t);
        
    //    //if (nGroupID != 0 && t != null)
    //    //{
    //    //    string str = t.GetLicensedLink("256844", 269011, "http://192.168.16.150:54321/mp4/2.eny", "", "", "", "", "", "");
    //    //}
    //    lblGetItemLeftViewLifeCycle.Text = c.GetItemLeftViewLifeCycle("conditionalaccess_134", "11111", "269011", "256844", false, "", "", "");
    //}

    //void btmClearOfflineItem_Click(object sender, EventArgs e)
    //{
    //    us.UsersService f = new us.UsersService();
    //    lblClear.Text = f.ClearUserOfflineAssets("users_134", "11111", "256844").ToString();
    //}

    //void btnRemoveOfflineItem_Click(object sender, EventArgs e)
    //{
    //    us.UsersService f = new us.UsersService();
    //    lblRemove.Text = f.RemoveUserOfflineAsset("users_134", "11111", "256844", "167631").ToString();
    //}

    //void repeaterOfflineREsault_ItemDataBound(object sender, RepeaterItemEventArgs e)
    //{
    //    if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
    //    {

    //        us.UserOfflineObject row = (us.UserOfflineObject)e.Item.DataItem;
    //        HtmlGenericControl ltr1 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr2 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr3 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr4 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr5 = new HtmlGenericControl("div");
    //        HtmlGenericControl ltr6 = new HtmlGenericControl("div");
            
    //        ltr1.InnerText = string.Format("- Group ID : {0}", row.m_GroupID.ToString());
    //        ltr2.InnerText = string.Format("- Site User GUID : {0}", row.m_SiteUserGUID);
    //        ltr3.InnerText = string.Format("- Media ID : {0}", row.m_MediaID);
    //        //ltr4.InnerText = string.Format("- Media File ID : {0}", row.m_MediaFileID);
    //        ltr5.InnerText = string.Format("- Create Date : {0}", row.m_CreateDate);
    //        ltr6.InnerText = string.Format("- Update Date : {0}", row.m_UpdateDate);

    //        e.Item.Controls.Add(ltr1);
    //        e.Item.Controls.Add(ltr2);
    //        e.Item.Controls.Add(ltr3);
    //        e.Item.Controls.Add(ltr4);
    //        e.Item.Controls.Add(ltr5);
    //        e.Item.Controls.Add(ltr6);
            
    //    }
    //}

    //void btnGetOfflineItem_Click(object sender, EventArgs e)
    //{
    //    us.UsersService f = new us.UsersService();
    //    us.UserOfflineObject[] arr = f.GetUserOfflineAssetsByFileType("users_134", "11111", "256844", "iPad Main");
    //    lblGetResault.Text = arr.Length.ToString();

    //    repeaterOfflineREsault.DataSource = arr;
    //    repeaterOfflineREsault.DataBind();

    //}

    //void btnAddOfflineItem_Click(object sender, EventArgs e)
    //{
    //    us.UsersService f = new us.UsersService();
    //    lblResault.Text = f.AddUserOfflineAsset("users_134", "11111", "256844", "167631").ToString();


    //}
    //protected void Page_Load(object sender, EventArgs e)
    //{

    //}
}