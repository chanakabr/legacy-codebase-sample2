using Ingest.Clients.ClientManager;
using Ingest.Models;
using Ingest.Pricing;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ingest.Importers
{
    public class BusinessModulesImporter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string DATES_FORMAT = "dd/MM/yyyy hh:mm:ss";
        private const string REQUIRED_ERROR_FORMAT = "{0} with code '{1}': '{2}' is required.\n";
        private const string FORMAT_ERROR_FORMAT = "{0} with code '{1}': wrong format of '{2}'.\n";
        private const string MISSING_ATTRIBUTE_ERROR_FORMAT = "{0} with code '{1}': missing attribute '{2}' for '{3}'.\n";
        private const string INGEST_ERROR_FORMAT = "{0} with code '{1}': ingest failed - {2}.\n";
        private const string INGEST_SUCCESS_FORMAT = "{0} with code '{1}': ingest succeeded, ID = {2}.\n";
        
        private const string MULTI_PRICE_PLAN = "multi price plan";
        private const string PRICE_PLAN = "price plan";
        private const string PPV = "ppv";

        private const string REPORT_FILENAME_FORMAT = "{0}_{1}"; // timestamp_guid
        
        private static DateTime DEFAULT_END_DATE = DateTime.MaxValue;

        private static object lockObject = new object();
        private static string reportLogPath = TCMClient.Settings.Instance.GetValue<string>("business_modules_report_log_path"); 

        private delegate Ingest.Pricing.BusinessModuleResponse CallPricingIngest<T>(int groupId, T module) where T : IngestModule;

        public static BusinessModuleIngestResponse Ingest(int groupId, string xml)
        {
            BusinessModuleIngestResponse response = new BusinessModuleIngestResponse()
            {
                Status = new Ingest.Models.Status((int)StatusCodes.Error, StatusCodes.Error.ToString())
            };

            string report;

            if (string.IsNullOrEmpty(xml))
                return response;
            
            XmlDocument xmlDoc = new XmlDocument();

            // try to load xml
            try
            {
                xmlDoc.LoadXml(xml);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("failed to load price plans xml.", ex);
                report = string.Format("failed to load price plans xml with error {0}", ex.Message);
                WriteReportLogToFile(report, response.ReportFilename);
                return response;
            }

            // get filename
            string filename = DateTime.UtcNow.ToString("yyyyMMdd_HH-mm-ss-fff");
            var attribute = xmlDoc.FirstChild.Attributes["report_filename"];
            if (attribute != null && !string.IsNullOrEmpty(attribute.InnerText))
                filename = string.Format("{0}_{1}", attribute.InnerText, filename);

            response.ReportFilename = filename;

            ProccessPricePlans(groupId, xmlDoc, response.ReportFilename);

            ProccessMultiPricePlans(groupId, xmlDoc, response.ReportFilename);

            ProccessPPVs(groupId, xmlDoc, response.ReportFilename);

            response.Status = new Ingest.Models.Status((int)StatusCodes.OK, StatusCodes.OK.ToString());

            return response;
        }

        private static void ProccessMultiPricePlans(int groupId, XmlDocument xmlDoc, string reportFilename)
        {
            // parse multi price plans xml 
            List<IngestMultiPricePlan> multiPricePlans = ParseMultiPricePlansXml(xmlDoc, reportFilename);

            if (multiPricePlans != null && multiPricePlans.Count > 0)
            {
                // create tasks for calling ws 
                Task[] tasks = new Task[multiPricePlans.Count];

                // start tasks            
                for (int i = 0; i < multiPricePlans.Count; i++)
                {
                    int index = i;
                    tasks[i] = Task.Factory.StartNew(() =>
                        InsertModule<IngestMultiPricePlan>(groupId, multiPricePlans[index], CallPricingMultiPricePlanIngest<IngestMultiPricePlan>, reportFilename));
                }
                Task.WaitAll(tasks);
            }
        }

        private static void ProccessPricePlans(int groupId, XmlDocument xmlDoc, string reportFilename)
        {
            List<IngestPricePlan> pricePlans = ParsePricePlansXml(xmlDoc, reportFilename);

            // insert price plans if found
            if (pricePlans != null && pricePlans.Count > 0)
            {
                // create tasks for calling ws 
                Task[] tasks = new Task[pricePlans.Count];

                // start tasks
                for (int i = 0; i < pricePlans.Count; i++)
                {
                    int index = i;
                    tasks[i] = Task.Factory.StartNew(() =>
                        InsertModule<IngestPricePlan>(groupId, pricePlans[index], CallPricingPricePlanIngest<IngestPricePlan>, reportFilename));
                }
                Task.WaitAll(tasks);
            }
        }

        private static void ProccessPPVs(int groupId, XmlDocument xmlDoc, string reportFilename)
        {
            List<IngestPPV> ppvs = ParsePPVsXml(xmlDoc, reportFilename);

            // insert ppvs if found
            if (ppvs != null && ppvs.Count > 0)
            {
                // create tasks for calling ws 
                Task[] tasks = new Task[ppvs.Count];

                // start tasks
                for (int i = 0; i < ppvs.Count; i++)
                {
                    int index = i;
                    tasks[i] = Task.Factory.StartNew(() =>
                        InsertModule<IngestPPV>(groupId, ppvs[index], CallPricingPPVIngest<IngestPPV>, reportFilename));
                }
                Task.WaitAll(tasks);
            }
        }

        private static void WriteReportLogToFile(string report, string reportFilename)
        {
            if (string.IsNullOrEmpty(report))
                return;

            try
            {
                File.AppendAllText(string.Format("{0}/{1}", reportLogPath, reportFilename), report);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("failed to write report to file with path {0}", reportLogPath, ex);
            }
        }

        private static void InsertModule<T>(int groupId, T module, CallPricingIngest<T> callPricingIngest, string reportFilename) where T : IngestModule
        {
            string report = string.Empty;

            Ingest.Pricing.BusinessModuleResponse ingestResponse = null;

            try
            {
                ingestResponse = callPricingIngest(groupId, module);
            }
            catch (Exception ex)
            {
                log.ErrorFormat(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, "error while calling ws pricing", ex);
                report = string.Format(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, "error while calling ws pricing");
                return;
            }

            // prepare report

            if (ingestResponse == null && ingestResponse.status == null)
            {
                log.ErrorFormat(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, "failed to receive ws pricing response");
                report = string.Format(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, "failed to receive ws pricing response");
                return;
            }

            if (ingestResponse.status.Code != (int)StatusCodes.OK)
            {
                log.ErrorFormat(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, ingestResponse.status.Message);
                report = string.Format(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, ingestResponse.status.Message);
            }
            else
            {
                report = string.Format(INGEST_SUCCESS_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, ingestResponse.Id);
            }


            // write report to file
            if (!string.IsNullOrEmpty(report))
            {
                try
                {
                    lock (lockObject)
                    {
                        File.AppendAllText(string.Format("{0}/{1}", reportLogPath, reportFilename), report);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code,
                        string.Format("failed to write report to file with path {0}", reportLogPath), ex);
                }
            }
        }

        private static Ingest.Pricing.BusinessModuleResponse CallPricingPricePlanIngest<T>(int groupId, IngestPricePlan module)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            switch (module.Action)
            {
                case eIngestAction.Insert:
                    response = ClientsManager.PricingClient().InsertPricePlan(groupId, module);
                    break;
                case eIngestAction.Update:
                    response = ClientsManager.PricingClient().UpdatePricePlan(groupId, module);
                    break;
                case eIngestAction.Delete:
                    response = ClientsManager.PricingClient().DeletePricePlan(groupId, module.Code);
                    break;
                default:
                    break;
            }

            return response;
        }

        private static Ingest.Pricing.BusinessModuleResponse CallPricingMultiPricePlanIngest<T>(int groupId, IngestMultiPricePlan module)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            switch (module.Action)
            {
                case eIngestAction.Insert:
                    response = ClientsManager.PricingClient().InsertMultiPricePlan(groupId, module);
                    break;
                case eIngestAction.Update:
                    response = ClientsManager.PricingClient().UpdateMultiPricePlan(groupId, module);
                    break;
                case eIngestAction.Delete:
                    response = ClientsManager.PricingClient().DeleteMultiPricePlan(groupId, module.Code);
                    break;
                default:
                    break;
            }

            return response;
        }

        private static Ingest.Pricing.BusinessModuleResponse CallPricingPPVIngest<T>(int groupId, IngestPPV module)
        {
            Ingest.Pricing.BusinessModuleResponse response = null;

            switch (module.Action)
            {
                case eIngestAction.Insert:
                    response = ClientsManager.PricingClient().InsertPPV(groupId, module);
                    break;
                case eIngestAction.Update:
                    response = ClientsManager.PricingClient().UpdatePPV(groupId, module);
                    break;
                case eIngestAction.Delete:
                    response = ClientsManager.PricingClient().DeletePPV(groupId, module.Code);
                    break;
                default:
                    break;
            }

            return response;
        }

        private static List<IngestPricePlan> ParsePricePlansXml(XmlDocument doc, string reportFilename)
        {
            List<IngestPricePlan> pricePlans = null;

            StringBuilder reportBuilder = new StringBuilder();

            // get all price plans nodes
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/ingest/price_plans/price_plan");

            if (nodes != null)
            {
                pricePlans = new List<IngestPricePlan>();
                IngestPricePlan pricePlan = null;
                XmlNodeList nodeList;
                string strVal;
                eIngestAction actionVal;
                bool boolVal;

                foreach (XmlNode node in nodes)
                {
                    // parse each price plan
                    try
                    {
                        pricePlan = new IngestPricePlan();

                        //code - mandatory
                        if (GetMandatoryAttributeStrValue(node, "code", PRICE_PLAN, string.Empty, ref reportBuilder, out strVal))
                            pricePlan.Code = strVal;
                        else
                            continue;

                        // action - mandatory
                        if (GetMandatoryAttributeEnumValue<eIngestAction>(node, "action", PRICE_PLAN, pricePlan.Code, ref reportBuilder, out actionVal))
                            pricePlan.Action = actionVal;
                        else
                            continue;

                        // if action is delete - no need for further validations
                        if (pricePlan.Action == eIngestAction.Delete)
                        {
                            pricePlans.Add(pricePlan);
                            continue;
                        }

                        // is active - mandatory
                        if (GetMandatoryAttributeBoolValue(node, "is_active", PRICE_PLAN, pricePlan.Code, ref reportBuilder, out boolVal))
                            pricePlan.IsActive = boolVal;
                        else
                            continue;

                        // is renewable
                        if (GetNodeBoolValue(node, "is_renewable", PRICE_PLAN, pricePlan.Code, ref reportBuilder, out boolVal))
                            pricePlan.IsRenewable = boolVal;
                        else
                            continue;

                        // full life cycle 
                        nodeList = node.SelectNodes("full_life_cycle");
                        if (nodeList != null && nodeList.Count > 0)
                            pricePlan.FullLifeCycle = nodeList[0].InnerText;

                        // view life cycle
                        nodeList = node.SelectNodes("view_life_cycle");
                        if (nodeList != null && nodeList.Count > 0)
                            pricePlan.ViewLifeCycle = nodeList[0].InnerText;

                        // max views
                        nodeList = node.SelectNodes("max_views");
                        if (nodeList != null && nodeList.Count > 0)
                            pricePlan.MaxViews = nodeList[0].InnerText;

                        // price code
                        nodeList = node.SelectNodes("price_code");
                        if (nodeList != null && nodeList.Count > 0)
                            pricePlan.PriceCode = nodeList[0].InnerText;

                        // recurring periods
                        nodeList = node.SelectNodes("recurring_periods");
                        if (nodeList != null && nodeList.Count > 0)
                            pricePlan.RecurringPeriods = nodeList[0].InnerText.ToLower();

                        // add price plan to response list
                        pricePlans.Add(pricePlan);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("error while parsing price plan xml: code = {0}", pricePlan.Code, ex);
                        reportBuilder.AppendFormat("error while parsing price plan xml: code = {0}, error = {1}",
                            !string.IsNullOrEmpty(pricePlan.Code) ? pricePlan.Code : string.Empty, ex.Message);
                    }
                }
            }

            WriteReportLogToFile(reportBuilder.ToString(), reportFilename);
            return pricePlans;
        }

        private static List<IngestMultiPricePlan> ParseMultiPricePlansXml(XmlDocument doc, string reportFilename)
        {
            List<IngestMultiPricePlan> multiPricePlans = null;

            StringBuilder reportBuilder = new StringBuilder();

            // get all price plans nodes
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/ingest/multi_price_plans/multi_price_plan");

            if (nodes != null)
            {
                multiPricePlans = new List<IngestMultiPricePlan>();
                IngestMultiPricePlan multiPricePlan = null;
                XmlNodeList nodeList;
                string strVal;
                eIngestAction actionVal;
                bool boolVal;
                KeyValuePair[] keyValArr;
                DateTime dateVal;
                int intVal;

                foreach (XmlNode node in nodes)
                {
                    // parse each price plan
                    try
                    {
                        multiPricePlan = new IngestMultiPricePlan();

                        //code - mandatory
                        if (GetMandatoryAttributeStrValue(node, "code", MULTI_PRICE_PLAN, string.Empty, ref reportBuilder, out strVal))
                            multiPricePlan.Code = strVal;
                        else
                            continue;

                        // action - mandatory
                        if (GetMandatoryAttributeEnumValue<eIngestAction>(node, "action", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, out actionVal))
                            multiPricePlan.Action = actionVal;
                        else
                            continue;

                        // if action is delete - no need for further validations
                        if (multiPricePlan.Action == eIngestAction.Delete)
                        {
                            multiPricePlans.Add(multiPricePlan);
                            continue;
                        }

                        // is active - mandatory
                        if (GetMandatoryAttributeBoolValue(node, "is_active", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, out boolVal))
                            multiPricePlan.IsActive = boolVal;
                        else
                            continue;

                        // title
                        if (GetNodeKeyValuePairsArrayValue(node, "titles/title", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, out keyValArr))
                            multiPricePlan.Titles = keyValArr;
                        else
                            continue;

                        // description
                        if (GetNodeKeyValuePairsArrayValue(node, "descriptions/description", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, out keyValArr))
                            multiPricePlan.Descriptions = keyValArr;
                        else
                            continue;


                        // start date
                        if (GetNodeDateTimeValue(node, "start_date", MULTI_PRICE_PLAN, multiPricePlan.Code, DateTime.UtcNow, ref reportBuilder, out dateVal))
                            multiPricePlan.StartDate = dateVal;
                        else
                            continue;

                        // end date
                        if (GetNodeDateTimeValue(node, "end_date", MULTI_PRICE_PLAN, multiPricePlan.Code, DEFAULT_END_DATE, ref reportBuilder, out dateVal))
                            multiPricePlan.EndDate = dateVal;
                        else
                            continue;

                        // internal discount - mandatory
                        if (GetMandatoryNodeStrValue(node, "internal_discount", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, out strVal))
                            multiPricePlan.InternalDiscount = strVal;
                        else
                            continue;

                        // is renewable
                        if (GetNodeBoolValue(node, "is_renewable", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, out boolVal))
                            multiPricePlan.IsRenewable = boolVal;
                        else
                            continue;

                        // grace period minutes 
                        if (GetNodeIntValue(node, "grace_period_minutes", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, out intVal))
                            multiPricePlan.GracePeriodMinutes = intVal;
                        else
                            continue;

                        // order number 
                        if (GetNodeIntValue(node, "order_number", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, out intVal))
                            multiPricePlan.OrderNumber = intVal;
                        else
                            continue;

                        // price plans
                        multiPricePlan.PricePlansCodes = GetNodeStringArray(node, "price_plan_codes/price_plan_code");

                        // channels
                        multiPricePlan.Channels = GetNodeStringArray(node, "channels/channel");

                        // file types
                        multiPricePlan.FileTypes= GetNodeStringArray(node, "file_types/file_type");

                        // coupon group - not supported
                        nodeList = node.SelectNodes("coupon_group");
                        if (nodeList != null && nodeList.Count > 0)
                            multiPricePlan.CouponGroup = nodeList[0].InnerText;

                        // product code
                        nodeList = node.SelectNodes("product_code");
                        if (nodeList != null && nodeList.Count > 0)
                            multiPricePlan.ProductCode = nodeList[0].InnerText;

                        // preview module - not supported
                        nodeList = node.SelectNodes("preview_module");
                        if (nodeList != null && nodeList.Count > 0)
                            multiPricePlan.PreviewModule = nodeList[0].InnerText;

                        // domain limitation module - not supported
                        nodeList = node.SelectNodes("domain_limitation_module");
                        if (nodeList != null && nodeList.Count > 0)
                            multiPricePlan.DomainLimitationModule = nodeList[0].InnerText;

                        // add multi price plan to response list
                        multiPricePlans.Add(multiPricePlan);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("error while parsing multi price plan xml: code = {0}", multiPricePlan.Code, ex);
                        reportBuilder.AppendFormat("error while parsing multi price plan xml: code = {0}, error = {1}",
                            !string.IsNullOrEmpty(multiPricePlan.Code) ? multiPricePlan.Code : string.Empty, ex.Message);
                    }
                }
            }

            WriteReportLogToFile(reportBuilder.ToString(), reportFilename);

            return multiPricePlans;
        }

        private static List<IngestPPV> ParsePPVsXml(XmlDocument doc, string reportFilename)
        {
            List<IngestPPV> ppvs = null;

            StringBuilder reportBuilder = new StringBuilder();

            // get all price plans nodes
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/ingest/ppvs/ppv");

            if (nodes != null)
            {
                ppvs = new List<IngestPPV>();
                IngestPPV ppv = null;
                XmlNodeList nodeList;
                string strVal;
                eIngestAction actionVal;
                bool boolVal;
                KeyValuePair[] keyValArr;

                foreach (XmlNode node in nodes)
                {
                    // parse each price plan
                    try
                    {
                        ppv = new IngestPPV();

                        //code - mandatory
                        if (GetMandatoryAttributeStrValue(node, "code", PPV, string.Empty, ref reportBuilder, out strVal))
                            ppv.Code = strVal;
                        else
                            continue;

                        // action - mandatory
                        if (GetMandatoryAttributeEnumValue<eIngestAction>(node, "action", PPV, ppv.Code, ref reportBuilder, out actionVal))
                            ppv.Action = actionVal;
                        else
                            continue;

                        // if action is delete - no need for further validations
                        if (ppv.Action == eIngestAction.Delete)
                        {
                            ppvs.Add(ppv);
                            continue;
                        }

                        // is active - mandatory
                        if (GetMandatoryAttributeBoolValue(node, "is_active", PPV, ppv.Code, ref reportBuilder, out boolVal))
                            ppv.IsActive = boolVal;
                        else
                            continue;

                        // usage module - mandatory
                        if (GetMandatoryNodeStrValue(node, "usage_module", PPV, ppv.Code, ref reportBuilder, out strVal))
                            ppv.UsageModule = strVal;
                        else
                            continue;

                        // title
                        if (GetNodeKeyValuePairsArrayValue(node, "descriptions/description", PPV, ppv.Code, ref reportBuilder, out keyValArr))
                            ppv.Descriptions = keyValArr;
                        else
                            continue;

                        // is renewable
                        if (GetNodeBoolValue(node, "subscription_only", PPV, ppv.Code, ref reportBuilder, out boolVal))
                            ppv.SubscriptionOnly = boolVal;
                        else
                            continue;

                        // is renewable
                        if (GetNodeBoolValue(node, "first_device_limitation", PPV, ppv.Code, ref reportBuilder, out boolVal))
                            ppv.FirstDeviceLimitation = boolVal;
                        else
                            continue;

                        // coupon group - not supported
                        nodeList = node.SelectNodes("coupon_group");
                        if (nodeList != null && nodeList.Count > 0)
                            ppv.CouponGroup = nodeList[0].InnerText;

                        // product code
                        nodeList = node.SelectNodes("product_code");
                        if (nodeList != null && nodeList.Count > 0)
                            ppv.ProductCode = nodeList[0].InnerText;

                        // discount
                        nodeList = node.SelectNodes("discount");
                        if (nodeList != null && nodeList.Count > 0)
                            ppv.Discount = nodeList[0].InnerText;

                        // price code
                        nodeList = node.SelectNodes("price_code");
                        if (nodeList != null && nodeList.Count > 0)
                            ppv.PriceCode = nodeList[0].InnerText;

                        // file types
                        ppv.FileTypes = GetNodeStringArray(node, "file_types/file_type");

                        // add multi price plan to response list
                        ppvs.Add(ppv);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("error while parsing ppv xml: code = {0}", ppv.Code, ex);
                        reportBuilder.AppendFormat("error while parsing ppv xml: code = {0}, error = {1}",
                            !string.IsNullOrEmpty(ppv.Code) ? ppv.Code : string.Empty, ex.Message);
                    }
                }
            }

            WriteReportLogToFile(reportBuilder.ToString(), reportFilename);

            return ppvs;
        }

        private static bool GetMandatoryAttributeStrValue(XmlNode node, string attributeName, string moduleName, string moduleCode, ref StringBuilder report, out string value)
        {
            value = string.Empty;
            var attribute = node.Attributes[attributeName];
            if (attribute != null)
                value = attribute.InnerText;
            else
            {
                log.ErrorFormat(REQUIRED_ERROR_FORMAT, moduleName, moduleCode, attributeName);
                report.AppendFormat(REQUIRED_ERROR_FORMAT, moduleName, moduleCode, attributeName);
                return false;
            }

            return true;
        }

        private static bool GetMandatoryAttributeEnumValue<T>(XmlNode node, string attributeName, string moduleName, string moduleCode, ref StringBuilder report, out T value) 
            where T : struct, IConvertible
        {
            value = default(T);

            var attribute = node.Attributes[attributeName];
            if (attribute != null && !string.IsNullOrEmpty(attribute.InnerText))
            {
                T enumVal;
                if (Enum.TryParse(attribute.InnerText, true, out enumVal))
                    value = enumVal;
                else
                {
                    log.ErrorFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName);
                    return false;
                }
            }
            else
            {
                log.ErrorFormat(MISSING_ATTRIBUTE_ERROR_FORMAT, moduleName, moduleCode, attributeName);
                report.AppendFormat(MISSING_ATTRIBUTE_ERROR_FORMAT, moduleName, moduleCode, attributeName);
                return false;
            }

            return true;
        }

        private static bool GetMandatoryAttributeBoolValue(XmlNode node, string attributeName, string moduleName, string moduleCode, ref StringBuilder report, out bool value)
        {
            value = false;

            var attribute = node.Attributes[attributeName];
            if (attribute != null && !string.IsNullOrEmpty(attribute.InnerText))
            {
                var strToParse = attribute.InnerText.ToLower();
                if (strToParse == "true")
                    value = true;
                else if (strToParse == "false")
                    value = false;
                else
                {
                    log.ErrorFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName);
                    return false;
                }
            }

            return true;
        }

        private static bool GetNodeKeyValuePairsArrayValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, out KeyValuePair[] value)
        {
            value = null;

            XmlAttribute attribute;

            var nodeList = node.SelectNodes(nodeName);
            if (nodeList != null && nodeList.Count > 0)
            {
                value = new KeyValuePair[nodeList.Count];
                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (string.IsNullOrEmpty(nodeList[i].InnerText))
                        return false;

                    if (nodeList[i].Attributes != null && (attribute = nodeList[i].Attributes["lang"]) != null && !string.IsNullOrEmpty(attribute.InnerText))
                    {

                        value[i] = new KeyValuePair() { key = attribute.InnerText, value = nodeList[i].InnerText };
                    }
                    else
                    {
                        log.ErrorFormat(MISSING_ATTRIBUTE_ERROR_FORMAT, moduleName, moduleCode, "lang", moduleName);
                        report.AppendFormat(MISSING_ATTRIBUTE_ERROR_FORMAT, moduleName, moduleCode, "lang", moduleName);
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool GetNodeDateTimeValue(XmlNode node, string nodeName, string moduleName, string moduleCode, DateTime defaultValue, ref StringBuilder report, out DateTime value)
        {
            value = defaultValue;

            DateTime date;

            var nodeList = node.SelectNodes(nodeName);
            if (nodeList != null && nodeList.Count > 0)
            {
                var strToParse = nodeList[0].InnerText;
                if (!string.IsNullOrEmpty(strToParse))
                {
                    if (DateTime.TryParseExact(strToParse, DATES_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        value = date;
                    }
                    else
                    {
                        log.ErrorFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName);
                        report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName);
                        return false;
                    }
                }
                else
                {
                    // set default value
                    value = defaultValue;
                }
            }

            return true;
        }

        private static bool GetMandatoryNodeStrValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, out string value)
        {
            value = string.Empty;

            var nodeList = node.SelectNodes(nodeName);
            if (nodeList != null && nodeList.Count > 0)
                value = nodeList[0].InnerText;

            if (string.IsNullOrEmpty(value))
            {
                log.ErrorFormat(REQUIRED_ERROR_FORMAT, moduleName, moduleCode, nodeName);
                report.AppendFormat(REQUIRED_ERROR_FORMAT, moduleName, moduleCode, nodeName);
                return false;
            }

            return true;
        }

        private static bool GetNodeBoolValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, out bool value)
        {
            value = false;

            var nodeList = node.SelectNodes(nodeName);
            if (nodeList != null && nodeList.Count > 0 && !string.IsNullOrEmpty(nodeList[0].InnerText))
            {
                var strToParse = nodeList[0].InnerText.ToLower();
                if (strToParse == "true")
                    value = true;
                else if (strToParse == "false")
                    value = false;
                else
                {
                    log.ErrorFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName);
                    return false;
                }
            }
            return true;
        }

        private static bool GetNodeIntValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, out int value)
        {
            value = 0;

            var nodeList = node.SelectNodes(nodeName);
            if (nodeList != null && nodeList.Count > 0 && !string.IsNullOrEmpty(nodeList[0].InnerText))
            {
                int minutes;
                if (int.TryParse(nodeList[0].InnerText, out minutes))
                    value = minutes;
                else
                {
                    log.ErrorFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName);
                    return false;
                }
            }

            return true;
        }

        private static string[] GetNodeStringArray(XmlNode node, string nodeName)
        {
            string[] response = null; 

            var nodeList = node.SelectNodes("price_plan_codes/price_plan_code");
            if (nodeList != null)
            {
                response = new string[nodeList.Count];
                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (!string.IsNullOrEmpty(nodeList[i].InnerText))
                        response[i] = nodeList[i].InnerText;
                }
            }

            return response;
        }
    }
}
