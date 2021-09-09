using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Core.Users;
using DAL;
using KLogMonitor;
using Microsoft.Extensions.Logging;

namespace ApiLogic.Users
{
    public interface IDomainLimitationModuleRepository
    {
        LimitationsManager Add(int groupId, long updaterId, LimitationsManager limitationsManager);
        LimitationsManager Update(int groupId, long updaterId, LimitationsManager limitationsManager);
        LimitationsManager Get(int groupId, int limitId);
        IEnumerable<int> GetDomainLimitationModuleIds(int groupId);
        bool Delete(int limitId, long updaterId);
    }

    public class DomainLimitationModuleRepository : IDomainLimitationModuleRepository
    {
        private readonly ILogger _logger;
        private readonly IDomainLimitationModuleDal _dlmDal;

        public DomainLimitationModuleRepository()
            : this(new DomainLimitationModuleDal(), new KLogger(nameof(DomainLimitationModuleRepository)))
        {
        }

        public DomainLimitationModuleRepository(IDomainLimitationModuleDal dlmDal, ILogger logger)
        {
            _dlmDal = dlmDal;
            _logger = logger;
        }

        public LimitationsManager Add(int groupId, long updaterId, LimitationsManager limitationsManager)
        {
            try
            {
                var limitationsManagerDTO = CreateLimitationsManagerDTO(limitationsManager);
                var resultLimitationsManagerDTO = _dlmDal.InsertGroupLimitsAndDeviceFamilies(limitationsManagerDTO, groupId, updaterId);
                var limitationManager = CreateLimitationsManager(resultLimitationsManagerDTO);

                return limitationManager;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while {nameof(Add)}: {e.Message}.");

                return null;
            }
        }

        public LimitationsManager Get(int groupId, int limitId)
        {
            try
            {
                var dataSet = _dlmDal.GetGroupLimitsAndDeviceFamilies(groupId, limitId);
                var oLimitationsManager = CreateLimitationsManager(dataSet);

                return oLimitationsManager;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while {nameof(Get)}: {nameof(groupId)}={groupId}, {nameof(limitId)}={limitId}, {e.Message}.");

                return null;
            }
        }

        public IEnumerable<int> GetDomainLimitationModuleIds(int groupId)
        {
            try
            {
                return _dlmDal.GetGroupDeviceLimitationModules(groupId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while {nameof(GetDomainLimitationModuleIds)}: {nameof(groupId)}={groupId}, {e.Message}.");

                return null;
            }
        }

        public bool Delete(int limitId, long updaterId)
        {
            try
            {
                var deletedRows = _dlmDal.DeleteGroupLimitsAndDeviceFamilies(limitId, updaterId: updaterId);

                return deletedRows > 0;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while {nameof(Delete)}: {nameof(limitId)}={limitId}, {e.Message}.");

                return false;
            }
        }

        private LimitationsManager CreateLimitationsManager(DAL.DTO.LimitationsManagerDTO limitationsManagerDTO)
        {
            LimitationsManager limitationsManager = null;

            #region GroupLevel + DLM Level
            if (limitationsManagerDTO != null) 
            {
                limitationsManager = new LimitationsManager();

                limitationsManager.domianLimitID = limitationsManagerDTO.domianLimitID;
                limitationsManager.DomainLimitName = limitationsManagerDTO.DomainLimitName;
                limitationsManager.Concurrency = limitationsManagerDTO.Concurrency;
                limitationsManager.npvrQuotaInSecs = limitationsManagerDTO.npvrQuotaInSecs;
                limitationsManager.Frequency = limitationsManagerDTO.Frequency;
                limitationsManager.FrequencyDescription = Core.Pricing.Utils.Instance.GetMinPeriodDescription(limitationsManager.Frequency);
                limitationsManager.Quantity = limitationsManagerDTO.Quantity;
                limitationsManager.nUserLimit = limitationsManagerDTO.nUserLimit;
                limitationsManager.UserFrequency = limitationsManagerDTO.UserFrequency;
                limitationsManager.UserFrequencyDescrition = Core.Pricing.Utils.Instance.GetMinPeriodDescription(limitationsManager.UserFrequency);
                limitationsManager.UserFrequency = limitationsManagerDTO.UserFrequency;
                limitationsManager.Description = limitationsManagerDTO.Description;
                #endregion

                #region DeviceFamily
                if (limitationsManagerDTO.lDeviceFamilyLimitations != null)
                {
                    limitationsManager.lDeviceFamilyLimitations = limitationsManagerDTO.lDeviceFamilyLimitations.Select(x => new DeviceFamilyLimitations()
                    {
                        deviceFamily = x.deviceFamily,
                        concurrency = x.concurrency,
                        deviceFamilyName = x.deviceFamilyName,
                        Frequency = x.Frequency,
                        quantity = x.quantity
                    }).ToList();
                }
                #endregion
            }

            return limitationsManager;
        }

        private DAL.DTO.LimitationsManagerDTO CreateLimitationsManagerDTO(LimitationsManager limitationsManager)
        {
            return new DAL.DTO.LimitationsManagerDTO
            {
                domianLimitID = limitationsManager.domianLimitID,
                Concurrency = limitationsManager.Concurrency,
                Frequency = limitationsManager.Frequency,
                Quantity = limitationsManager.Quantity,
                DomainLimitName = limitationsManager.DomainLimitName,
                UserFrequency = limitationsManager.UserFrequency,
                nUserLimit = limitationsManager.nUserLimit,
                Description = limitationsManager.Description,
                lDeviceFamilyLimitations = limitationsManager.lDeviceFamilyLimitations.Select(x => new DAL.DTO.DeviceFamilyLimitationsDTO()
                {
                    concurrency = x.concurrency,
                    deviceFamily = x.deviceFamily,
                    deviceFamilyName = x.deviceFamilyName,
                    Frequency = x.Frequency,
                    quantity = x.quantity
                }).ToList()
            };
        }

        public LimitationsManager Update(int groupId, long updaterId, LimitationsManager limitationsManager)
        {
            try
            {
                var dto = _dlmDal.UpdateGroupLimitsAndDeviceFamilies(groupId, updaterId, CreateLimitationsManagerDTO(limitationsManager));
                var limitationManager = CreateLimitationsManager(dto);

                return limitationManager;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error with DB while trying to {nameof(Update)}: limitId={limitationsManager.domianLimitID}, exception={e.Message}.");

                return null;
            }
        }
    }
}
