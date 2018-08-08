using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using System.Data;
using System.Xml;
using KLogMonitor;
using System.Web.Hosting;
using System.Reflection;
using System.IO;
using ApiObjects.Roles;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ApiObjects;

namespace PermissionsManager
{
    public class PermissionsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
       
        #region Consts

        private const string ROLE = "role";
        private const string PERMISSION = "permission";
        private const string ROLE_PERMISSION = "role_permission";
        private const string PERMISSION_ITEM = "permission_item";
        private const string PERMISSION_PERMISSION_ITEM = "permission_permission_item";

        #endregion

        #region Main Methods

        public static bool Export(string fileName)
        {
            bool result = false;

            if (string.IsNullOrEmpty(fileName))
            {
                return result;
            }

            InitializeLogging();
            ConfigurationManager.ApplicationConfiguration.Initialize(true, true);

            try
            {
                DataSet allPermissions = ApiDAL.Get_PermissionsForExport();

                if (allPermissions != null && allPermissions.Tables != null && allPermissions.Tables.Count >= 5)
                {
                    RenameTables(allPermissions);

                    // create output directory
                    string directory = Path.GetDirectoryName(fileName);

                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    allPermissions.WriteXml(fileName);

                    log.InfoFormat("export into file {0} has been completed", fileName);

                    result = true;
                }
                else
                {
                    log.ErrorFormat("Failed getting valid data set of permissions for export.");
                }
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Failed exporting permissions, ex = {0}", ex);
            }

            return result;
        }

        public static bool Import(string fileName)
        {
            bool result = false;

            if (string.IsNullOrEmpty(fileName))
            {
                return result;
            }

            InitializeLogging();

            log.InfoFormat("Starting import of permission from file {0}", fileName);

            // create output directory
            string directory = Path.GetDirectoryName(fileName);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            ConfigurationManager.ApplicationConfiguration.Initialize(true, true);

            try
            {
                #region Initialize and start

                DataSet source = new DataSet();
                source.ReadXml(fileName);

                if (source == null || source.Tables == null || source.Tables.Count < 5)
                {
                    log.ErrorFormat("Import failed: reading from XML resulted in empty data set or data set with less than 5 tables");

                    return result;
                }
                else
                {
                    HashSet<string> tableNames = new HashSet<string>();

                    foreach (DataTable table in source.Tables)
                    {
                        tableNames.Add(table.TableName);
                    }

                    if (!tableNames.Contains(ROLE) ||
                        !tableNames.Contains(PERMISSION) ||
                        !tableNames.Contains(ROLE_PERMISSION) ||
                        !tableNames.Contains(PERMISSION_ITEM) ||
                        !tableNames.Contains(PERMISSION_PERMISSION_ITEM))
                    {
                        log.ErrorFormat("Import failed: reading from XML resulted in data set with wrong tables.");

                        return result;
                    }
                }

                DataSet destination = ApiDAL.Get_PermissionsForExport();

                if (destination == null || destination.Tables == null || destination.Tables.Count < 5)
                {
                    log.ErrorFormat("Import failed: loading current permission resulted in empty data set or data set with less than 5 tables");

                    return result;
                }

                RenameTables(destination);

                // save backup of existing data before continuing 
                string backupFileName = AddSuffix(fileName, "_backup");

                destination.WriteXml(backupFileName);

                #endregion

                Dictionary<string, int> rolesDictionary = new Dictionary<string, int>();
                Dictionary<string, int> permissionsDictionary = new Dictionary<string, int>();
                Dictionary<string, int> permissionItemsDictionary = new Dictionary<string, int>();

                #region Role
                {
                    var sourceRoles = source.Tables[ROLE];
                    var destinationRoles = destination.Tables[ROLE];

                    sourceRoles.PrimaryKey = new DataColumn[] { sourceRoles.Columns["id"] };
                    destinationRoles.PrimaryKey = new DataColumn[] { destinationRoles.Columns["id"] };

                    // Insert/Update
                    foreach (DataRow sourceRole in sourceRoles.Rows)
                    {
                        int sourceId = Convert.ToInt32(sourceRole["id"]);
                        string sourceName = Convert.ToString(sourceRole["name"]);

                        DataRow destinationRole = destinationRoles.AsEnumerable().FirstOrDefault(row =>
                            Convert.ToString(row["name"]) == sourceName);

                        // not found - it is new, insert
                        if (destinationRole == null)
                        {
                            int newId = ApiDAL.InsertRole(sourceName);

                            rolesDictionary[sourceName] = newId;

                            log.InfoFormat("!!INSERT!! Table : {0} Source id : {1} Source name : {2}", ROLE, sourceId, sourceName);
                        }
                        else
                        // found - check for differences
                        {
                            // in case of role we have only name field, so we're cool like this
                        }
                    }

                    // Delete
                    foreach (DataRow destinationRole in destinationRoles.Rows)
                    {
                        int destinationId = Convert.ToInt32(destinationRole["id"]);
                        string destinationName = Convert.ToString(destinationRole["name"]);

                        rolesDictionary[destinationName] = destinationId;

                        DataRow sourceRole = sourceRoles.AsEnumerable().FirstOrDefault(row =>
                            Convert.ToString(row["name"]) == destinationName);

                        // not found in source - it was deleted
                        if (sourceRole == null)
                        {
                            //bool actionResult = ApiDAL.DeleteRole(0, destinationId);

                            log.InfoFormat("!!NOT EXISTS IN SOURCE!! Table : {0} Id : {1} Name : {2}", ROLE, destinationId, destinationName);
                        }
                    }
                }
                #endregion

                #region Permission
                {
                    var sourceTable = source.Tables[PERMISSION];
                    var destinationTable = destination.Tables[PERMISSION];

                    sourceTable.PrimaryKey = new DataColumn[] { sourceTable.Columns["id"] };
                    destinationTable.PrimaryKey = new DataColumn[] { destinationTable.Columns["id"] };

                    // Insert/Update
                    foreach (DataRow sourceRow in sourceTable.Rows)
                    {
                        int sourceId = Convert.ToInt32(sourceRow["id"]);
                        string sourceName = Convert.ToString(sourceRow["name"]);

                        DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                            Convert.ToString(row["name"]) == sourceName);

                        int sourceType = Convert.ToInt32(sourceRow["type"]);
                        object sourceUsersGroupObject = sourceRow["users_group"];
                        string sourceUsersGroup = null;

                        if (sourceUsersGroupObject != null && sourceUsersGroupObject != DBNull.Value)
                        {
                            sourceUsersGroup = Convert.ToString(sourceUsersGroupObject);
                        }

                        // not found - it is new, insert
                        if (destinationRow == null)
                        {
                            int newId = ApiDAL.InsertPermission(sourceName, sourceType, sourceUsersGroup);

                            permissionsDictionary[sourceName] = newId;

                            log.InfoFormat("!!INSERT!! Table : {0} Source Id : {1} name : {2} type : {3} users group : {4}", PERMISSION,
                                sourceId, sourceName, sourceType, sourceUsersGroup);
                        }
                        else
                        // found - check for differences
                        {
                            int destinationId = Convert.ToInt32(destinationRow["id"]);
                            int destinationType = ExtractValue<int>(destinationRow, "type");
                            string destinationUsersGroup = ExtractValue<string>(destinationRow, "users_group");

                            if (
                                destinationType != sourceType ||
                                destinationUsersGroup != sourceUsersGroup
                                )
                            {
                                bool actionResult = ApiDAL.UpdatePermission(destinationId, sourceName, sourceType, sourceUsersGroup);

                                log.InfoFormat("!!UPDATE!! Table : {0} Destination Id : {1} name : {2} type : {3} users group : {4}", PERMISSION,
                                    destinationId, sourceName, sourceType, sourceUsersGroup);
                            }
                        }
                    }

                    // Delete - permission
                    foreach (DataRow destinationRow in destinationTable.Rows)
                    {
                        int destinationId = Convert.ToInt32(destinationRow["id"]);
                        string destinationName = Convert.ToString(destinationRow["name"]);

                        permissionsDictionary[destinationName] = destinationId;

                        DataRow sourceRow = sourceTable.AsEnumerable().FirstOrDefault(row =>
                            Convert.ToString(row["name"]) == destinationName);

                        // not found in source - it was deleted
                        if (sourceRow == null)
                        {
                            //bool actionResult = ApiDAL.DeletePermission(destinationId);

                            log.InfoFormat("!!NOT EXISTS IN SOURCE!! Table : {0} Id : {1}", PERMISSION, destinationId);
                        }
                    }
                }
                #endregion

                #region Role Permission
                {
                    var sourceTable = source.Tables[ROLE_PERMISSION];
                    var destinationTable = destination.Tables[ROLE_PERMISSION];

                    sourceTable.PrimaryKey = new DataColumn[] { sourceTable.Columns["id"] };
                    destinationTable.PrimaryKey = new DataColumn[] { destinationTable.Columns["id"] };

                    // Insert/Update
                    foreach (DataRow sourceRow in sourceTable.Rows)
                    {
                        int sourceId = Convert.ToInt32(sourceRow["id"]);
                        int sourceRoleId = Convert.ToInt32(sourceRow["role_id"]);
                        int sourcePermissionId = Convert.ToInt32(sourceRow["permission_id"]);
                        string sourceRoleName = ExtractValue<string>(sourceRow, "role_name");
                        string sourcePermissionName = ExtractValue<string>(sourceRow, "permission_name");

                        int? sourceIsExcluded = ExtractNullableInteger(sourceRow, "is_excluded");

                        DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                             sourceRoleName == Convert.ToString(row["role_name"]) &&
                             sourcePermissionName == Convert.ToString(row["permission_name"]));

                        if (!rolesDictionary.ContainsKey(sourceRoleName))
                        {
                            log.ErrorFormat("Import error : try to import permission_role with non existing role {0}", sourceRoleName);
                        }

                        if (!permissionsDictionary.ContainsKey(sourcePermissionName))
                        {
                            log.ErrorFormat("Import error : try to import permission_role with non existing permission {0}", sourcePermissionName);
                        }

                        if (rolesDictionary.ContainsKey(sourceRoleName) && permissionsDictionary.ContainsKey(sourcePermissionName))
                        {
                            int destinationRoleId = rolesDictionary[sourceRoleName];
                            int destinationPermissionId = permissionsDictionary[sourcePermissionName];

                            // not found - it is new, insert
                            if (destinationRow == null)
                            {
                                int newId = ApiDAL.InsertPermissionRole(destinationRoleId, destinationPermissionId, sourceIsExcluded);

                                log.InfoFormat("!!INSERT!! Table : {0} Source Id : {1} role_id : {2} permission_id : {3} is_excluded : {4} " +
                                    "role_name : {5} permission_name : {6}",
                                    ROLE_PERMISSION,
                                    sourceId, destinationRoleId, destinationPermissionId, sourceIsExcluded,
                                    sourceRoleName,
                                    sourcePermissionName);
                            }
                            else
                            // found - check for differences
                            {
                                int destinationId = Convert.ToInt32(destinationRow["id"]);

                                int? destinationIsExcluded = ExtractNullableInteger(destinationRow, "is_excluded");

                                if (
                                    sourceIsExcluded != destinationIsExcluded
                                    )
                                {
                                    bool actionResult = ApiDAL.UpdatePermissionRole(destinationId, sourceIsExcluded);

                                    log.InfoFormat("!!UPDATE!! Table : {0} destination Id : {1} role_id : {2} permission_id : {3} " +
                                        "is_excluded : {4} role_name : {5} permission_name : {6}",
                                    ROLE_PERMISSION,
                                    sourceId, destinationRoleId, destinationPermissionId, sourceIsExcluded,
                                    sourceRoleName,
                                    sourcePermissionName);
                                }
                            }
                        }
                    }

                    // Delete role permission
                    foreach (DataRow destinationRow in destinationTable.Rows)
                    {
                        int destinationId = Convert.ToInt32(destinationRow["id"]);

                        int destinationRoleId = Convert.ToInt32(destinationRow["role_id"]);
                        int destinationPermissionId = Convert.ToInt32(destinationRow["permission_id"]);
                        string destinationRoleName = ExtractValue<string>(destinationRow, "role_name");
                        string destinationPermissionName = ExtractValue<string>(destinationRow, "permission_name");

                        DataRow sourceRow = sourceTable.AsEnumerable().FirstOrDefault(row =>
                             destinationRoleName == Convert.ToString(row["role_name"]) &&
                             destinationPermissionName == Convert.ToString(row["permission_name"]));

                        // not found in source - it was deleted
                        if (sourceRow == null)
                        {
                            //bool actionResult = ApiDAL.DeleteRolePermission(destinationId);

                            log.InfoFormat("!!NOT EXISTS IN SOURCE!! Table : {0} Id : {1}", ROLE_PERMISSION, destinationId);
                        }
                    }
                }
                #endregion

