using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class DynamicMailRequest : MailRequestObj
    {
        public List<KeyValuePair> values;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();

            MCGlobalMergeVars firstNameMergeVar = new MCGlobalMergeVars();
            firstNameMergeVar.name = "FIRSTNAME";
            firstNameMergeVar.content = this.m_sFirstName;
            retVal.Add(firstNameMergeVar);
            
            MCGlobalMergeVars lastNameMergeVar = new MCGlobalMergeVars();
            lastNameMergeVar.name = "LASTNAME";
            lastNameMergeVar.content = this.m_sLastName;
            retVal.Add(lastNameMergeVar);


            if (values != null)
            {
                foreach (KeyValuePair item in values)
                {
                    if (retVal.Where(x => x.name == item.key).Count() == 0) // this key name not exsits yet! 
                    {
                        retVal.Add(
                            new MCGlobalMergeVars()
                            {
                                name = item.key,
                                content = item.value
                            });
                    }
                }
            }

            return retVal;
        }
    }
}
