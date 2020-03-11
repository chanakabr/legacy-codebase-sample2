using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class EmailNotificationRequest : MailRequestObj
    {   
        public string m_sTagToFollow;
        public string m_sMediaName;     
        public string m_mediaPicURL;
        public string m_mediaType;
        public string m_content;
        public string m_mediaId;
        public List<TagPair> m_tagList;
        
        public string m_runTime;
        public string m_userFollowTags;
        public string m_catalogStartDate;
        public string m_startDate;       


        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();
            switch (this.m_eMailType)
            {
                case eMailTemplateType.Notification:
                    {
                        MCGlobalMergeVars tagToFollowVar = new MCGlobalMergeVars();
                        tagToFollowVar.name = "FOLLOWED_TAG";
                        tagToFollowVar.content = this.m_sTagToFollow;
                        retVal.Add(tagToFollowVar);

                        MCGlobalMergeVars MediaNameVar = new MCGlobalMergeVars();
                        MediaNameVar.name = "MEDIANAME";
                        MediaNameVar.content = this.m_sMediaName;
                        retVal.Add(MediaNameVar);

                        MCGlobalMergeVars firstNameVar = new MCGlobalMergeVars();
                        firstNameVar.name = "FIRSTNAME";
                        firstNameVar.content = this.m_sFirstName;
                        retVal.Add(firstNameVar);

                        MCGlobalMergeVars lastNameVar = new MCGlobalMergeVars();
                        lastNameVar.name = "LASTNAME";
                        lastNameVar.content = this.m_sLastName;
                        retVal.Add(lastNameVar);

                        MCGlobalMergeVars mediaPicURL = new MCGlobalMergeVars();
                        mediaPicURL.name = "MEDIAPIC";
                        mediaPicURL.content = this.m_mediaPicURL;
                        retVal.Add(mediaPicURL);

                        MCGlobalMergeVars mediaType = new MCGlobalMergeVars();
                        mediaType.name = "MEDIATYPE";
                        mediaType.content = this.m_mediaType;
                        retVal.Add(mediaType);

                        MCGlobalMergeVars content = new MCGlobalMergeVars();
                        content.name = "CONTENT";
                        content.content = this.m_content;
                        retVal.Add(content);

                        MCGlobalMergeVars mediaId = new MCGlobalMergeVars();
                        mediaId.name = "MEDIAID";
                        mediaId.content = this.m_mediaId;
                        retVal.Add(mediaId);

                        foreach (TagPair tag in m_tagList)
                        {
                            MCGlobalMergeVars seriesName = new MCGlobalMergeVars();
                            seriesName.name = tag.key.ToUpper().Replace(" ",string.Empty);
                            seriesName.content =  tag.value.ToUpper();
                            retVal.Add(seriesName);
                        }

                        MCGlobalMergeVars startDateVar = new MCGlobalMergeVars();
                        startDateVar.name = "STARTDATE";
                        startDateVar.content = this.m_startDate;
                        retVal.Add(startDateVar);

                        MCGlobalMergeVars catalogStartDateVar = new MCGlobalMergeVars();
                        catalogStartDateVar.name = "CATALOGSTARTDATE";
                        catalogStartDateVar.content = this.m_catalogStartDate;
                        retVal.Add(catalogStartDateVar);
                           

                        MCGlobalMergeVars userFollowVar = new MCGlobalMergeVars();
                        userFollowVar.name = "FOLLOWED_TAGS_ALL";
                        userFollowVar.content = this.m_userFollowTags;
                        retVal.Add(userFollowVar);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return retVal;
        }
    }

    public class TagPair
    {
        public string key;
        public string value;

    }
}
