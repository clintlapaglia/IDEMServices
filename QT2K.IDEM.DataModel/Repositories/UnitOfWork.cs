using QT2K.IDEM.DataModel.Repositories.Interfaces;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace QT2K.IDEM.DataModel.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        #region Attributes
        private DbContext context;
        private bool disposed = false;
        #endregion

        #region Constructors
        public UnitOfWork(DbContext context)
        {
            this.context = context;
        }
        #endregion

        //#region Repositories
        //private BaseRepository<tAccountUtente> _accountRepository;
        //public BaseRepository<tAccountUtente> AccountRepository
        //{
        //    get
        //    {
        //        if (this._accountRepository == null)
        //            this._accountRepository = new BaseRepository<tAccountUtente>(context);
        //        return _accountRepository;
        //    }
        //}

        //private BaseRepository<tAnagrafeClienti> _clientiRepository;
        //public BaseRepository<tAnagrafeClienti> ClientiRepository
        //{
        //    get
        //    {
        //        if (this._clientiRepository == null)
        //            this._clientiRepository = new ClientiRepository(context);
        //        return _clientiRepository;
        //    }
        //}

        //private BaseRepository<v2_Incident> _incidentRepository;
        //public BaseRepository<v2_Incident> IncidentRepository
        //{
        //    get
        //    {
        //        if (this._incidentRepository == null)
        //            this._incidentRepository = new IncidentRepository(context);
        //        return _incidentRepository;
        //    }
        //}

        //private BaseRepository<v2_CalendarioIncident> _calendarioIncidentRepository;
        //public BaseRepository<v2_CalendarioIncident> CalendarioIncidentRepository
        //{
        //    get
        //    {
        //        if (this._calendarioIncidentRepository == null)
        //            this._calendarioIncidentRepository = new CalendarioIncidentRepository(context);
        //        return _calendarioIncidentRepository;
        //    }
        //}

        //private StoredRepository _storedRepository;
        //public StoredRepository StoredRepository
        //{
        //    get
        //    {
        //        if (this._storedRepository == null)
        //            this._storedRepository = new StoredRepository((AtcContext)context);
        //        return _storedRepository;
        //    }
        //}
        //#endregion

        #region Methods
        public int Save()
        {
            return context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
