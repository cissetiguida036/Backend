
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.NoteUseCases.Create;

namespace UniversiteDomainUnitTests;

public class NoteUnitTest
{
    [Test]
    public async Task AjouterNoteEtudiant()
    {
        // Préparation des données
        long idEtudiant = 1;
        long idUe = 2;
        float valeur = 15.5f;

        Ue ue = new Ue
        {
            Id = idUe,
            NumeroUe = "UE2",
            Intitule = "Programmation C#"
        };

        Parcours parcours = new Parcours
        {
            Id = 3,
            NomParcours = "MIAGE",
            AnneeFormation = 1
        };

        parcours.UesEnseignees!.Add(ue);

        Etudiant etudiant = new Etudiant
        {
            Id = idEtudiant,
            NumEtud = "2026001",
            Nom = "Durant",
            Prenom = "Jean",
            Email = "jean@example.com",
            ParcoursSuivi = parcours
        };

        // Création des faux repositories
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockUe = new Mock<IUeRepository>();
        var mockNote = new Mock<INoteRepository>();

        mockEtudiant
            .Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(new List<Etudiant> { etudiant });

        mockUe
            .Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        // Aucune note existante
        mockNote
            .Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync(new List<Note>());

        Note noteFinale = new Note
        {
            Etudiant = etudiant,
            Ue = ue,
            Valeur = valeur
        };

        mockNote
            .Setup(r => r.CreateAsync(It.IsAny<Note>()))
            .ReturnsAsync(noteFinale);

        // Création de la fausse factory
        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory.Setup(f => f.EtudiantRepository())
            .Returns(mockEtudiant.Object);

        mockFactory.Setup(f => f.UeRepository())
            .Returns(mockUe.Object);

        mockFactory.Setup(f => f.NoteRepository())
            .Returns(mockNote.Object);

        // Création du Use Case
        var useCase = new AddNoteEtudiantUseCase(
            mockFactory.Object);

        // Exécution
        var resultat = await useCase.ExecuteAsync(
            etudiant, ue, valeur);

        // Vérification du résultat
        Assert.That(resultat.Valeur, Is.EqualTo(15.5f));
        Assert.That(resultat.Etudiant.Id, Is.EqualTo(idEtudiant));
        Assert.That(resultat.Ue.Id, Is.EqualTo(idUe));

        mockNote.Verify(
            r => r.CreateAsync(It.IsAny<Note>()),
            Times.Once);
    }

    [Test]
    public void RefuserNoteSuperieureA20()
    {
        var mockFactory = new Mock<IRepositoryFactory>();

        var useCase = new AddNoteEtudiantUseCase(
            mockFactory.Object);

        Etudiant etudiant = new Etudiant { Id = 1 };
        Ue ue = new Ue { Id = 2 };

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await useCase.ExecuteAsync(
                etudiant, ue, 25f));
    }

    [Test]
    public void RefuserNoteNegative()
    {
        var mockFactory = new Mock<IRepositoryFactory>();

        var useCase = new AddNoteEtudiantUseCase(
            mockFactory.Object);

        Etudiant etudiant = new Etudiant { Id = 1 };
        Ue ue = new Ue { Id = 2 };

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await useCase.ExecuteAsync(
                etudiant, ue, -2f));
    }

    [Test]
    public void RefuserNoteEnDouble()
    {
        // Préparation des données
        Ue ue = new Ue
        {
            Id = 2,
            NumeroUe = "UE2",
            Intitule = "Programmation C#"
        };

        Parcours parcours = new Parcours
        {
            Id = 3,
            NomParcours = "MIAGE",
            AnneeFormation = 1
        };

        parcours.UesEnseignees!.Add(ue);

        Etudiant etudiant = new Etudiant
        {
            Id = 1,
            ParcoursSuivi = parcours
        };

        Note noteExistante = new Note
        {
            Etudiant = etudiant,
            Ue = ue,
            Valeur = 12f
        };

        // Création des faux repositories
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockUe = new Mock<IUeRepository>();
        var mockNote = new Mock<INoteRepository>();

        mockEtudiant
            .Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(new List<Etudiant> { etudiant });

        mockUe
            .Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        // Une note existe déjà pour cet étudiant
        mockNote
            .Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync(new List<Note> { noteExistante });

        // Création de la fausse factory
        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory.Setup(f => f.EtudiantRepository())
            .Returns(mockEtudiant.Object);

        mockFactory.Setup(f => f.UeRepository())
            .Returns(mockUe.Object);

        mockFactory.Setup(f => f.NoteRepository())
            .Returns(mockNote.Object);

        var useCase = new AddNoteEtudiantUseCase(
            mockFactory.Object);

        // Vérifier que la note en double est refusée
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await useCase.ExecuteAsync(
                etudiant, ue, 15f));

        // Vérifier qu'aucune nouvelle note n'est créée
        mockNote.Verify(
            r => r.CreateAsync(It.IsAny<Note>()),
            Times.Never);
    }

    [Test]
    public void RefuserNoteUeHorsParcours()
    {
        Ue ue = new Ue
        {
            Id = 5,
            NumeroUe = "UE5",
            Intitule = "Réseaux"
        };

        Parcours parcours = new Parcours
        {
            Id = 3,
            NomParcours = "MIAGE",
            AnneeFormation = 1
        };

        // Cette UE n'appartient pas au parcours

        Etudiant etudiant = new Etudiant
        {
            Id = 1,
            ParcoursSuivi = parcours
        };

        // Création des faux repositories
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockUe = new Mock<IUeRepository>();
        var mockNote = new Mock<INoteRepository>();

        mockEtudiant
            .Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(new List<Etudiant> { etudiant });

        mockUe
            .Setup(r => r.FindByConditionAsync(
                It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory.Setup(f => f.EtudiantRepository())
            .Returns(mockEtudiant.Object);

        mockFactory.Setup(f => f.UeRepository())
            .Returns(mockUe.Object);

        mockFactory.Setup(f => f.NoteRepository())
            .Returns(mockNote.Object);

        var useCase = new AddNoteEtudiantUseCase(
            mockFactory.Object);

        // Vérifier que l'opération est refusée
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await useCase.ExecuteAsync(
                etudiant, ue, 15f));

        // Aucune note ne doit être enregistrée
        mockNote.Verify(
            r => r.CreateAsync(It.IsAny<Note>()),
            Times.Never);
    }
}


