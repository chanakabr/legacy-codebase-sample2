using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TVPPro.SiteManager.Objects;
using System.Diagnostics;
using System.Web;
using Phx.Lib.Log;
using System.Reflection;
using TVinciShared;

namespace TVPPro.SiteManager.Helper
{
    public class MediaCorpAddressHelper
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static MediaCorpAddressHelper _Instance = new MediaCorpAddressHelper();
        private static object locker = new object();
        private string fileDbUrl = System.Configuration.ConfigurationManager.AppSettings["PostalCodeDbFilesUrl"];

        private Dictionary<string, MediaCorpPostcodeEntity> postalCodeDic = new Dictionary<string, MediaCorpPostcodeEntity>();
        private Dictionary<string, MediaCorpStreetEntity> streetDic = new Dictionary<string, MediaCorpStreetEntity>();
        private Dictionary<string, MediaCorpBuildingEntity> buildingDic = new Dictionary<string, MediaCorpBuildingEntity>();
        private Dictionary<string, MediaCorpWalkupEntity[]> walkupDic = new Dictionary<string, MediaCorpWalkupEntity[]>();
        private static Dictionary<string, MediaCorpAddressEntity> addressDic = new Dictionary<string, MediaCorpAddressEntity>();

        private readonly string StreetsDBUrl;
        private readonly string PostalCodeDBUrl;
        private readonly string BuildingDBUrl;
        private readonly string WalkupDBUrl;


        private MediaCorpAddressHelper()
        {
            string baseUrl = HttpContext.Current.ServerMapPath(fileDbUrl);
            StreetsDBUrl = string.Concat(baseUrl, "streets.txt");
            PostalCodeDBUrl = string.Concat(baseUrl, "postcode.txt");
            BuildingDBUrl = string.Concat(baseUrl, "building.txt");
            WalkupDBUrl = string.Concat(baseUrl, "walkup.txt");
            IsValid = true;

        }

