using UnityEngine;

public class Component_Switcher : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] components;

    void Update()
    {
        for (int i = 1; i <= components.Length; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                ActivateOnly(i - 1);
            }
        }

        if (Input.GetKey("escape")) { Application.Quit(); }

    }

    private void ActivateOnly(int indexToActivate)
    {
        for (int i = 0; i < components.Length; i++)
        {
            components[i].enabled = (i == indexToActivate);
        }
    }
}
