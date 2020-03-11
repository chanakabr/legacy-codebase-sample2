using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using ApiObjects.Notification;

namespace Core.Notification
{
    /// <summary>
    /// A wrapper class for the MessageBox api(implemented by wcf service).
    /// </summary>
    public class MessageBoxServiceWrapper
    {

        #region Old singleton code
        //private MessageBox_WCF.IapiClient mMessageBoxWcfClient = null; 

        //private  MessageBoxServiceWrapper()
        //{
        //    try
        //    {
        //        CreateProxy();
        //    }
        //    catch (Exception ex)
        //    {
        //        CloseProxy();
        //    }
        //}

        //public static MessageBoxServiceWrapper Instance
        //{
        //    get
        //    {
        //        MessageBoxServiceWrapper retInstance = Nested.instance;
        //        return retInstance;
        //    }
        //}

        //private void CreateProxy()
        //{
        //    mMessageBoxWcfClient = new MessageBox_WCF.IapiClient();
        //    mMessageBoxWcfClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(string.Format("{0}?username=messagebox&password=Tvinci", mMessageBoxWcfClient.Endpoint.Address.Uri.OriginalString));
        //    mMessageBoxWcfClient.Open();
        //}
        //private void CloseProxy()
        //{
        //    if (mMessageBoxWcfClient != null)
        //    {
        //        mMessageBoxWcfClient.Abort();
        //        mMessageBoxWcfClient.Close();
        //    }
        //}

        //class Nested
        //{
        //    internal static readonly MessageBoxServiceWrapper instance = new MessageBoxServiceWrapper();
        //    // Explicit static constructor to tell C# compiler
        //    // not to mark type as beforefieldinit
        //    static Nested()
        //    {
        //    }
        //}
        #endregion
        
        //#region public Methods
        //public long AddMessage(NotificationMessage notificationMessage)
        //{
        //    long messageID = 0;
        //    MessageBox_WCF.eMessageType messageType;
        //    MessageBox_WCF.MessageAction[] actions; 
        //    string text;
        //    string title; 
        //    string appName; 
        //    string recipient;
        //    int nBadge = 0;
        //    DateTime publishDate;

        //    ParseNotificationMessage(notificationMessage, out messageType, out actions, out text, out title, out appName, out recipient, out publishDate, out nBadge);
        //    messageID =  AddMessage(messageType, actions, text, title, appName, recipient, publishDate, nBadge); 
        //    return messageID;
        //}
        
        //public long AddMessage(MessageBox_WCF.eMessageType messageType, MessageBox_WCF.MessageAction[] actions, string text, string title, string appName, string recipient, DateTime publishDate, int nBadge)
        //{
        //    MessageBox_WCF.MessageData messageData = CreateMessageData(messageType, actions, null, text, title, nBadge);
        //    long messageID = AddMessage(messageData, appName, recipient, publishDate);
        //    return messageID;
        //}

        //public long AddMessage(MessageBox_WCF.MessageData messageData, string appName, string recipient, DateTime publishDate)
        //{
        //    long messageID = 0;
        //    MessageBox_WCF.AddMessageRequest request = new MessageBox_WCF.AddMessageRequest();
        //    request.Data = messageData;
        //    request.AppName = appName;
        //    request.Recipient = recipient;
        //    request.PublishDate = publishDate;
        //    MessageBox_WCF.IapiClient messageBoxClient = null;

        //    try
        //    {
        //        messageBoxClient = new MessageBox_WCF.IapiClient();
        //        messageBoxClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(string.Format("{0}?username=messagebox&password=Tvinci", messageBoxClient.Endpoint.Address.Uri.OriginalString));
        //        //messageBoxClient.Endpoint.Address = new EndpointAddress("http://mbox.prd.tvincidns.com/api.svc?username=messagebox&password=Tvinci"); //Prod Link
        //        messageID = messageBoxClient.AddMessage(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        //string str = ex.Message; //TBD: Write to log
        //        #region Logging
        //        #endregion
        //    }
        //    finally
        //    {
        //        if (messageBoxClient != null)
        //        {
        //            messageBoxClient.Close();
        //            messageBoxClient = null;
        //        }
        //    }
        //    return messageID;
        //}
        //#endregion