                #region Permission Item
                {
                    var sourceTable = source.Tables[PERMISSION_ITEM];
                    var destinationTable = destination.Tables[PERMISSION_ITEM];

                    sourceTable.PrimaryKey = new DataColumn[] { sourceTable.Columns["id"] };
                    destinationTable.PrimaryKey = new DataColumn[] { destinationTable.Columns["id"] };

                    // Insert/Update
                    foreach (DataRow sourceRow in sourceTable.Rows)
                    {
                        int sourceId = Convert.ToInt32(sourceRow["id"]);
                        string sourceName = ExtractValue<string>(sourceRow, "name");
                        int? sourceType = ExtractNullableInteger(sourceRow, "type");
                        string sourceService = ExtractValue<string>(sourceRow, "service");
                        string sourceAction = ExtractValue<string>(sourceRow, "action");
                        string sourceObject = ExtractValue<string>(sourceRow, "object");
                        string sourceParameter = ExtractValue<string>(sourceRow, "parameter");

                        DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                             sourceName == ExtractValue<string>(row, "name"));

                        // not found - it is new, insert new permission item
                        if (destinationRow == null)
                        {
                            int newId = ApiDAL.InsertPermissionItem(sourceName, sourceType, sourceService, sourceAction, sourceObject, sourceParameter);

                            permissionItemsDictionary[sourceName] = newId;

                            log.InfoFormat("!!INSERT!! Table : {0} Source Id : {1} name : {2} type: {3} service : {4} action : {5} object : {6} parameter : {7}",
                                PERMISSION_ITEM, sourceId, sourceName, sourceType, sourceService, sourceAction, sourceObject, sourceParameter);
                        }
                        else
                        // found - check for differences - permission item
                        {
                            int destinationId = Convert.ToInt32(destinationRow["id"]);
                            string destinationName = ExtractValue<string>(destinationRow, "name");
                            int? destinationType = ExtractNullableInteger(destinationRow, "type");
                            string destinationService = ExtractValue<string>(destinationRow, "service");
                            string destinationAction = ExtractValue<string>(destinationRow, "action");
                            string destinationObject = ExtractValue<string>(destinationRow, "object");
                            string destinationParameter = ExtractValue<string>(destinationRow, "parameter");

                            if (
                                sourceType != destinationType ||
                                sourceService != destinationService ||
                                sourceAction != destinationAction ||
                                sourceObject != destinationObject ||
                                sourceParameter != destinationParameter
                                )
                            {
                                bool actionResult = 
                                    ApiDAL.UpdatePermissionItem(destinationId, sourceType, destinationName, 
                                        sourceService, sourceAction, sourceObject, sourceParameter);

                                log.InfoFormat("!!UPDATE!! Table : {0} destination Id : {1} name : {2} type: {3} service : {4} action : {5} object : {6} parameter : {7}",
                                    PERMISSION_ITEM, destinationId, sourceName, sourceType, sourceService, sourceAction, sourceObject, sourceParameter);
                            }
                        }
                    }

