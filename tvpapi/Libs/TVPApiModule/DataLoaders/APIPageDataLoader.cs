using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Helpers;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader;
using KLogMonitor;
using System.Reflection;

namespace TVPApi
{
    public class APIPageDataLoader : TVPPro.SiteManager.DataLoaders.PageDataLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int GroupID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "GroupID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "GroupID", value);
            }
        }

        public PlatformType Platform
        {
            get
            {
                return Parameters.GetParameter<PlatformType>(eParameterType.Retrieve, "Platform", PlatformType.Unknown);
            }
            set
            {
                Parameters.SetParameter<PlatformType>(eParameterType.Retrieve, "Platform", value);
            }
        }

        public bool IsShared
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "IsShared", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "IsShared", value);
            }
        }

        protected override dsPageData CreateSourceResult()
        {
            ConnectionManager connMng = new ConnectionManager(GroupID, Platform, IsShared);
            dsPageData result = new dsPageData();
            logger.InfoFormat("CreateSourceResult-> Start Retrieve Data - {0}_{1}", GroupID, Platform);

            //Fill pages table
            //new DatabaseDirectAdapter(delegate (TVPApi.ODBCWrapper.DataSetSelectQuery query)
            //{
            //    query.SetConnectionString(connMng.GetClientConnectionString());
            //    query += "select q.id,q.TOKEN as 'PageToken', tmi.ID as 'ParentPageID', q.URL, q.Name, q.Description, q.BreadCrumbText, q.SitePageMetadataID, q.SIDE_PROFILE_ID as SideProfileID, q.BOTTOM_PROFILE_ID as BottomProfileID, q.MENU_ID as MenuID, q.FOOTER_ID as FooterID, q.MIDDLE_FOOTER_ID as MiddleFooterID, q.CULTURE as 'LanguageCulture', q.PAGE_PROFILE_ID as ProfileID, q.IS_PROTECTED as IsProtected, q.KEYWORDS as Keywords, q.IS_ACTIVE as IsActive, ";
            //    query += "q.BrandingBigPicID, q.BrandingSmallPicID, q.BrandingPixelHeight, q.BrandingRecurringVertical, q.BrandingRecurringHorizonal from ";
            //    query += "(SELECT sp.ID, sp.PAGE_PROFILE_ID, sp.IS_PROTECTED, sp.SIDE_PROFILE_ID , sp.BOTTOM_PROFILE_ID, sp.MENU_ID, sp.FOOTER_ID, sp.MIDDLE_FOOTER_ID, sp.IS_ACTIVE, sp.BRANDING_BIG_PIC_ID as 'BrandingBigPicID', sp.BRANDING_SMALL_PIC_ID as 'BrandingSmallPicID', sp.BRANDING_PIXEL_HEIGHT as 'BrandingPixelHeight', sp.IS_RECURRING_HORIZONTAL as 'BrandingRecurringHorizonal', ";
            //    query += "sp.IS_RECURRING_VERTICAL as 'BrandingRecurringVertical',lpt.TOKEN, lpt.URL, spm.Name, spm.Description, spm.BreadCrumbText, spm.page_structure_id AS SitePageMetadataID, spm.KEYWORDS, lg.CULTURE FROM ";
            //    query += "tvp_pages_structure sp, tvp_pages_texts spm, lu_page_types lpt, lu_languages lg where sp.ID = spm.PAGE_STRUCTURE_ID and ";
            //    query += "spm.LANGUAGE_ID = lg.ID and lpt.id=sp.PAGE_TYPE and ";
            //    query += "sp.is_active in (0,1) and ";
            //    query += DatabaseHelper.AddCommonFields("sp.Status", string.Empty, eExecuteLocation.Application, true);
            //    query += DatabaseHelper.AddCommonFields("lg.Status", string.Empty, eExecuteLocation.Application, false);
            //    query += ")q LEFT JOIN tvp_menu_items tmi ON tmi.LINK = q.URL";
            //}, result.Pages).Execute();

            // Fill page galleries table
            //new DatabaseDirectAdapter(delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
            //{
            //    query.SetConnectionString(connMng.GetClientConnectionString());
            //    query += "select q_all.* from (select top_q1.* from ";
            //    query += "(select q.TVMChannelID,q.channel_sub,q.ID,q.SitePageID,q.main_location,q.location,q.GalleryType, q.UiComponentType ,q.NumberOfItemsPerPage, q.BooleanParam, q.NumericParam, ";
            //    query += "q.NumOfItems,q.ViewType,q.PictureSize,q.item_link,q.SubPicSize, q.PIC_MAIN, q.PIC_SUB, q.swf, q.MainPlayerUN,q.MainPlayerPass, q.TvmAccountUN, q.TvmAccountPass, ";
            //    query += "q.SubPlayerUN,q.SubPlayerPass,q7.title as GroupTitle,q7.MAIN_DESCRIPTION as GroupDescription, ";
            //    query += "q7.SUB_DESCRIPTION as GroupSubDescription,q6.title,q6.MAIN_DESCRIPTION, q6.SUB_DESCRIPTION, ";
            //    query += "q6.TOOLTIP_TEXT,q6.CODE3 as LANG_CODE,q6.CULTURE,q7.CODE3 as MAIN_LANG_CODE,q7.CULTURE as MAIN_CULTURE, q7.LINKS_HEADER as LinkHeader, q.inner_order_num,q.order_num, q.FAMILY_NUM as Family_Num ";
            //    query += "from (";
            //    query += "select tcgi.channel_main as TVMChannelID ,tcgi.channel_sub,tpmg.profile_id, tcgi.order_num as inner_order_num, lp.description as location, tpmg.order_num, tpmg.FAMILY_NUM, tpmg.ID, tcgi.ID as ItemID, tpmg.PAGE_STRUCTURE_ID as SitePageID, tpmg.MAIN_NUM as main_location, ttcgt.id as GalleryType, ttcgt.UI_COMPONENT_TYPE as UiComponentType, ";
            //    query += "tcgi.PAGE_SIZE as NumberOfItemsPerPage, tcgi.MAX_RESULT_NUM as NumOfItems, tcgi.BOOLEAN as BooleanParam, tcgi.NUMERIC AS NumericParam, lgt.DESCRIPTION as ViewType, lgps.TOKEN as PictureSize, tcgi.LINK as item_link, lgps2.TOKEN as SubPicSize, tcgi.PIC_MAIN, tcgi.PIC_SUB, tcgi.swf, ";
            //    query += "tam.PLAYER_UN as MainPlayerUN, tam.PLAYER_PASS as MainPlayerPass, tas.PLAYER_UN as SubPlayerUN, tas.PLAYER_PASS as SubPlayerPass, tac.PLAYER_UN as TvmAccountUN, tac.PLAYER_PASS as TvmAccountPass ";
            //    query += "from lu_profile_types lp, tvp_galleries tpmg, tvp_galleries_items tcgi, tvp_template_channels_gallery_types ttcgt, lu_gallery_view_types lgt, lu_gallery_pic_sizes lgps, lu_gallery_pic_sizes lgps2, ";
            //    query += "tvp_tvm_accounts tam, tvp_tvm_accounts tas, tvp_tvm_accounts tac ";
            //    query += "where ";
            //    query += "lp.id=tpmg.LOCATION_ID and tpmg.profile_id=0 and tpmg.CHANNEL_TEMPLATE_ID=ttcgt.ID and tpmg.STATUS=1 and tcgi.TVP_GALLERY_ID=tpmg.id and lgt.id=tcgi.VIEW_TYPE and lgps.id=tcgi.PIC_SIZE and lgps2.id=tcgi.SUB_PIC_SIZE and ";
            //    query += DatabaseHelper.AddCommonFields("tcgi.STATUS", "tcgi.IS_ACTIVE", eExecuteLocation.Application, true);
            //    query += DatabaseHelper.AddCommonFields("tpmg.STATUS", "tpmg.IS_ACTIVE", eExecuteLocation.Application, true);
            //    query += "tam.id=tcgi.MAIN_TVM_ACCOUNT_ID and tas.id=tcgi.SUB_TVM_ACCOUNT_ID and tac.id=tpmg.TVM_ACCOUNT_ID )q ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tcgt.tvp_gallery_item_ID,tcgt.title,tcgt.MAIN_DESCRIPTION,tcgt.SUB_DESCRIPTION,tcgt.TOOLTIP_TEXT,ll.CODE3,ll.CULTURE from ";
            //    query += "tvp_galleries_items_text tcgt,lu_languages ll where tcgt.language_ID=ll.id and ll.isUsed = 1)q6 on q6.tvp_gallery_item_ID=q.ItemID ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tgt.tvp_gallery_ID,tgt.title,tgt.MAIN_DESCRIPTION,tgt.SUB_DESCRIPTION,tgt.LINKS_HEADER,ll.CODE3,ll.CULTURE from ";
            //    query += "tvp_galleries_text tgt,lu_languages ll where tgt.language_ID=ll.id and ll.isUsed = 1)q7 on q7.tvp_gallery_ID=q.ID";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgl.tvp_gallery_id,ll.culture,ll.code3 from ";
            //    //query += "tvp_galleries_langs lo_pmgl,lu_languages ll where ll.id=lo_pmgl.language_id and ll.isUsed = 1 and lo_pmgl.is_active=1 and lo_pmgl.status=1)q1 on q1.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgd.tvp_gallery_id,ld.DESCRIPTION from ";
            //    //query += "tvp_galleries_devices lo_pmgd,lu_devices ld where ld.id=lo_pmgd.device_id and lo_pmgd.is_active=1 and lo_pmgd.status=1)q2 on q2.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgc.tvp_gallery_id,lc.COUNTRY_NAME,lc.COUNTRY_CD2 from ";
            //    //query += "tvp_galleries_countries lo_pmgc,lu_countries lc where lc.id=lo_pmgc.country_id and lo_pmgc.is_active=1 and lo_pmgc.status=1)q3 on q3.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgu.tvp_gallery_id,lo_pmgu.USER_STATE_ID from ";
            //    //query += "tvp_galleries_user_states lo_pmgu,lu_user_states lu where lu.id=lo_pmgu.USER_STATE_ID and lo_pmgu.is_active=1 and lo_pmgu.status=1)q4 on q4.tvp_gallery_id=q.id ";
            //    query += ")top_q1 ";

            //    query += "UNION ALL ";

            //    query += "select top_q2.* from (";
            //    query += "select q.TVMChannelID,q.channel_sub,q.ID,q.SitePageID,q.main_location,q5.description as location,q.GalleryType, q.UiComponentType ,q.NumberOfItemsPerPage, q.BooleanParam, q.NumericParam, ";
            //    query += "q.NumOfItems,q.ViewType,q.PictureSize,q.item_link,q.SubPicSize, q.PIC_MAIN, q.PIC_SUB, q.swf, q.MainPlayerUN,q.MainPlayerPass,q.TvmAccountUN,q.TvmAccountPass, ";
            //    query += "q.SubPlayerUN,q.SubPlayerPass, q7.title as GroupTitle,q7.MAIN_DESCRIPTION as GroupDescription,q7.SUB_DESCRIPTION as GroupSubDescription,q6.title, ";
            //    query += "q6.MAIN_DESCRIPTION,q6.SUB_DESCRIPTION,q6.TOOLTIP_TEXT,q6.CODE3 as LANG_CODE,q6.CULTURE,q7.CODE3 as MAIN_LANG_CODE, ";
            //    query += "q7.CULTURE as MAIN_CULTURE, q7.LINKS_HEADER as LinkHeader, q.inner_order_num,q.order_num, q.FAMILY_NUM as Family_Num ";
            //    query += "from (";
            //    query += "select tcgi.channel_main as TVMChannelID ,tcgi.channel_sub,tpmg.profile_id, tcgi.NUMERIC AS NumericParam,tcgi.order_num as inner_order_num, tcgi.BOOLEAN as BooleanParam, tpmg.order_num, tpmg.FAMILY_NUM, tpmg.ID, tcgi.ID as ItemID, tps.ID as SitePageID, tpmg.MAIN_NUM as main_location, ttcgt.id as GalleryType, ttcgt.UI_COMPONENT_TYPE as UiComponentType, tcgi.PAGE_SIZE as NumberOfItemsPerPage, tcgi.MAX_RESULT_NUM as NumOfItems, lgt.DESCRIPTION as ViewType, ";
            //    query += "lgps.TOKEN as PictureSize, tcgi.LINK as item_link, lgps2.TOKEN as SubPicSize, tcgi.PIC_MAIN, tcgi.PIC_SUB, tcgi.swf, tam.PLAYER_UN as MainPlayerUN, tam.PLAYER_PASS as MainPlayerPass, tas.PLAYER_UN as SubPlayerUN, tas.PLAYER_PASS as SubPlayerPass, tac.PLAYER_UN as TvmAccountUN, tac.PLAYER_PASS as TvmAccountPass ";
            //    query += "from ";
            //    query += "tvp_galleries tpmg, tvp_galleries_items tcgi, tvp_template_channels_gallery_types ttcgt, lu_gallery_view_types lgt, lu_gallery_pic_sizes lgps, lu_gallery_pic_sizes lgps2, tvp_tvm_accounts tam, tvp_tvm_accounts tas, tvp_tvm_accounts tac, tvp_pages_structure tps ";
            //    query += "where ";
            //    query += "(tps.TOP_PROFILE_ID=tpmg.profile_id or tps.SIDE_PROFILE_ID=tpmg.profile_id or tps.BOTTOM_PROFILE_ID=tpmg.profile_id) and tpmg.STATUS=1 and tpmg.profile_id<>0 and ttcgt.id=tpmg.channel_template_id and tpmg.CHANNEL_TEMPLATE_ID=ttcgt.ID and tcgi.TVP_gallery_ID=tpmg.id and";
            //    query += DatabaseHelper.AddCommonFields("tpmg.STATUS", "tpmg.IS_ACTIVE", eExecuteLocation.Application, true);
            //    query += DatabaseHelper.AddCommonFields("tcgi.STATUS", "tcgi.IS_ACTIVE", eExecuteLocation.Application, true);
            //    query += "lgt.id=tcgi.VIEW_TYPE and lgps.id=tcgi.PIC_SIZE and lgps2.id=tcgi.SUB_PIC_SIZE and tam.id=tcgi.MAIN_TVM_ACCOUNT_ID and tas.id=tcgi.SUB_TVM_ACCOUNT_ID and tac.id=tpmg.TVM_ACCOUNT_ID)q ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tcgt.tvp_gallery_item_ID,tcgt.title,tcgt.MAIN_DESCRIPTION,tcgt.SUB_DESCRIPTION,tcgt.TOOLTIP_TEXT,ll.CODE3,ll.CULTURE from ";
            //    query += "tvp_galleries_items_text tcgt,lu_languages ll where tcgt.language_ID=ll.id and ll.isUsed = 1)q6 on q6.tvp_gallery_item_ID=q.ItemID ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tgt.tvp_gallery_ID,tgt.title,tgt.MAIN_DESCRIPTION,tgt.SUB_DESCRIPTION, tgt.LINKS_HEADER,ll.CODE3,ll.CULTURE from ";
            //    query += "tvp_galleries_text tgt,lu_languages ll where tgt.language_ID=ll.id and ll.isUsed = 1)q7 on q7.tvp_gallery_ID=q.ID";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tp.id,lp.DESCRIPTION from ";
            //    query += "tvp_profiles tp,lu_profile_types lp where lp.id=tp.PROFILE_TYPE and tp.is_active=1 and tp.status=1)q5 on q5.id=q.PROFILE_ID";
            //    query += ")top_q2 ";
            //    query += ")q_all ";
            //    query += "order by SitePageID,location,main_location,order_num,inner_order_num";
            //}, result.PageGalleries).Execute();

            //Get All in active galleries (for editorial mode)
            //new DatabaseDirectAdapter(delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
            //{
            //    query.SetConnectionString(connMng.GetClientConnectionString());
            //    query += "select q_all.* from (select top_q1.* from ";
            //    query += "(select q.TVMChannelID,q.channel_sub,q.ID,q.SitePageID,q.main_location,q.location,q.GalleryType, q.UiComponentType ,q.NumberOfItemsPerPage, q.BooleanParam, q.NumericParam, ";
            //    query += "q.NumOfItems,q.ViewType,q.PictureSize,q.item_link,q.SubPicSize,q.PIC_MAIN, q.PIC_SUB, q.swf, q.MainPlayerUN,q.MainPlayerPass, q.TvmAccountUN, q.TvmAccountPass, ";
            //    query += "q.SubPlayerUN,q.SubPlayerPass,q7.title as GroupTitle,q7.MAIN_DESCRIPTION as GroupDescription, ";
            //    query += "q7.SUB_DESCRIPTION as GroupSubDescription,q6.title,q6.MAIN_DESCRIPTION,q6.SUB_DESCRIPTION, ";
            //    query += "q6.TOOLTIP_TEXT,q6.CODE3 as LANG_CODE,q6.CULTURE,q7.CODE3 as MAIN_LANG_CODE,q7.CULTURE as MAIN_CULTURE, q7.LINKS_HEADER as LinkHeader, q.inner_order_num,q.order_num, q.FAMILY_NUM as Family_Num ";
            //    query += "from (";
            //    query += "select tcgi.channel_main as TVMChannelID ,tcgi.channel_sub,tpmg.profile_id, tcgi.order_num as inner_order_num, lp.description as location, tpmg.order_num, tpmg.FAMILY_NUM, tpmg.ID, tcgi.ID as ItemID, tpmg.PAGE_STRUCTURE_ID as SitePageID, tpmg.MAIN_NUM as main_location, ttcgt.id as GalleryType, ttcgt.UI_COMPONENT_TYPE as UiComponentType, ";
            //    query += "tcgi.PAGE_SIZE as NumberOfItemsPerPage, tcgi.MAX_RESULT_NUM as NumOfItems, tcgi.BOOLEAN as BooleanParam, tcgi.NUMERIC AS NumericParam, lgt.DESCRIPTION as ViewType, lgps.TOKEN as PictureSize, tcgi.LINK as item_link, lgps2.TOKEN as SubPicSize, tcgi.PIC_MAIN, tcgi.PIC_SUB, tcgi.swf, ";
            //    query += "tam.PLAYER_UN as MainPlayerUN, tam.PLAYER_PASS as MainPlayerPass, tas.PLAYER_UN as SubPlayerUN, tas.PLAYER_PASS as SubPlayerPass, tac.PLAYER_UN as TvmAccountUN, tac.PLAYER_PASS as TvmAccountPass ";
            //    query += "from lu_profile_types lp, tvp_galleries tpmg, tvp_galleries_items tcgi, tvp_template_channels_gallery_types ttcgt, lu_gallery_view_types lgt, lu_gallery_pic_sizes lgps, lu_gallery_pic_sizes lgps2, ";
            //    query += "tvp_tvm_accounts tam, tvp_tvm_accounts tas, tvp_tvm_accounts tac ";
            //    query += "where ";
            //    query += "lp.id=tpmg.LOCATION_ID and tpmg.profile_id=0 and tpmg.CHANNEL_TEMPLATE_ID=ttcgt.ID and tpmg.STATUS=1 and tcgi.TVP_GALLERY_ID=tpmg.id and lgt.id=tcgi.VIEW_TYPE and lgps.id=tcgi.PIC_SIZE and lgps2.id=tcgi.SUB_PIC_SIZE and ((tpmg.IS_ACTIVE <> 1 ) or (tpmg.IS_ACTIVE = 1 and tcgi.IS_ACTIVE <> 1)) and";
            //    query += DatabaseHelper.AddCommonFields("tcgi.STATUS", string.Empty, eExecuteLocation.Application, true);
            //    query += DatabaseHelper.AddCommonFields("tpmg.STATUS", string.Empty, eExecuteLocation.Application, true);
            //    query += "tam.id=tcgi.MAIN_TVM_ACCOUNT_ID and tas.id=tcgi.SUB_TVM_ACCOUNT_ID and tac.id=tpmg.TVM_ACCOUNT_ID )q ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tcgt.tvp_gallery_item_ID,tcgt.title,tcgt.MAIN_DESCRIPTION,tcgt.SUB_DESCRIPTION,tcgt.TOOLTIP_TEXT,ll.CODE3,ll.CULTURE from ";
            //    query += "tvp_galleries_items_text tcgt,lu_languages ll where tcgt.language_ID=ll.id and ll.isUsed = 1)q6 on q6.tvp_gallery_item_ID=q.ItemID ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tgt.tvp_gallery_ID,tgt.title,tgt.MAIN_DESCRIPTION,tgt.SUB_DESCRIPTION,tgt.LINKS_HEADER,ll.CODE3,ll.CULTURE from ";
            //    query += "tvp_galleries_text tgt,lu_languages ll where tgt.language_ID=ll.id and ll.isUsed = 1)q7 on q7.tvp_gallery_ID=q.ID";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgl.tvp_gallery_id,ll.culture,ll.code3 from ";
            //    //query += "tvp_galleries_langs lo_pmgl,lu_languages ll where ll.id=lo_pmgl.language_id and ll.isUsed = 1 and lo_pmgl.is_active=1 and lo_pmgl.status=1)q1 on q1.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgd.tvp_gallery_id,ld.DESCRIPTION from ";
            //    //query += "tvp_galleries_devices lo_pmgd,lu_devices ld where ld.id=lo_pmgd.device_id and lo_pmgd.is_active=1 and lo_pmgd.status=1)q2 on q2.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgc.tvp_gallery_id,lc.COUNTRY_NAME,lc.COUNTRY_CD2 from ";
            //    //query += "tvp_galleries_countries lo_pmgc,lu_countries lc where lc.id=lo_pmgc.country_id and lo_pmgc.is_active=1 and lo_pmgc.status=1)q3 on q3.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgu.tvp_gallery_id,lo_pmgu.USER_STATE_ID from ";
            //    //query += "tvp_galleries_user_states lo_pmgu,lu_user_states lu where lu.id=lo_pmgu.USER_STATE_ID and lo_pmgu.is_active=1 and lo_pmgu.status=1)q4 on q4.tvp_gallery_id=q.id ";
            //    query += ")top_q1 ";

            //    query += "UNION ALL ";

            //    query += "select top_q2.* from (";
            //    query += "select q.TVMChannelID,q.channel_sub,q.ID,q.SitePageID,q.main_location,q5.description as location,q.GalleryType, q.UiComponentType ,q.NumberOfItemsPerPage, q.BooleanParam, q.NumericParam, ";
            //    query += "q.NumOfItems,q.ViewType,q.PictureSize,q.item_link,q.SubPicSize,q.PIC_MAIN,q.PIC_SUB, q.swf, q.MainPlayerUN,q.MainPlayerPass,q.TvmAccountUN,q.TvmAccountPass, ";
            //    query += "q.SubPlayerUN,q.SubPlayerPass, q7.title as GroupTitle,q7.MAIN_DESCRIPTION as GroupDescription,q7.SUB_DESCRIPTION as GroupSubDescription,q6.title, ";
            //    query += "q6.MAIN_DESCRIPTION,q6.SUB_DESCRIPTION,q6.TOOLTIP_TEXT,q6.CODE3 as LANG_CODE,q6.CULTURE,q7.CODE3 as MAIN_LANG_CODE, ";
            //    query += "q7.CULTURE as MAIN_CULTURE, q7.LINKS_HEADER as LinkHeader, q.inner_order_num,q.order_num, q.FAMILY_NUM as Family_Num ";
            //    query += "from (";
            //    query += "select tcgi.channel_main as TVMChannelID ,tcgi.channel_sub,tpmg.profile_id, tcgi.NUMERIC AS NumericParam,tcgi.order_num as inner_order_num, tcgi.BOOLEAN as BooleanParam, tpmg.order_num, tpmg.FAMILY_NUM, tpmg.ID, tcgi.ID as ItemID, tps.ID as SitePageID, tpmg.MAIN_NUM as main_location, ttcgt.id as GalleryType, ttcgt.UI_COMPONENT_TYPE as UiComponentType, tcgi.PAGE_SIZE as NumberOfItemsPerPage, tcgi.MAX_RESULT_NUM as NumOfItems, lgt.DESCRIPTION as ViewType, ";
            //    query += "lgps.TOKEN as PictureSize, tcgi.LINK as item_link, lgps2.TOKEN as SubPicSize, tcgi.PIC_MAIN, tcgi.PIC_SUB, tcgi.swf, tam.PLAYER_UN as MainPlayerUN, tam.PLAYER_PASS as MainPlayerPass, tas.PLAYER_UN as SubPlayerUN, tas.PLAYER_PASS as SubPlayerPass, tac.PLAYER_UN as TvmAccountUN, tac.PLAYER_PASS as TvmAccountPass ";
            //    query += "from ";
            //    query += "tvp_galleries tpmg, tvp_galleries_items tcgi, tvp_template_channels_gallery_types ttcgt, lu_gallery_view_types lgt, lu_gallery_pic_sizes lgps, lu_gallery_pic_sizes lgps2, tvp_tvm_accounts tam, tvp_tvm_accounts tas, tvp_tvm_accounts tac, tvp_pages_structure tps ";
            //    query += "where ";
            //    query += "(tps.TOP_PROFILE_ID=tpmg.profile_id or tps.SIDE_PROFILE_ID=tpmg.profile_id or tps.BOTTOM_PROFILE_ID=tpmg.profile_id) and tpmg.STATUS=1 and tpmg.profile_id<>0 and ttcgt.id=tpmg.channel_template_id and tpmg.CHANNEL_TEMPLATE_ID=ttcgt.ID and tcgi.TVP_gallery_ID=tpmg.id and ((tpmg.IS_ACTIVE <> 1 ) or (tpmg.IS_ACTIVE = 1 and tcgi.IS_ACTIVE <> 1)) and";
            //    query += DatabaseHelper.AddCommonFields("tpmg.STATUS", string.Empty, eExecuteLocation.Application, true);
            //    query += DatabaseHelper.AddCommonFields("tcgi.STATUS", string.Empty, eExecuteLocation.Application, true);
            //    query += "lgt.id=tcgi.VIEW_TYPE and lgps.id=tcgi.PIC_SIZE and lgps2.id=tcgi.SUB_PIC_SIZE and tam.id=tcgi.MAIN_TVM_ACCOUNT_ID and tas.id=tcgi.SUB_TVM_ACCOUNT_ID and tac.id=tpmg.TVM_ACCOUNT_ID)q ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tcgt.tvp_gallery_item_ID,tcgt.title,tcgt.MAIN_DESCRIPTION,tcgt.SUB_DESCRIPTION,tcgt.TOOLTIP_TEXT,ll.CODE3,ll.CULTURE from ";
            //    query += "tvp_galleries_items_text tcgt,lu_languages ll where tcgt.language_ID=ll.id and ll.isUsed = 1)q6 on q6.tvp_gallery_item_ID=q.ItemID ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tgt.tvp_gallery_ID,tgt.title,tgt.MAIN_DESCRIPTION,tgt.SUB_DESCRIPTION, tgt.LINKS_HEADER,ll.CODE3,ll.CULTURE from ";
            //    query += "tvp_galleries_text tgt,lu_languages ll where tgt.language_ID=ll.id and ll.isUsed = 1)q7 on q7.tvp_gallery_ID=q.ID";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgl.tvp_gallery_id,ll.culture,ll.code3 from ";
            //    //query += "tvp_galleries_langs lo_pmgl,lu_languages ll where ll.id=lo_pmgl.language_id and ll.isUsed = 1 and lo_pmgl.is_active=1 and lo_pmgl.status=1)q1 on q1.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgd.tvp_gallery_id,ld.DESCRIPTION from ";
            //    //query += "tvp_galleries_devices lo_pmgd,lu_devices ld where ld.id=lo_pmgd.device_id and lo_pmgd.is_active=1 and lo_pmgd.status=1)q2 on q2.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgc.tvp_gallery_id,lc.COUNTRY_NAME,lc.COUNTRY_CD2 from ";
            //    //query += "tvp_galleries_countries lo_pmgc,lu_countries lc where lc.id=lo_pmgc.country_id and lo_pmgc.is_active=1 and lo_pmgc.status=1)q3 on q3.tvp_gallery_id=q.id ";
            //    //query += "LEFT OUTER JOIN ";
            //    //query += "(select lo_pmgu.tvp_gallery_id,lo_pmgu.USER_STATE_ID from ";
            //    //query += "tvp_galleries_user_states lo_pmgu,lu_user_states lu where lu.id=lo_pmgu.USER_STATE_ID and lo_pmgu.is_active=1 and lo_pmgu.status=1)q4 on q4.tvp_gallery_id=q.id ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select tp.id,lp.DESCRIPTION from ";
            //    query += "tvp_profiles tp,lu_profile_types lp where lp.id=tp.PROFILE_TYPE and tp.is_active=1 and tp.status=1)q5 on q5.id=q.PROFILE_ID";
            //    query += ")top_q2 ";
            //    //query += "where (LANG_CODE is null) and (MAIN_LANG_CODE is null or LOCALE_LANG_CODE3 is null or MAIN_LANG_CODE=LOCALE_LANG_CODE3) ";
            //    query += ")q_all ";
            //    query += "order by SitePageID,location,main_location,order_num,inner_order_num";

            //}, result.InActivePageGalleries).Execute();

            //Fill Gallery Locales
            //new DatabaseDirectAdapter(delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
            //{
            //    query.SetConnectionString(connMng.GetClientConnectionString());
            //    query += "select q.*, q1.Language, q2.Device, q3.Country, q4.UserState from ";
            //    query += "((select tcgi.ID as GalleryID from ";
            //    query += "tvp_galleries tcgi where tcgi.STATUS=1)q";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select lo_pmgl.tvp_gallery_id,ll.Name as Language from   tvp_galleries_langs lo_pmgl,lu_languages ll where ll.id=lo_pmgl.language_id and ll.isUsed = 1 and lo_pmgl.status=1)q1 on q1.tvp_gallery_id=q.GalleryID ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select lo_pmgd.tvp_gallery_id,ld.DESCRIPTION as Device from   ";
            //    query += "tvp_galleries_devices lo_pmgd,lu_devices ld where ld.id=lo_pmgd.device_id and lo_pmgd.status=1)q2 on q2.tvp_gallery_id=q.GalleryID ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select lo_pmgc.tvp_gallery_id,lc.COUNTRY_NAME as Country from ";
            //    query += "tvp_galleries_countries lo_pmgc,lu_countries lc where lc.id=lo_pmgc.country_id and lo_pmgc.status=1)q3 on q3.tvp_gallery_id=q.GalleryID ";
            //    query += "LEFT OUTER JOIN ";
            //    query += "(select lo_pmgu.tvp_gallery_id,lo_pmgu.USER_STATE_ID as UserState from ";
            //    query += "tvp_galleries_user_states lo_pmgu,lu_user_states lu where lu.id=lo_pmgu.USER_STATE_ID and lo_pmgu.status=1)q4 on q4.tvp_gallery_id=q.GalleryID) ";
            //    query += "where q1.Language Is Not null or q2.Device Is not null or q3.Country is not null or q4.UserState is not null";
            //}, result.GalleryLocales).Execute();

            // Fill gallery buttons table
            //new DatabaseDirectAdapter(delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
            //{
            //    query.SetConnectionString(connMng.GetClientConnectionString());
            //    query += "select tgb.TVP_GALLERY_ID as GalleryID, tgbt.VALUE as Text, tgb.LINK as Link, tgb.ORDER_NUM as ItemOrder, tgb.BUTTON_TYPE as Type, ll.CULTURE as MainCulture  ";
            //    query += "from tvp_galleries_buttons tgb ,tvp_galleries_buttons_text tgbt, lu_languages ll where ";
            //    query += "tgbt.tvp_gallery_button_ID = tgb.ID and ll.ID = tgbt.LANGUAGE_ID and ";
            //    query += DatabaseHelper.AddCommonFields("tgb.STATUS", "tgb.IS_ACTIVE", eExecuteLocation.Application, false);
            //}, result.GalleryButtons).Execute();

            // Fill TVMAccounts table
            new DatabaseDirectAdapter(delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
            {
                query.SetConnectionString(connMng.GetClientConnectionString());
                query += "select q.*, q1.MediaType, q1.TvmTypeID from ";
                query += "(select tta.ID, tta.Name , tta.Player_UN , tta.Player_Pass, tta.Base_Group_ID, tta.Group_ID, tta.API_WS_USER, tta.API_WS_PASSWORD";
                query += "from tvp_tvm_accounts tta where ";
                query += DatabaseHelper.AddCommonFields("tta.STATUS", "tta.IS_ACTIVE", eExecuteLocation.Application, false);
                query += ")q";
                query += "LEFT OUTER JOIN";
                query += "(select mct.TVMAccountID, mct.Name as MediaType, mct.TvmTypeID";
                query += "from lu_MediasContentType mct)q1 on q1.TVMAccountID = q.ID";

            }, result.TVMAccounts).Execute();

            logger.InfoFormat("CreateSourceResult-> Finish retrieve data - {0}_{1}", GroupID, Platform);
            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{99EC1E4F-F140-441d-B43F-B1C46F473A36}"); }
        }

    }
}
