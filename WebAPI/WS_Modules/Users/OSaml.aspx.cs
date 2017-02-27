using System;
using System.Xml;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using KLogMonitor;
using Core.Users;
using Core.Users.Saml;
using ComponentSpace.SAML2.Protocols;
using ComponentSpace.SAML2.Profiles.SingleLogout;
using ComponentSpace.SAML2.Assertions;
//using ComponentSpace.SAML2;
//using ComponentSpace.SAML2.Configuration;
//using ComponentSpace.SAML2.Bindings;

namespace WS_Users
{
    public partial class OSaml : System.Web.UI.Page
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        SamlProviderObject prov;

        private SAML loadSamlConfig(int nGroupID)
        {
            try
            {
                SAML s = SamlConfig.Instance.GetSaml(nGroupID);
                return s;
            }
            catch (Exception ex)
            {
                log.Error("loadSamlConfig", ex);
                return null;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            SamlResponseObject samlLogOut = null;
            bool goRedirect = true;

            if (Request != null)
            {
                if (Request.Form["SAMLResponse"] != null && Request.Form["RelayState"] != null)
                {
                    //LogIn Response
                    // Get the provider details       
                    string sRelayState = OSamlUtils.DecodeFrom64(Request.Form["RelayState"].ToString());
                    string sProviderId = string.Empty;
                    string sScope = string.Empty;
                    string[] splitRelayState = sRelayState.Split(';');
                    if (splitRelayState.Length > 0)
                    {
                        sProviderId = splitRelayState[0];
                        if (splitRelayState.Length > 1)
                            sScope = splitRelayState[1];
                    }
                    int nProviderId = ODBCWrapper.Utils.GetIntSafeVal(sProviderId);
                    prov = OSamlUtils.Get_ProviderDetails(nProviderId);
                    SAML samlConfig = loadSamlConfig(prov.GroupID);

                    goRedirect = false;
                    string SamlResponse = OSamlUtils.DecodeSAMLResponse(Request.Form["SAMLResponse"].ToString());
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(SamlResponse);

                    SamlResponseObject samlResponse = OSamlUtils.VerifyUser(xDoc, samlConfig, prov);
                    samlResponse.Scope = sScope;
                    string sResponse = OSamlUtils.Redirect(samlResponse, prov.GroupID);
                    Response.Redirect(sResponse);
                }
                else if (Request.Params["SAMLResponse"] != null && Request.Params["RelayState"] != null)
                {
                    XmlDocument xDoclogOut;
                    try
                    {
                        // Get the provider details                               
                        string sRelayState = OSamlUtils.DecodeFrom64(Request.Params["RelayState"].ToString());
                        string sProviderId = string.Empty;
                        string sScope = string.Empty;
                        string[] splitRelayState = sRelayState.Split(';');
                        if (splitRelayState.Length > 0)
                        {
                            sProviderId = splitRelayState[0];
                            if (splitRelayState.Length > 1)
                                sScope = splitRelayState[1];
                        }

                        int nProviderId = ODBCWrapper.Utils.GetIntSafeVal(sProviderId);
                        prov = OSamlUtils.Get_ProviderDetails(nProviderId);
                        SAML samlConfig = loadSamlConfig(prov.GroupID);

                        goRedirect = false;
                        string logOutResponse = OSamlUtils.DecodeLogOutResponse(Request.Params["SAMLResponse"].ToString());
                        xDoclogOut = new XmlDocument();
                        xDoclogOut.LoadXml(logOutResponse);

                        SamlResponseObject samlResponse = OSamlUtils.VerifyLogOut(xDoclogOut, samlConfig, prov);
                        samlResponse.Scope = sScope;
                        string sResponse = OSamlUtils.Redirect(samlResponse, prov.GroupID);
                        Response.Redirect(sResponse, false);
                    }
                    catch (Exception ex)
                    {
                        log.Error("goDirect to login/logOut failed", ex);
                    }
                }
            }

            // Redirect to the target URL, with login/logout request
            if (goRedirect)
            {
                string coGuid = string.Empty;
                string guid = string.Empty;
                bool isLogOut = false;

                try
                {
                    Guid newGuid = Guid.NewGuid();
                    SamlProviderObject samlP = new SamlProviderObject();
                    SAML samlConfig = null;
                    if (Request.Params["ProviderID"] != null)
                    {
                        int nProviderID = ODBCWrapper.Utils.GetIntSafeVal(Request.Params["ProviderID"]);
                        samlP = OSamlUtils.Get_ProviderDetails(nProviderID);
                        samlConfig = loadSamlConfig(samlP.GroupID);
                    }
                    if (Request.Params["scope"] != null)
                    {
                        if (samlP != null)
                        {
                            samlP.Scope = Request.Params["scope"].ToString().Replace("\"", string.Empty);
                        }
                    }

                    if (Request.Params["t"] != null && Request.Params["t"].ToString().Replace("\"", string.Empty) == "lo")//logOut Request
                        isLogOut = true;

                    if (isLogOut)
                    {
                        samlLogOut = new SamlResponseObject();
                        if (Request.Params["AssertionID"] != null) // "Assertion_ID" from the login response
                        {
                            samlLogOut.Assertion_ID = Request.Params["AssertionID"].ToString().Replace("\"", string.Empty);
                        }

                        samlLogOut.Session_ID = Request.Params["SessionID"].ToString().Replace("\"", string.Empty); // Session_ID from the login response
                        samlLogOut.Name_ID = Request.Params["NameID"].ToString().Replace("\"", string.Empty); // Name_ID from the login response

                        LogoutAction(samlP, samlLogOut, samlConfig);
                    }
                    else // logIn Request
                    {
                        if (Request.Params["GuidID"] != null)
                        {
                            guid = Request.Params["GuidID"].ToString().Replace("\"", string.Empty);
                            newGuid = new Guid(guid);
                        }

                        LogInAction(newGuid, samlP, samlConfig);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("goDirect to login/logOut failed", ex);
                }
            }
        }

        private void LogInAction(Guid newGuid, SamlProviderObject samlP, SAML samlConfig)
        {
            #region - for future use
            /* ComponentSpace.SAML2.Protocols.AuthnRequest authnRequest = new ComponentSpace.SAML2.Protocols.AuthnRequest();
             try
             {
                 authnRequest.Destination = samlP.UrlCreds;
                 authnRequest.Issuer = new ComponentSpace.SAML2.Assertions.Issuer(samlP.Issuer);
                 authnRequest.ForceAuthn = false;
                 authnRequest.NameIDPolicy = new ComponentSpace.SAML2.Protocols.NameIDPolicy("urn:oasis:names:tc:SAML:2.0:nameid-format:transient", null, true);
                 authnRequest.ProtocolBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect";
                 authnRequest.AssertionConsumerServiceURL = "1";
             
                 // Serialize the authentication request to XML for transmission.
                 XmlElement authnRequestXml = authnRequest.ToXml();
               
                 // Sign the authentication request.
                 RSACryptoServiceProvider rsaPrivateKey = new RSACryptoServiceProvider();
                 rsaPrivateKey.FromXmlString(samlConfig.privateKey);

                 ComponentSpace.SAML2.Protocols.SAMLMessageSignature.Generate(authnRequestXml,rsaPrivateKey);
                  XmlElement authnRequestXml = CreateAuthnRequest();

                 // Create and cache the relay state so we remember which SP resource the user wishes 
                 // to access after SSO.
                 string spResourceURL = new Uri(samlP.UrlCreds).ToString();//, ResolveUrl(FormsAuthentication.GetRedirectUrl("", false))).ToString();
               
                 string relayState = OSamlUtils.EncodeTo64(string.Format("{0};{1}",samlP.ID.ToString() ,samlP.Scope));

                 // Send the authentication request to the identity provider over the selected binding.
                 string idpURL = string.Format("{0}?{1}={2}", samlP.UrlCreds, "binding", HttpUtility.UrlEncode("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect"));

                 string sSigAlg1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                 string encodeSigAlg1 = Server.UrlEncode(sSigAlg1);
                 ComponentSpace.SAML2.Profiles.SSOBrowser.ServiceProvider.SendAuthnRequestByHTTPRedirect(Response, samlP.UrlCreds, authnRequestXml, relayState, rsaPrivateKey);
             }
             catch (Exception ex)
             {
             }
         */
            #endregion

            //SAMLRequest=value&RelayState=value        
            StringBuilder fullTargetURL = new StringBuilder(samlP.UrlCreds);
            fullTargetURL.Append("?SAMLRequest=");

            StringBuilder targetURL = new StringBuilder(string.Empty);
            targetURL.Append("<samlp:AuthnRequest");
            targetURL.AppendFormat(" xmlns:{0}=\"{1}\"", Core.Users.Saml.Prefixes.SAMLP, Core.Users.Saml.NamespaceURIs.Protocol);
            targetURL.AppendFormat(" xmlns:{0}=\"{1}\"", Core.Users.Saml.Prefixes.SAML, Core.Users.Saml.NamespaceURIs.Assertion);
            targetURL.AppendFormat("{0}=\"{1}\"", " ID", newGuid.ToString());
            targetURL.Append(" Version=\"2.0\"");
            targetURL.AppendFormat("{0}=\"{1}\"", " IssueInstant", DateTime.UtcNow.AddDays(1.0).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
            targetURL.Append(" AssertionConsumerServiceIndex=\"1\"");
            targetURL.AppendFormat("{0}=\"{1}\"", " Destination", samlP.UrlCreds);
            targetURL.AppendFormat("{0}=\"{1}\"", " samlp:AssertionConsumerServiceURL", samlP.ReturnURL);
            targetURL.Append(" ProtocolBinding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect\">");
            targetURL.AppendFormat("{0}{1}{2}", " <saml:Issuer xmlns:saml=\"urn:oasis:names:tc:SAML:2.0:assertion\">", samlP.Issuer, " </saml:Issuer>");
            targetURL.Append(" <samlp:NameIDPolicy Format=\"urn:oasis:names:tc:SAML:2.0:nameid-format:transient\"");
            targetURL.Append(" AllowCreate=\"true\"/>");
            targetURL.Append(" </samlp:AuthnRequest>");
            byte[] signedData;
            String signatureBase64encodedString = string.Empty;
            //digitally sign an XML document
            if (samlConfig != null)
            {
                try
                {
                    signedData = OSamlUtils.SignatureRequest(samlConfig, targetURL, ref signatureBase64encodedString);
                }
                catch (Exception ex)
                {
                    log.Error("goDirect to login/logOut failed -  at create signature", ex);
                }
            }

            //parameter that will return 
            StringBuilder relayState = new StringBuilder(string.Empty);
            relayState.Append(OSamlUtils.EncodeTo64(string.Format("{0};{1}", samlP.ID.ToString(), samlP.Scope)));
            string encodeStringUrl = OSamlUtils.EncodeSAMLRequestParam(targetURL.ToString());
            string sSigAlg1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            string encodeSigAlg1 = Server.UrlEncode(sSigAlg1);

            fullTargetURL = fullTargetURL.AppendFormat("{0}&RelayState={1}&SigAlg={2}&Signature={3}", encodeStringUrl, Server.UrlEncode(relayState.ToString()), encodeSigAlg1, Server.UrlEncode(signatureBase64encodedString));
            try
            {
                Response.Redirect(fullTargetURL.ToString(), false);
            }
            catch (Exception ex)
            {
                log.Error("LogInAction", ex);
            }
        }

        private void LogoutAction(SamlProviderObject samlP, SamlResponseObject oSaml, SAML samlConfig)
        {
            try
            {
                LogoutRequest logoutRequest = new LogoutRequest();
                logoutRequest.Issuer = new Issuer(samlP.Issuer);
                logoutRequest.NameID = new NameID(oSaml.Name_ID);
                logoutRequest.Destination = samlP.LogOutURL;
                logoutRequest.ID = oSaml.Assertion_ID;
                logoutRequest.SessionIndexes.Add(new SessionIndex(oSaml.Session_ID));
                // Serialize the logout request to XML for transmission.
                XmlElement logoutRequestXml = logoutRequest.ToXml();

                // Send the logout request to the IdP over HTTP redirect.
                string logoutURL = samlP.LogOutURL;
                string privateKey = samlConfig.privateKey;

                RSACryptoServiceProvider rsaPrivateKey = new RSACryptoServiceProvider();
                rsaPrivateKey.FromXmlString(privateKey);
                StringBuilder relayState = new StringBuilder(string.Empty);
                relayState.Append(OSamlUtils.EncodeTo64(string.Format("{0};{1}", samlP.ID.ToString(), samlP.Scope)));

                SingleLogoutService.SendLogoutRequestByHTTPRedirect(Response, logoutURL, logoutRequestXml, relayState.ToString(), rsaPrivateKey);
            }
            catch (Exception ex)
            {
                log.Error("LogoutAction", ex);
            }
            #region oldCod
            /*
            //LogoutRequest
            StringBuilder fullTargetURL = new StringBuilder(samlP.LogOutURL);
            fullTargetURL.Append("?SAMLRequest=");
            //request     
            //samlP.ReturnURL = "http://liat-pc/Ws_users/OSaml.aspx";
            //samlP.Issuer = "http://liat-pc.tvinci.local";
            StringBuilder targetURL = new StringBuilder(string.Empty);
            targetURL.Append("<samlp:LogoutRequest ");
            targetURL.AppendFormat(" xmlns:{0}=\"{1}\"", Users.Saml.Prefixes.SAMLP, Users.Saml.NamespaceURIs.Protocol);
            targetURL.AppendFormat(" xmlns:{0}=\"{1}\"", Users.Saml.Prefixes.SAML, Users.Saml.NamespaceURIs.Assertion);
                                           
            targetURL.AppendFormat("{0}=\"{1}\"", " ID", oSaml.Assertion_ID);
            targetURL.Append(" Version=\"2.0\"");
            targetURL.AppendFormat("{0}=\"{1}\"", " IssueInstant", DateTime.UtcNow.AddDays(1.0).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"));
           // targetURL.Append(" Destination=\"https://auth.delanewi.nl:443/xs/IDPSloRedirect/metaAlias/web/idp\"");
            targetURL.AppendFormat(" Destination=\"{0}\"",  samlP.LogOutURL);
            targetURL.Append(" > ");
            targetURL.AppendFormat("{0}{1}{2}", "<saml:Issuer>", samlP.Issuer, "</saml:Issuer>");
            targetURL.AppendFormat("{0}=\"{1}\" {2}=\"{3}\" {4} {5} {6}", " <saml:NameID NameQualifier", samlConfig.entityID, "Format",
                "urn:oasis:names:tc:SAML:2.0:nameid-format:transient", ">", oSaml.Name_ID, "</saml:NameID>");
            targetURL.AppendFormat("{0}{1}{2}", "<samlp:SessionIndex>", oSaml.Session_ID, "</samlp:SessionIndex>");
            targetURL.AppendFormat("</samlp:LogoutRequest>");
            
            byte[] signedData;
            String signatureBase64encodedString = string.Empty;
            //digitally sign an XML document
            if (samlConfig != null)
            {
                try
                {
                  /*  string encode1 = OSamlUtils.EncodeSAMLRequestParam(targetURL.ToString());
                   // encode1 = Server.UrlEncode(encode1);
                    string sSigAlg = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
                    encode1 += "&SigAlg=" + Server.UrlEncode(sSigAlg);

                    string privateKey = samlConfig.privateKey;

                    RSACryptoServiceProvider rsaPrivateKey = new RSACryptoServiceProvider();
                    rsaPrivateKey.FromXmlString(privateKey);

                    // Create some bytes to be signed. 
                    ASCIIEncoding ByteConverter = new ASCIIEncoding();

                    byte[] dataBytes = ByteConverter.GetBytes(Server.UrlEncode("SAMLRequest=" + encode1));
                    // Create a buffer for the memory stream. 
                    byte[] buffer = new byte[dataBytes.Length];
                    // Create a MemoryStream.
                    MemoryStream mStream = new MemoryStream(buffer);

                    // Write the bytes to the stream and flush it.
                    mStream.Write(dataBytes, 0, dataBytes.Length);

                    mStream.Flush();
                    // Create a new instance of the RSACryptoServiceProvider class  
                    // and automatically create a new key-pair.
                    RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                    // Export the key information to an RSAParameters object. 
                    // You must pass true to export the private key for signing. 
                    // However, you do not need to export the private key 
                    // for verification.
                    RSAParameters Key = rsaPrivateKey.ExportParameters(true);

                    // Hash and sign the data. 
                    signedData = OSamlUtils.HashAndSignBytes(mStream, Key);

                    // Close the MemoryStream.
                    mStream.Close();
                    signatureBase64encodedString = Convert.ToBase64String(signedData);
                    */
            /*
                    signedData = OSamlUtils.SignatureRequest(samlConfig, targetURL, ref signatureBase64encodedString);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("{0}-{1}", "goDirect to login/logOut failed -  at create signature ", ex.Message));
                }
            }

            //parameter that will return 
            string encodeStringUrl = OSamlUtils.EncodeSAMLRequestParam(targetURL.ToString());
            string sSigAlg1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            string encodeSigAlg1 = Server.UrlEncode(sSigAlg1);
            
            StringBuilder relayState = new StringBuilder(string.Empty);
            relayState.Append(OSamlUtils.EncodeTo64(string.Format("{0};{1}", samlP.ID.ToString(), samlP.Scope)));

            fullTargetURL.AppendFormat("{0}&RelayState={1}&SigAlg={2}&Signature={3}", encodeStringUrl, Server.UrlEncode(relayState.ToString()), encodeSigAlg1, Server.UrlEncode(signatureBase64encodedString));

         // fullTargetURL.AppendFormat("{0}&RelayState={1}", encodeStringUrl, Server.UrlEncode(relayState.ToString()));

            try
            {
                Response.Redirect(fullTargetURL.ToString(), false);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("{0}-{1}", "LogoutAction ", ex.Message));
            }*/
            #endregion
        }
    }
}