using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

/// <summary>
/// Summary description for MessageBox
/// </summary>

public class MessageQueueItem
{
    public DateTime CreateDate { get; set; }
    public MBMessage Message { get; set; }
}

class MessageBox
{
    private List<MessageQueueItem> m_messagesQueue = new List<MessageQueueItem>();

    private static ReaderWriterLockSlim m_MessageLocker = new ReaderWriterLockSlim();

    private static MessageBox m_instance;

    public static MessageBox Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new MessageBox();
            }

            return m_instance;
        }
    }

    public void Send(MBMessage message)
    {
        string sMessageID = System.Guid.NewGuid().ToString();

        if (m_MessageLocker.TryEnterWriteLock(200))
        {
            try
            {
                m_messagesQueue.Add(new MessageQueueItem() { CreateDate = DateTime.Now, Message = message });
            }
            finally
            {
                m_MessageLocker.ExitWriteLock();
            }
        }
    }

    public MBMessage GetNewMessage(string sRecieverUDID)
    {
        MBMessage msg = null;
        if (m_MessageLocker.TryEnterReadLock(200))
        {
            try
            {
                foreach (MessageQueueItem mqi in m_messagesQueue)
                {
                    if (mqi.Message.SendToUDID.ToLower().Equals(sRecieverUDID.ToLower()))
                    {
                        msg = mqi.Message;
                        m_messagesQueue.Remove(mqi);
                        break;
                    }
                }
            }
            finally
            {
                m_MessageLocker.ExitReadLock();
            }
        }

        return msg;
    }
}

[Serializable]
public class MBMessage
{
    public int MediaID { get; set; }
    public int MediaTypeID { get; set; }
    public string SiteGuid { get; set; }
    public int Location { get; set; }
    public string SendToUDID { get; set; }
    public string Action { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    //You should define an Empty Constructor to allow PokeIn to de-serialize
    public MBMessage()
    {
    }
}