namespace PutraJayaNT.Models.Inventory
{
    public class Warehouse
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var warehouse = obj as Warehouse;
            if (warehouse == null) return false;
            else return this.ID.Equals(warehouse.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
