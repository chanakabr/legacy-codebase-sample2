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
            ConfigurationManager.ApplicationConfiguration.Initialize(true);

            try
            {
                DataSet allPermissions = ApiDAL.Get_PermissionsForExport();

                if (allPermissions != null && allPermissions.Tables != null && allPermissions.Tables.Count >= 5)
                {
                    RenameTables(allPermissions);

                    allPermissions.WriteXml(fileName);

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

            ConfigurationManager.ApplicationConfiguration.Initialize(true);

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

                        int? sourceIsExcluded = ExtractValue<int?>(sourceRow, "is_excluded");

                        DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                             sourceRoleName == Convert.ToString(row["role_name"]) &&
                             sourcePermissionName == Convert.ToString(row["permission_name"]));

                        int destinationRoleId = rolesDictionary[sourceRoleName];
                        int destinationPermissionId = permissionsDictionary[sourcePermissionName];

                        // not found - it is new, insert
                        if (destinationRow == null)
                        {
                            int newId = ApiDAL.InsertPermissionRole(sourceRoleId, destinationRoleId, destinationPermissionId);

                            log.InfoFormat("!!INSERT!! Table : {0} Source Id : {1} role_id : {2} permission_id : {3} is_excluded : {4} role_name : {5} permission_name : {6}",
                                ROLE_PERMISSION,
                                sourceId, destinationRoleId, destinationPermissionId, sourceIsExcluded,
                                sourceRoleName,
                                sourcePermissionName);
                        }
                        else
                        // found - check for differences
                        {
                            int destinationId = Convert.ToInt32(destinationRow["id"]);

                            int? destinationIsExcluded = ExtractValue<int?>(destinationRow, "is_excluded");

                            if (
                                sourceIsExcluded != destinationIsExcluded
                                )
                            {
                                bool actionResult = ApiDAL.UpdatePermissionRole(destinationId, sourceIsExcluded);

                                log.InfoFormat("!!UPDATE!! Table : {0} destination Id : {1} role_id : {2} permission_id : {3} is_excluded : {4} role_name : {5} permission_name : {6}",
                                ROLE_PERMISSION,
                                sourceId, destinationRoleId, destinationPermissionId, sourceIsExcluded,
                                sourceRoleName,
                                sourcePermissionName);
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
                        int? sourceType = ExtractValue<int?>(sourceRow, "type");
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
                            int? destinationType = ExtractValue<int?>(destinationRow, "type");
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
                                bool actionResult = ApiDAL.UpdatePermissionItem(destinationId, sourceType, destinationName, sourceService, sourceAction, sourceObject, sourceParameter);

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
                        int? sourceIsExcluded = ExtractValue<int?>(sourceRow, "is_excluded");
                        string sourcePermissionName = ExtractValue<string>(sourceRow, "permission_name");
                        string sourcePermissionItemName = ExtractValue<string>(sourceRow, "permission_item_name");

                        DataRow destinationRow = destinationTable.AsEnumerable().FirstOrDefault(row =>
                             sourcePermissionName == ExtractValue<string>(row, "permission_name") &&
                             sourcePermissionItemName == ExtractValue<string>(row, "permission_item_name"));

                        int destinationPermissionId = permissionsDictionary[sourcePermissionName];
                        int destinationPermissionItemId = permissionItemsDictionary[sourcePermissionItemName];

                        // not found - it is new, insert new permission permission item
                        if (destinationRow == null)
                        {
                            int newId = ApiDAL.InsertPermissionPermissionItem(destinationPermissionId, destinationPermissionItemId, sourceIsExcluded);

                            log.InfoFormat(
                                "!!INSERT!! Table : {0} Source Id : {1} permission_id : {2} permission_item_id: {3} is_excluded : {4} permission_name : {5} permission_item_name : {6}",
                                PERMISSION_PERMISSION_ITEM, sourceId, destinationPermissionId, destinationPermissionItemId, sourceIsExcluded,
                                sourcePermissionName, sourcePermissionItemName);
                        }
                        else
                        // found - check for differences - permission permission item
                        {
                            int destinationId = Convert.ToInt32(destinationRow["id"]);
                            int? destinationIsExcluded = ExtractValue<int?>(destinationRow, "is_excluded");

                            if (
                                sourceIsExcluded != destinationIsExcluded
                                )
                            {
                                bool actionResult = ApiDAL.UpdatePermissionPermissionItem(destinationId, sourceIsExcluded);

                                log.InfoFormat(
                                    "!!UPDATE!! Table : {0} Destination Id : {1} permission_id : {2} permission_item_id: {3} is_excluded : {4} permission_name : {5} permission_item_name : {6}",
                                    PERMISSION_PERMISSION_ITEM, sourceId, destinationPermissionId, destinationPermissionItemId, sourceIsExcluded,
                                    sourcePermissionName, sourcePermissionItemName);
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
            ConfigurationManager.ApplicationConfiguration.Initialize(true);

            log.InfoFormat("Starting deleting of permissions from file {0}", fileName);

            try
            {
                DataSet source = new DataSet();
                source.ReadXml(fileName);

                if (source == null || source.Tables == null || source.Tables.Count < 5)
                {
                    log.ErrorFormat("Delete failed: reading from XML resulted in empty data set or data set with less than 5 tables");

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
                #endregion

                #region Role Permission
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

                            int destinationRoleId = rolesDictionary[sourceRoleName];
                            int destinationPermissionId = permissionsDictionary[sourcePermissionName];
                            
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
                #endregion

                #region Permission Item
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
                #endregion

                #region Permission Permission Item
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

                            int destinationPermissionId = permissionsDictionary[sourcePermissionName];
                            int destinationPermissionItemId = permissionItemsDictionary[sourcePermissionItemName];
                            
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
        #endregion

        #region Utility Methods

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
