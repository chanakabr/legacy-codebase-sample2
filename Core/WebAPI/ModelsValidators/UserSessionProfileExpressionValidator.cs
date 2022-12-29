using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.Users.UserSessionProfile;

namespace WebAPI.ModelsValidators
{
    public static class UserSessionProfileExpressionValidator
    {
        public static void Validate(this KalturaUserSessionProfileExpression model)
        {
            switch (model)
            {
                case KalturaUserSessionCondition c: c.Validate(); break;
                case KalturaExpressionAnd c: c.Validate(); break;
                case KalturaExpressionNot c: c.Validate(); break;
                case KalturaExpressionOr c: c.Validate(); break;
                default: throw new NotImplementedException($"Validate for {model.objectType} is not implemented");
            }
        }

        public static int ConditionsSum(this KalturaUserSessionProfileExpression model)
        {
            switch (model)
            {
                case KalturaUserSessionCondition c: return c.ConditionsSum();
                case KalturaExpressionAnd c: return c.ConditionsSum(); 
                case KalturaExpressionNot c: return c.ConditionsSum();
                case KalturaExpressionOr c: return c.ConditionsSum();
                default: throw new NotImplementedException($"ConditionsSum for {model.objectType} is not implemented");
            }
        }
    }

    public static class UserSessionConditionValidator
    {
        private static readonly HashSet<KalturaRuleConditionType> VALID_CONDITIONS = new HashSet<KalturaRuleConditionType>()
        {
            KalturaRuleConditionType.SEGMENTS,
            KalturaRuleConditionType.DYNAMIC_KEYS,
            KalturaRuleConditionType.DEVICE_BRAND,
            KalturaRuleConditionType.DEVICE_FAMILY,
            KalturaRuleConditionType.DEVICE_MANUFACTURER,
            KalturaRuleConditionType.DEVICE_MODEL,
            KalturaRuleConditionType.DEVICE_DYNAMIC_DATA
        };

        public static int ConditionsSum(this KalturaUserSessionCondition model)
        {
            return model.Condition.ConditionsCount();
        }

        public static void Validate(this KalturaUserSessionCondition model)
        {
            if (model.Condition == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "condition");
            }

            if (!VALID_CONDITIONS.Contains(model.Condition.Type))
            {
                throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "condition", model.Condition.objectType);
            }

            model.Condition.Validate(VALID_CONDITIONS);
        }
    }

    public static class ExpressionAndValidator
    {
        public static int ConditionsSum(this KalturaExpressionAnd model)
        {
            return model.Expressions.Sum(_ => _.ConditionsSum());
        }

        public static void Validate(this KalturaExpressionAnd model)
        {
            foreach (var item in model.Expressions)
            {
                item.Validate();
            }
        }
    }

    public static class ExpressionNotValidator
    {
        public static int ConditionsSum(this KalturaExpressionNot model)
        {
            return model.Expression.ConditionsSum();
        }

        public static void Validate(this KalturaExpressionNot model)
        {
            if (model.Expression == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "expression");
            }

            model.Expression.Validate();
        }
    }

    public static class ExpressionOrValidator
    {
        public static int ConditionsSum(this KalturaExpressionOr model)
        {
            return model.Expressions.Sum(_ => _.ConditionsSum());
        }

        public static void Validate(this KalturaExpressionOr model)
        {
            foreach (var item in model.Expressions)
            {
                item.Validate();
            }
        }
    }
}
