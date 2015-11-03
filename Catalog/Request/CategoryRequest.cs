using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using Catalog.Response;
using KLogMonitor;


namespace Catalog.Request
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

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
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

                    var catChannels = dtCatChan.AsEnumerable().Select(r => new
                    {
                        CategoryID = r.Field<long>("CATEGORY_ID"),
                        ChannelID = r.Field<long>("ID"),
                        Title = r.Field<string>("NAME"),
                        Description = r.Field<string>("DESCRIPTION"),
                        EditorRemarks = r.Field<string>("EDITOR_REMARKS"),
                        LinearStartTime = r.Field<DateTime>("LINEAR_START_TIME")
                        //,
                        //PicUrl = r.Field<string>("PIC_URL"),
                        //PicSize = r.Field<string>("PIC_SIZE")
                    }).Distinct()
                        .GroupBy(cc => cc.CategoryID)
                        .ToDictionary(cc => cc.Key, cc => cc.ToList());

                    // Make category-pictures dictionary
                    Dictionary<long, List<Picture>> dChanPics = dtCatChan.AsEnumerable()
                        .Select(r => new
                        {
                            ID = (long)r.Field<long>("ID"),
                            PicUrl = r.Field<string>("PIC_URL"),
                            PicSize = r.Field<string>("PIC_SIZE")
                        })
                        .Where(p => (!string.IsNullOrEmpty(p.PicUrl)))
                        .GroupBy(c => c.ID)
                        .ToDictionary(c => c.Key, c => c.ToList()
                            .Select(cp => new Picture() { m_sURL = cp.PicUrl, m_sSize = (cp.PicSize == "0X0" ? "full" : cp.PicSize) }).ToList());

                    List<CategoryResponse> cats = dtCat.AsEnumerable().Select(r => new CategoryResponse()
                                                {
                                                    ID = (int)r.Field<long>("ID"),
                                                    m_sTitle = r.Field<string>("NAME"),
                                                    //((nLanguage == 0 || nLanguage == groupLangID) ? 
                                                    //            r.Field<string>("CATEGORY_NAME") :
                                                    //            r.Field<string>("NAME")),
                                                    m_nParentCategoryID = (int)r.Field<long>("PARENT_CATEGORY_ID"),
                                                    m_sCoGuid = r.Field<string>("CO_GUID")
                                                })
                                                .GroupBy(c => c.ID)
                                                .Select(c => c.First())
                                                .ToList();

                    // Make category-pictures dictionary
                    Dictionary<int, List<Picture>> dCatPics = dtCat.AsEnumerable()
                        .Select(r => new
                        {
                            ID = (int)r.Field<long>("ID"),
                            PicUrl = r.Field<string>("PIC_URL"),
                            PicSize = r.Field<string>("PIC_SIZE")
                        })
                        .Where(p => (!string.IsNullOrEmpty(p.PicUrl)))
                        .GroupBy(c => c.ID)
                        .ToDictionary(c => c.Key, c => c.ToList()
                            .Select(cp => new Picture() { m_sURL = cp.PicUrl, m_sSize = (cp.PicSize == "0X0" ? "full" : cp.PicSize) }).ToList());


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
