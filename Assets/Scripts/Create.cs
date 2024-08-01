using UnityEngine;
using MazeInterface;
using Unity.VisualScripting;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEditor.Localization.Editor;
using System.Globalization;

public class Create : MonoBehaviour
{
    [SerializeField] private int Width;
    [SerializeField] private int Height;
    [SerializeField] private string Name;
    [SerializeField] private GameObject Cell = null;
    private CultureInfo LanguageEnum = new CultureInfo("en-US");
    void Start()
    {
        Revlectoin();
    }
    private void Revlectoin()
    {
        string[] plugins = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Assets\\Plugins", "*.dll");
        print(string.Join(", ", plugins));
        //Debug.Log(Directory.GetCurrentDirectory() + "\\Assets\\Plugins");
        Assembly assembly = null;
        foreach (string plugin in plugins)
        {
            Assembly cur = Assembly.LoadFrom(plugin);
            var att = (IdentificationAttribute)cur.GetCustomAttribute(typeof(IdentificationAttribute));
            if (plugin.Contains("ClassLibrary1"))
            {
                if (att.Name == Name)
                    assembly = cur;
            }
            //if (att.Name == Name)
            //    assembly = cur;
        }
        if (assembly == null)
        {
            print("error name " + Name);
            return;
        }
        Type[] types = assembly.GetTypes();
        Type mazeType = null;
        foreach (Type t in types)
            if (t.GetInterfaces().Contains(typeof(IMaze)))
                mazeType = t;
        if (mazeType == null) 
        {
            print("No such Type");
            return;
        }
        ConstructorInfo[] ci = mazeType.GetConstructors();
        int j;
        for (j = 0; j < ci.Length; j++)
        {
            ParameterInfo[] parameterInfos = ci[j].GetParameters();
            if (parameterInfos.Length == 2)
                break;
        }
        if (j == ci.Length) 
        {
            print("No matching constructor found.");
            return;
        }
        object obj = ci[j].Invoke(new object[] { Width, Height });
        IMaze lm = (IMaze) obj;
        lm.Generate();
        Debug.Log(lm);
        Camera.main.transform.position = new Vector3(Width / 2, -Height / 2, Camera.main.transform.position.z);
        Camera.main.transform.GetComponent<Camera>().orthographicSize = Mathf.Max(Width, Height) / 2 + 1;
        Visual(lm);
    }
    private void Visual(IMaze lm)
    {
        GameObject temp = null;
        float x = 0.5f, y = -0.5f;
        for (int i = 0; i < lm.Length; i++)
        {
            temp = Instantiate(Cell, new Vector2(x, y), Quaternion.identity);
            if (lm[i].left && lm[i].cellType != CellType.Enter)
                temp.transform.GetChild(0).transform.gameObject.SetActive(true);
            if (lm[i].up)
                temp.transform.GetChild(1).transform.gameObject.SetActive(true);
            if (lm.isRight(i) && lm[i].cellType != CellType.Exit)
                temp.transform.GetChild(2).transform.gameObject.SetActive(true);
            if (lm.isDown(i))
                temp.transform.GetChild(3).transform.gameObject.SetActive(true);
            x += temp.transform.localScale.x;
            if (lm.isRight(i))
            {
                x = 0.5f;
                y -= temp.transform.localScale.y;
            }
            temp.transform.parent = transform;
        }
    }
}
