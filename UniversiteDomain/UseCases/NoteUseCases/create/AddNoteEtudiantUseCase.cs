
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases.Create;

public class AddNoteEtudiantUseCase(
    IRepositoryFactory repositoryFactory)
{
    public async Task<Note> ExecuteAsync(
        Etudiant etudiant,
        Ue ue,
        float valeur)
    {
        ArgumentNullException.ThrowIfNull(etudiant);
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        // Règle 1 : la note doit être entre 0 et 20
        if (float.IsNaN(valeur) ||
            float.IsInfinity(valeur) ||
            valeur < 0 || valeur > 20)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valeur),
                "La note doit être comprise entre 0 et 20.");
        }

        // Vérifier que l'étudiant et l'UE existent
        var etudiants = await repositoryFactory
            .EtudiantRepository()
            .FindByConditionAsync(e => e.Id == etudiant.Id);

        if (etudiants == null || etudiants.Count == 0)
        {
            throw new InvalidOperationException(
                "L'étudiant n'existe pas.");
        }

        var ues = await repositoryFactory
            .UeRepository()
            .FindByConditionAsync(u => u.Id == ue.Id);

        if (ues == null || ues.Count == 0)
        {
            throw new InvalidOperationException(
                "L'UE n'existe pas.");
        }

        // Récupérer l'étudiant existant
        var etudiantExistant = etudiants[0];

        // Règle 2 : l'étudiant doit suivre un parcours
        var parcours = etudiantExistant.ParcoursSuivi;

        if (parcours == null)
        {
            throw new InvalidOperationException(
                "L'étudiant n'est inscrit dans aucun parcours.");
        }

        // Vérifier que l'UE appartient au parcours
        bool ueDansParcours =
            parcours.UesEnseignees != null &&
            parcours.UesEnseignees.Any(u => u.Id == ue.Id);

        if (!ueDansParcours)
        {
            throw new InvalidOperationException(
                "Cette UE n'appartient pas au parcours de l'étudiant.");
        }

        // Règle 3 : une seule note par étudiant et par UE
        var notesExistantes = await repositoryFactory
            .NoteRepository()
            .FindByConditionAsync(n =>
                n.Etudiant.Id == etudiant.Id &&
                n.Ue.Id == ue.Id);

        if (notesExistantes != null &&
            notesExistantes.Count > 0)
        {
            throw new InvalidOperationException(
                "Cet étudiant possède déjà une note dans cette UE.");
        }

        // Création de la note
        var note = new Note
        {
            Etudiant = etudiantExistant,
            Ue = ues[0],
            Valeur = valeur
        };

        // Enregistrement
        var resultat = await repositoryFactory
            .NoteRepository()
            .CreateAsync(note);

        await repositoryFactory.SaveChangesAsync();

        return resultat;
    }
}
