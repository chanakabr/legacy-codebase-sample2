
namespace ApiObjects
{
    public class CDVRAdapterBase
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public CDVRAdapterBase() 
        {
        } 

        public CDVRAdapterBase(CDVRAdapterBase cdvrAdapterBase)
        {
            this.ID = cdvrAdapterBase.ID;
            this.Name = cdvrAdapterBase.Name;
        }

        public CDVRAdapterBase(int id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
