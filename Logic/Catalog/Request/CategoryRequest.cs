using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using Core.Catalog.Response;
using KLogMonitor;
using TVinciShared;
using System.IO;


namespace Core.Catalog.Request
{
    /**************************************************************************
    * Get Category Channel List
    * return all the :
    * Channels related to a specific category id
    * ************************************************************************/
    [DataContract]
    public class CategoryRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public int m_nCategoryID;

        public CategoryRequest()
            : base()
        {
        }

        public CategoryRequest(Int32 nCategoryID, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nCategoryID = nCategoryID;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            CategoryResponse response = new CategoryResponse();

            try
            {
                CategoryRequest request = (CategoryRequest)oBaseRequest;

                if (request == null || request.m_oFilter == null)
                    throw new Exception("request object is null or required variable is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures do not match");

                //Get Channels For CategoryID
                int nLanguage = 0;
                if (request.m_oFilter != null)
                {
                    nLanguage = request.m_oFilter.m_nLanguage;
                }

                DataSet dsCatAndChannels = CatalogDAL.GetGroupCategoriesAndChannels(request.m_nGroupID, nLanguage);
                if (dsCatAndChannels != null && dsCatAndChannels.Tables.Count > 0)
                {
                    DataTable dtCat = dsCatAndChannels.Tables[0];
                    DataTable dtCatChan = dsCatAndChannels.Tables[1];
                    DataTable dtGroupLang = dsCatAndChannels.Tables[2];
                    DataTable dtCategoryData = dsCatAndChannels.Tables[3];
                    DataTable dtChannelData = dsCatAndChannels.Tables[4];

                    var catChannels = dtCatChan.AsEnumerable().Select(r => new
                    {
                        CategoryID = Utils.GetLongSafeVal(r, "CATEGORY_ID"),
                        ChannelID = Utils.GetLongSafeVal(r, "ID"),
                        Title = Utils.GetStrSafeVal(r, "NAME"),
                        Description = Utils.GetStrSafeVal(r, "DESCRIPTION"),
                        EditorRemarks = Utils.GetStrSafeVal(r, "EDITOR_REMARKS"),
                        LinearStartTime = r.Field<DateTime>("LINEAR_START_TIME")
                    }).Distinct()
                        .GroupBy(cc => cc.CategoryID)
                        .ToDictionary(cc => cc.Key, cc => cc.ToList());



                    Dictionary<long, List<Picture>> dChanPics = new Dictionary<long, List<Picture>>();

                    // use old/new image server
                    if (WS_Utils.IsGroupIDContainedInConfig(m_nGroupID, "USE_OLD_IMAGE_SERVER", ';'))
                    {

                        // Make category-pictures dictionary
                        dChanPics = dtCatChan.AsEnumerable()
                            .Select(r => new
                            {
                                ID = Utils.GetLongSafeVal(r, "ID"),
                                PicUrl = Utils.GetStrSafeVal(r, "PIC_URL"),
                                PicSize = Utils.GetStrSafeVal(r, "PIC_SIZE")
                            })
                            .Where(p => (!string.IsNullOrEmpty(p.PicUrl)))
                            .Distinct()
                            .GroupBy(c => c.ID)
                            .ToDictionary(c => c.Key, c => c.ToList()
                                .Select(cp => new Picture() { m_sURL = cp.PicUrl, m_sSize = (cp.PicSize == "0X0" ? "full" : cp.PicSize) }).ToList());
                    }
                    else
                    {
                        dChanPics = dtChannelData.AsEnumerable()
                            .Select(r => new
                            {
                                ID = Utils.GetLongSafeVal(r, "ID"),
                                ChannelId = Utils.GetLongSafeVal(r, "channel_id"),

                                // build image URL. 
                                // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>/width/<image_width>/height/<image_height>/quality/<image_quality>
                                // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10/width/432/height/230/quality/100
                                PicUrl = Utils.GetIntSafeVal(r, "WIDTH") == 0 ?
                                                                  ImageUtils.BuildImageUrl(m_nGroupID,
                                                                  Path.GetFileNameWithoutExtension(Utils.GetStrSafeVal(r, "BASE_URL")) + "_" + Utils.GetIntSafeVal(r, "RATIO_ID"),
                                                                  Utils.GetIntSafeVal(r, "VERSION"),
                                                                  0,
                                                                  0,
                                                                  100,
                                                                  true) :
                                                                  ImageUtils.BuildImageUrl(m_nGroupID,
                                                                  Path.GetFileNameWithoutExtension(Utils.GetStrSafeVal(r, "BASE_URL")) + "_" + Utils.GetIntSafeVal(r, "RATIO_ID"),
                                                                  Utils.GetIntSafeVal(r, "VERSION"),
                                                                  Utils.GetIntSafeVal(r, "WIDTH"),
                                                                  Utils.GetIntSafeVal(r, "HEIGHT"),
                                                                  100),
                                Version = Utils.GetIntSafeVal(r, "VERSION"),
                                Ratio = Utils.GetStrSafeVal(r, "RATIO"),
                                PicSize = Utils.GetStrSafeVal(r, "PIC_SIZE"),
                                Image_Id = Path.GetFileNameWithoutExtension(Utils.GetStrSafeVal(r, "BASE_URL")) + "_" + Utils.GetIntSafeVal(r, "RATIO_ID")
                            })
                            .Distinct()
                            .GroupBy(c => c.ChannelId)
                            .ToDictionary(c => c.Key, c => c.ToList()
                                .Select(cp => new Picture()
                                {
                                    m_sSize = (cp.PicSize == "0X0" ? "full" : cp.PicSize),
                                    m_sURL = cp.PicUrl,
                                    id = cp.Image_Id,
                                    version = cp.Version,
                                    ratio = cp.Ratio
                                }).ToList());
                    }



                    List<CategoryResponse> cats = dtCat.AsEnumerable().Select(r => new CategoryResponse()
                                                {
                                                    ID = Utils.GetIntSafeVal(r, "ID"),
                                                    m_sTitle = Utils.GetStrSafeVal(r, "NAME"),
                                                    m_nParentCategoryID = Utils.GetIntSafeVal(r, "PARENT_CATEGORY_ID"),
                                                    m_sCoGuid = Utils.GetStrSafeVal(r, "CO_GUID")
                                                })
                                                .GroupBy(c => c.ID)
                                                .Select(c => c.First())
                                                .ToList();

                    // use old/new image server
                    Dictionary<int, List<Picture>> dCatPics = new Dictionary<int, List<Picture>>();
                    if (WS_Utils.IsGroupIDContainedInConfig(m_nGroupID, "USE_OLD_IMAGE_SERVER", ';'))
                    {

                        // Make category-pictures dictionary
                        dCatPics = dtCat.AsEnumerable()
                            .Select(r => new
                            {
                                ID = Utils.GetIntSafeVal(r, "ID"),
                                PicUrl = Utils.GetStrSafeVal(r, "PIC_URL"),
                                PicSize = Utils.GetStrSafeVal(r, "PIC_SIZE")
                            })
                            .Where(p => (!string.IsNullOrEmpty(p.PicUrl)))
                            .Distinct()
                            .GroupBy(c => c.ID)
                            .ToDictionary(c => c.Key, c => c.ToList()
                                .Select(cp => new Picture() { m_sURL = cp.PicUrl, m_sSize = (cp.PicSize == "0X0" ? "full" : cp.PicSize) }).ToList());
                    }
                    else
                    {
                        // Make category-pictures dictionary
                        dCatPics = dtCategoryData.AsEnumerable()
                                .Select(r => new
                                {
                                    ID = Utils.GetLongSafeVal(r, "ID"),
                                    CategoryId = Utils.GetLongSafeVal(r, "category_id"),

                                    // build image URL. 
                                    // template: <image_server_url>/p/<partner_id>/entry_id/<image_id>/version/<image_version>/width/<image_width>/height/<image_height>/quality/<image_quality>
                                    // Example:  http://localhost/ImageServer/Service.svc/GetImage/p/215/entry_id/123/version/10/width/432/height/230/quality/100
                                    PicUrl = Utils.GetIntSafeVal(r, "WIDTH") == 0 ?
                                                                      ImageUtils.BuildImageUrl(m_nGroupID,
                                                                      Path.GetFileNameWithoutExtension(Utils.GetStrSafeVal(r, "BASE_URL")) + "_" + Utils.GetIntSafeVal(r, "RATIO_ID"),
                                                                      Utils.GetIntSafeVal(r, "VERSION"),
                                                                      0,
                                                                      0,
                                                                      100,
                                                                      true) :
                                                                      ImageUtils.BuildImageUrl(m_nGroupID,
                                                                      Path.GetFileNameWithoutExtension(Utils.GetStrSafeVal(r, "BASE_URL")) + "_" + Utils.GetIntSafeVal(r, "RATIO_ID"),
                                                                      Utils.GetIntSafeVal(r, "VERSION"),
                                                                      Utils.GetIntSafeVal(r, "WIDTH"),
                                                                      Utils.GetIntSafeVal(r, "HEIGHT"),
                                                                      100),
                                    Version = Utils.GetIntSafeVal(r, "VERSION"),
                                    Ratio = Utils.GetStrSafeVal(r, "RATIO"),
                                    PicSize = Utils.GetStrSafeVal(r, "PIC_SIZE"),
                                    Image_Id = Path.GetFileNameWithoutExtension(Utils.GetStrSafeVal(r, "BASE_URL")) + "_" + Utils.GetIntSafeVal(r, "RATIO_ID")
                                })
                            .Distinct()
                            .GroupBy(c => c.CategoryId)
                            .ToDictionary(c => (int)c.Key, c => c.ToList()
                                .Select(cp => new Picture()
                                {
                                    m_sSize = (cp.PicSize == "0X0" ? "full" : cp.PicSize),
                                    m_sURL = cp.PicUrl,
                                    id = cp.Image_Id,
                                    version = cp.Version,
                                    ratio = cp.Ratio
                                }).ToList());
                    }

                    // If requested category not found, return empty response
                    if (!cats.Any(c => c.ID == request.m_nCategoryID))
                    {
                        return response;
                    }

                    if (cats.Count > 0 && dCatPics.Count > 0)
                    {
                        cats.ForEach(c => c.m_lPics = dCatPics.ContainsKey(c.ID) ? dCatPics[c.ID] : null);
                    }



                    if (catChannels.Count > 0 && cats.Count > 0)
                    {
                        cats.ForEach(c =>
                        {
                            if (catChannels.ContainsKey(c.ID))
                            {
                                c.m_oChannels = catChannels[c.ID]
                                                .Select(cc => new channelObj()
                                                                {
                                                                    m_nChannelID = (int)cc.ChannelID,
                                                                    m_nGroupID = request.m_nGroupID,
                                                                    m_sDescription = cc.Description,
                                                                    m_sEditorRemarks = cc.EditorRemarks,
                                                                    m_sTitle = cc.Title,
                                                                    m_dLinearStartTime = Convert.ToDateTime(cc.LinearStartTime),
                                                                    m_lPic = dChanPics.ContainsKey(cc.ChannelID) ? dChanPics[cc.ChannelID] : null
                                                                })
                                                .ToList();
                            }
                        });
                    }

                    CategoryResponse cRoot = cats.FirstOrDefault(c => c.ID == request.m_nCategoryID);

                    if (cRoot != null)
                    {
                        cRoot.m_oChildCategories = FindTreeChildren(cats, cRoot.ID);
                    }

                    return cRoot;

                }

            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("msg:{0}, st:{1}", ex.Message, ex.StackTrace), ex);
                throw ex;
            }

