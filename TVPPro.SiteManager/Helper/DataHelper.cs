using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Tvinci.Data.TVMDataLoader.Protocols.SingleMedia;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.Manager;
using System.IO;
using System.Security.Cryptography;
using CachingManager;

namespace TVPPro.SiteManager.Helper
{
    public class DataHelper
    {

        public static void SetCacheObject(string sKey, object obj)
        {
            string sCallerMethodName = new StackFrame(1).GetMethod().Name;
            string sUniqeKey = String.Format("{0}_{1}", sCallerMethodName, sKey.ToLower());

            CachingManager.CachingManager.SetCachedData(sKey, obj, 60 * 60 * 12, System.Runtime.Caching.CacheItemPriority.Default, 0, true);
            //if (Caching[sUniqeKey] != null)
            //{
            //    HttpRuntime.Cache[sUniqeKey] = obj; 
            //}
            //else
            //{
            //    HttpRuntime.Cache.Add(sUniqeKey, obj, null, DateTime.Now.AddHours(12), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Low, null);
            //}
        }

        public static object GetCacheObject(string sKey)
        {
            object oRet = null;

            string sCallerMethodName = new StackFrame(1).GetMethod().Name;
            string sUniqeKey = String.Format("{0}_{1}", sCallerMethodName, sKey.ToLower());

            oRet = CachingManager.CachingManager.GetCachedData(sKey);
            //if (HttpRuntime.Cache[sUniqeKey] != null)
            //{
            //    oRet = HttpRuntime.Cache[sUniqeKey];
            //}

            return oRet;
        }

		public static PageGallery GetGalleryByID(long GalleryID)
		{
			PageContext PC = PageData.Instance.GetCurrentPage();
            IEnumerable<PageGallery> galleryList = PC.Galleries;
            if (SessionHelper.LocaleInfo != null && SessionHelper.LocaleInfo.IsAdminLocale)
            {
                galleryList = galleryList.Union(PC.InActiveGalleries);
            }
			PageGallery TheGallery =
                    (from gallery in galleryList
                     where gallery.GalleryID == GalleryID &&
                     (gallery.MainCulture.Equals(TextLocalization.Instance.UserContext.Culture))
					 select gallery)
					.FirstOrDefault();

			return TheGallery;
		}

        public static void CollectMetasInfo(ref dsItemInfo dsResult, Object mediaInfo)
        {
            dsItemInfo.MetasRow rowMeta = dsResult.Metas.NewMetasRow();

            PropertyInfo[] mediaInfoProperties = mediaInfo.GetType().GetProperties();

            foreach (PropertyInfo property in mediaInfoProperties)
            {
                if (Regex.IsMatch(property.Name.ToLower(), @"meta([0-9]*\x5F[a-z_]*)"))
                {
                    object objMeta = property.GetValue(mediaInfo, null);

                    if (objMeta == null) continue;

                    String sMetaName = (String)objMeta.GetType().InvokeMember("name", BindingFlags.GetProperty, null, objMeta, new Object[] { });
                    String sMetaValue = (String)objMeta.GetType().InvokeMember("value", BindingFlags.GetProperty, null, objMeta, new Object[] { });

                    if (!String.IsNullOrEmpty(sMetaName) && !String.IsNullOrEmpty(sMetaValue))
                    {
                        DataColumn colMetaName = (dsResult.Metas.Columns.Contains(sMetaName)) ? dsResult.Metas.Columns[sMetaName] : dsResult.Metas.Columns.Add(sMetaName, typeof(String));
                        rowMeta[colMetaName] = sMetaValue;
                    }
                }
                if (property.Name.ToLower().Equals("id"))
                {
                    rowMeta.ID = property.GetValue(mediaInfo, null).ToString();
                }
            }

            dsResult.Metas.AddMetasRow(rowMeta);
        }

        public static void CollectTagsInfo(ref dsItemInfo dsResult, ref responsemedia mediaInfo)
        {
            dsItemInfo.TagsRow rowTag = dsResult.Tags.NewTagsRow();

            foreach (tags_collectionstag_type tagType in mediaInfo.tags_collections)
            {
                String sTagType = tagType.name;
                foreach (tag tagElement in tagType.tagCollection)
                {
                    if (!dsResult.Tags.Columns.Contains(sTagType))
                    {
                        DataColumn colTagName = dsResult.Tags.Columns.Add(sTagType, typeof(string));

                        rowTag[colTagName] = tagElement.name;
                    }
                    else
                    {
                        rowTag[sTagType] = (!String.IsNullOrEmpty(rowTag[sTagType].ToString())) ? string.Concat(rowTag[sTagType].ToString(), "|", tagElement.name) : tagElement.name;
                    }
                }
            }
            rowTag["ID"] = mediaInfo.id;
            dsResult.Tags.AddTagsRow(rowTag);
        }

