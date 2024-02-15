using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public interface IInteractable
    {
        string GetInteractPrompt();
        void OnInteract();
    }
}
