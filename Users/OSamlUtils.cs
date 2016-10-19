using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using System.Data;
using DAL;
using System.Security.Cryptography;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography.Xml;
using Users.Saml;
using System.IO;
using System.Web;
using System.IO.Compression;
//using ComponentSpace.SAML2.Assertions;
using System.Xml.XPath;
using System.Collections;
using ComponentSpace.SAML2;
using ComponentSpace.SAML2.Assertions;
using ComponentSpace.SAML2.Protocols;
using ComponentSpace.SAML2.Metadata;
using KLogMonitor;
using System.Reflection;


namespace Users
{
    public static class OSamlUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static SamlResponseObject HandleCredentials(SamlCredentialObject credsObj, SamlProviderObject prov)
        {
            try
            {
                SamlResponseObject oResponseObject = null;
                DataTable dt = DAL.SSODal.IsUserExsits(credsObj.Customer_ID, prov.ID);
                if (dt != null && dt.DefaultView.Count > 0)// This User already Exists
                {
                    //fill ResponseObject
                    string sSiteGuid = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["user_site_guid"]); //TODO when return UID change the return value 
                    oResponseObject = UpdateUser(sSiteGuid, credsObj, prov.ID);
                }
                else //Create new user
                {
                    oResponseObject = AddNewUser(prov, credsObj);
                    log.Debug(string.Format("new User : tvinciID={0} ", oResponseObject.Tvinci_ID));
                }

