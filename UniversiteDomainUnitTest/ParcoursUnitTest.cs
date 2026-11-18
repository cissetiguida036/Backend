
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.ParcoursUseCases.Create;
using UniversiteDomain.UseCases.ParcoursUseCases.EtudiantDansParcours;
using UniversiteDomain.UseCases.ParcoursUseCases.UeDansParcours;

namespace UniversiteDomainUnitTests;

public class ParcoursUnitTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task CreateParcoursUseCase()
    {
        long idParcours = 1;
        string nomParcours = "Ue 1";
        int anneFormation = 2;

        // On crée le parcours qui doit être ajouté en base
        Parcours parcoursAvant = new Parcours
        {
            NomParcours = nomParcours,
            AnneeFormation = anneFormation
        };

        var mockParcours = new Mock<IParcoursRepository>();

        // Le parcours n'existe pas encore
        mockParcours
            .Setup(repo => repo.FindByConditionAsync(
                It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync(new List<Parcours>());

        // Simulation de la création
        Parcours parcoursFinal = new Parcours
        {
            Id = idParcours,
            NomParcours = nomParcours,
            AnneeFormation = anneFormation
        };

        mockParcours
            .Setup(repo => repo.CreateAsync(parcoursAvant))
            .ReturnsAsync(parcoursFinal);

        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory
            .Setup(facto => facto.ParcoursRepository())
            .Returns(mockParcours.Object);

        CreateParcoursUseCase useCase =
            new CreateParcoursUseCase(mockFactory.Object);

        var parcoursTeste = await useCase.ExecuteAsync(parcoursAvant);

        Assert.That(parcoursTeste.Id,
            Is.EqualTo(parcoursFinal.Id));
        Assert.That(parcoursTeste.NomParcours,
            Is.EqualTo(parcoursFinal.NomParcours));
        Assert.That(parcoursTeste.AnneeFormation,
            Is.EqualTo(parcoursFinal.AnneeFormation));
    }

    [Test]
    public async Task AddEtudiantDansParcoursUseCase()
    {
        long idEtudiant = 1;
        long idParcours = 3;

        Etudiant etudiant = new Etudiant
        {
            Id = 1,
            NumEtud = "1",
            Nom = "nom1",
            Prenom = "prenom1",
            Email = "1"
        };

        Parcours parcours = new Parcours
        {
            Id = 3,
            NomParcours = "Ue 3",
            AnneeFormation = 1
        };

        // Initialisation des faux repositories
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockParcours = new Mock<IParcoursRepository>();

        List<Etudiant> etudiants = new List<Etudiant>();
        etudiants.Add(new Etudiant { Id = 1 });

        mockEtudiant
            .Setup(repo => repo.FindByConditionAsync(
                It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync((Expression<Func<Etudiant, bool>> condition) =>
                etudiants.AsQueryable()
                    .Where(condition)
                    .ToList());

        List<Parcours> parcourses = new List<Parcours>();
        parcourses.Add(parcours);

        Parcours parcoursFinal = new Parcours
        {
            Id = 3,
            NomParcours = "Ue 3",
            AnneeFormation = 1
        };

        parcoursFinal.Inscrits.Add(etudiant);

        mockParcours
            .Setup(repo => repo.FindByConditionAsync(
                It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync((Expression<Func<Parcours, bool>> condition) =>
                parcourses.AsQueryable()
                    .Where(condition)
                    .ToList());

        mockParcours
            .Setup(repo => repo.AddEtudiantAsync(
                idParcours, idEtudiant))
            .ReturnsAsync(parcoursFinal);

        // Création d'une fausse factory
        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory
            .Setup(facto => facto.EtudiantRepository())
            .Returns(mockEtudiant.Object);

        mockFactory
            .Setup(facto => facto.ParcoursRepository())
            .Returns(mockParcours.Object);

        AddEtudiantDansParcoursUseCase useCase =
            new AddEtudiantDansParcoursUseCase(mockFactory.Object);

        // Exécution
        var parcoursTest = await useCase.ExecuteAsync(
            idParcours, idEtudiant);

        // Vérifications
        Assert.That(parcoursTest.Id,
            Is.EqualTo(parcoursFinal.Id));
        Assert.That(parcoursTest.Inscrits, Is.Not.Null);
        Assert.That(parcoursTest.Inscrits.Count,
            Is.EqualTo(1));
        Assert.That(parcoursTest.Inscrits[0].Id,
            Is.EqualTo(idEtudiant));
    }

    [Test]
    public async Task AddUeDansParcoursUseCase()
    {
        long idParcours = 3;
        long idUe = 1;

        Ue ue = new Ue
        {
            Id = idUe,
            NumeroUe = "UE1",
            Intitule = "Programmation C#"
        };

        Parcours parcours = new Parcours
        {
            Id = idParcours,
            NomParcours = "MIAGE",
            AnneeFormation = 1
        };

        // Initialisation des faux repositories
        var mockUe = new Mock<IUeRepository>();
        var mockParcours = new Mock<IParcoursRepository>();

        mockUe
            .Setup(repo => repo.FindByConditionAsync(
                It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        List<Parcours> parcoursExistants = new List<Parcours>
        {
            parcours
        };

        mockParcours
            .Setup(repo => repo.FindByConditionAsync(
                It.IsAny<Expression<Func<Parcours, bool>>>()))
            .ReturnsAsync((Expression<Func<Parcours, bool>> condition) =>
                parcoursExistants.AsQueryable()
                    .Where(condition)
                    .ToList());

        // Parcours attendu après l'ajout de l'UE
        Parcours parcoursFinal = new Parcours
        {
            Id = idParcours,
            NomParcours = "MIAGE",
            AnneeFormation = 1
        };

        parcoursFinal.UesEnseignees.Add(ue);

        mockParcours
            .Setup(repo => repo.AddUeAsync(idParcours, idUe))
            .ReturnsAsync(parcoursFinal);

        // Création d'une fausse factory
        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory
            .Setup(facto => facto.UeRepository())
            .Returns(mockUe.Object);

        mockFactory
            .Setup(facto => facto.ParcoursRepository())
            .Returns(mockParcours.Object);

        AddUeDansParcoursUseCase useCase =
            new AddUeDansParcoursUseCase(mockFactory.Object);

        // Exécution du use case
        var parcoursTest = await useCase.ExecuteAsync(
            idParcours, idUe);

        // Vérification du résultat
        Assert.That(parcoursTest.Id,
            Is.EqualTo(idParcours));

        Assert.That(parcoursTest.UesEnseignees,
            Is.Not.Null);

        Assert.That(parcoursTest.UesEnseignees.Count,
            Is.EqualTo(1));

        Assert.That(parcoursTest.UesEnseignees[0].Id,
            Is.EqualTo(idUe));
    }
}



