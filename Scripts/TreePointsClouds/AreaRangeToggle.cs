using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class AreaRangeToggle : MonoBehaviour
{
    public AreaRange AreaRange;
    private TreeCloudsManager cloudsManager;
    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        cloudsManager = transform.parent.GetComponent<AreaRangeWindow>().CloudsManager;
        toggle.onValueChanged.AddListener(delegate { cloudsManager.SetAreaActivity(AreaRange, toggle.isOn); });
    }
}
