using System.Collections.Generic;
using UnityEngine;

public class PetDatabase : MonoBehaviour
{
    public static PetDatabase Instance { get; private set; }
    public List<PetConfig> allPets = new List<PetConfig>();

    void Awake() { Instance = this; }

    public PetConfig GetPet(int id) { return allPets.Find(p => p.petId == id); }
}