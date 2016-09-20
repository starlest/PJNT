namespace ECRP.ViewModels.Customer
{
    using Models.Customer;
    using MVVMFramework;

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class CustomerGroupVM : ViewModelBase<CustomerGroup>
    {
        public int ID => Model.ID;

        public string Name => Model.Name;

        public int CreditTerms => Model.CreditTerms;

        public int MaxInvoices => Model.MaxInvoices;

        public override bool Equals(object obj)
        {
            var customerGroup = obj as CustomerGroupVM;
            return customerGroup != null && ID.Equals(customerGroup.ID);
        }
    }
}