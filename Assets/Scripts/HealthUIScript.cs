using UnityEngine;
using UnityEngine.UI;
public class HealthUIScript : MonoBehaviour
{
    private Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider = GetComponentInChildren<Slider>();

        slider.value = 100;
    }

    // Update is called once per frame
    public void ChangePlayerHealth(int value)
    {
        slider.value = value;
    }
}
