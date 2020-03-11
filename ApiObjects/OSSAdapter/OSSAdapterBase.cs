
namespace ApiObjects
{
    public class OSSAdapterBase
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public OSSAdapterBase()
        {
        }

        public OSSAdapterBase(OSSAdapterBase ossAdapterBase)
        {
            this.ID = ossAdapterBase.ID;
            this.Name = ossAdapterBase.Name;
        }

        public OSSAdapterBase(int id, string name)
        {
            this.ID = id;
            this.Name = name;
        }
    }
}
