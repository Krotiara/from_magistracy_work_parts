using UnityEngine;
using UnityEngine.UI;

public class DefectsCollector : MonoBehaviour
{
    private int currentShownChild;
    private DefectsCollectorManager defectsCollectorManager;
    /// <summary>
    /// Go to next or previous child and set it active
    /// </summary>
    /// <param name="to">direction (1 - next; -1 - previous) </param>
    public void LeafoverChildren(int to)
    {
        transform.GetChild(currentShownChild).gameObject.SetActive(false);
        currentShownChild += to;
        if (currentShownChild < 0)
            currentShownChild = transform.childCount - 2;
        if (currentShownChild == transform.childCount-1)
            currentShownChild = 0;
        transform.GetChild(currentShownChild).gameObject.SetActive(true);
        defectsCollectorManager.SetCurrentDefectNumber(currentShownChild+1);
    }

    private void Start()
    {
        if (transform.childCount > 1)
        {
            CreateCollectorUI();
        }
    }

    private void CreateCollectorUI()
    {
        GameObject collectorUIPref = Resources.Load<GameObject>(@"Prefabs/UI/DefectsCollectorUI");
        GameObject collectorUI = Instantiate(collectorUIPref);
        collectorUI.name = "DefectsCollector";
        collectorUI.transform.parent = transform;
        collectorUI.transform.position = transform.GetChild(0).transform.position;
        defectsCollectorManager = collectorUI.GetComponent<DefectsCollectorManager>();
        defectsCollectorManager.SetDefectsCount(transform.childCount-1);

        defectsCollectorManager.transform.Find("Buttons/ToRight").GetComponent<Button>()
            .onClick.AddListener(delegate { LeafoverChildren(1); });
        defectsCollectorManager.transform.Find("Buttons/ToLeft").GetComponent<Button>()
            .onClick.AddListener(delegate { LeafoverChildren(-1); });
    }
}
