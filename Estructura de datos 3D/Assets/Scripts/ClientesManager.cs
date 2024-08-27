using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float distanceBetweenClients = 2f;
    [SerializeField] private Transform exitPoint;
    private List<Sections> sections = new List<Sections>();
    private Queue<GameObject> clients = new Queue<GameObject>();
    private Vector3 nextPosition;
    private bool isMoving = false;

    private void Start()
    {
        if (clientPrefab == null)
        {
            Debug.LogError("clientPrefab no está asignado.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("spawnPoint no está asignado.");
            return;
        }

        nextPosition = spawnPoint.position;
        StartCoroutine(SpawnClients());

        Shelf[] shelves = FindObjectsOfType<Shelf>();
        foreach (Shelf shelf in shelves)
        {
            sections.AddRange(shelf.Sections);
        }

        if (sections.Count == 0)
        {
            Debug.LogWarning("No se han encontrado secciones en el estante.");
        }
    }

    private void SpawnClient()
    {
        if (sections.Count > 0)
        {
            GameObject client = Instantiate(clientPrefab, nextPosition, Quaternion.identity);
            clients.Enqueue(client);
            nextPosition += Vector3.back * distanceBetweenClients;

            Client clientScript = client.GetComponent<Client>();
            if (clientScript != null)
            {
                clientScript.SetExit(exitPoint);
            }

            if (!isMoving)
            {
                isMoving = true;
                StartCoroutine(MoveClient());
            }
        }
        else
        {
            Debug.LogWarning("No se han encontrado secciones para agregar clientes.");
        }

    }
    private IEnumerator SpawnClients()
    {
        while (true)
        {
            if (clients.Count < MaxClients)
            {
                SpawnClient();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    private const int MaxClients = 10;



    private IEnumerator MoveClient()
    {
        while (true)
        {
            if (clients.Count == 0)
            {
                isMoving = false;
                yield break;
            }

            GameObject client = clients.Peek();

            if (client == null || !client.activeInHierarchy)
            {
                clients.Dequeue();
                continue;
            }

            Client clientScript = client.GetComponent<Client>();
            if (clientScript == null)
            {
                clients.Dequeue();
                continue;
            }

            List<Sections> availableSections = sections.Where(section => section.HasItems()).ToList();

            if (availableSections.Count == 0)
            {
                // Espera hasta que se agreguen ítems nuevos
                yield return new WaitUntil(() => sections.Any(section => section.HasItems()));
                continue; // Vuelve a intentar mover al cliente
            }

            Sections selectedSection = availableSections[Random.Range(0, availableSections.Count)];
            Transform objective = selectedSection.transform;
            float speed = 5f;

            while (client != null && client.activeInHierarchy && Vector3.Distance(client.transform.position, objective.position) > 0.1f && !clientScript.IsMovingToExit())
            {
                if (client == null || !client.activeInHierarchy)
                {
                    break;
                }

                client.transform.position = Vector3.MoveTowards(client.transform.position, objective.position, speed * Time.deltaTime);
                yield return null;
            }

            if (clientScript != null)
            {
                clientScript.SetExit(exitPoint);
                StartCoroutine(clientScript.MoveToExit());
            }

            yield return new WaitUntil(() => client == null || !client.activeInHierarchy);

            clients.Dequeue();

            UpdateClientPositions();

            if (clients.Count > 0)
            {
                StartCoroutine(MoveClient());
                yield break;
            }
        }
    }

    private void UpdateClientPositions()
    {
        Vector3 currentPosition = spawnPoint.position;

        foreach (GameObject client in clients)
        {
            if (client != null && client.activeInHierarchy)
            {
                client.transform.position = currentPosition;
                currentPosition += Vector3.back * distanceBetweenClients;
            }
        }

        nextPosition = currentPosition;
    }


    public void NotifyClientReachedExit()
    {
        if (clients.Count > 0 && !isMoving)
        {
            isMoving = true;
            StartCoroutine(MoveClient());
        }
    }
    public void NotifyItemAdded()
    {
        if (!isMoving)
        {
            isMoving = true;
            StartCoroutine(MoveClient());
        }
    }

}
