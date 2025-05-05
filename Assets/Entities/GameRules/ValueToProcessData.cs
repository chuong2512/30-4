namespace HoleBox
{
    public class IngressData
    {
        public int ID;
        public int Number;

        public IngressData(int id, int number)
        {
            this.ID     = id;
            this.Number = number;
        }
    }

    public class DistributedData
    {
        public enum DistributedType
        {
            UfoToOnQueue,
            UfoToStatic,
            StaticToOnQueue,
        }

        public DistributedType type;
        public int             fromId;
        public int             toId;
        public int             number;
        public int             colorId;

        public DistributedData(DistributedType type, int fromId, int toId, int number, int colorId)
        {
            this.type    = type;
            this.fromId  = fromId;
            this.toId    = toId;
            this.number  = number;
            this.colorId = colorId;
        }
    }
}