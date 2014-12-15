using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

/// <summary>
/// Summary description for DomainLastPositionResponse
/// </summary>
public class DomainLastPositionResponse
{

    public List<LastPosition> m_lPositions { get; set; }
    public string m_sDescription { get; set; }
    public string m_sStatus { get; set; }

}