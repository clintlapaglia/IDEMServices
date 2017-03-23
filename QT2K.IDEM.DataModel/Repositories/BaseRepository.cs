using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace QT2K.IDEM.DataModel.Repositories
{
    public class BaseRepository<TEntity> where TEntity : class
    {
        internal DbContext context;
        internal DbSet<TEntity> dbSet;

        public BaseRepository(DbContext context)
        {
            this.context = context;
            dbSet = context.Set<TEntity>();
        }

        #region Async

        public virtual async Task<ICollection<TEntity>> GetAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int page = -1,
            int pageSize = -1,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            //Filtro
            if (filter != null)
            {
                query = query.Where(filter);
            }

            //Inclusione
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            //Ordinamento
            if (orderBy != null)
            {
                query = orderBy(query);
                if (page > -1 && pageSize > -1)
                    return await query.Skip(page * pageSize).Take(pageSize).ToListAsync();
            }

            return await query.ToListAsync();
        }

        public virtual async Task<int> CountAsync(
            Expression<Func<TEntity, bool>> filter = null,
            int page = -1,
            int pageSize = -1)
        {
            IQueryable<TEntity> query = dbSet;

            //Filtro
            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.CountAsync();
        }

        #region CRUD Async

        public virtual Task<TEntity> GetByIdAsync(object id)
        {
            return dbSet.FindAsync(id);
        }

        public virtual async Task InsertAsync(TEntity entity, bool normalizationEnabled = true, bool validationEnabled = true)
        {
            //Aggiunge se non è già presente e se la validazione va a buon fine
            if ((!validationEnabled || await ValidateAsync(entity)) && !Exists(entity))
            {
                //normalizzo il contenuto se ci sono dati sporchi
                if (normalizationEnabled)
                    await NormalizeContentAsync(entity);
                //scrivo nel trace l'entità modificata
                //WriteTrace(entity, string.Empty, TraceTypeStrings.Creazione);
                dbSet.Add(entity);
            }
        }

        public virtual async Task UpdateAsync(int id, TEntity entity, bool normalizationEnabled = true, bool validationEnabled = true, bool checkWarningEnabled = true)
        {
            //Se la validazione va a buon fine
            if (!validationEnabled || await ValidateAsync(entity))
            {
                //TEntity entityToUpdate = await dbSet.FindAsync(id);
                //if (entityToUpdate != null)
                //    WriteTrace(entityToUpdate, string.Empty, TraceTypeStrings.Aggiornamento_Origine);
                //_context.Entry(entityToUpdate).State = EntityState.Detached;
                dbSet.Attach(entity);
                if (context.Entry(entity) != null)
                {
                    //normalizzo il contenuto se ci sono dati sporchi
                    if (normalizationEnabled)
                        await NormalizeContentAsync(entity);
                    //verifico che le entità correlate non generino warnings
                    if (checkWarningEnabled)
                        await CheckWarningsOnRelatedEntities(entity);
                    //marco l'entità modificata
                    context.Entry(entity).State = EntityState.Modified;
                    //scrivo nel trace l'entità modificata
                    //WriteTrace(entity, string.Empty, TraceTypeStrings.Aggiornamento_Modificato);
                }
            }
        }

        public virtual async Task DeleteAsync(int id, bool validationEnabled = true)
        {
            TEntity entityToDelete = await dbSet.FindAsync(id);
            //se la validazione va a buon fine
            if (await ValidateOnDeleteAsync(entityToDelete))
            {
                if (context.Entry(entityToDelete) != null && context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    dbSet.Attach(entityToDelete);
                }
                //WriteTrace(entityToDelete, string.Empty, TraceTypeStrings.Eliminazione);
                dbSet.Remove(entityToDelete);
            }
        }

        #endregion

        #endregion

        public bool Exists(TEntity entity)
        {
            return context.Set<TEntity>().Local.Any(e => e == entity);
        }

        #region Validation

        /// <summary>
        /// Metodo di validazione entità
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<bool> ValidateOnDeleteAsync(TEntity entity)
        {
            if (entity == null)
                throw new DbEntityValidationException("Entità da eliminare non trovata");
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Metodo di validazione entità
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<bool> ValidateAsync(TEntity entity) { return await Task.FromResult(true); }

        /// <summary>
        /// Metodo di controllo dei contenuti di una entità prima che sia salvata
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual async Task<bool> NormalizeContentAsync(TEntity entity) { return await Task.FromResult(true); }

        /// <summary>
        /// Verifica, in caso di Update su DB, che i collegamenti con altre entità chiave possano generare degli avvisi da documentare. Nel caso li memorizza e li mette a disposizione del Repository
        /// </summary>
        protected virtual async Task<bool> CheckWarningsOnRelatedEntities(TEntity entity) { return await Task.FromResult(true); }

        #endregion

    }
}
