using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using System;

namespace PutraJayaNT.Utilities
{
    class UnitOfWork : IDisposable
    {
        private ERPContext m_Context = null;
        private Repository<User> userRepository = null;
        private Repository<Item> itemRepository = null;
        private Repository<Category> categoryRepository = null;
        private Repository<Supplier> supplierRepository = null;

        private Repository<SalesTransaction> salesTransactionRepository = null;
        private Repository<PurchaseTransaction> purchaseTransactionRepository = null;

        private Repository<LedgerAccount> ledgerAccountRepository = null;
        private Repository<LedgerTransaction> ledgerTransactionRepository = null;
        private Repository<LedgerGeneral> ledgerGeneralRepository = null;
        private Repository<LedgerAccountBalance> ledgerAccountBalanceRepository = null;

        public UnitOfWork()
        {
            m_Context = new ERPContext();
        }

        public void SaveChanges()
        {
            m_Context.SaveChanges();
        }

        public void Dispose()
        {
            ((IDisposable)m_Context).Dispose();
        }

        public Repository<User> UserRepository
        {
            get
            {
                if (userRepository == null)
                    userRepository = new Repository<User>(m_Context);
                return userRepository;
            }
        }

        public Repository<Item> ItemRepository
        {
            get
            {
                if (itemRepository == null)
                    itemRepository = new Repository<Item>(m_Context);
                return itemRepository;
            }
        }

        public Repository<Category> CategoryRepository
        {
            get
            {
                if (categoryRepository == null)
                    categoryRepository = new Repository<Category>(m_Context);
                return categoryRepository;
            }
        }

        public Repository<SalesTransaction> SalesTransactionRepository
        {
            get
            {
                if (salesTransactionRepository == null)
                    salesTransactionRepository = new Repository<SalesTransaction>(m_Context);
                return salesTransactionRepository;
            }
        }

        public Repository<PurchaseTransaction> PurchaseTransactionRepository
        {
            get
            {
                if (purchaseTransactionRepository == null)
                    purchaseTransactionRepository = new Repository<PurchaseTransaction>(m_Context);
                return purchaseTransactionRepository;
            }
        }

        public Repository<Supplier> SupplierRepository
        {
            get
            {
                if (supplierRepository == null)
                    supplierRepository = new Repository<Supplier>(m_Context);
                return supplierRepository;
            }
        }

        public Repository<LedgerAccount> LedgerAccountRepository
        {
            get
            {
                if (ledgerAccountRepository == null)
                    ledgerAccountRepository = new Repository<LedgerAccount>(m_Context);
                return ledgerAccountRepository;
            }
        }

        public Repository<LedgerTransaction> LedgerTransactionRepository
        {
            get
            {
                if (ledgerTransactionRepository == null)
                    ledgerTransactionRepository = new Repository<LedgerTransaction>(m_Context);
                return ledgerTransactionRepository;
            }
        }

        public Repository<LedgerGeneral> LedgerGeneralRepository
        {
            get
            {
                if (ledgerGeneralRepository == null)
                    ledgerGeneralRepository = new Repository<LedgerGeneral>(m_Context);
                return ledgerGeneralRepository;
            }
        }

        public Repository<LedgerAccountBalance> LedgerAccountBalanceRepository
        {
            get
            {
                if (ledgerAccountBalanceRepository == null)
                    ledgerAccountBalanceRepository = new Repository<LedgerAccountBalance>(m_Context);
                return ledgerAccountBalanceRepository;
            }
        }
    }
}