        //#region private Methods
        //private void ParseNotificationMessage(NotificationMessage notificationMessage, out MessageBox_WCF.eMessageType messageType, out MessageBox_WCF.MessageAction[] actions, out string text, out string title, out string appName, out string recipient, out DateTime publishDate, out int nBadge)         
        //{
        //    messageType = GetMessageType(notificationMessage.Type);
        //    actions = null;
        //    if (notificationMessage.Actions != null && notificationMessage.Actions.Length > 0)
        //    {
        //        actions = new MessageBox_WCF.MessageAction[notificationMessage.Actions.Length];
        //        for(int i = 0; i < notificationMessage.Actions.Length; i++) 
        //        {
                 
        //            MessageBox_WCF.MessageAction action = new MessageBox_WCF.MessageAction();
        //            action.Text = notificationMessage.Actions[i].Text;
        //            action.Link =  notificationMessage.Actions[i].Link;
        //            actions[i] = action; 
        //        }
        //    }            
        //    text = notificationMessage.MessageText;
        //    title = notificationMessage.Title;
        //    appName = notificationMessage.AppName;
        //    recipient = notificationMessage.UdID; 
        //    publishDate = notificationMessage.PublishDate;
        //    nBadge = notificationMessage.nBadge;
        //}

        //private MessageBox_WCF.eMessageType GetMessageType(NotificationMessageType notificationMessageType)
        //{
        //    MessageBox_WCF.eMessageType messageType = default(MessageBox_WCF.eMessageType);
        //    switch (notificationMessageType)
        //    {
        //        case NotificationMessageType.Push:
        //            {
        //                messageType = MessageBox_WCF.eMessageType.Push;
        //                break;
        //            }
        //        case NotificationMessageType.Alert:
        //            {
        //                messageType = MessageBox_WCF.eMessageType.Alert;
        //                break;
        //            }
        //        case NotificationMessageType.Live:
        //            {
        //                messageType = MessageBox_WCF.eMessageType.Live;
        //                break;
        //            }
        //        default:
        //            {
        //                break;
        //            }
        //    }
        //    return messageType;
        //}

        //private NotificationInterface.MessageBox_WCF.MessageData CreateMessageData(MessageBox_WCF.eMessageType messageType, MessageBox_WCF.MessageAction[] actions, ExtensionDataObject extensionData, string text, string title, int nBadge)
        //{
        //    MessageBox_WCF.MessageData messageData = null;

        //    switch (messageType)
        //    {
        //        case MessageBox_WCF.eMessageType.Alert:
        //            {
        //                messageData = new MessageBox_WCF.AlertMessageData()
        //                {
        //                    Actions = actions,
        //                    ExtensionData = extensionData,
        //                    Text = text,
        //                    Title = title
        //                };
        //                break;
        //            }

        //        case MessageBox_WCF.eMessageType.Live:
        //            {
        //                messageData = new MessageBox_WCF.LiveMessageData()
        //                {
        //                    Actions = actions,
        //                    ExtensionData = extensionData,
        //                    Text = text
        //                };
        //                break;
        //            }

        //        case MessageBox_WCF.eMessageType.Push:
        //            {
        //                messageData = new MessageBox_WCF.PushMessageData()
        //                {
        //                    Actions = actions,
        //                    ExtensionData = extensionData,
        //                    Text = text,
        //                    Badge = nBadge
        //                };
        //                break;
        //            }

        //        default:
        //            {
        //                break;
        //            }
        //    }
        //    return messageData;
        //}
        //#endregion
    }
}
