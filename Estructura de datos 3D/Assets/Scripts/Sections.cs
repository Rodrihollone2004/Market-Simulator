using System.Collections.Generic;
using UnityEngine;

public class Sections : MonoBehaviour
{
    private Stack<GameObject> stacks = new Stack<GameObject>();
    private Shelf shelf;

    public Stack<GameObject> Stacks { get => stacks; set => stacks = value; }

    public void AddItemToSlot(GameObject itemPrefab, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (stacks.Count < 5)
            {
                GameObject newItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
                newItem.transform.SetParent(transform);
                newItem.transform.localPosition = Vector3.back * 0.1f * stacks.Count;
                newItem.tag = "Item";
                stacks.Push(newItem);
            }
        }
    }

    public void RemoveItemFromSlot()
    {
        if (stacks.Count > 0)
        {
            GameObject itemToRemove = stacks.Pop();
            Destroy(itemToRemove);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Sections sections = hit.transform.GetComponent<Sections>();

                if (sections == this)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        AddItemToSlot(shelf.ItemPrefab);
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        RemoveItemFromSlot();
                    }
                }
            }
        }
    }

    private void Start()
    {
        shelf = GetComponentInParent<Shelf>();
    }
    public bool HasItems()
    {
        return stacks.Count > 0;
    }
    public void RemoveItem()
    {
        GameObject itemToRemove = stacks.Pop();
    }

}
