namespace GestApp.Application.Services.Interfaces;

/// <summary>
/// Fornisce metodi per eseguire operazioni sul database all'interno di una transazione.
/// Se si verifica un errore, la transazione viene annullata (rollback).
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Esegue l'operazione specificata all'interno di una transazione.
    /// Se l'operazione va a buon fine, la transazione viene committata; altrimenti viene effettuato il rollback.
    /// </summary>
    /// <param name="operation">La funzione asincrona che rappresenta l'operazione da eseguire in transazione.</param>
    /// <returns>Un task che rappresenta l'operazione asincrona.</returns>
    Task<T> ExecuteOperationInTransactionAsync<T>(Func<Task<T>> operation);
}
