using ApiObjects;
using ApiObjects.IngestBusinessModules;
using Ingest.Clients.ClientManager;
using Ingest.Models;
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

        private const string DATES_FORMAT = "dd/MM/yyyy HH:mm:ss";
        private const string MANDATORY_ERROR_FORMAT = "{0} with code '{1}': {3} failed - '{2}' is mandatory.\n";
        private const string FORMAT_ERROR_FORMAT = "{0} with code '{1}': {3} failed - wrong format of '{2}'.\n";
        private const string MISSING_ATTRIBUTE_ERROR_FORMAT = "{0} with code '{1}': {4} failed - missing attribute '{2}' for '{3}'.\n";
        private const string INGEST_ERROR_FORMAT = "{0} with code '{1}': {3} failed - {2}.\n";
        private const string INGEST_SUCCESS_FORMAT = "{0} with code '{1}': {3} succeeded, ID = {2}.\n";
        private const string LOG_MANDATORY_ERROR_FORMAT = "ingest report ID '{3}': {0} with code '{1}': {4} failed - '{2}' is mandatory.\n";
        private const string LOG_FORMAT_ERROR_FORMAT = "ingest report ID '{3}': {0} with code '{1}': {4} failed - wrong format of '{2}'.\n";
        private const string LOG_MISSING_ATTRIBUTE_ERROR_FORMAT = "ingest report ID '{4}': {0} with code '{1}': {5} failed - missing attribute '{2}' for '{3}'.\n";
        private const string LOG_INGEST_ERROR_FORMAT = "ingest report ID '{3}': {0} with code '{1}': {4} failed - {2}.\n";
        private const string LOG_INGEST_SUCCESS_FORMAT = "ingest report ID '{3}': {0} with code '{1}': {4} succeeded, ID = {2}.\n";
        
        private const string MULTI_PRICE_PLAN = "multi price plan";
        private const string PRICE_PLAN = "price plan";
        private const string PPV = "ppv";

        private const string REPORT_FILENAME_FORMAT = "{0}_{1}"; // timestamp_guid
        
        private static DateTime DEFAULT_END_DATE = DateTime.MaxValue;

        private static object lockObject = new object();
        private static string reportLogPath = TCMClient.Settings.Instance.GetValue<string>("business_modules_report_log_path"); 

        private delegate BusinessModuleResponse CallPricingIngest<T>(int groupId, T module) where T : IngestModule;

        public static BusinessModuleIngestResponse Ingest(int groupId, string xml)
        {
            BusinessModuleIngestResponse response = new BusinessModuleIngestResponse()
            {
                Status = new Ingest.Models.Status((int)StatusCodes.Error, StatusCodes.Error.ToString())
            };

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
                log.ErrorFormat("ingest report ID '{0}': failed to load price plans xml.", response.ReportId, ex);
                return response;
            }

            // get filename
            string filename = DateTime.UtcNow.ToString("yyyyMMdd_HH-mm-ss-fff");
            var attribute = xmlDoc.FirstChild.Attributes["id"];
            if (attribute != null && !string.IsNullOrEmpty(attribute.InnerText))
                filename = string.Format("{0}_{1}", attribute.InnerText, filename);

            response.ReportId = filename;

            ProccessPricePlans(groupId, xmlDoc, response.ReportId);

            ProccessMultiPricePlans(groupId, xmlDoc, response.ReportId);

            ProccessPPVs(groupId, xmlDoc, response.ReportId);

            response.Status = new Ingest.Models.Status((int)StatusCodes.OK, StatusCodes.OK.ToString());

            return response;
        }

        private static void ProccessMultiPricePlans(int groupId, XmlDocument xmlDoc, string reportId)
        {
            // parse multi price plans xml 
            List<IngestMultiPricePlan> multiPricePlans = ParseMultiPricePlansXml(xmlDoc, reportId, groupId);

            if (multiPricePlans != null && multiPricePlans.Count > 0)
            {
                // create tasks for calling ws 
                Task[] tasks = new Task[multiPricePlans.Count];

                // create context for logs
                KlogMonitorHelper.ContextData ctx = new KlogMonitorHelper.ContextData();

                // start tasks            
                for (int i = 0; i < multiPricePlans.Count; i++)
                {
                    int index = i;
                    tasks[i] = Task.Factory.StartNew(() =>
                        {
                            ctx.Load();
                            InsertModule<IngestMultiPricePlan>(groupId, multiPricePlans[index], CallPricingMultiPricePlanIngest<IngestMultiPricePlan>, reportId);
                        });
                }
                Task.WaitAll(tasks);
            }
        }

        private static void ProccessPricePlans(int groupId, XmlDocument xmlDoc, string reportId)
        {
            List<IngestPricePlan> pricePlans = ParsePricePlansXml(xmlDoc, reportId, groupId);

            // insert price plans if found
            if (pricePlans != null && pricePlans.Count > 0)
            {
                // create tasks for calling ws 
                Task[] tasks = new Task[pricePlans.Count];

                // create context for logs
                KlogMonitorHelper.ContextData ctx = new KlogMonitorHelper.ContextData();

                // start tasks
                for (int i = 0; i < pricePlans.Count; i++)
                {
                    int index = i;
                    tasks[i] = Task.Factory.StartNew(() =>
                        {
                            ctx.Load();
                            InsertModule<IngestPricePlan>(groupId, pricePlans[index], CallPricingPricePlanIngest<IngestPricePlan>, reportId);
                        });
                }
                Task.WaitAll(tasks);
            }
        }

        private static void ProccessPPVs(int groupId, XmlDocument xmlDoc, string reportId)
        {
            List<IngestPPV> ppvs = ParsePPVsXml(xmlDoc, reportId, groupId);

            // insert ppvs if found
            if (ppvs != null && ppvs.Count > 0)
            {
                // create tasks for calling ws 
                Task[] tasks = new Task[ppvs.Count];

                // create context for logs
                KlogMonitorHelper.ContextData ctx = new KlogMonitorHelper.ContextData();

                // start tasks
                for (int i = 0; i < ppvs.Count; i++)
                {
                    int index = i;
                    tasks[i] = Task.Factory.StartNew(() =>
                    {
                        ctx.Load();
                        InsertModule<IngestPPV>(groupId, ppvs[index], CallPricingPPVIngest<IngestPPV>, reportId);
                    });
                    
                }
                Task.WaitAll(tasks);
            }
        }

        private static void WriteReportLogToFile(string report, string reportId, int groupId)
        {
            if (string.IsNullOrEmpty(report))
                return;
            
            var reportFullPath = string.Format("{0}/{1}/{2}", reportLogPath, groupId, reportId);

            try
            {
                string directoryName = Path.GetDirectoryName(reportFullPath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.AppendAllText(reportFullPath, report);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ingest report ID '{1}': failed to write report to file with path {0}", reportFullPath, reportId, ex);
            }
        }

        private static void InsertModule<T>(int groupId, T module, CallPricingIngest<T> callPricingIngest, string reportId) where T : IngestModule
        {
            string report = string.Empty;
            string logMessage = string.Empty;
            bool success = false;

            BusinessModuleResponse ingestResponse = null;

            try
            {
                ingestResponse = callPricingIngest(groupId, module);
            }
            catch (Exception ex)
            {
                logMessage = string.Format(LOG_INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, "error while calling ws pricing", reportId, module.Action.ToString().ToLower(), ex);
                report = string.Format(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, "error while calling ws pricing", module.Action.ToString().ToLower());
                return;
            }

            // prepare report
            if (ingestResponse == null && ingestResponse.status == null)
            {
                logMessage = string.Format(LOG_INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, "failed to receive ws pricing response", reportId, module.Action.ToString().ToLower());
                report = string.Format(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, "failed to receive ws pricing response", module.Action.ToString().ToLower());
                return;
            }

            if (ingestResponse.status.Code != (int)StatusCodes.OK)
            {
                logMessage = string.Format(LOG_INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, ingestResponse.status.Message, reportId, module.Action.ToString().ToLower());
                report = string.Format(INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, ingestResponse.status.Message, module.Action.ToString().ToLower());
            }
            else
            {
                // success
                success = true;
                logMessage = string.Format(LOG_INGEST_SUCCESS_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, ingestResponse.Id, reportId, module.Action.ToString().ToLower());
                report = string.Format(INGEST_SUCCESS_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code, ingestResponse.Id, module.Action.ToString().ToLower());
            }


            // write report to file
            if (!string.IsNullOrEmpty(report))
            {
                lock (lockObject)
                {
                    var reportFullPath = string.Format("{0}/{1}/{2}", reportLogPath, groupId, reportId);

                    try
                    {
                        string directoryName = Path.GetDirectoryName(reportFullPath);
                        if (!Directory.Exists(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }

                        File.AppendAllText(reportFullPath, report);
                        
                        if (success)
                            log.Debug(logMessage);
                        else
                            log.Error(logMessage);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat(LOG_INGEST_ERROR_FORMAT, Utils.Utils.GetBusinessModuleName(module), module.Code,
                            string.Format("failed to write report to file with path {0}", reportFullPath), reportId, ex);
                    }
                }
            }
        }

        private static BusinessModuleResponse CallPricingPricePlanIngest<T>(int groupId, IngestPricePlan module)
        {
            BusinessModuleResponse response = null;

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

        private static BusinessModuleResponse CallPricingMultiPricePlanIngest<T>(int groupId, IngestMultiPricePlan module)
        {
            BusinessModuleResponse response = null;

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

        private static BusinessModuleResponse CallPricingPPVIngest<T>(int groupId, IngestPPV module)
        {
            BusinessModuleResponse response = null;

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
                    if (response != null && response.status != null && response.status.Code == (int)Models.StatusCodes.OK && response.Id > 0)
                    {
                        if (!ClientsManager.ApiClient().UpdateFreeFileTypeOfModule(groupId, response.Id))
                        {
                            log.Error(string.Format("Failed updating index for ppvModule: {0}, groupID: {1}", response.Id, groupId));
                        }
                    }
                    break;
                default:
                    break;
            }

            return response;
        }

        private static List<IngestPricePlan> ParsePricePlansXml(XmlDocument doc, string reportId, int groupId)
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
                double? dVal;
                eIngestAction actionVal;               
                bool? boolVal;
                int? intVal;

                foreach (XmlNode node in nodes)
                {
                    // parse each price plan
                    try
                    {
                        pricePlan = new IngestPricePlan();

                        //code - mandatory
                        if (GetMandatoryAttributeStrValue(node, "code", PRICE_PLAN, string.Empty, ref reportBuilder, reportId, "ingest", out strVal))
                            pricePlan.Code = strVal;
                        else
                            continue;

                        // action - mandatory
                        if (GetMandatoryAttributeEnumValue<eIngestAction>(node, "action", PRICE_PLAN, pricePlan.Code, ref reportBuilder, reportId, "ingest", out actionVal))
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
                        if (GetAttributeBoolValue(node, "is_active", PRICE_PLAN, pricePlan.Code, ref reportBuilder, reportId, pricePlan.Action.ToString().ToLower(), out boolVal))
                            pricePlan.IsActive = boolVal;
                        else
                            continue;

                        // is renewable
                        if (GetNodeBoolValue(node, "is_renewable", PRICE_PLAN, pricePlan.Code, ref reportBuilder, reportId, pricePlan.Action.ToString().ToLower(), out boolVal))
                            pricePlan.IsRenewable = boolVal;
                        else
                            continue;

                        // full life cycle - mandatory
                        if (GetNodeStrValue(node, "full_life_cycle", PRICE_PLAN, pricePlan.Code, ref reportBuilder, reportId, pricePlan.Action.ToString().ToLower(), out strVal))
                            pricePlan.FullLifeCycle = strVal;
                        else
                            continue;

                        // view life cycle - mandatory
                        if (GetNodeStrValue(node, "view_life_cycle", PRICE_PLAN, pricePlan.Code, ref reportBuilder, reportId, pricePlan.Action.ToString().ToLower(), out strVal))
                            pricePlan.ViewLifeCycle = strVal;
                        else
                            continue;
                        
                        // max views - mandatory
                        if (GetNodeIntValue(node, "max_views", PRICE_PLAN, pricePlan.Code, ref reportBuilder, reportId, pricePlan.Action.ToString().ToLower(), out intVal))
                            pricePlan.MaxViews = intVal;
                        else
                            continue;

                        // price code (price) - mandatory
                        if (GetPriceCodeNode(node, "price_code", PRICE_PLAN, pricePlan.Code, ref reportBuilder, reportId, pricePlan.Action.ToString().ToLower(), out dVal, out strVal))                        
                        {
                            if (dVal != null && !string.IsNullOrEmpty(strVal))
                            {
                                pricePlan.PriceCode = new IngestPriceCode();
                                pricePlan.PriceCode.Price = dVal;
                                pricePlan.PriceCode.Currency = strVal;
                            }
                        }
                        else
                            continue;
                        
                        // recurring periods - mandatory
                        if (GetNodeIntValue(node, "recurring_periods", PRICE_PLAN, pricePlan.Code, ref reportBuilder, reportId, pricePlan.Action.ToString().ToLower(), out intVal))
                            pricePlan.RecurringPeriods = intVal;
                        else
                            continue;

                        // discount 
                        nodeList = node.SelectNodes("discount");
                        if (nodeList != null && nodeList.Count > 0)
                            pricePlan.Discount = nodeList[0].InnerText;
                        else //discount filed is NOT provided
                            pricePlan.Discount = null;

                        // add price plan to response list
                        pricePlans.Add(pricePlan);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("ingest report ID '{1}': error while parsing price plan xml: code = {0}", pricePlan.Code, reportId, ex);
                        reportBuilder.AppendFormat("error while parsing price plan xml: code = {0}, error = {1}",
                            !string.IsNullOrEmpty(pricePlan.Code) ? pricePlan.Code : string.Empty, ex.Message);
                    }
                }
            }

            WriteReportLogToFile(reportBuilder.ToString(), reportId, groupId);
            return pricePlans;
        }



        private static List<IngestMultiPricePlan> ParseMultiPricePlansXml(XmlDocument doc, string reportId, int groupId)
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
                bool? boolVal;
                KeyValuePair[] keyValArr;
                DateTime? dateVal;
                int? intVal;

                foreach (XmlNode node in nodes)
                {
                    // parse each price plan
                    try
                    {
                        multiPricePlan = new IngestMultiPricePlan();

                        //code - mandatory
                        if (GetMandatoryAttributeStrValue(node, "code", MULTI_PRICE_PLAN, string.Empty, ref reportBuilder, reportId, "ingest", out strVal))
                            multiPricePlan.Code = strVal;
                        else
                            continue;

                        // action - mandatory
                        if (GetMandatoryAttributeEnumValue<eIngestAction>(node, "action", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, reportId, "ingest", out actionVal))
                            multiPricePlan.Action = actionVal;
                        else
                            continue;

                        // if action is delete - no need for further validations
                        if (multiPricePlan.Action == eIngestAction.Delete)
                        {
                            multiPricePlans.Add(multiPricePlan);
                            continue;
                        }
                       

                        // is active - mandatory for insert
                        if (GetAttributeBoolValue(node, "is_active", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out boolVal))
                                multiPricePlan.IsActive = boolVal;
                            else
                                continue;
                       
                        // title
                        if (GetNodeKeyValuePairsArrayValue(node, "titles/title", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out keyValArr))
                            multiPricePlan.Titles = keyValArr.ToList();
                        else
                            continue;

                        // description
                        if (GetNodeKeyValuePairsArrayValue(node, "descriptions/description", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out keyValArr))
                            multiPricePlan.Descriptions = keyValArr.ToList();
                        else
                            continue;


                        // start date
                        if (GetNodeDateTimeValue(node, "start_date", MULTI_PRICE_PLAN, multiPricePlan.Code, DateTime.UtcNow, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out dateVal))
                            multiPricePlan.StartDate = dateVal;
                        else
                            continue;

                        // end date
                        if (GetNodeDateTimeValue(node, "end_date", MULTI_PRICE_PLAN, multiPricePlan.Code, DEFAULT_END_DATE, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out dateVal))
                            multiPricePlan.EndDate = dateVal;
                        else
                            continue;

                        // internal discount - mandatory
                        if (GetNodeStrValue(node, "internal_discount", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out strVal))
                            multiPricePlan.InternalDiscount = strVal;
                        else
                            continue;

                        // is renewable
                        if (GetNodeBoolValue(node, "is_renewable", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out boolVal))
                            multiPricePlan.IsRenewable = boolVal;
                        else
                            continue;

                        // grace period minutes 
                        if (GetNodeIntValue(node, "grace_period_minutes", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out intVal))
                            multiPricePlan.GracePeriodMinutes = intVal;
                        else
                            continue;

                        // order number 
                        if (GetNodeIntValue(node, "order_number", MULTI_PRICE_PLAN, multiPricePlan.Code, ref reportBuilder, reportId, multiPricePlan.Action.ToString().ToLower(), out intVal))
                            multiPricePlan.OrderNumber = intVal;
                        else
                            continue;

                        // price plans
                        multiPricePlan.PricePlansCodes = GetNodeStringList(node, "price_plan_codes/price_plan_code");

                        // channels - mandatory
                        multiPricePlan.Channels = GetNodeStringList(node, "channels/channel");

                        if (multiPricePlan.Action == eIngestAction.Insert && (multiPricePlan.Channels == null || multiPricePlan.Channels.Count == 0))
                        {
                            log.ErrorFormat(LOG_MANDATORY_ERROR_FORMAT, MULTI_PRICE_PLAN, multiPricePlan.Code, "channels", reportId, multiPricePlan.Action.ToString().ToLower());
                            reportBuilder.AppendFormat(MANDATORY_ERROR_FORMAT, MULTI_PRICE_PLAN, multiPricePlan.Code, "channels", multiPricePlan.Action.ToString().ToLower());
                            continue;
                        }

                        // file types
                        multiPricePlan.FileTypes= GetNodeStringList(node, "file_types/file_type");

                        // coupon group - not supported
                        nodeList = node.SelectNodes("coupon_group");
                        if (nodeList != null && nodeList.Count > 0)
                            multiPricePlan.CouponGroup = nodeList[0].InnerText;
                        else
                            multiPricePlan.CouponGroup = null;

                        // product code
                        nodeList = node.SelectNodes("product_code");
                        if (nodeList != null && nodeList.Count > 0)
                            multiPricePlan.ProductCode = nodeList[0].InnerText;
                        else
                            multiPricePlan.ProductCode = null;

                        // preview module - not supported
                        nodeList = node.SelectNodes("preview_module");
                        if (nodeList != null && nodeList.Count > 0)
                            multiPricePlan.PreviewModule = nodeList[0].InnerText;
                        else
                            multiPricePlan.PreviewModule = null;

                        // domain limitation module - not supported
                        nodeList = node.SelectNodes("domain_limitation_module");
                        if (nodeList != null && nodeList.Count > 0)
                            multiPricePlan.DomainLimitationModule = nodeList[0].InnerText;
                        else
                            multiPricePlan.DomainLimitationModule = null;

                        // add multi price plan to response list
                        multiPricePlans.Add(multiPricePlan);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("ingest report ID '{1}': error while parsing multi price plan xml: code = {0}", multiPricePlan.Code, reportId, ex);
                        reportBuilder.AppendFormat("error while parsing multi price plan xml: code = {0}, error = {1}",
                            !string.IsNullOrEmpty(multiPricePlan.Code) ? multiPricePlan.Code : string.Empty, ex.Message);
                    }
                }
            }

            WriteReportLogToFile(reportBuilder.ToString(), reportId, groupId);

            return multiPricePlans;
        }

        private static List<IngestPPV> ParsePPVsXml(XmlDocument doc, string reportId, int groupId)
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
                double? dVal;
                eIngestAction actionVal;
                bool? boolVal;

                KeyValuePair[] keyValArr;

                foreach (XmlNode node in nodes)
                {
                    // parse each price plan
                    try
                    {
                        ppv = new IngestPPV();

                        //code - mandatory
                        if (GetMandatoryAttributeStrValue(node, "code", PPV, string.Empty, ref reportBuilder, reportId, "ingest", out strVal))
                            ppv.Code = strVal;
                        else
                            continue;

                        // action - mandatory
                        if (GetMandatoryAttributeEnumValue<eIngestAction>(node, "action", PPV, ppv.Code, ref reportBuilder, reportId, ppv.Action.ToString().ToLower(), out actionVal))
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
                        if (GetAttributeBoolValue(node, "is_active", PPV, ppv.Code, ref reportBuilder, reportId, ppv.Action.ToString().ToLower(), out boolVal))
                            ppv.IsActive = boolVal;
                        else
                            continue;

                        // usage module - mandatory
                        if (GetNodeStrValue(node, "usage_module", PPV, ppv.Code, ref reportBuilder, reportId, ppv.Action.ToString().ToLower(), out strVal))
                            ppv.UsageModule = strVal;
                        else
                            continue;

                        // title
                        if (GetNodeKeyValuePairsArrayValue(node, "descriptions/description", PPV, ppv.Code, ref reportBuilder, reportId, ppv.Action.ToString().ToLower(), out keyValArr))
                            ppv.Descriptions = keyValArr.ToList();
                        else
                            continue;

                        // SubscriptionOnly
                        if (GetNodeBoolValue(node, "subscription_only", PPV, ppv.Code, ref reportBuilder, reportId, ppv.Action.ToString().ToLower(), out boolVal))
                            ppv.SubscriptionOnly = boolVal;
                        else
                            continue;

                        // FirstDeviceLimitation
                        if (GetNodeBoolValue(node, "first_device_limitation", PPV, ppv.Code, ref reportBuilder, reportId, ppv.Action.ToString().ToLower(), out boolVal))
                            ppv.FirstDeviceLimitation = boolVal;
                        else
                            continue;
                                                
                        // price code (price) - mandatory
                        if (GetPriceCodeNode(node, "price_code", PRICE_PLAN, ppv.Code, ref reportBuilder, reportId, ppv.Action.ToString().ToLower(), out dVal, out strVal))
                        {
                            if (dVal != null && !string.IsNullOrEmpty(strVal))
                            {
                                ppv.PriceCode = new IngestPriceCode();
                                ppv.PriceCode.Price = dVal;
                                ppv.PriceCode.Currency = strVal;
                            }
                        }
                        else
                            continue;

                        // coupon group - not supported
                        nodeList = node.SelectNodes("coupon_group");
                        if (nodeList != null && nodeList.Count > 0)
                            ppv.CouponGroup = nodeList[0].InnerText;
                        else
                            ppv.CouponGroup = null;

                        // product code
                        nodeList = node.SelectNodes("product_code");
                        if (nodeList != null && nodeList.Count > 0)
                            ppv.ProductCode = nodeList[0].InnerText;
                        else
                            ppv.ProductCode = null;

                        // discount
                        nodeList = node.SelectNodes("discount");
                        if (nodeList != null && nodeList.Count > 0)
                            ppv.Discount = nodeList[0].InnerText;
                        else
                            ppv.Discount = null;

                        // file types
                        ppv.FileTypes = GetNodeStringList(node, "file_types/file_type");

                        // add ppv to response list
                        ppvs.Add(ppv);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("ingest report ID '{1}': error while parsing ppv xml: code = {0}", ppv.Code, reportId, ex);
                        reportBuilder.AppendFormat("error while parsing ppv xml: code = {0}, error = {1}",
                            !string.IsNullOrEmpty(ppv.Code) ? ppv.Code : string.Empty, ex.Message);
                    }
                }
            }

            WriteReportLogToFile(reportBuilder.ToString(), reportId, groupId);

            return ppvs;
        }

        private static bool GetMandatoryAttributeStrValue(XmlNode node, string attributeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out string value)
        {
            value = string.Empty;
            var attribute = node.Attributes[attributeName];
            if (attribute != null && !string.IsNullOrEmpty(attribute.InnerText))
                value = attribute.InnerText;
            else
            {
                log.ErrorFormat(LOG_MANDATORY_ERROR_FORMAT, moduleName, moduleCode, attributeName, reportId, action);
                report.AppendFormat(MANDATORY_ERROR_FORMAT, moduleName, moduleCode, attributeName, action);
                return false;
            }

            return true;
        }

        private static bool GetMandatoryAttributeEnumValue<T>(XmlNode node, string attributeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out T value) 
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
                    log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName, reportId, action);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName, action);
                    return false;
                }
            }
            else
            {
                log.ErrorFormat(LOG_MISSING_ATTRIBUTE_ERROR_FORMAT, moduleName, moduleCode, attributeName, moduleName, reportId, action);
                report.AppendFormat(MISSING_ATTRIBUTE_ERROR_FORMAT, moduleName, moduleCode, attributeName, moduleName, action);
                return false;
            }

            return true;
        }

        private static bool GetMandatoryAttributeBoolValue(XmlNode node, string attributeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out bool value)
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
                    log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName, reportId, action);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName, action);
                    return false;
                }
            }

            return true;
        }

        private static bool GetNodeKeyValuePairsArrayValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out KeyValuePair[] value)
        {
            value = null;

            XmlAttribute attribute;

            var nodeList = node.SelectNodes(nodeName);
            if (nodeList != null && nodeList.Count > 0)
            {
                Dictionary<string, string> results = new Dictionary<string, string>();

                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (string.IsNullOrEmpty(nodeList[i].InnerText))
                        continue;

                    if (nodeList[i].Attributes != null && (attribute = nodeList[i].Attributes["lang"]) != null && !string.IsNullOrEmpty(attribute.InnerText))
                    {
                        if (!results.ContainsKey(attribute.InnerText))
                            results.Add(attribute.InnerText, nodeList[i].InnerText);
                    }
                    else
                    {
                        log.ErrorFormat(LOG_MISSING_ATTRIBUTE_ERROR_FORMAT, moduleName, moduleCode, "lang", nodeName, reportId, action);
                        report.AppendFormat(MISSING_ATTRIBUTE_ERROR_FORMAT, moduleName, moduleCode, "lang", nodeName, action);
                        return false;
                    }
                }

                value = results.Select(r => new KeyValuePair() { key = r.Key, value = r.Value }).ToArray();
            }

            return true;
        }

        private static bool GetNodeDateTimeValue(XmlNode node, string nodeName, string moduleName, string moduleCode, DateTime defaultValue, ref StringBuilder report, string reportId, string action, 
           out DateTime? value)
        {
            value = defaultValue;

            DateTime date;

            var nodeList = node.SelectNodes(nodeName);
            if ((nodeList == null || nodeList.Count == 0) && eIngestAction.Update.ToString().ToLower() == action)
            {
                value = null;
            }

            else if (nodeList != null && nodeList.Count > 0)
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
                        log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                        report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
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

        private static bool GetMandatoryNodeStrValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out string value)
        {
            value = string.Empty;

            var nodeList = node.SelectNodes(nodeName);
            if (nodeList != null && nodeList.Count > 0)
                value = nodeList[0].InnerText;

            if (string.IsNullOrEmpty(value))
            {
                log.ErrorFormat(LOG_MANDATORY_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                report.AppendFormat(MANDATORY_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                return false;
            }

            return true;
        }

        private static bool GetNodeStrValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out string value)
        {
            value = string.Empty;

            var nodeList = node.SelectNodes(nodeName);
            if ((nodeList == null || nodeList.Count == 0) && eIngestAction.Update.ToString().ToLower() == action)
            {
                value = null;
            }
            else if (nodeList != null && nodeList.Count > 0)
            {
                value = nodeList[0].InnerText;

                if (string.IsNullOrEmpty(value))
                {
                    log.ErrorFormat(LOG_MANDATORY_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                    report.AppendFormat(MANDATORY_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                    return false;
                }
            }

            return true;
        }

        private static bool GetNodeBoolValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out bool? value)
        {
            value = false;

            var nodeList = node.SelectNodes(nodeName);
            if ((nodeList == null || nodeList.Count == 0) && eIngestAction.Update.ToString().ToLower() == action)
            {
                value = null;
            }
            else if (nodeList != null && nodeList.Count > 0 && !string.IsNullOrEmpty(nodeList[0].InnerText))
            {
                var strToParse = nodeList[0].InnerText.ToLower();
                if (strToParse == "true")
                    value = true;
                else if (strToParse == "false")
                    value = false;
                else
                {
                    log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                    return false;
                }
            }
            return true;
        }

        private static bool GetAttributeBoolValue(XmlNode node, string attributeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out bool? value)
        {
            value = false;

            var attribute = node.Attributes[attributeName];
            if (attribute == null && eIngestAction.Update.ToString().ToLower() == action)
            {
                value = null;
            }
            else if (attribute != null && !string.IsNullOrEmpty(attribute.InnerText))
            {
                var strToParse = attribute.InnerText.ToLower();
                if (strToParse == "true")
                    value = true;
                else if (strToParse == "false")
                    value = false;
                else
                {
                    log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName, reportId, action);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, attributeName, action);
                    return false;
                }
            }

            return true;
        }

        private static bool GetNodeIntValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out int? value)
        {
            value = 0;

            var nodeList = node.SelectNodes(nodeName);
            if ((nodeList == null || nodeList.Count == 0) && eIngestAction.Update.ToString().ToLower() == action)
            {
                value = null;
            }
            else if (nodeList != null && nodeList.Count > 0 && !string.IsNullOrEmpty(nodeList[0].InnerText))
            {
                int minutes;
                if (int.TryParse(nodeList[0].InnerText, out minutes))
                    value = minutes;
                else
                {
                    log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                    return false;
                }
            }

            return true;
        }

        private static bool GetMandatoryNodeIntValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out int value)
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
                    log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                    return false;
                }
            }
            else
            {
                log.ErrorFormat(LOG_MANDATORY_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                report.AppendFormat(MANDATORY_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                return false;
            }

            return true;
        }

        private static List<string> GetNodeStringList(XmlNode node, string nodeName)
        {
            List<string> response = null;

            var nodeList = node.SelectNodes(nodeName);
            if (nodeList != null)
            {
                response = new List<string>();
                for (int i = 0; i < nodeList.Count; i++)
                {
                    if (!string.IsNullOrEmpty(nodeList[i].InnerText))
                        response[i] = nodeList[i].InnerText;
                }

                response = response.Where(r => r != null).ToList();
            }

            return response;
        }

        private static bool GetNodeDoublePositiveValue(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out double? value)
        {
            value = 0;

            var nodeList = node.SelectNodes(nodeName);
            if ((nodeList == null || nodeList.Count == 0) && eIngestAction.Update.ToString().ToLower() == action)
            {
                value = null;
            }
            else if (nodeList != null && nodeList.Count > 0 && !string.IsNullOrEmpty(nodeList[0].InnerText))
            {
                double tempVal;
                if (double.TryParse(nodeList[0].InnerText, out tempVal) && tempVal > 0.0)
                {                    
                    value = tempVal;
                }
                else
                {
                    log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                    report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                    return false;
                }
            }
            else
            {
                log.ErrorFormat(LOG_MANDATORY_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                report.AppendFormat(MANDATORY_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                return false;
            }

            return true;
        }

        private static bool GetPriceCodeNode(XmlNode node, string nodeName, string moduleName, string moduleCode, ref StringBuilder report, string reportId, string action, out double? dVal, out string strVal)
        {
            XmlNodeList nodeList = node.SelectNodes("price_code");
            strVal = string.Empty;
            dVal = null;
            if (nodeList != null && nodeList.Count > 0)
            {
                if (!GetNodeDoublePositiveValue(node, "price_code/price", PRICE_PLAN, moduleCode, ref report, reportId, action, out dVal))
                    return false;

                // price code (currency) - mandatory
                if (!GetNodeStrValue(node, "price_code/currency", PRICE_PLAN, moduleCode, ref report, reportId, action, out strVal))
                    return false;
            }
            else if (action == eIngestAction.Insert.ToString().ToLower())
            {
                log.ErrorFormat(LOG_FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, reportId, action);
                report.AppendFormat(FORMAT_ERROR_FORMAT, moduleName, moduleCode, nodeName, action);
                return false;
            }
            return true;
        }
    }
}