            return (BaseResponse)response;
        }

        private List<CategoryResponse> FindTreeChildren(List<CategoryResponse> cats, int parentCategoryID)
        {
            List<CategoryResponse> lChildren = cats.Where(c => c.m_nParentCategoryID == parentCategoryID).ToList();

            foreach (var c in lChildren)
            {
                c.m_oChildCategories = FindTreeChildren(cats, c.ID);
            }

            return lChildren;
        }

        private channelObj GetChannelFromDbRow(DataRow chRow)
        {
            channelObj chObj = new channelObj();
            chObj.m_nChannelID = Utils.GetIntSafeVal(chRow, "id");
            chObj.m_nGroupID = Utils.GetIntSafeVal(chRow, "group_id");
            chObj.m_sDescription = Utils.GetStrSafeVal(chRow, "Description");
            chObj.m_sTitle = Utils.GetStrSafeVal(chRow, "title");
            chObj.m_sEditorRemarks = Utils.GetStrSafeVal(chRow, "EDITOR_REMARKS");

            if (!string.IsNullOrEmpty(chRow["LINEAR_START_TIME"].ToString()))
            {
                chObj.m_dLinearStartTime = System.Convert.ToDateTime(chRow["LINEAR_START_TIME"].ToString());
            }

            return chObj;
        }
    }

}
