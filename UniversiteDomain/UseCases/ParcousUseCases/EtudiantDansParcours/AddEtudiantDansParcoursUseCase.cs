
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Exceptions.ParcoursExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;

public class AddEtudiantDansParcoursUseCase(
    IRepositoryFactory repositoryFactory)
{
    // Rajout d'un étudiant dans un parcours
    public async Task<Parcours> ExecuteAsync(
        Parcours parcours, Etudiant etudiant)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(etudiant);

        return await ExecuteAsync(parcours.Id, etudiant.Id);
    }

    public async Task<Parcours> ExecuteAsync(
        long idParcours, long idEtudiant)
    {
        await CheckBusinessRules(idParcours, idEtudiant);

        return await repositoryFactory.ParcoursRepository()
            .AddEtudiantAsync(idParcours, idEtudiant);
    }

    // Rajout de plusieurs étudiants dans un parcours
    public async Task<Parcours> ExecuteAsync(
        Parcours parcours, List<Etudiant> etudiants)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(etudiants);

        long[] idsEtudiants = etudiants
            .Select(e => e.Id)
            .ToArray();

        return await ExecuteAsync(parcours.Id, idsEtudiants);
    }

    public async Task<Parcours> ExecuteAsync(
        long idParcours, long[] idsEtudiants)
    {
        ArgumentNullException.ThrowIfNull(idsEtudiants);

        // Vérifier toutes les règles avant de modifier les données
        foreach (var idEtudiant in idsEtudiants)
        {
            await CheckBusinessRules(idParcours, idEtudiant);
        }

        return await repositoryFactory.ParcoursRepository()
            .AddEtudiantAsync(idParcours, idsEtudiants);
    }

    private async Task CheckBusinessRules(
        long idParcours, long idEtudiant)
    {
        // Vérification des paramètres
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idParcours);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idEtudiant);

        // Vérification des repositories
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentNullException.ThrowIfNull(repositoryFactory.EtudiantRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.ParcoursRepository());

        // On recherche l'étudiant
        List<Etudiant> etudiants = await repositoryFactory
            .EtudiantRepository()
            .FindByConditionAsync(e => e.Id == idEtudiant);

        if (etudiants == null || etudiants.Count == 0)
        {
            throw new EtudiantNotFoundException(
                idEtudiant.ToString());
        }

        // On recherche le parcours
        List<Parcours> parcours = await repositoryFactory
            .ParcoursRepository()
            .FindByConditionAsync(p => p.Id == idParcours);

        if (parcours == null || parcours.Count == 0)
        {
            throw new ParcoursNotFoundException(
                idParcours.ToString());
        }

        // Vérifier si l'étudiant est déjà inscrit
        List<Etudiant> inscrits = await repositoryFactory
            .EtudiantRepository()
            .FindByConditionAsync(
                e => e.Id == idEtudiant &&
                     e.ParcoursSuivi != null &&
                     e.ParcoursSuivi.Id == idParcours);

        if (inscrits != null && inscrits.Count > 0)
        {
            throw new DuplicateInscriptionException(
                "Cet étudiant est déjà inscrit dans ce parcours.");
        }
    }
}
