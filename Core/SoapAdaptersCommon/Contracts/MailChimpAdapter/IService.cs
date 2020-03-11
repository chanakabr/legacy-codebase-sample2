using AdapaterCommon.Models;
using MailChimpAdapter.Models;
using System.Collections.Generic;
using System.ServiceModel;

namespace MailChimpAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(long adapterId, string settings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        AnnouncementListResponse Subscribe(long adapterId, string firstName, string lastName, string mailAddress, string token ,List<string> announcementExternalIds, long timeStamp, string signature);

        [OperationContract]
        AnnouncementListResponse UnSubscribe(long adapterId, string mailAddress, List<string> announcementExternalIds, long timeStamp, string signature);

        [OperationContract]
        AdapterStatus UpdateUser(long adapterId, long userId, string oldMailAddress, string newMailAddress, string firstName, string lastName, string token, List<string> announcementExternalIds, long timeStamp, string signature);

        [OperationContract]
        AdapterStatus Publish(long adapterId, string announcementExternalId, string templateId, string subject,  List<KeyValue> mergeKeyList, long timeStamp, string signature);

        [OperationContract]
        AnnouncementResponse CreateAnnouncement(long adapterId, string announcementName, long timeStamp, string signature);

        [OperationContract]
        AdapterStatus DeleteAnnouncement(long adapterId, string announcementExternalId, long timeStamp, string signature);
    }
}
