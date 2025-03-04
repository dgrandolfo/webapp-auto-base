using GestApp.Infrastructure.Data;
using GestApp.Application.Services.Interfaces;

namespace GestApp.Application.Services;

/// <inheritdoc />
public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Inizializza una nuova istanza di <see cref="TransactionService"/>.
    /// </summary>
    /// <param name="dbContext">Il contesto del database da utilizzare per le operazioni transazionali.</param>
    public TransactionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<T> ExecuteOperationInTransactionAsync<T>(Func<Task<T>> operation)
    {
        // Avvia una nuova transazione
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Esegui l'operazione e ottieni il risultato
            T result = await operation();
            // Salva i cambiamenti nel contesto (se non già fatto all'interno dell'operazione)
            await _dbContext.SaveChangesAsync();
            // Effettua il commit della transazione
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception)
        {
            // Effettua il rollback in caso di errore
            await transaction.RollbackAsync();
            throw;
        }
    }

}
