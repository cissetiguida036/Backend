namespace UniversiteDomain.Entities;

public class Note
{
    public Etudiant Etudiant { get; set; } = null!;

    public Ue Ue { get; set; } = null!;

    public float Valeur { get; set; }
}