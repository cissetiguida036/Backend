
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create;

public class CreateParcoursUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Parcours> ExecuteAsync(
        string nomParcours,
        int anneeFormation)
    {
        var parcours = new Parcours
        {
            NomParcours = nomParcours,
            AnneeFormation = anneeFormation
        };

        return await ExecuteAsync(parcours);
    }

    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        await CheckBusinessRules(parcours);

        Parcours resultat = await repositoryFactory
            .ParcoursRepository()
            .CreateAsync(parcours);

        await repositoryFactory.ParcoursRepository()
            .SaveChangesAsync();

        return resultat;
    }

    private async Task CheckBusinessRules(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        if (string.IsNullOrWhiteSpace(parcours.NomParcours))
        {
            throw new InvalidNomParcoursException(
                "Le nom du parcours est obligatoire.");
        }

        if (parcours.AnneeFormation <= 0)
        {
            throw new InvalidAnneeFormationException(
                "L'année de formation doit être positive.");
        }

        var existants = await repositoryFactory.ParcoursRepository()
            .FindByConditionAsync(
                p => p.NomParcours == parcours.NomParcours
                     && p.AnneeFormation == parcours.AnneeFormation);

        if (existants.Count > 0)
        {
            throw new DuplicateParcoursException(
                "Ce parcours existe déjà.");
        }
    }
}