        public static void CollectTagsInfo(ref dsItemInfo dsResult, object mediaInfo)
        {
            String sID = mediaInfo.GetType().InvokeMember("id", BindingFlags.GetProperty, null, mediaInfo, new object[] { }).ToString();

            dsItemInfo.TagsRow rowTag = dsResult.Tags.NewTagsRow(); //dsResult.Tags.AddTagsRow(sID);

            object tagCollectionType = mediaInfo.GetType().InvokeMember("tags_collections", BindingFlags.GetProperty, null, mediaInfo, new object[] { });
            ArrayList tagTypeCollection = (ArrayList)tagCollectionType.GetType().InvokeMember("tag_typeCollection", BindingFlags.GetProperty, null, tagCollectionType, new object[] { });

            for (System.Collections.IEnumerator item = tagTypeCollection.GetEnumerator(); item.MoveNext(); )
            {
                String sTagType = item.Current.GetType().InvokeMember("name", BindingFlags.GetProperty, null, item.Current, new object[] { }).ToString();
                ArrayList tagCollection = (ArrayList)item.Current.GetType().InvokeMember("tagCollection", BindingFlags.GetProperty, null, item.Current, new object[] { });

                for (System.Collections.IEnumerator tagItem = tagCollection.GetEnumerator(); tagItem.MoveNext(); )
                {
                    string sTagValue = tagItem.Current.GetType().InvokeMember("name", BindingFlags.GetProperty, null, tagItem.Current, new object[] { }).ToString();
                    if (!dsResult.Tags.Columns.Contains(sTagType))
                    {
                        DataColumn colTagName = dsResult.Tags.Columns.Add(sTagType, typeof(string));
                        rowTag[colTagName] = sTagValue;
                    }
                    else
                    {
                        rowTag[sTagType] = (!String.IsNullOrEmpty(rowTag[sTagType].ToString()))? string.Concat(rowTag[sTagType], "|", sTagValue) : sTagValue;
                    }
                }
            }
            rowTag["ID"] = sID;
            dsResult.Tags.AddTagsRow(rowTag);
        }

        //public static String GetValueFromRelation(ref ContentPartItem<DataRow> Container, String sRelationName, String sColumnName)
        //{
        //    String sRet = String.Empty;

        //    if (Container != null && Container.Item.GetChildRows(sRelationName).Length > 0 && Container.Item.GetChildRows(sRelationName)[0].Table.Columns.Contains(sColumnName))
        //    {
        //        sRet = Container.Item.GetChildRows(sRelationName)[0][sColumnName].ToString();
        //    }

        //    return sRet;
        //}

        //public static String GetValueFromRelation(ref ContentPartItem<DataRowView> Container, String sRelationName, String sColumnName)
        //{
        //    String sRet = String.Empty;

        //    if (Container != null && Container.Item.CreateChildView(sRelationName).Table.Rows.Count > 0 && Container.Item.CreateChildView(sRelationName).Table.Columns.Contains(sColumnName))
        //    {
        //        sRet = Container.Item.CreateChildView(sRelationName)[0][sColumnName].ToString();
        //    }

        //    return sRet;
        //}

        public static Dictionary<string, string> GetDictionaryFromTagPairs(string sTagPairs)
        {
            Dictionary<string, string> dictRet = new Dictionary<string, string>();

            if (!String.IsNullOrEmpty(sTagPairs))
            {
                string[] arrTagPairs = sTagPairs.Split('|');
                foreach (string sTagPair in arrTagPairs)
                {
                    string[] arrTag = sTagPair.Split('=');
                    if (arrTag.Length > 1)
                    {
                        dictRet.Add(arrTag[0].Trim(), arrTag[1].Trim());
                    }
                }
            }
            return dictRet;
        }

        public static List<String> GetAutoCompleteList()
        {
            List<String> lstResponse = new List<String>();

            string[] arrMetaNames = MediaConfiguration.Instance.Data.TVM.AutoCompleteValues.Metadata.ToString().Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] arrTagNames = MediaConfiguration.Instance.Data.TVM.AutoCompleteValues.Tags.ToString().Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            string[] arrMediaTypeIDs = MediaConfiguration.Instance.Data.TVM.AutoCompleteValues.MediaTypeIDs.ToString().Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (arrMediaTypeIDs != null && arrMediaTypeIDs.Length > 0)
            {
                foreach (string mediaTypeID in arrMediaTypeIDs)
                {
                    CustomAutoCompleteLoader customAutoCompleteLoader = new CustomAutoCompleteLoader() { MediaTypeID = int.Parse(mediaTypeID), MetaNames = arrMetaNames, TagNames = arrTagNames };
                    lstResponse.AddRange(customAutoCompleteLoader.Execute());
                }
            }
            else
            {
                CustomAutoCompleteLoader customAutoCompleteLoader = new CustomAutoCompleteLoader() { MetaNames = arrMetaNames, TagNames = arrTagNames };
                lstResponse = new List<String>(customAutoCompleteLoader.Execute());
            }
            
            return lstResponse;
        }

        public static byte[] encryptStringToBytes_AES(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the stream used to encrypt to an in memory
            // array of bytes.
            MemoryStream msEncrypt = null;

            // Declare the RijndaelManaged object
            // used to encrypt the data.
            RijndaelManaged aesAlg = null;

            try
            {
                // Create a RijndaelManaged object
                // with the specified key and IV.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                msEncrypt = new MemoryStream();
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {

                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                }

            }
            finally
            {

                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return msEncrypt.ToArray();

        }

        public static string decryptStringFromBytes_AES(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            MemoryStream msDecrypt = null;
            CryptoStream csDecrypt = null;
            StreamReader srDecrypt = null;
            RijndaelManaged aesAlg = null;

            string plaintext = null;
            try
            {
                aesAlg = new RijndaelManaged();
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                msDecrypt = new MemoryStream(cipherText);
                csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                srDecrypt = new StreamReader(csDecrypt);

                plaintext = srDecrypt.ReadToEnd();
            }
            finally
            {
                if (srDecrypt != null)
                    srDecrypt.Close();
                if (csDecrypt != null)
                    csDecrypt.Close();
                if (msDecrypt != null)
                    msDecrypt.Close();

                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

    }
}
