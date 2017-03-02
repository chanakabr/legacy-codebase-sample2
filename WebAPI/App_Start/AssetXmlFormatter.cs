using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Runtime.Serialization;
using WebAPI.Models.General;
using System.Text;
using WebAPI.Managers.Models;
using WebAPI.Exceptions;
using System.Web.Http;
using System.Collections.Generic;
using System.Collections;
using WebAPI.Models.Catalog;
using TVinciShared;

namespace WebAPI.App_Start
{
    public class AssetXmlFormatter : MediaTypeFormatter
    {
        public AssetXmlFormatter()
        {
            MediaTypeMappings.Add(new QueryStringMapping("format", "30", "application/xml"));
        }

        public override bool CanReadType(Type type)
        {
            if (type == (Type)null)
                throw new ArgumentNullException("type");

            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        [XmlRoot("feed")]
        public class Feed
        {
            [XmlElement("export", IsNullable = true)]
            public Export Export { get; set; }
        }

        public class Export
        {
            [XmlElement("export", IsNullable = true)]
            public List<Media> Media { get; set; }
        }

        public class Media
        {
            [XmlAttribute("co_guid")]
            public string CoGuid { get; set; }

            [XmlAttribute("entry_id")]
            public string EntryId { get; set; }

            [XmlAttribute("action")]
            public string Action { get; set; }

            [XmlAttribute("is_active")]
            public string IsActive { get; set; }

            [XmlAttribute("erase")]
            public string Erase { get; set; }

            public Media()
            {
                this.Action = "insert";
                this.IsActive = "true";
                this.Erase = "false";
            }

            [XmlElement("basic", IsNullable = true)]
            public Basic Basic { get; set; }

            [XmlElement("structure", IsNullable = true)]
            public Structure Structure { get; set; }

            [XmlElement("files", IsNullable = true)]
            public Files Files { get; set; }
        }

        public class Basic
        {
            [XmlElement("media_type", IsNullable = true)]
            public string MediaType { get; set; }

            [XmlElement("name", IsNullable = true)]
            public Name Name { get; set; }

            [XmlElement("description", IsNullable = true)]
            public Description Description { get; set; }

            [XmlElement("thumb", IsNullable = true)]
            public Thumb Thumb { get; set; }

            [XmlElement("pic_ratios", IsNullable = true)]
            public PicsRatio PicsRatio { get; set; }

            [XmlElement("rules", IsNullable = true)]
            public Rules Rules { get; set; }

            [XmlElement("dates", IsNullable = true)]
            public Dates Dates { get; set; }
        }

        public class Name
        {
            [XmlElement("value", IsNullable = true)]
            public Value Value { get; set; }
        }

        public class Description
        {
            [XmlElement("value", IsNullable = true)]
            public Value Value { get; set; }
        }

        public class Thumb
        {
            [XmlAttribute("url")]
            public string Url { get; set; }
        }

        public class PicsRatio
        {
            [XmlElement("ratio", IsNullable = true)]
            public List<Ratio> Ratios { get; set; }
        }

        public class Ratio
        {
            [XmlAttribute("thumb")]
            public string Thumb { get; set; }

            [XmlAttribute("ratio")]
            public string RatioText { get; set; }
        }

        public class Rules
        {
            [XmlElement("watch_per_rule", IsNullable = true)]
            public string WatchPerRule { get; set; }

            [XmlElement("geo_block_rule", IsNullable = true)]
            public string GeoBlockRule { get; set; }

            [XmlElement("device_rule", IsNullable = true)]
            public string DeviceRule { get; set; }
        }

        public class Dates
        {
            [XmlElement("start", IsNullable = true)]
            public string Start { get; set; }

            [XmlElement("final_end", IsNullable = true)]
            public string End { get; set; }

            [XmlElement("catalog_start", IsNullable = true)]
            public string CatalogStart { get; set; }

            [XmlElement("catalog_end", IsNullable = true)]
            public string CatalogEnd { get; set; }
        }

        public class Value
        {
            [XmlElement("lang", IsNullable = true)]
            public string Lang { get; set; }

            // to add CDATA ValueText and ValueTextContent should be used
            [XmlIgnore]
            public string ValueText { get; set; }

            [XmlText]
            public XmlNode[] ValueTextContent
            {
                get
                {
                    var dummy = new XmlDocument();
                    return new XmlNode[] { dummy.CreateCDataSection(ValueText) };
                }
                set
                {
                    if (value == null)
                    {
                        ValueText = null;
                        return;
                    }

                    if (value.Length != 1)
                    {
                        throw new InvalidOperationException(
                            String.Format(
                                "Invalid array length {0}", value.Length));
                    }

                    ValueText = value[0].Value;
                }
            }
        }

        public class Structure
        {
            [XmlElement("strings", IsNullable = true)]
            public Strings Strings { get; set; }

            [XmlElement("doubles", IsNullable = true)]
            public Doubles Doubles { get; set; }

            [XmlElement("booleans", IsNullable = true)]
            public Booleans Booleans { get; set; }

            [XmlElement("metas", IsNullable = true)]
            public Metas Metas { get; set; }
        }

        public class Strings
        {
           [XmlElement("meta", IsNullable = true)]
            public List<Meta> Metas { get; set; }
        }

        public class Container
        {
            [XmlElement("value", IsNullable = true)]
            public List<Value> Values { get; set; }
        }

        public class Meta
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("ml_handling")]
            public string MlHandling { get; set; }