        public static MediaCorpAddressHelper Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (locker)
                    {
                        if (_Instance == null)
                            _Instance = new MediaCorpAddressHelper();
                    }
                }
                return _Instance;
            }
        }





        public MediaCorpAddressEntity this[string token]
        {
            get
            {
                if (addressDic.Count == 0)
                    ReadFileDB();
                return addressDic[token];

            }
        }
        public static bool IsValid { get; private set; }
        private void ReadFileDB()
        {
            try
            {

                using (StreamReader reader = new StreamReader(StreetsDBUrl))
                {
                    string line, streetName, streetKey, streetId;
                    while ((line = reader.ReadLine()) != null)
                    {
                        streetKey = line.Substring(0, 7);
                        streetName = line.Substring(7, 32);
                        //removing white spaces
                        int indx = streetName.IndexOf("  ");
                        if (indx != -1)
                            streetName = streetName.Remove(indx);
                        streetId = line.Substring(39, 6);
                        if (streetDic.ContainsKey(streetKey))
                            continue;
                        streetDic.Add(streetKey, new MediaCorpStreetEntity()
                        {
                            StreetKey = streetKey,
                            StreeID = streetId,
                            StreetName = streetName

                        });
                    }


                }
            }
            catch { logger.DebugFormat("failed to load {0} DB url", StreetsDBUrl); IsValid = false; }
            try
            {
                using (StreamReader reader = new StreamReader(BuildingDBUrl))
                {
                    string line, buildingName, buildingKey;
                    char typeFlag;
                    while ((line = reader.ReadLine()) != null)
                    {
                        buildingKey = line.Substring(0, 6);
                        buildingName = line.Substring(6, 45);
                        // the word THE is at the end of building name so cut it from name and place it at the beginning.
                        int indx = buildingName.IndexOf(", THE");
                        if (indx != -1)
                        {
                            buildingName = buildingName.Substring(0, indx);
                            buildingName = "THE " + buildingName;
                        }
                        typeFlag = line.Substring(51, 1)[0];
                        buildingDic.Add(buildingKey, new MediaCorpBuildingEntity()
                        {
                            BuildingKey = buildingKey,
                            BuildingName = buildingName,
                            TypeFlag = typeFlag
                        });
                    }

                }
            }
            catch { logger.DebugFormat("failed to load {0} DB url", BuildingDBUrl); IsValid = false; }
            try
            {
                using (StreamReader reader = new StreamReader(WalkupDBUrl))
                {
                    string line, postalCode, buildingNumber, streetKey, lastPostalCode = string.Empty;
                    char walkupIndicator;
                    List<MediaCorpWalkupEntity> list = new List<MediaCorpWalkupEntity>();
                    while ((line = reader.ReadLine()) != null || true)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            walkupDic.Add(lastPostalCode, list.ToArray());
                            list.Clear();
                            break;
                        }

                        postalCode = line.Substring(0, 6);
                        buildingNumber = line.Substring(6, 7);
                        streetKey = line.Substring(13, 7);
                        walkupIndicator = line.Substring(20, 1)[0];

                        if (lastPostalCode == postalCode || string.IsNullOrEmpty(lastPostalCode))
                        {
                            list.Add(new MediaCorpWalkupEntity()
                            {
                                BuildingNumber = buildingNumber,
                                PostalCode = postalCode,
                                StreetKey = streetKey,
                                WalkupIndicator = walkupIndicator
                            });
                        }
                        else
                        {

                            walkupDic.Add(lastPostalCode, list.ToArray());

                            list.Clear();
                            list.Add(new MediaCorpWalkupEntity()
                            {
                                BuildingNumber = buildingNumber,
                                PostalCode = postalCode,
                                StreetKey = streetKey,
                                WalkupIndicator = walkupIndicator
                            });
                        }
                        lastPostalCode = postalCode;
                    }

                }
            }
            catch { logger.DebugFormat("failed to load {0} DB url", WalkupDBUrl); IsValid = false; }
            MediaCorpAddressEntity add;
            try
            {
                using (StreamReader reader = new StreamReader(PostalCodeDBUrl))
                {
                    string line, postalCode, buildingNumber, streetKey, buildingKey;
                    char addressType;
                    while ((line = reader.ReadLine()) != null)
                    {
                        postalCode = line.Substring(0, 6);
                        addressType = line.Substring(6, 1)[0];
                        buildingNumber = line.Substring(7, 7);
                        streetKey = line.Substring(14, 7);
                        buildingKey = line.Substring(21, 6);
                        //some building has empty building keys so trim white space.
                        buildingKey = buildingKey.Trim(' ');

                        //we dont need P.O boxs or window type postal codes so filter them out
                        if (addressType == 'P' || addressType == 'B' || addressType == 'W')
                            continue;
                        add = new MediaCorpAddressEntity();
                        if (addressType == 'U')
                        {
                            //add.BuildingNumber = buildingNumber;
                            add.AddressType = addressType;
                            add.StreetName = streetDic[walkupDic[postalCode][0].StreetKey].StreetName;
                            //add.WalkupIndicator = walkupDic[postalCode].WalkupIndicator;
                            if (!string.IsNullOrEmpty(buildingKey))
                                add.BuildingName = buildingDic[buildingKey].BuildingName;
                            add.WalkupArray = walkupDic[postalCode];

                        }
                        else
                        {


                            add.BuildingNumber = buildingNumber;
                            add.StreetName = streetDic[streetKey].StreetName;
                            if (!string.IsNullOrEmpty(buildingKey))
                                add.BuildingName = buildingDic[buildingKey].BuildingName;
                            add.AddressType = addressType;
                        }
                        addressDic.Add(postalCode, add);




                    }
                }

            }
            catch { logger.DebugFormat("failed to load {0} DB url", PostalCodeDBUrl); IsValid = false; }
            Dispose();

        }

        private void Dispose()
        {
            buildingDic.Clear();
            postalCodeDic.Clear();
            streetDic.Clear();
            walkupDic.Clear();

        }

    }
}
