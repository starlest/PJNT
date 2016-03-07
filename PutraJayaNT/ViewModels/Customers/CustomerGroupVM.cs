using MVVMFramework;
using PutraJayaNT.Models;

namespace PutraJayaNT.ViewModels.Customers
{
    public class CustomerGroupVM : ViewModelBase<CustomerGroup>
    {
        public int ID => Model.ID;

        public string Name => Model.Name;

        public int CreditTerms => Model.CreditTerms;

        public int MaxInvoices => Model.MaxInvoices;

        public override bool Equals(object obj)
        {
            var customerGroup = obj as CustomerGroupVM;
            if (customerGroup == null) return false;
            else return this.ID.Equals(customerGroup.ID);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
