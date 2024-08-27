using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<Sections> sections = new List<Sections>();

    public GameObject ItemPrefab { get => itemPrefab; }
    public List<Sections> Sections { get => sections; set => sections = value; }

    private ClientManager clientManager;
    private void Awake()
    {
        sections = GetComponentsInChildren<Sections>().ToList();
        clientManager = FindObjectOfType<ClientManager>();

        Debug.Log("Secciones encontradas: " + sections.Count);

        if (sections.Count == 0)
        {
            Debug.LogWarning("No se han encontrado secciones en el estante.");
        }
    }

    void Start()
    {
        if (sections.Count > 0)
        {
            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].AddItemToSlot(itemPrefab, Random.Range(0, 5));
            }
        }
        else
        {
            Debug.LogWarning("No se han encontrado secciones para añadir items.");
        }

        NotifyClients();
    }

    private void NotifyClients()
    {
        if (clientManager != null)
        {
            clientManager.NotifyItemAdded();
        }
    }
}