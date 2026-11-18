using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.UeUseCases.Create;

public class CreateUeUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Ue> ExecuteAsync(
        string numeroUe, string intitule)
    {
        var ue = new Ue
        {
            NumeroUe = numeroUe,
            Intitule = intitule
        };

        return await ExecuteAsync(ue);
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);

        var resultat = await repositoryFactory.UeRepository()
            .CreateAsync(ue);

        await repositoryFactory.UeRepository()
            .SaveChangesAsync();

        return resultat;
    }

    private async Task CheckBusinessRules(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        if (string.IsNullOrWhiteSpace(ue.NumeroUe))
        {
            throw new ArgumentException(
                "Le numéro de l'UE est obligatoire.");
        }

        if (string.IsNullOrWhiteSpace(ue.Intitule) ||
            ue.Intitule.Length <= 3)
        {
            throw new ArgumentException(
                "L'intitulé doit contenir plus de 3 caractères.");
        }

        var existantes = await repositoryFactory.UeRepository()
            .FindByConditionAsync(
                u => u.NumeroUe == ue.NumeroUe);

        if (existantes.Count > 0)
        {
            throw new ArgumentException(
                "Une UE avec ce numéro existe déjà.");
        }
    }
}