                return oResponseObject;
            }
            catch (Exception ex)
            {
                log.Error("OSamlUtils.HandleCredentials", ex);
                return null;
            }
        }

        private static SamlResponseObject UpdateUser(string sSiteGuid, SamlCredentialObject credsObj, int providerID)
        {
            int update = DAL.SSODal.Update_UserOperator(credsObj.Customer_ID, providerID, sSiteGuid);
            return new SamlResponseObject
            {
                Provider_ID = providerID,
                Tvinci_ID = sSiteGuid,
                Customer_ID = credsObj.Customer_ID,
                Status = update > 0 ? "OK" : "Error",
                Error = update > 0 ? string.Empty : "Internal error in update users_operators",
                Domain_ID = GetDomainID(sSiteGuid)
            };
        }

        public static SamlResponseObject AddNewUser(SamlProviderObject prov, SamlCredentialObject credsObj)
        {
            int domain_id = 0;

            string sWSUserName = string.Empty;
            string sWSPassword = string.Empty;
            WS_Utils.GetWSUNPass(prov.GroupID, "SSOAddNewUSER", "users", "1.1.1.1", ref sWSUserName, ref sWSPassword);

            BaseUsers t = null;
            prov.GroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "AddNewUser", ref t);
            UserResponseObject wsRespObject = new UserResponseObject();
            if (prov.GroupID != 0 && t != null)
            {
                wsRespObject = t.AddNewUser(new UserBasicData() { m_sUserName = credsObj.Customer_ID, m_CoGuid = credsObj.Customer_ID }, new UserDynamicData(), Guid.NewGuid().ToString());
            }

            if (wsRespObject.m_RespStatus == ResponseStatus.OK || wsRespObject.m_RespStatus == ResponseStatus.UserExists)
            {
                string message = "success to create new user or user is exists ";
                if (wsRespObject.m_user != null)
                    message = string.Format(" success to create new user or user is exists  {0}", wsRespObject.m_user.m_sSiteGUID);
                log.Debug("SAML - " + message);
                if (wsRespObject.m_RespStatus != ResponseStatus.UserExists)
                {

                    Domain d = new Domain();
                    string Customer_ID = credsObj.Customer_ID + "'s Domain";
                    Customer_ID = Customer_ID.Substring(0, Math.Min(Customer_ID.Length, 255)); // column "name" in table domains declare with 255 chars .
                    d.CreateNewDomain(Customer_ID, string.Empty, prov.GroupID, int.Parse(wsRespObject.m_user.m_sSiteGUID), "");
                    domain_id = d.m_nDomainID;
                }
                else
                {
                    domain_id = wsRespObject.m_user.m_domianID;
                }
                log.Debug("SAML - " + string.Format("success to create new domain = {0}", domain_id));

                int update = SSODal.Create_UserOperator(credsObj.Customer_ID, prov.ID);

                return new SamlResponseObject
                {
                    Provider_ID = prov.ID,
                    Tvinci_ID = wsRespObject.m_user.m_sSiteGUID,
                    Customer_ID = credsObj.Customer_ID,
                    Status = update > 0 ? "OK" : "Error",
                    Error = update > 0 ? string.Empty : "Internal error: failed add to users_operators",
                    Domain_ID = domain_id
                };
            }
            else
            {
                return new SamlResponseObject
                {
                    Provider_ID = prov.ID,
                    Tvinci_ID = wsRespObject.m_user.m_sSiteGUID,
                    Customer_ID = credsObj.Customer_ID,
                    Status = "Error",
                    Error = "Internal Error: " + wsRespObject.m_RespStatus,
                    Domain_ID = domain_id
                };
            }

        }

        private static int GetDomainID(string sSiteGuid)
        {
            DataTable dt = SSODal.Get_DomainIDByUser(sSiteGuid);
            if (dt != null && dt.DefaultView.Count > 0)
            {
                return ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["domain_id"]);
            }
            return 0;
        }


        public static SamlProviderObject Get_ProviderDetails(string entityID)
        {
            return Get_Provider(entityID, null);
        }

        public static SamlProviderObject Get_ProviderDetails(int nProviderID)
        {
            return Get_Provider(null, nProviderID);
        }

        private static SamlProviderObject Get_Provider(string entityId, int? providerId)
        {
            SamlProviderObject oProviderObject = null;
            DataTable dt = DAL.SSODal.Get_ProviderDetails(entityId, providerId);
            if (dt != null)
            {
                if (dt.DefaultView.Count > 0) // This Provider Exists
                {
                    //fill SamlProviderObject
                    oProviderObject = new SamlProviderObject
                    {
                        ID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]),
                        GroupID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["GROUP_ID"]),
                        Name = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["NAME"]),
                        ClientId = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["Client_Id"]),
                        ClientSecret = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["Client_Secret"]),
                        UrlCreds = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["URL_CREDS"]),
                        LogOutURL = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["URL_LogOut"]),
                        ReturnURL = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["Consumer_Service_URL"]),
                        Issuer = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["Issuer"])
                    };
                }
            }
            return oProviderObject;
        }


        #region Encode/Decode Methods

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static string EncodeSAMLRequestParam(string saml)
        {
            byte[] samlByte = Encoding.UTF8.GetBytes(saml);
            using (var output = new MemoryStream())
            {
                using (var zip = new DeflateStream(output, CompressionMode.Compress))
                {
                    zip.Write(samlByte, 0, samlByte.Length);
                }
                var base64 = Convert.ToBase64String(output.ToArray());
                return HttpUtility.UrlEncode(base64);
            }
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
            return returnValue;
        }

        public static string DecodeSAMLResponse(string response)
        {
            var utf8 = Encoding.UTF8;
            var bytes = utf8.GetBytes(response);
            using (var output = new MemoryStream())
            {
                using (new DeflateStream(output, CompressionMode.Decompress))
                {
                    output.Write(bytes, 0, bytes.Length);
                }
                var base64 = utf8.GetString(output.ToArray());
                return utf8.GetString(Convert.FromBase64String(base64));
            }
        }

        public static string DecodeLogOutResponse(string input)
        {
            try
            {
                byte[] input3 = Convert.FromBase64String(input);
                using (MemoryStream inputStream = new MemoryStream(input3))
                {
                    using (DeflateStream gzip =
                      new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        using (StreamReader reader =
                          new StreamReader(gzip, System.Text.Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        private static XmlElement DecryptAssertion(XmlElement xmlElement, RSACryptoServiceProvider rsa)
        {
            EncryptedAssertion encryptedAssertion = new EncryptedAssertion(xmlElement);
            return encryptedAssertion.DecryptToXml(rsa);
        }

        //public static SamlResponseObject VerifyUser(ComponentSpace.SAML2.Protocols.SAMLResponse xSamlResponse, SAML samlConfig, SamlProviderObject prov)
        public static SamlResponseObject VerifyUser(XmlDocument xSamlResponse, SAML samlConfig, SamlProviderObject prov)
        {
            if (xSamlResponse == null)
                return null;

            string statusCode = "FAILED";
            string issuer = string.Empty;
            string coGuid = string.Empty;
            string coGuidStatus = string.Empty;
            string nameID = string.Empty;
            string sessionID = string.Empty;
            string assertionID = string.Empty;
            string inResponseTo = string.Empty;
            bool verified = false;
            SamlResponseObject samlResponse = null;
            string x509Verify = string.Empty;
            try
            {
                log.Debug("SAML - VerifyUser");
                #region Get All Values From Response

                ComponentSpace.SAML2.Protocols.SAMLResponse samlResponseObj = new ComponentSpace.SAML2.Protocols.SAMLResponse(xSamlResponse.DocumentElement);
                //Create the RSACryptoServiceProvider to decrypte the saml assertion response
                string privateKey = samlConfig.privateKey;
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(privateKey);

                //open the encrypte data 
                IList<ComponentSpace.SAML2.Assertions.EncryptedAssertion> encryptedAssertionList = samlResponseObj.GetEncryptedAssertions();
                XmlElement xmlSAMLAssertion = null;
                SAMLAssertion samlAssertion = null;

                log.Debug("SAML - start loop all the EncryptedAssertions() of samlResponse");
                foreach (ComponentSpace.SAML2.Assertions.EncryptedAssertion item in encryptedAssertionList)
                {
                    xmlSAMLAssertion = item.DecryptToXml(rsa);
                    samlAssertion = new SAMLAssertion(xmlSAMLAssertion);
                    if (samlAssertion.Issuer != null)
                        issuer = samlAssertion.Issuer.NameIdentifier;
                    if (samlAssertion.Subject != null && samlAssertion.Subject.NameID != null)
                        nameID = samlAssertion.Subject.NameID.NameIdentifier;
                    if (samlResponseObj.IsSuccess())
                        statusCode = "Success";
                    inResponseTo = samlResponseObj.InResponseTo;
                    assertionID = samlAssertion.ID;

                    foreach (AuthnStatement authnStatement in samlAssertion.GetAuthenticationStatements())
                    {
                        sessionID = authnStatement.SessionIndex;
                    }
                    //Access attribute statements
                    foreach (AttributeStatement attributeStatement in samlAssertion.GetAttributeStatements())
                    {
                        foreach (SAMLAttribute samlAttribute in attributeStatement.GetUnencryptedAttributes())
                        {
                            switch (samlAttribute.Name)
                            {
                                case "saml2Film1ID":
                                    foreach (AttributeValue attributeValue in samlAttribute.Values)
                                    {
                                        if (!string.IsNullOrEmpty(attributeValue.Type))
                                        {
                                            coGuid = ODBCWrapper.Utils.GetSafeStr(attributeValue.Data);
                                        }
                                    }
                                    break;
                                case "saml2Film1Status"://user status
                                    foreach (AttributeValue attributeValue in samlAttribute.Values)
                                    {
                                        if (!string.IsNullOrEmpty(attributeValue.Type))
                                        {
                                            coGuidStatus = ODBCWrapper.Utils.GetSafeStr(attributeValue.Data);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    log.Debug("SAML - Finish loop all the EncryptedAssertions() of samlResponse");
                }

                #region OLD CODE
                //XmlNode xNode = xSamlResponse.GetElementsByTagName("samlp:StatusCode")[0]; // StatusCode
                //if (xNode != null)
                //    statusCode = xNode.Attributes["Value"].Value;

                //xNode = xSamlResponse.GetElementsByTagName("saml:Issuer")[0]; //issuer
                //if (xNode != null)
                //    issuer = xNode.InnerText;  


                //xNode = xSamlResponse.GetElementsByTagName("samlp:Response")[0]; //InResponseTo
                // if (xNode != null)
                //     inResponseTo = xNode.Attributes["InResponseTo"].Value;

                // xNode = xSamlResponse.GetElementsByTagName("saml:NameID")[0]; //NameID
                // if (xNode != null)
                //     nameID = xNode.InnerText;

                //xNode = xSamlResponse.GetElementsByTagName("saml:AuthnStatement")[0]; //SessionIndex
                // if (xNode != null)
                //     sessionID = xNode.Attributes["SessionIndex"].Value;

                //xNode = xSamlResponse.GetElementsByTagName("saml:Assertion")[0]; //AssertionID 
                //if (xNode != null)
                //    assertionID = xNode.Attributes["ID"].Value;

                //if (xSamlResponse.GetElementsByTagName("saml:AttributeStatement") != null && xSamlResponse.GetElementsByTagName("saml:AttributeStatement")[0] != null)
                //{
                //    XmlNodeList attributeStatement = xSamlResponse.GetElementsByTagName("saml:AttributeStatement")[0].ChildNodes;
                //    foreach (XmlNode attribute in attributeStatement)
                //    {
                //        foreach (XmlAttribute att in attribute.Attributes)
                //        {
                //            switch (att.Value)
                //            {
                //                case "saml2Film1ID":
                //                    coGuid = attribute.InnerText;
                //                    break;
                //                case "saml2Film1Status"://user status
                //                    coGuidStatus = attribute.InnerText;
                //                    break;
                //                default:
                //                    break;
                //            }
                //        }
                //    }
                //}

                //Decrypt the SAML xml
                //string encrypteDataString = string.Empty;
                //xNode = xSamlResponse.GetElementsByTagName("xenc:EncryptedData")[0];
                //XmlNode encryptedDataNode = xSamlResponse.GetElementsByTagName("saml:EncryptedAssertion")[0];

                //if (xNode != null)
                //{
                //    XmlNodeList attributeEncryptedData = xNode.ChildNodes;
                //    foreach (XmlNode encryptedData in attributeEncryptedData)
                //    {
                //        if (encryptedData.Name.ToLower().Equals("xenc:cipherdata"))
                //        {
                //            XmlNodeList attributeCipherData = xNode.ChildNodes;
                //            foreach (XmlNode cipherData in attributeCipherData)
                //            {
                //                if (cipherData.Name.ToLower().Equals("xenc:cipherdata"))
                //                {

                //                    encrypteDataString = cipherData.InnerText;
                //                    /*
                //                    XmlNodeList cipherValue = cipherData.ChildNodes;//.GetElementsByTagName("xenc:CipherValue")[0];
                //                    foreach (XmlNode cipherValueNode in cipherValue)
                //                    {
                //                        if (cipherData.Name.ToLower().Equals("xenc:ciphervalue"))

                //                            encrypteDataString = cipherValueNode.InnerText;
                //                    }
                //                     * */
                //                }
                //            }
                //        }
                //    }
                //}

                // The SAML response contains a signed SAML assertion
                // XmlElement samlAssertionElement = samlResponseObj.Signature.GetElementsByTagName("ds:X509Certificate")[0] as XmlElement;
                //  xSamlResponse.GetSignedAssertions()[0];

                //Verify the SAML assertion signature
                //result = ComponentSpace.SAML2.Protocols.SAMLMessageSignature.Verify(samlAssertionElement);

                //   XmlElement decryptedElement = encryptedAssertionNode.DecryptToXml(x509Certificate, null);


                //   IList<ComponentSpace.SAML2.Assertions.AttributeStatement> asset = samlAssertion.GetAttributeStatements();//    xSamlResponse



                //Get response Private Key 
                /*  XmlDocument encryptedXmlDoc = new XmlDocument();
                  encryptedXmlDoc.LoadXml(xSamlResponse.InnerXml);//encryptedDataNode.InnerXml); // get all the xenc:EncryptedData
                  EncryptedXml exml = new EncryptedXml(encryptedXmlDoc);
                 string keyName = string.Empty;

                 exml.AddKeyNameMapping(keyName, rsa);
                 exml.DecryptDocument();
                 */


                /*   if (!string.IsNullOrEmpty(encrypteDataString))
                       {
                           try
                           {
                               string sm = DecryptString(encrypteDataString);

                       



                               XmlDocument docD = new XmlDocument();
                     


                              // string ggg = Decryption(encrypteDataString, privateKey);

                       

                    
                               encrypteDataString = HttpUtility.UrlDecode(encrypteDataString);
                               byte[] bytesToDecrypt2 = rsa.Decrypt(Convert.FromBase64String(encrypteDataString), false);
                             //  byte[] bytesToDecrypt2 = rsa.Decrypt(HttpUtility.UrlDecodeToBytes(encrypteDataString), false);
                               ASCIIEncoding ByteConverter = new ASCIIEncoding();
                               string s = ByteConverter.GetString(bytesToDecrypt2);


                       
                               List<byte[]> bytesList = splitByteArray(encrypteDataString);
                               foreach (byte[] bytesItem in bytesList)
                               {
                             
                                   byte[] bytesToDecrypt2 = rsa.Decrypt(bytesItem, false);
                               }


                               string ggg = DecryptString(encrypteDataString, privateKey.Length, privateKey);
                       
                           }
                           catch (Exception ex)
                           {

                           }


                       }
                      */
                #endregion
                #endregion

                bool result = true; ;
                if (coGuidStatus.ToLower() != "active")
                {
                    result = false;
                    //"The user is not Active;
                    samlResponse = new SamlResponseObject();
                    samlResponse.Error = "user is not Active";
                    samlResponse.Status = "Error";
                    samlResponse.Customer_ID = coGuid;
                    samlResponse.Guid_ID = inResponseTo;
                    samlResponse.Name_ID = nameID;
                    samlResponse.Session_ID = sessionID;
                    samlResponse.Assertion_ID = assertionID;
                    samlResponse.Scope = string.Empty;
                }
                else
                {
                    // Verifies XML digital signatures                  
                    verified = VerifySignature(samlResponseObj, rsa);
                    log.Debug("SAML - " + string.Format("VerifySignature = {0} ", verified));
                    // The SAML response contains a signed SAML assertion
                    XmlElement samlAssertionElement = samlResponseObj.Signature.GetElementsByTagName("ds:X509Certificate")[0] as XmlElement;

                    //check that both certification equel
                    if (samlConfig.x509.Trim() != samlAssertionElement.InnerText.Trim())
                    {
                        result = false;
                    }
                    log.Debug("SAML - " + string.Format("x509 equel = {0} ", result));

                    if (!result || !verified)
                        result = false;

                    if (result)
                    {
                        //return to the client with id user 
                        if (!string.IsNullOrEmpty(statusCode) && statusCode.ToLower().Contains("success")
                        && !string.IsNullOrEmpty(issuer) && issuer.Contains(samlConfig.entityID)
                        && !string.IsNullOrEmpty(coGuid))
                        {
                            log.Debug("SAML - " + string.Format("user is ok to logIn coGuid = {0}", coGuid));
                            SamlCredentialObject credsObj = new SamlCredentialObject();
                            credsObj.Customer_ID = coGuid;
                            samlResponse = OSamlUtils.HandleCredentials(credsObj, prov);
                            samlResponse.Status = "OK";
                            samlResponse.Guid_ID = inResponseTo;
                            samlResponse.Name_ID = nameID;
                            samlResponse.Session_ID = sessionID;
                            samlResponse.Assertion_ID = assertionID;
                            samlResponse.Scope = string.Empty;
                        }
                    }
                    else
                    {
                        log.Debug("SAML - " + string.Format("The XML signature is not valid coGuid = {0}", coGuid));
                        //"The XML signature is not valid.");
                        samlResponse = new SamlResponseObject();
                        samlResponse.Error = "The XML signature is not valid";
                        samlResponse.Status = "Error";
                        samlResponse.Customer_ID = coGuid;
                        samlResponse.Guid_ID = inResponseTo;
                        samlResponse.Name_ID = nameID;
                        samlResponse.Session_ID = sessionID;
                        samlResponse.Assertion_ID = assertionID;
                        samlResponse.Scope = string.Empty;
                    }
                }
                return samlResponse;
            }
            catch (Exception ex)
            {
                log.Error("VerifyUser", ex);
                samlResponse = new SamlResponseObject();
                samlResponse.Status = "Error";
                samlResponse.Error = ex.Message;
                return samlResponse;
            }
        }

        // received the LogOut Response and verify it 
        public static SamlResponseObject VerifyLogOut(XmlDocument xSamlResponse, SAML samlConfig, SamlProviderObject prov)
        {
            string statusCode = string.Empty;
            string inResponseTo = string.Empty;
            SamlResponseObject samlResponse = null;
            try
            {
                if (xSamlResponse != null)
                {
                    ComponentSpace.SAML2.Protocols.LogoutResponse samlResponseObj = new ComponentSpace.SAML2.Protocols.LogoutResponse(xSamlResponse.DocumentElement);
                    samlResponse = new SamlResponseObject();
                    samlResponse.Status = "Error";

                    if (samlResponseObj.Status != null)
                    {
                        if (samlResponseObj.Status.StatusCode.Code.ToLower().Contains("success"))
                            samlResponse.Status = "LogoutOK";
                        if (samlResponseObj.Status.StatusMessage != null)
                            samlResponse.Error = samlResponseObj.Status.StatusMessage.Message;
                    }
                    inResponseTo = samlResponseObj.InResponseTo;
                    samlResponse.Assertion_ID = inResponseTo;
                    samlResponse.Provider_ID = prov.ID;
                    log.Debug("SAML - " + string.Format("LogOutResponse  Status={0}, inResponseTo={1}", samlResponse.Status, inResponseTo));
                }
                return samlResponse;
            }
            catch (Exception ex)
            {
                log.Error("VerifyLogOut", ex);
                samlResponse = new SamlResponseObject();
                samlResponse.Status = "Error";
                samlResponse.Error = ex.Message;
                return samlResponse;
            }
        }

        public static string Redirect(SamlResponseObject samlResponse, int nGroupID)
        {

            string sRespURL = string.Empty;
            DataTable dt = DAL.SSODal.GetScopeRedirectUrl(nGroupID, samlResponse.Scope, samlResponse.Provider_ID);
            if (dt != null && dt.DefaultView.Count > 0)
            {
                sRespURL = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["Redirect_URL"]);

                if (!string.IsNullOrEmpty(sRespURL))
                {
                    sRespURL += samlResponse.ToJSON();
                }
            }
            log.Debug("SAML - " + string.Format("create redirect to IdP  = {0}", sRespURL));
            return sRespURL;
        }

        public static byte[] SignatureRequest(SAML samlConfig, StringBuilder targetURL, ref String signatureBase64encodedString)
        {
            byte[] signedData;
            string privateKey = samlConfig.privateKey;

            using (RSACryptoServiceProvider rsaPrivateKey = new RSACryptoServiceProvider())
            {
                rsaPrivateKey.FromXmlString(privateKey);

                // Create some bytes to be signed. 
                ASCIIEncoding ByteConverter = new ASCIIEncoding();

                byte[] dataBytes = ByteConverter.GetBytes("SAMLRequest=" + OSamlUtils.EncodeSAMLRequestParam(targetURL.ToString()));
                // Create a buffer for the memory stream. 
                byte[] buffer = new byte[dataBytes.Length];
                // Create a MemoryStream.
                using (MemoryStream mStream = new MemoryStream(buffer))
                {

                    // Write the bytes to the stream and flush it.
                    mStream.Write(dataBytes, 0, dataBytes.Length);

                    mStream.Flush();

                    // Export the key information to an RSAParameters object. 
                    // You must pass true to export the private key for signing. 
                    // However, you do not need to export the private key 
                    // for verification.
                    RSAParameters Key = rsaPrivateKey.ExportParameters(true);

                    // Hash and sign the data. 
                    signedData = HashAndSignBytes(mStream, Key);

                }
                signatureBase64encodedString = Convert.ToBase64String(signedData);
            }
            return signedData;
        }
        private static byte[] HashAndSignBytes(Stream DataStream, RSAParameters Key)
        {
            try
            {
                // Reset the current position in the stream to  
                // the beginning of the stream (0). RSACryptoServiceProvider 
                // can't verify the data unless the the stream position 
                // is set to the starting position of the data.
                DataStream.Position = 0;

                // Create a new instance of RSACryptoServiceProvider using the  
                // key from RSAParameters.  
                using (RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider())
                {

                    RSAalg.ImportParameters(Key);

                    // Hash and sign the data. Pass a new instance of SHA1CryptoServiceProvider 
                    // to specify the use of SHA1 for hashing. 
                    return RSAalg.SignData(DataStream, new SHA1CryptoServiceProvider());
                }
            }
            catch (CryptographicException e)
            {
                log.Error("HashAndSignBytes login failed ", e);
                return null;
            }
        }

        // Verifies XML digital signatures on SAML v2.0 assertions, requests, responses and metadata.
        // where the file contains a SAML assertion, request, response or metadata.
        private static bool VerifySignature(SAMLResponse samlResponseObj, RSACryptoServiceProvider rsa)
        {
            bool verified = false;
            try
            {
                log.Debug("SAML - VerifySignature");


                XmlElement xElementVerified = DecryptAssertion(samlResponseObj.GetEncryptedAssertion().ToXml(), rsa);

                switch (xElementVerified.NamespaceURI)
                {
                    case ComponentSpace.SAML2.Utility.SAML.NamespaceURIs.Assertion:
                        if (SAMLAssertionSignature.IsSigned(xElementVerified))
                            verified = SAMLAssertionSignature.Verify(xElementVerified);
                        break;
                    case ComponentSpace.SAML2.Utility.SAML.NamespaceURIs.Protocol:
                        if (SAMLMessageSignature.IsSigned(xElementVerified))
                            verified = SAMLMessageSignature.Verify(xElementVerified);
                        break;
                    case ComponentSpace.SAML2.Utility.SAML.NamespaceURIs.Metadata:
                        if (SAMLMetadataSignature.IsSigned(xElementVerified))
                            verified = SAMLMetadataSignature.Verify(xElementVerified);
                        break;
                }
                return verified;
            }
            catch (ComponentSpace.SAML2.Exceptions.SAMLSignatureException sx)
            {
                log.Error("VerifySignature", sx);
                return false;
            }
            catch (Exception ex)
            {
                log.Error("VerifySignature", ex);
                return false;
            }
        }
    }
}