            [XmlElement("value", IsNullable = true)]
            public Value Value { get; set; }

            [XmlElement("container", IsNullable = true)]
            public Container Container { get; set; }

            public Meta()
            {
                this.MlHandling = "unique";
            }
        }

        public class MetaWithoutInnerElement
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("ml_handling")]
            public string MlHandling { get; set; }

            [XmlText]
            public string Value { get; set; }

            public MetaWithoutInnerElement()
            {
                this.MlHandling = "unique";
            }
        }

        public class Doubles
        {
            [XmlElement("meta", IsNullable = true)]
            public List<MetaWithoutInnerElement> Metas { get; set; }
        }

        public class Booleans
        {
            [XmlElement("booleans", IsNullable = true)]
            public List<MetaWithoutInnerElement> BooleanList { get; set; }
        }

        public class Metas
        {
            [XmlElement("meta", IsNullable = true)]
            public List<Meta> MetasList { get; set; }
        }

        public class Files
        {
            [XmlElement("file", IsNullable = true)]
            public List<MediaFile> MediaFiles { get; set; }
        }

        public class MediaFile
        {
            [XmlAttribute("type")]
            public string Type { get; set; }

            [XmlAttribute("assetDuration")]
            public string AssetDuration { get; set; }

            [XmlAttribute("quality")]
            public string Quality { get; set; }

            [XmlAttribute("handling_type")]
            public string HandlingType { get; set; }

            [XmlAttribute("cdn_name")]
            public string CdnName { get; set; }

            [XmlAttribute("cdn_code")]
            public string CdnCode { get; set; }

            [XmlAttribute("alt_cdn_code")]
            public string AltCdnCode { get; set; }

            [XmlAttribute("co_guid")]
            public string CoGuid { get; set; }

            [XmlAttribute("billing_type")]
            public string BillingType { get; set; }

            [XmlAttribute("PPV_MODULE")]
            public string PpvModule { get; set; }

            [XmlAttribute("product_code")]
            public string ProductCode { get; set; }

            public MediaFile()
            {
                this.Quality = "HIGH";
            }
        }

        /// <summary>
        /// change image URL to request the original URL
        /// Example:
        /// GetImage/p/215/entry_id/123/version/10/width/432/height/230/quality/100 
        /// change to: 
        /// GetImage/p/215/entry_id/123/version/10/width/0/height/0/quality/100
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string ManipulateImageUrl(string url)
        {
            if (url == null)
                return string.Empty;

            // to lower
            url = url.ToLower();

            // check URL contains new image server format
            if (!url.Contains("entry_id/"))
                return url;

            var segments = new Uri(url).Segments;
            if (segments == null)
                return url;

            string width = string.Empty;
            string height = string.Empty;
            for (int i = 0; i < segments.Length; i++)
            {
                // search for width/<number>
                if (segments[i].Replace("/", string.Empty) == "width")
                    width = segments[i + 1].Replace("/", string.Empty);

                // search for height/<number>
                if (segments[i].Replace("/", string.Empty) == "height")
                    height = segments[i + 1].Replace("/", string.Empty);
            }

            // replace width/<number> to width/0
            if (!string.IsNullOrEmpty(width))
                url = url.Replace("width/" + width, "width/0");

            // replace height/<number> to height/0
            if (!string.IsNullOrEmpty(height))
                url = url.Replace("height/" + height, "height/0");

            return url;
        }

        private XmlDocument SerializeToXmlDocument(Feed input)
        {
            XmlSerializer ser = new XmlSerializer(input.GetType(), new Type[] { typeof(StatusWrapper) });
            XmlDocument xd = null;
            using (MemoryStream memStm = new MemoryStream())
            {
                ser.Serialize(memStm, input);
                memStm.Position = 0;
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;

                using (var xtr = XmlReader.Create(memStm, settings))
                {
                    xd = new XmlDocument();
                    xd.Load(xtr);
                }
            }

            return xd;
        }

        public Feed ConvertResultToIngestObj(KalturaAssetListResponse listResponse)
        {
            Media media;
            Feed feed = new Feed();
            feed.Export = new Export();
            feed.Export.Media = new List<Media>();

            if (listResponse == null)
                return feed;

            if (listResponse.Objects != null)
            {
                foreach (var asset in listResponse.Objects)
                {
                    // add media
                    media = new Media();

                    // TODO: get co_guid
                    media.CoGuid = string.Empty;

                    // TODO: get entry_id
                    media.EntryId = string.Empty;

                    // add basic
                    media.Basic = new Basic();

                    // TODO: get media_type
                    media.Basic.MediaType = string.Empty;

                    // add name
                    media.Basic.Name = new Name()
                    {
                        Value = new Value()
                        {
                            ValueText = asset.Name ?? string.Empty,
                            // TODO: add language
                            Lang = string.Empty
                        }
                    };

                    // add description
                    media.Basic.Description = new Description()
                    {
                        Value = new Value()
                        {
                            ValueText = asset.Description ?? string.Empty,
                            // TODO: add language
                            Lang = string.Empty
                        }
                    };

                    // add dates
                    media.Basic.Dates = new Dates()
                    {
                        CatalogEnd = asset.EndDate != null && asset.EndDate != 0 ? DateUtils.UnixTimeStampToDateTime((long)asset.EndDate).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                        End = asset.EndDate != null && asset.EndDate != 0 ? DateUtils.UnixTimeStampToDateTime((long)asset.EndDate).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                        CatalogStart = asset.StartDate != null && asset.EndDate != 0 ? DateUtils.UnixTimeStampToDateTime((long)asset.StartDate).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                        Start = asset.StartDate != null && asset.EndDate != 0 ? DateUtils.UnixTimeStampToDateTime((long)asset.StartDate).ToString("dd/MM/yyyy HH:mm:ss") : string.Empty
                    };

                    // TODO: add rules
                    media.Basic.Rules = new Rules()
                    {
                        DeviceRule = string.Empty,
                        GeoBlockRule = string.Empty,
                        WatchPerRule = string.Empty
                    };

                    // add pics_ratios
                    media.Basic.PicsRatio = new PicsRatio() { Ratios = new List<Ratio>() };
                    if (asset.Images != null)
                    {
                        // group by ratio, take max size 
                        List<KalturaMediaImage> images = asset.Images.GroupBy(x => x.Ratio).Select(y => y.OrderByDescending(x => ((x.Height ?? 0) * (x.Width ?? 0))).First()).ToList();

                        bool thumbUpdated = false;
                        foreach (var image in images)
                        {
                            // add thumb
                            if (!thumbUpdated)
                            {
                                media.Basic.Thumb = new Thumb() { Url = image.Url != null ? ManipulateImageUrl(image.Url) : string.Empty };
                                thumbUpdated = true;
                            }

                            // ratio 
                            media.Basic.PicsRatio.Ratios.Add(new Ratio()
                            {
                                RatioText = image.Ratio ?? string.Empty,
                                Thumb = image.Url != null ? ManipulateImageUrl(image.Url) : string.Empty
                            });
                        }
                    }

                    // add structure
                    media.Structure = new Structure()
                    {
                        Booleans = new Booleans() { BooleanList = new List<MetaWithoutInnerElement>() },
                        Doubles = new Doubles() { Metas = new List<MetaWithoutInnerElement>() },
                        Strings = new Strings() { Metas = new List<Meta>() }
                    };

                    if (asset.Metas != null)
                    {
                        foreach (KeyValuePair<string, KalturaValue> entry in asset.Metas)
                        {
                            // add strings
                            if (entry.Value.GetType() == typeof(KalturaStringValue))
                            {
                                media.Structure.Strings.Metas.Add(new Meta()
                                {
                                    Name = entry.Key ?? string.Empty,
                                    Value = new Value()
                                    {
                                        // TODO: fill language
                                        Lang = string.Empty,
                                        ValueText = ((KalturaStringValue)entry.Value).value ?? string.Empty
                                    }
                                });
                            }

                            // add doubles
                            if (entry.Value.GetType() == typeof(KalturaDoubleValue))
                            {
                                media.Structure.Doubles.Metas.Add(new MetaWithoutInnerElement()
                                {
                                    Name = entry.Key ?? string.Empty,
                                    Value = ((KalturaDoubleValue)entry.Value).value.ToString()
                                });
                            }

                            // add doubles (from integers)
                            if (entry.Value.GetType() == typeof(KalturaIntegerValue))
                            {
                                media.Structure.Doubles.Metas.Add(new MetaWithoutInnerElement()
                                {
                                    Name = entry.Key ?? string.Empty,
                                    Value = ((KalturaIntegerValue)entry.Value).value.ToString()
                                });
                            }

                            // add booleans 
                            if (entry.Value.GetType() == typeof(KalturaBooleanValue))
                            {
                                media.Structure.Doubles.Metas.Add(new MetaWithoutInnerElement()
                                {
                                    Name = entry.Key ?? string.Empty,
                                    Value = ((KalturaBooleanValue)entry.Value).value ? "true" : "false"
                                });
                            }
                        }
                    }

                    // add tags
                    media.Structure.Metas = new Metas() { MetasList = new List<Meta>() };
                    if (asset.Tags != null)
                    {
                        Meta meta;
                        foreach (KeyValuePair<string, KalturaStringValueArray> entry in asset.Tags)
                        {
                            meta = new Meta()
                            {
                                Name = entry.Key ?? string.Empty,
                                Container = new Container() { Values = new List<Value>() }
                            };

                            if (entry.Value.Objects != null)
                            {
                                foreach (KalturaStringValue item in entry.Value.Objects)
                                {
                                    // add tag container
                                    meta.Container.Values.Add(new Value()
                                    {
                                        // TODO: update language
                                        Lang = string.Empty,
                                        ValueText = item.value ?? string.Empty
                                    });
                                    media.Structure.Metas.MetasList.Add(meta);
                                }
                            }
                        }
                    }

                    // add files
                    media.Files = new Files() { MediaFiles = new List<MediaFile>() };

                    if (asset.MediaFiles != null)
                    {
                        foreach (var file in asset.MediaFiles)
                        {
                            media.Files.MediaFiles.Add(new MediaFile()
                            {
                                AssetDuration = file.Duration != null ? file.Duration.ToString() : string.Empty,
                                Type = file.Type ?? string.Empty,
                                CdnCode = file.Url ?? string.Empty,

                                // TODO: check if external ID = co_guid
                                CoGuid = file.ExternalId ?? string.Empty,

                                // TODO: fill all missing values
                                AltCdnCode = string.Empty,
                                BillingType = string.Empty,
                                CdnName = string.Empty,
                                HandlingType = string.Empty,
                                PpvModule = string.Empty,
                                ProductCode = string.Empty
                            });
                        }
                    }

                    // add media
                    feed.Export.Media.Add(media);
                }
            }

            return feed;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content,
            System.Net.TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                Feed resultFeed = new Feed();
                if (value != null)
                {
                    // validate expected type was received
                    StatusWrapper restResultWrapper = (StatusWrapper)value;
                    if (restResultWrapper != null && restResultWrapper.Result != null && restResultWrapper.Result is KalturaAssetListResponse)
                        resultFeed = ConvertResultToIngestObj((KalturaAssetListResponse)restResultWrapper.Result);

                    XmlDocument doc = SerializeToXmlDocument(resultFeed);
                    var buf = Encoding.UTF8.GetBytes(doc.OuterXml);
                    writeStream.Write(buf, 0, buf.Length);
                }
            });
        }
    }
}