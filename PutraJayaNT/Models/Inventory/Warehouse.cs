namespace ECERP.Models.Inventory
{
    #pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Warehouse
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var warehouse = obj as Warehouse;
            return warehouse != null && ID.Equals(warehouse.ID);
        }
    }
}
