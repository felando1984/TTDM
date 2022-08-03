using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Battlehub.UIControls;
using TMPro;

using System.Linq;

//Example of hierarchical data item
public class ExampleDataItem
{
    public string Name;
    //... additional fields here ...//

    public ExampleDataItem Parent;
    public List<ExampleDataItem> Children;

    public ExampleDataItem(string name)
    {
        Name = name;
        Children = new List<ExampleDataItem>();
    }

    public override string ToString()
    {
        return Name;
    }
}

public class GetStarted : MonoBehaviour
{
    private VirtualizingTreeView m_treeView;

    public Camera mainCamera;

    //root level data items
    //private List<ExampleDataItem> m_items;
    private List<GameObject> m_items;

    void Start()
    {
        m_treeView = GetComponent<VirtualizingTreeView>();

        //This event fired for each item that becomes visible
        m_treeView.ItemDataBinding += OnItemDataBinding;

        //This event is fired for each expanded item
        m_treeView.ItemExpanding += OnItemExpanding;
        m_treeView.SelectionChanged += OnSelectionChanged;
        m_treeView.ItemDoubleClick += OnItemDoubleClick;

        //This event is triggered for each item after drag & drop
        //m_treeView.ItemDrop += OnItemDrop;

        //Create data items 
        //m_items = new List<ExampleDataItem>();
        //for (int i = 0; i < 1000; ++i)
        //{
        //    m_items.Add(new ExampleDataItem("Data Item " + i));
        //}
        m_items = new List<GameObject>();
        m_items.Add(GameObject.Find("CameraCollection"));
        m_items.Add(GameObject.Find("LightsCollection"));

        //Populate tree view with data items
        m_treeView.Items = m_items;
    }

    private void OnSelectionChanged(object sender, SelectionChangedArgs e)
    {
#if UNITY_EDITOR
        //Do something on selection changed (just syncronized with editor's hierarchy for demo purposes)
        UnityEditor.Selection.objects = e.NewItems.OfType<GameObject>().ToArray();
#endif
    }

    void OnDestroy()
    {
        if (m_treeView != null)
        {
            m_treeView.ItemDataBinding -= OnItemDataBinding;
            m_treeView.ItemExpanding -= OnItemExpanding;
            //m_treeView.ItemDrop -= OnItemDrop;
            m_treeView.ItemDoubleClick -= OnItemDoubleClick;
        }
    }

    //void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
    //{
    //    ExampleDataItem item = (ExampleDataItem)e.Item;

    //    //Get the controls from ItemsPresenter and copy the data into them.
    //    TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
    //    text.text = item.Name;

    //    Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];
    //    icon.sprite = Resources.Load<Sprite>("IconNew");

    //    //Notify the tree of the presence of child data items.
    //    e.HasChildren = item.Children.Count > 0;
    //}
    /// <summary>
    /// This method called for each data item during databinding operation
    /// You have to bind data item properties to ui elements in order to display them.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
    {
        GameObject dataItem = e.Item as GameObject;
        if (dataItem != null)
        {
            //We display dataItem.name using UI.Text 
            TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
            text.text = dataItem.name;

            //Load icon from resources
            Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];
            icon.sprite = Resources.Load<Sprite>("IconNew");

            //And specify whether data item has children (to display expander arrow if needed)

            e.HasChildren = dataItem.transform.childCount > 0;

        }
    }

    //void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
    //{
    //    ExampleDataItem item = (ExampleDataItem)e.Item;

    //    //Return children to the tree view
    //    e.Children = item.Children;
    //}
    private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
    {
        //get parent data item (game object in our case)
        GameObject gameObject = (GameObject)e.Item;
        if (gameObject.transform.childCount > 0)
        {
            //get children
            List<GameObject> children = new List<GameObject>();

            for (int i = 0; i < gameObject.transform.childCount; ++i)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;

                children.Add(child);
            }

            //Populate children collection
            e.Children = children;
        }
    }

    private void OnItemDoubleClick(object sender, ItemArgs args)
    {
        //get parent data item (game object in our case)
        if (args.Items == null)
        {
            return;
        }
        GameObject dataItem = args.Items[0] as GameObject;
        mainCamera.transform.LookAt(dataItem.transform);
        Debug.Log(dataItem.name);
    }

    //void OnItemDrop(object sender, ItemDropArgs args)
    //{
    //    if (args.DropTarget == null)
    //    {
    //        return;
    //    }

    //    //Handle ItemDrop event using standard handler.
    //    m_treeView.ItemDropStdHandler<ExampleDataItem>(args,
    //        (item) => item.Parent,
    //        (item, parent) => item.Parent = parent,
    //        (item, parent) => ChildrenOf(parent).IndexOf(item),
    //        (item, parent) => ChildrenOf(parent).Remove(item),
    //        (item, parent, i) => ChildrenOf(parent).Insert(i, item));
    //}

    //List<ExampleDataItem> ChildrenOf(ExampleDataItem parent)
    //{
    //    if (parent == null)
    //    {
    //        return m_items;
    //    }
    //    return parent.Children;
    //}
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            m_treeView.SelectedItems = m_treeView.Items.OfType<object>().Take(5).ToArray();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            m_treeView.SelectedItem = null;
        }
    }
}