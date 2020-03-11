using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for SubscriptionSet
/// </summary>
public class SubscriptionSet
{

    public long ID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool InList { get; set; }    

    public SubscriptionSet()
    {
        this.InList = false;        
    }

    public SubscriptionSet(SubscriptionSet set)
    {
        this.ID = set.ID;
        this.Title = set.Title;
        this.Description = set.Description;
        this.InList = set.InList;        
    }

}

public class SubscriptionSetWithOrder : SubscriptionSet
{
    public int OrderNum { get; set; }

    public SubscriptionSetWithOrder()
        : base()
    {
        this.OrderNum = 0;
    }

    public SubscriptionSetWithOrder(SubscriptionSet set)
        : base(set)
    {
        this.OrderNum = 0;
    }

    public SubscriptionSetWithOrder(SubscriptionSetWithOrder set)
        : base(set)
    {
        this.OrderNum = set.OrderNum;
    }

}