                    // Delete permission item
                    foreach (DataRow destinationRow in destinationTable.Rows)
                    {
                        int destinationId = Convert.ToInt32(destinationRow["id"]);
                        string destinationName = ExtractValue<string>(destinationRow, "name");

                        permissionItemsDictionary[destinationName] = destinationId;

                        DataRow sourceRow = sourceTable.AsEnumerable().FirstOrDefault(row =>
                            destinationName == ExtractValue<string>(row, "name"));

                        // not found in source - it was deleted
                        if (sourceRow == null)
                        {
                            //bool actionResult = ApiDAL.DeletePermissionItem(destinationId);

                            log.InfoFormat("!!NOT EXISTS IN SOURCE!! Table : {0} Id : {1}", PERMISSION_ITEM, destinationId);
                        }
                    }
                }
                #endregion

                #region Permission Permission Item
                {
                    var sourceTable = source.Tables[PERMISSION_PERMISSION_ITEM];
                    var destinationTable = destination.Tables[PERMISSION_PERMISSION_ITEM];

                    sourceTable.PrimaryKey = new DataColumn[] { sourceTable.Columns["id"] };
                    destinationTable.PrimaryKey = new DataColumn[] { destinationTable.Columns["id"] };

                    // Insert/Update
                    foreach (DataRow sourceRow in sourceTable.Rows)
                    {
                        int sourceId = Convert.ToInt32(sourceRow["id"]);
                        int sourcePermissionId = ExtractValue<int>(sourceRow, "permission_id");
                        int sourcePermissionItemId = ExtractValue<int>(sourceRow, "permission_item_id");
                        int? sourceIsExcluded = ExtractNullableInteger(sourceRow, "is_excluded");
                        string sourcePermissionName = ExtractValue<string>(sourceRow, "permission_name");
                        string sourcePermissionItemName = ExtractValue<string>(sourceRow, "permission_item_name");

                        DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                             sourcePermissionName == ExtractValue<string>(row, "permission_name") &&
                             sourcePermissionItemName == ExtractValue<string>(row, "permission_item_name"));


                        if (!permissionsDictionary.ContainsKey(sourcePermissionName))
                        {
                            log.ErrorFormat("Import error : try to import permission_permission_item with non existing permission {0}", 
                                sourcePermissionName);
                        }

                        if (!permissionItemsDictionary.ContainsKey(sourcePermissionItemName))
                        {
                            log.ErrorFormat("Import error : try to import permission_permission_item with non existing permission_item {0}", 
                                sourcePermissionItemName);
                        }

                        if (permissionItemsDictionary.ContainsKey(sourcePermissionItemName) && permissionsDictionary.ContainsKey(sourcePermissionName))
                        {
                            int destinationPermissionId = permissionsDictionary[sourcePermissionName];
                            int destinationPermissionItemId = permissionItemsDictionary[sourcePermissionItemName];

                            // not found - it is new, insert new permission permission item
                            if (destinationRow == null)
                            {
                                int newId = ApiDAL.InsertPermissionPermissionItem(destinationPermissionId, destinationPermissionItemId, sourceIsExcluded);

                                log.InfoFormat(
                                    "!!INSERT!! Table : {0} Source Id : {1} permission_id : {2} permission_item_id: {3} is_excluded : {4} " +
                                    "permission_name : {5} permission_item_name : {6}",
                                    PERMISSION_PERMISSION_ITEM, sourceId, destinationPermissionId, destinationPermissionItemId, sourceIsExcluded,
                                    sourcePermissionName, sourcePermissionItemName);
                            }
                            else
                            // found - check for differences - permission permission item
                            {
                                int destinationId = Convert.ToInt32(destinationRow["id"]);
                                int? destinationIsExcluded = ExtractNullableInteger(destinationRow, "is_excluded");

                                if (
                                    sourceIsExcluded != destinationIsExcluded
                                    )
                                {
                                    bool actionResult = ApiDAL.UpdatePermissionPermissionItem(destinationId, sourceIsExcluded);

                                    log.InfoFormat(
                                        "!!UPDATE!! Table : {0} Destination Id : {1} permission_id : {2} permission_item_id: {3} is_excluded : {4} " + 
                                        "permission_name : {5} permission_item_name : {6}",
                                        PERMISSION_PERMISSION_ITEM, sourceId, destinationPermissionId, destinationPermissionItemId, sourceIsExcluded,
                                        sourcePermissionName, sourcePermissionItemName);
                                }
                            }
                        }
                    }
                    
                    // Delete permission permission item
                    foreach (DataRow destinationRow in destinationTable.Rows)
                    {
                        int destinationId = Convert.ToInt32(destinationRow["id"]);
                        int destinationPermissionId = ExtractValue<int>(destinationRow, "permission_id");
                        int destinationPermissionItemId = ExtractValue<int>(destinationRow, "permission_item_id");
                        string destinationPermissionName = ExtractValue<string>(destinationRow, "permission_name");
                        string destinationPermissionItemName = ExtractValue<string>(destinationRow, "permission_item_name");

                        DataRow sourceRow = sourceTable.AsEnumerable().FirstOrDefault(row =>
                             destinationPermissionName == ExtractValue<string>(row, "permission_name") &&
                             destinationPermissionItemName == ExtractValue<string>(row, "permission_item_name"));

                        // not found in source - it was deleted
                        if (sourceRow == null)
                        {
                            //bool actionResult = ApiDAL.DeletePermissionPermissionItem(destinationId);

                            log.InfoFormat("!!NOT EXISTS IN SOURCE!! Table : {0} Id : {1}", PERMISSION_PERMISSION_ITEM, destinationId);
                        }
                    }
                }
                #endregion

                result = true;

                log.InfoFormat("import from file {0} has been completed", fileName);
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Failed importing permissions, ex = {0}", ex);
            }

            log.InfoFormat("Finished import of permission from file {0}, result is {1}", fileName, result);

            return result;
        }

        public static bool Delete(string fileName)
        {
            bool result = false;

            if (string.IsNullOrEmpty(fileName))
            {
                return result;
            }

            InitializeLogging();
            ConfigurationManager.ApplicationConfiguration.Initialize(true, true);

            log.InfoFormat("Starting deleting of permissions from file {0}", fileName);

            try
            {
                DataSet source = new DataSet();
                source.ReadXml(fileName);

                if (source == null || source.Tables == null || source.Tables.Count == 0)
                {
                    log.ErrorFormat("Delete failed: reading from XML resulted in empty data set");

                    return result;
                }

                DataSet destination = ApiDAL.Get_PermissionsForExport();

                if (destination == null || destination.Tables == null || destination.Tables.Count < 5)
                {
                    log.ErrorFormat("Delete failed: loading current permission resulted in empty data set or data set with less than 5 tables");

                    return result;
                }

                RenameTables(destination);

                // save backup of existing data before continuing 
                string backupFileName = AddSuffix(fileName, "_backup");

                destination.WriteXml(backupFileName);

                Dictionary<string, int> rolesDictionary = new Dictionary<string, int>();
                Dictionary<string, int> permissionsDictionary = new Dictionary<string, int>();
                Dictionary<string, int> permissionItemsDictionary = new Dictionary<string, int>();

                #region Role
                {
                    var destinationRoles = destination.Tables[ROLE];

                    foreach (DataRow destinationRole in destinationRoles.Rows)
                    {
                        int destinationId = Convert.ToInt32(destinationRole["id"]);
                        string destinationName = Convert.ToString(destinationRole["name"]);

                        rolesDictionary[destinationName] = destinationId;
                    }

                    if (source.Tables.Contains(ROLE))
                    {
                        var sourceTable = source.Tables[ROLE];

                        if (sourceTable != null)
                        {
                            destinationRoles.PrimaryKey = new DataColumn[] { destinationRoles.Columns["id"] };

                            foreach (DataRow sourceRole in sourceTable.Rows)
                            {
                                string sourceName = Convert.ToString(sourceRole["name"]);

                                DataRow destinationRole = destinationRoles.AsEnumerable().FirstOrDefault(row =>
                                    Convert.ToString(row["name"]) == sourceName);

                                if (destinationRole != null)
                                {
                                    int destinationId = Convert.ToInt32(destinationRole["id"]);
                                    string destinationName = Convert.ToString(destinationRole["name"]);

                                    bool actionResult = ApiDAL.DeleteRole(0, destinationId);

                                    log.InfoFormat("!!DELETE!! Table : {0} destinationId : {1} Source name : {2}", ROLE, destinationId, sourceName);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Permission
                {
                    var destinationTable = destination.Tables[PERMISSION];

                    // Delete - permission
                    foreach (DataRow destinationRow in destinationTable.Rows)
                    {
                        int destinationId = Convert.ToInt32(destinationRow["id"]);
                        string destinationName = Convert.ToString(destinationRow["name"]);

                        permissionsDictionary[destinationName] = destinationId;
                    }

                    if (source.Tables.Contains(PERMISSION))
                    {
                        var sourceTable = source.Tables[PERMISSION];

                        if (sourceTable != null)
                        {
                            destinationTable.PrimaryKey = new DataColumn[] { destinationTable.Columns["id"] };

                            foreach (DataRow sourceRow in sourceTable.Rows)
                            {
                                int sourceId = Convert.ToInt32(sourceRow["id"]);
                                string sourceName = Convert.ToString(sourceRow["name"]);

                                DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                                    Convert.ToString(row["name"]) == sourceName);

                                if (destinationRow != null)
                                {
                                    int destinationId = Convert.ToInt32(destinationRow["id"]);
                                    string destinationName = Convert.ToString(destinationRow["name"]);

                                    bool actionResult = ApiDAL.DeletePermission(destinationId);

                                    log.InfoFormat("!!DELETE!! Table : {0} Id : {1} name : {2}", PERMISSION, destinationId, destinationName);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Role Permission
                {
                    if (source.Tables.Contains(ROLE_PERMISSION))
                    {
                        var sourceTable = source.Tables[ROLE_PERMISSION];

                        if (sourceTable != null)
                        {
                            var destinationTable = destination.Tables[ROLE_PERMISSION];

                            destinationTable.PrimaryKey = new DataColumn[] { destinationTable.Columns["id"] };

                            foreach (DataRow sourceRow in sourceTable.Rows)
                            {
                                int sourceId = Convert.ToInt32(sourceRow["id"]);
                                int sourceRoleId = Convert.ToInt32(sourceRow["role_id"]);
                                int sourcePermissionId = Convert.ToInt32(sourceRow["permission_id"]);
                                string sourceRoleName = ExtractValue<string>(sourceRow, "role_name");
                                string sourcePermissionName = ExtractValue<string>(sourceRow, "permission_name");

                                DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                                     sourceRoleName == Convert.ToString(row["role_name"]) &&
                                     sourcePermissionName == Convert.ToString(row["permission_name"]));
                                
                                if (destinationRow != null)
                                {
                                    int destinationId = Convert.ToInt32(destinationRow["id"]);

                                    bool actionResult = ApiDAL.DeletePermissionRole(destinationId);

                                    log.InfoFormat("!!DELETE!! Table : {0} Id : {1} role name : {2} permission name : {3}",
                                        ROLE_PERMISSION, destinationId, sourceRoleName, sourcePermissionName);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Permission Item
                {
                    if (source.Tables.Contains(PERMISSION_ITEM))
                    {
                        var destinationTable = destination.Tables[PERMISSION_ITEM];

                        foreach (DataRow destinationRow in destinationTable.Rows)
                        {
                            int destinationId = Convert.ToInt32(destinationRow["id"]);
                            string destinationName = ExtractValue<string>(destinationRow, "name");

                            permissionItemsDictionary[destinationName] = destinationId;
                        }

                        var sourceTable = source.Tables[PERMISSION_ITEM];

                        if (sourceTable != null)
                        {
                            destinationTable.PrimaryKey = new DataColumn[] { destinationTable.Columns["id"] };

                            foreach (DataRow sourceRow in sourceTable.Rows)
                            {
                                int sourceId = Convert.ToInt32(sourceRow["id"]);
                                string sourceName = ExtractValue<string>(sourceRow, "name");

                                DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                                     sourceName == ExtractValue<string>(row, "name"));

                                if (destinationRow != null)
                                {
                                    int destinationId = Convert.ToInt32(destinationRow["id"]);
                                    string destinationName = ExtractValue<string>(destinationRow, "name");

                                    bool actionResult = ApiDAL.DeletePermissionItem(destinationId);

                                    log.InfoFormat("!!DELETE!! Table : {0} Id : {1} name : {2}", PERMISSION_ITEM, destinationId, destinationName);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Permission Permission Item
                {
                    if (source.Tables.Contains(PERMISSION_PERMISSION_ITEM))
                    {
                        var sourceTable = source.Tables[PERMISSION_PERMISSION_ITEM];

                        if (sourceTable != null)
                        {
                            var destinationTable = destination.Tables[PERMISSION_PERMISSION_ITEM];

                            destinationTable.PrimaryKey = new DataColumn[] { destinationTable.Columns["id"] };

                            foreach (DataRow sourceRow in sourceTable.Rows)
                            {
                                int sourceId = Convert.ToInt32(sourceRow["id"]);
                                int sourcePermissionId = ExtractValue<int>(sourceRow, "permission_id");
                                int sourcePermissionItemId = ExtractValue<int>(sourceRow, "permission_item_id");
                                string sourcePermissionName = ExtractValue<string>(sourceRow, "permission_name");
                                string sourcePermissionItemName = ExtractValue<string>(sourceRow, "permission_item_name");

                                DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                                     sourcePermissionName == ExtractValue<string>(row, "permission_name") &&
                                     sourcePermissionItemName == ExtractValue<string>(row, "permission_item_name"));

                                //int destinationPermissionId = permissionsDictionary[sourcePermissionName];
                                //int destinationPermissionItemId = permissionItemsDictionary[sourcePermissionItemName];

                                if (destinationRow != null)
                                {
                                    int destinationId = Convert.ToInt32(destinationRow["id"]);

                                    bool actionResult = ApiDAL.DeletePermissionPermissionItem(destinationId);

                                    log.InfoFormat("!!DELETE!! Table : {0} Id : {1} permission name : {2} permission item name : {3}",
                                        PERMISSION_PERMISSION_ITEM, destinationId, sourcePermissionName, sourcePermissionItemName);
                                }
                            }
                        }
                    }
                }
                #endregion

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Failed deleting permissions, ex = {0}", ex);
            }

            log.InfoFormat("Finished deleting of permission from file {0}, result is {1}", fileName, result);

            return result;
        }

        public static bool InitializeFolder(string folderName)
        {
            bool result = false;

            try
            {
                if (string.IsNullOrEmpty(folderName))
                {
                    return result;
                }

                InitializeLogging();
                ConfigurationManager.ApplicationConfiguration.Initialize(true, true);

                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }

                string permissionItemsFolderName = string.Format("{0}\\permission_items", folderName);
                string permissionItemsControllersFolderName = string.Format("{0}\\controllers", permissionItemsFolderName);
                string permissionItemsObjectsFolderName = string.Format("{0}\\objects", permissionItemsFolderName);

                if (!Directory.Exists(permissionItemsFolderName))
                {
                    Directory.CreateDirectory(permissionItemsFolderName);
                }

                if (!Directory.Exists(permissionItemsControllersFolderName))
                {
                    Directory.CreateDirectory(permissionItemsControllersFolderName);
                }

                if (!Directory.Exists(permissionItemsObjectsFolderName))
                {
                    Directory.CreateDirectory(permissionItemsObjectsFolderName);
                }

                JsonSerializer jsonSerlizer = JsonSerializer.CreateDefault();
                jsonSerlizer.NullValueHandling = NullValueHandling.Ignore;

                Roles roles;
                Dictionary<string, PermissionItems> permissionItemsFiles;
                Permissions permissions;

                BuildPermissionsDictionaries(out roles, out permissionItemsFiles, out permissions);

                #region Write files

                JObject rolesObject = JObject.FromObject(roles, jsonSerlizer);

                string rolesString = rolesObject.ToString();

                File.WriteAllText(string.Format("{0}\\roles.json", folderName), rolesString);

                JObject permissionsObject = JObject.FromObject(permissions, jsonSerlizer);

                string permissionsString = permissionsObject.ToString();

                File.WriteAllText(string.Format("{0}\\permissions.json", folderName), permissionsString);

                foreach (var permissionItemFile in permissionItemsFiles.Values)
                {
                    JObject permissionItemObject = JObject.FromObject(permissionItemFile, jsonSerlizer);

                    string permissionItemString = permissionItemObject.ToString();

                    string currentFolder = string.Empty;

                    switch (permissionItemFile.type)
                    {
                        case ePermissionItemType.Parameter:
                            {
                                currentFolder = permissionItemsObjectsFolderName;
                                break;
                            }
                        case ePermissionItemType.Action:
                        case ePermissionItemType.Argument:
                            {
                                currentFolder = permissionItemsControllersFolderName;
                                break;
                            }
                        default:
                            break;
                    }

                    if (string.IsNullOrEmpty(currentFolder))
                    {
                        log.ErrorFormat("Could not find folder for permission item {0}", permissionItemFile.name);
                    }
                    else
                    {
                        string filePath = string.Format("{0}\\{1}.json", currentFolder, permissionItemFile.name);
                        File.WriteAllText(filePath, permissionItemString);
                    }
                }

                #endregion

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                log.ErrorFormat("Failed initializing folder of permissions. ex = {0}", ex);
            }

            return result;
        }

        public static bool LoadFolder(string folderName)
        {
            bool result = false;

            if (string.IsNullOrEmpty(folderName))
            {
                return result;
            }

            string jsonString = string.Empty;

            try
            {
                InitializeLogging();
                ConfigurationManager.ApplicationConfiguration.Initialize(true, true);

                #region Get data from files

                string[] allfiles = Directory.GetFiles(folderName, "*.json", SearchOption.AllDirectories);

                List<FilePermission> permissionsFromFiles = new List<FilePermission>();
                Dictionary<string, FilePermission> dictionaryPermissionsFromFiles = new Dictionary<string, FilePermission>();
                List<FileRole> rolesFromFiles = new List<FileRole>();
                Dictionary<string, FileRole> dictionaryRolesFromFiles = new Dictionary<string, FileRole>();
                List<FilePermissionItem> permissionItemsFromFiles = new List<FilePermissionItem>();
                Dictionary<string, FilePermissionItem> dictionaryPermissionItemsFromFiles = new Dictionary<string, FilePermissionItem>();
                
                foreach (string filePath in allfiles)
                {
                    jsonString = File.ReadAllText(filePath);

                    var fileJson = JObject.Parse(jsonString);

                    var roles = fileJson["roles"];
                    var permissions = fileJson["permissions"];
                    var permissionItems = fileJson["permission_items"];

                    if (roles != null)
                    {
                        foreach (JObject roleJson in roles)
                        {
                            FileRole role = roleJson.ToObject<FileRole>();

                            if (role != null)
                            {
                                rolesFromFiles.Add(role);

                                if (!dictionaryRolesFromFiles.ContainsKey(role.Name))
                                {
                                    dictionaryRolesFromFiles.Add(role.Name, role);
                                }
                            }
                        }
                    }

                    if (permissions != null)
                    {
                        foreach (JObject permissionJson in permissions)
                        {
                            FilePermission permission = permissionJson.ToObject<FilePermission>();

                            if (permission != null)
                            {
                                permissionsFromFiles.Add(permission);

                                if (!dictionaryPermissionsFromFiles.ContainsKey(permission.Name))
                                {
                                    dictionaryPermissionsFromFiles.Add(permission.Name, permission);
                                }
                            }
                        }
                    }

                    if (permissionItems != null)
                    {
                        foreach (JObject permissionItemJson in permissionItems)
                        {
                            FilePermissionItem permissionItem = permissionItemJson.ToObject<FilePermissionItem>();

                            if (permissionItem != null)
                            {
                                permissionItemsFromFiles.Add(permissionItem);

                                if (!dictionaryPermissionItemsFromFiles.ContainsKey(permissionItem.Name))
                                {
                                    dictionaryPermissionItemsFromFiles.Add(permissionItem.Name, permissionItem);
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Compare to database

                #region Get data, build dictionaries

                Roles rolesFromDatabase;
                Dictionary<string, FileRole> dictionaryRolesFromDatabase = new Dictionary<string, FileRole>();
                Dictionary<string, PermissionItems> permissionItemsFromDatabase;
                Dictionary<string, FilePermissionItem> dictionaryPermissionItemsFromDatabase = new Dictionary<string, FilePermissionItem>();
                Permissions permissionsFromDatabase;
                Dictionary<string, FilePermission> dictionaryPermissionsFromDatabase = new Dictionary<string, FilePermission>();

                BuildPermissionsDictionaries(out rolesFromDatabase, out permissionItemsFromDatabase, out permissionsFromDatabase);
                DataSet dataSet = ApiDAL.Get_PermissionsForExport();
                DataTable rolePermissionsTable = null;
                DataTable permissionPermissionItemTable = null;

                if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count >= 5)
                {
                    RenameTables(dataSet);
                    rolePermissionsTable = dataSet.Tables[ROLE_PERMISSION];
                    permissionPermissionItemTable = dataSet.Tables[PERMISSION_PERMISSION_ITEM];
                }

                #endregion

                #region Permissions

                // run on all permissions we found in DB
                foreach (var permission in permissionsFromDatabase.permissions)
                {
                    // fill dictionary of permission we found in DB
                    dictionaryPermissionsFromDatabase[permission.Name] = permission;

                    // check if exists in files
                    if (dictionaryPermissionsFromFiles.ContainsKey(permission.Name))
                    {
                        var filePermission = dictionaryPermissionsFromFiles[permission.Name];

                        // if permission exists both in file and in DB - check if update is required (only users_group is relevant)
                        if (permission.UsersGroup != filePermission.UsersGroup)
                        {
                            ApiDAL.UpdatePermission((int)permission.Id, filePermission.Name, (int)ePermissionType.Group, filePermission.UsersGroup);
                            log.InfoFormat("!! UPDATE !! Permissions - id {0} name {1}", permission.Id, permission.Name);
                        }
                    }
                    else
                    {
                        // if permission exists in DB but not in file - it should be deleted
                        ApiDAL.DeletePermission((int)permission.Id);
                        log.InfoFormat("!! DELETE !! Permissions - id {0} name {1}", permission.Id, permission.Name);

                    }
                }

                // run on all permissions we have in files
                foreach (var permission in permissionsFromFiles)
                {
                    // check if exists in DB
                    if (!dictionaryPermissionsFromDatabase.ContainsKey(permission.Name))
                    {
                        // if not, it is new and should be added
                        int newId = ApiDAL.InsertPermission(permission.Name, (int)ePermissionType.Group, permission.UsersGroup);
                        permission.Id = newId;

                        log.InfoFormat("!! INSERT !! Permissions - id {0} name {1}", permission.Id, permission.Name);

                        // update dictionary for future use - id is important
                        dictionaryPermissionsFromDatabase[permission.Name] = permission;
                    }
                }

                #endregion

                #region Roles

                // run on all roles we found in DB
                foreach (var dbRole in rolesFromDatabase.roles)
                {
                    dictionaryRolesFromDatabase[dbRole.Name] = dbRole;

                    // find role in files - if exists it might be an update
                    if (dictionaryRolesFromFiles.ContainsKey(dbRole.Name))
                    {
                        var roleFromFile = dictionaryRolesFromFiles[dbRole.Name];

                        // bool = is excluded
                        // create dictionary of data from file
                        Dictionary<string, bool> rolePermissionsFromFile = new Dictionary<string, bool>();

                        if (roleFromFile.permissionsNames != null)
                        {
                            foreach (var permissionName in roleFromFile.permissionsNames)
                            {
                                rolePermissionsFromFile[permissionName] = false;
                            }
                        }

                        if (roleFromFile.excludedPermissionsNames != null)
                        {
                            foreach (var permissionName in roleFromFile.excludedPermissionsNames)
                            {
                                rolePermissionsFromFile[permissionName] = true;
                            }
                        }

                        // bool = is excluded
                        // create dictionary of data from DB
                        Dictionary<string, bool> rolePermissionFromDatabase = new Dictionary<string, bool>();

                        if (dbRole.permissionsNames != null)
                        {
                            foreach (var permissionName in dbRole.permissionsNames)
                            {
                                rolePermissionFromDatabase[permissionName] = false;
                            }

                            foreach (var permissionName in dbRole.excludedPermissionsNames)
                            {
                                rolePermissionFromDatabase[permissionName] = true;
                            }
                        }

                        // compare role permissions - start with those written in file
                        foreach (var permissionFromFile in rolePermissionsFromFile)
                        {
                            string permissionName = permissionFromFile.Key;
                            bool isExcluded = permissionFromFile.Value;

                            // if role permission exists in file but not in DB - add it to DB
                            if (!rolePermissionFromDatabase.ContainsKey(permissionName))
                            {
                                int permissionId = (int)dictionaryPermissionsFromDatabase[permissionName].Id;
                                ApiDAL.InsertPermissionRole((int)dbRole.Id, permissionId, Convert.ToInt32(isExcluded));

                                log.InfoFormat("!! INSERT !! Permission Role - permission id = {0} role id = {1} permission name = {2} role name = {3}",
                                    permissionId, dbRole.Id, permissionName, dbRole.Name);
                            }
                            else
                            {
                                // if it exists in both, check if is excluded was changed
                                if (isExcluded != rolePermissionFromDatabase[permissionName])
                                {
                                    bool updateResult = false;

                                    if (rolePermissionsTable != null)
                                    {
                                        var rowToUpdate = rolePermissionsTable.AsEnumerable().FirstOrDefault(row =>
                                        Convert.ToString(row["role_name"]) == dbRole.Name &&
                                        Convert.ToString(row["permission_name"]) == permissionName);

                                        if (rowToUpdate != null)
                                        {
                                            int permissionRoleId = Convert.ToInt32(rowToUpdate["id"]);
                                            updateResult = ApiDAL.UpdatePermissionRole(permissionRoleId, Convert.ToInt32(isExcluded));

                                            log.InfoFormat("!! UPDATE !! Permission Role - permission role id = {0}, role name = {1} permission name = {2}",
                                                permissionRoleId, dbRole.Name, permissionName);
                                        }
                                    }

                                    if (!updateResult)
                                    {
                                        log.ErrorFormat("could not update role permission for role = {0} and permission = {1}", dbRole.Name, permissionFromFile.Key);
                                    }
                                }
                            }
                        }

                        // compare role permissions - from DB to files
                        foreach (string permissionName in rolePermissionFromDatabase.Keys)
                        {
                            // if role permission exists in DB but not in file - delete from DB
                            if (!rolePermissionsFromFile.ContainsKey(permissionName))
                            {
                                bool deleteResult = false;

                                if (rolePermissionsTable != null)
                                {
                                    var rowToDelete = rolePermissionsTable.AsEnumerable().FirstOrDefault(row =>
                                    Convert.ToString(row["role_name"]) == dbRole.Name &&
                                    Convert.ToString(row["permission_name"]) == permissionName);

                                    if (rowToDelete != null)
                                    {
                                        int permissionRoleId = Convert.ToInt32(rowToDelete["id"]);
                                        deleteResult = ApiDAL.DeletePermissionRole(permissionRoleId);

                                        log.InfoFormat("!! UPDATE !! Permission Role - permission role id = {0}, role name = {1} permission name = {2}",
                                            permissionRoleId, dbRole.Name, permissionName);
                                    }
                                }

                                if (!deleteResult)
                                {
                                    log.ErrorFormat("could not delete role permission for role = {0} and permission = {1}", dbRole.Name, permissionName);
                                }
                            }
                        }
                    }
                    else
                    {
                        // if role exists in DB but not in file - it should be deleted
                        ApiDAL.DeleteRole(0, (int)dbRole.Id);

                        log.InfoFormat("!! DELETE !! Roles - id {0} name {1}", dbRole.Id, dbRole.Name);
                    }
                }

                // run on all roles we have in files
                foreach (var role in rolesFromFiles)
                {
                    // check if exists in DB
                    if (!dictionaryRolesFromDatabase.ContainsKey(role.Name))
                    {
                        // if not, it is new and should be added
                        int newId = ApiDAL.InsertRole(role.Name);

                        log.InfoFormat("!! INSERT !! Roles - id {0} name {1}", newId, role.Name);

                        role.Id = newId;

                        // update roles dictionary - although it is not important anymore
                        dictionaryRolesFromDatabase[role.Name] = role;

                        // insert role permissions - not excluded
                        foreach (var rolePermissionName in role.permissionsNames)
                        {
                            int permissionId = (int)dictionaryPermissionsFromDatabase[rolePermissionName].Id;
                            ApiDAL.InsertPermissionRole((int)role.Id, permissionId, 0);

                            log.InfoFormat("!! INSERT !! Permission Role - permission id = {0} role id = {1} permission name = {2} role name = {3}",
                                permissionId, newId, rolePermissionName, role.Name);
                        }

                        // insert role permissions - excluded
                        foreach (var rolePermissionName in role.excludedPermissionsNames)
                        {
                            int permissionId = (int)dictionaryPermissionsFromDatabase[rolePermissionName].Id;
                            ApiDAL.InsertPermissionRole((int)role.Id, permissionId, 1);

                            log.InfoFormat("!! INSERT !! Permission Role - permission id = {0} role id = {1} permission name = {2} role name = {3}",
                                permissionId, newId, rolePermissionName, role.Name);
                        }
                    }
                }

                #endregion

                #region Permission Items

                // run on all permission items we found in database
                foreach (var permissionItemGroup in permissionItemsFromDatabase)
                {
                    foreach (var dbPermissionItem in permissionItemGroup.Value.permissionItems)
                    {
                        // fill dictionary of permission items we found in DB
                        dictionaryPermissionItemsFromDatabase[dbPermissionItem.Name] = dbPermissionItem;

                        // check if exists in file
                        if (dictionaryPermissionItemsFromFiles.ContainsKey(dbPermissionItem.Name))
                        {
                            var filePermissionItem = dictionaryPermissionItemsFromFiles[dbPermissionItem.Name];

                            // if permission item exists both in file and if DB - check if update of basic data is required
                            if (dbPermissionItem.Action != filePermissionItem.Action ||
                                dbPermissionItem.Type != filePermissionItem.Type ||
                                dbPermissionItem.Object != filePermissionItem.Object ||
                                dbPermissionItem.Parameter != filePermissionItem.Parameter ||
                                dbPermissionItem.Service != filePermissionItem.Service)
                            {
                                bool updateResult = ApiDAL.UpdatePermissionItem(
                                    (int)dbPermissionItem.Id,
                                    Convert.ToInt32(filePermissionItem.Type),
                                    filePermissionItem.Name,
                                    filePermissionItem.Service,
                                    filePermissionItem.Action,
                                    filePermissionItem.Object,
                                    filePermissionItem.Parameter);

                                log.InfoFormat("!! UPDATE !! Permission Items - id {0} name {1}", dbPermissionItem.Id, filePermissionItem.Name);
                            }

                            //
                            // now let's move on to permission -> permission item relation
                            //

                            // bool = is excluded
                            // create dictionary of data from file
                            Dictionary<string, bool> permissionPermissionItemsFromFile = new Dictionary<string, bool>();

                            if (filePermissionItem.permissionsNames != null)
                            {
                                foreach (var permissionName in filePermissionItem.permissionsNames)
                                {
                                    permissionPermissionItemsFromFile[permissionName] = false;
                                }
                            }

                            if (filePermissionItem.excludedPermissionsNames != null)
                            {
                                foreach (var permissionName in filePermissionItem.excludedPermissionsNames)
                                {
                                    permissionPermissionItemsFromFile[permissionName] = true;
                                }
                            }

                            // bool = is excluded
                            // create dictionary of data from DB
                            Dictionary<string, bool> permissionPermissionItemsFromDatabase = new Dictionary<string, bool>();

                            if (dbPermissionItem.permissionsNames != null)
                            {
                                foreach (var permissionName in dbPermissionItem.permissionsNames)
                                {
                                    permissionPermissionItemsFromDatabase[permissionName] = false;
                                }

                                foreach (var permissionName in dbPermissionItem.excludedPermissionsNames)
                                {
                                    permissionPermissionItemsFromDatabase[permissionName] = true;
                                }
                            }

                            // compare permission permission items - start with those written in file
                            foreach (var permissionFromFile in permissionPermissionItemsFromFile)
                            {
                                string permissionName = permissionFromFile.Key;
                                bool isExcluded = permissionFromFile.Value;

                                // if permission permission items exists in file but not in DB - add it to DB
                                if (!permissionPermissionItemsFromDatabase.ContainsKey(permissionName))
                                {
                                    int permissionId = (int)dictionaryPermissionsFromDatabase[permissionName].Id;
                                    int newPermissionPermissionItemId = 
                                        ApiDAL.InsertPermissionPermissionItem(permissionId, (int)dbPermissionItem.Id, Convert.ToInt32(isExcluded));

                                    log.InfoFormat("!! INSERT !! Permission Permission Item - permission id = {0} permission item id = {1} " +
                                        "permission name = {2} permission item name = {3}",
                                        permissionId, dbPermissionItem.Id, permissionName, dbPermissionItem.Name);
                                }
                                else
                                {
                                    // if it exist in both, check if is excluded was changed
                                    if (permissionFromFile.Value != permissionPermissionItemsFromDatabase[permissionName])
                                    {
                                        bool updateResult = false;

                                        if (permissionPermissionItemTable != null)
                                        {
                                            var rowToUpdate = permissionPermissionItemTable.AsEnumerable().FirstOrDefault(row =>
                                            Convert.ToString(row["permission_item_name"]) == filePermissionItem.Name &&
                                            Convert.ToString(row["permission_name"]) == permissionName);

                                            if (rowToUpdate != null)
                                            {
                                                int permissionPermissionItemId = Convert.ToInt32(rowToUpdate["id"]);
                                                updateResult = 
                                                    ApiDAL.UpdatePermissionPermissionItem(permissionPermissionItemId, Convert.ToInt32(isExcluded));

                                                log.InfoFormat("!! UPDATE !! Permission Permission Item - id = {0}, permission item name = {1} permission name = {2}",
                                                    permissionPermissionItemId, filePermissionItem.Name, permissionName);
                                            }
                                        }

                                        if (!updateResult)
                                        {
                                            log.ErrorFormat("could not update permission permission item for permission item = {0} and permission = {1}", 
                                                dbPermissionItem.Name, permissionName);
                                        }
                                    }
                                }
                            }

                            // compare role permissions - from DB to files
                            foreach (string permissionName in permissionPermissionItemsFromDatabase.Keys)
                            {
                                // if role permission exists in DB but not in file - delete from DB
                                if (!permissionPermissionItemsFromFile.ContainsKey(permissionName))
                                {
                                    bool deleteResult = false;

                                    if (permissionPermissionItemTable != null)
                                    {
                                        var rowToDelete = permissionPermissionItemTable.AsEnumerable().FirstOrDefault(row =>
                                        Convert.ToString(row["permission_item_name"]) == dbPermissionItem.Name &&
                                        Convert.ToString(row["permission_name"]) == permissionName);

                                        if (rowToDelete != null)
                                        {
                                            int permissionPermissionItemId = Convert.ToInt32(rowToDelete["id"]);
                                            deleteResult = ApiDAL.DeletePermissionPermissionItem(permissionPermissionItemId);

                                            log.InfoFormat("!! DELETE !! Permission Permission Item - id = {0}, permission item name = {1} permission name = {2}",
                                                permissionPermissionItemId, filePermissionItem.Name, permissionName);
                                        }
                                    }

                                    if (!deleteResult)
                                    {
                                        log.ErrorFormat("could not delete permission permission item for permission item = {0} and permission = {1}",
                                            dbPermissionItem.Name, permissionName);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // if permission item exists in DB but not in file - it should be deleted
                            bool deleteResult = ApiDAL.DeletePermissionItem((int)dbPermissionItem.Id);

                            log.InfoFormat("!! DELETE !! Permission Items - id {0} name {1}", dbPermissionItem.Id, dbPermissionItem.Name);
                        }
                    }
                }

                // run on all permission items we have in files
                foreach (var permissionItem in permissionItemsFromFiles)
                {
                    // check if exists in DB
                    if (!dictionaryPermissionItemsFromDatabase.ContainsKey(permissionItem.Name))
                    {
                        // if not, it is new and should be added
                        int newId = ApiDAL.InsertPermissionItem(permissionItem.Name, (int)permissionItem.Type,
                            permissionItem.Service, permissionItem.Action, permissionItem.Object, permissionItem.Parameter);

                        permissionItem.Id = newId;

                        log.InfoFormat("!! INSERT !! Permission Items - id {0} name {1}", newId, permissionItem.Name);

                        dictionaryPermissionItemsFromDatabase[permissionItem.Name] = permissionItem;

                        // insert permission permission items - not excluded
                        foreach (var permissionItemPermissionName in permissionItem.permissionsNames)
                        {
                            int permissionId = (int)dictionaryPermissionsFromDatabase[permissionItemPermissionName].Id;
                            ApiDAL.InsertPermissionPermissionItem(permissionId, (int)newId, 0);

                            log.InfoFormat("!! INSERT !! Permission Permission Item - permission id = {0} permission item id = {1} " +
                                "permission name = {2} permission item name = {3}",
                                permissionId, permissionItem.Id, permissionItemPermissionName, permissionItem.Name);
                        }

                        // insert permission permission items - excluded
                        foreach (var permissionItemPermissionName in permissionItem.excludedPermissionsNames)
                        {
                            int permissionId = (int)dictionaryPermissionsFromDatabase[permissionItemPermissionName].Id;
                            ApiDAL.InsertPermissionPermissionItem(permissionId, (int)newId, 1);

                            log.InfoFormat("!! INSERT !! Permission Permission Item - permission id = {0} permission item id = {1} " +
                                "permission name = {2} permission item name = {3}",
                                permissionId, permissionItem.Id, permissionItemPermissionName, permissionItem.Name);
                        }
                    }
                }

                #endregion

                #endregion

                result = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed loading folder of permissions. ex = {0}, jsonString = {1}", ex, jsonString);
            }

            return result;
        }
        
        #endregion

        #region Utility Methods

        private static void BuildPermissionsDictionaries(
            out Roles roles, out Dictionary<string, PermissionItems> 
            permissionItemsFiles, out Permissions permissions)
        {
            List<Role> allRoles = ApiDAL.GetRoles(0, null);

            List<FileRole> slimRoles = allRoles.Select(r => new FileRole(r)).ToList();

            roles = new Roles()
            {
                roles = slimRoles
            };

            Dictionary<long, FilePermission> permissionsDictionary = new Dictionary<long, FilePermission>();
            Dictionary<long, FilePermissionItem> permissionItemsDictionary = new Dictionary<long, FilePermissionItem>();
            permissionItemsFiles = new Dictionary<string, PermissionItems>();

            // create dictionary of permissions and permission items by their id
            foreach (var role in allRoles)
            {
                if (role != null && role.Permissions != null)
                {
                    foreach (var permission in role.Permissions)
                    {
                        permissionsDictionary[permission.Id] = new FilePermission(permission as GroupPermission);

                        if (permission.PermissionItems != null)
                        {
                            foreach (var permissionItem in permission.PermissionItems)
                            {
                                FilePermissionItem slimPermissionItem = new FilePermissionItem(permissionItem);

                                string permissionItemFileName = slimPermissionItem.GetFileName().ToLower();

                                if (!string.IsNullOrEmpty(permissionItemFileName))
                                {
                                    if (!permissionItemsFiles.ContainsKey(permissionItemFileName))
                                    {
                                        permissionItemsFiles[permissionItemFileName] = new PermissionItems()
                                        {
                                            name = permissionItemFileName,
                                            type = slimPermissionItem.Type
                                        };
                                    }

                                    if (!permissionItemsDictionary.ContainsKey(permissionItem.Id))
                                    {
                                        permissionItemsDictionary[permissionItem.Id] = slimPermissionItem;

                                        permissionItemsFiles[permissionItemFileName].permissionItems.Add(slimPermissionItem);
                                    }
                                    else
                                    {
                                        slimPermissionItem = permissionItemsDictionary[permissionItem.Id];
                                    }

                                    if (!permissionItem.IsExcluded)
                                    {
                                        slimPermissionItem.permissionsNames.Add(permission.Name);
                                    }
                                    else
                                    {
                                        slimPermissionItem.excludedPermissionsNames.Add(permission.Name);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var slimPermissions = permissionsDictionary.Values.ToList();

            permissions = new Permissions()
            {
                permissions = slimPermissions
            };
        }

        private static void RenameTables(DataSet dataSet)
        {
            dataSet.Tables[0].TableName = ROLE;
            dataSet.Tables[1].TableName = PERMISSION;
            dataSet.Tables[2].TableName = ROLE_PERMISSION;
            dataSet.Tables[3].TableName = PERMISSION_ITEM;
            dataSet.Tables[4].TableName = PERMISSION_PERMISSION_ITEM;
            dataSet.DataSetName = "permissions_dataset";
        }

        private static string AddSuffix(string fileName, string suffix)
        {
            string fDir = Path.GetDirectoryName(fileName);
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string fExt = Path.GetExtension(fileName);
            return Path.Combine(fDir, String.Concat(fName, suffix, fExt));
        }

        /// <summary>
        /// Extracts a dynamic type value from a data row in the most efficient way
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T ExtractValue<T>(DataRow row, string fieldName)
        {
            T result = default(T);

            try
            {
                if (row != null)
                {
                    object value = row[fieldName];

                    if (value != null && value != DBNull.Value)
                    {
                        result = (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }
            catch
            {
            }

            return (result);
        }

        /// <summary>
        /// Extracts a nullable integer value from a data row in the most efficient way
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static int? ExtractNullableInteger(DataRow row, string fieldName)
        {
            int? result = null;

            try
            {
                if (row != null)
                {
                    object value = row[fieldName];

                    if (value != null && value != DBNull.Value)
                    {
                        result = Convert.ToInt32(value);
                    }
                }
            }
            catch
            {
            }

            return (result);
        }

        private static void InitializeLogging()
        {
            // build log4net partial file name
            string partialLogName = string.Empty;

            //// try to get application name from virtual path
            //string ApplicationAlias = HostingEnvironment.ApplicationVirtualPath;
            //if (ApplicationAlias.Length > 2)
            //    partialLogName = ApplicationAlias.Substring(1);
            //else
            //{
            //    // try to get application name from application ID
            //    string applicationID = HostingEnvironment.ApplicationID;
            //    if (!string.IsNullOrEmpty(applicationID))
            //    {
            //        var appIdArr = applicationID.Split('/');
            //        if (appIdArr != null && appIdArr.Length > 0)
            //            partialLogName = appIdArr[appIdArr.Length - 1];
            //    }
            //}

            if (string.IsNullOrWhiteSpace(partialLogName))
            {
                // error getting application name - invent a log name
                partialLogName = Guid.NewGuid().ToString();
            }

            log4net.GlobalContext.Properties["LogName"] = partialLogName;

            // set monitor and log configuration files
            KMonitor.Configure("log4net.config", KLogEnums.AppType.WS);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WS);
        }

        #endregion
    }
}
