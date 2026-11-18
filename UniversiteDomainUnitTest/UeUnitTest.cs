using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.UeUseCases.Create;

namespace UniversiteDomainUnitTests;

public class UeUnitTest
{
    [Test]
    public async Task CreateUeUseCase()
    {
        // Données de l'UE à créer
        string numeroUe = "UE1";
        string intitule = "Programmation C#";
        long idUe = 1;

        Ue ueAvant = new Ue
        {
            NumeroUe = numeroUe,
            Intitule = intitule
        };

        // Création du faux repository
        var mockUe = new Mock<IUeRepository>();

        // On simule l'absence d'une UE avec le même numéro
        mockUe
            .Setup(repo => repo.FindByConditionAsync(
                It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue>());

        // On simule la création de l'UE
        Ue ueFinale = new Ue
        {
            Id = idUe,
            NumeroUe = numeroUe,
            Intitule = intitule
        };

        mockUe
            .Setup(repo => repo.CreateAsync(ueAvant))
            .ReturnsAsync(ueFinale);

        // Création de la fausse factory
        var mockFactory = new Mock<IRepositoryFactory>();

        mockFactory
            .Setup(f => f.UeRepository())
            .Returns(mockUe.Object);

        // Création du Use Case
        var useCase = new CreateUeUseCase(mockFactory.Object);

        // Exécution
        var resultat = await useCase.ExecuteAsync(ueAvant);

        // Vérification du résultat
        Assert.That(resultat.Id, Is.EqualTo(idUe));
        Assert.That(resultat.NumeroUe, Is.EqualTo(numeroUe));
        Assert.That(resultat.Intitule, Is.EqualTo(intitule));
    }
}