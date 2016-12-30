namespace ECERP.ViewModels.Customer
{
    using Models.Customer;
    using MVVMFramework;

    public class CityVM: ViewModelBase<City>
    {
        public int ID => Model.ID;

        public string Name => Model.Name;
    }
}
