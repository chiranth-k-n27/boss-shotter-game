using System;
using System.Collections;
using System.Collections.Generic;
using MobileShooter.Core;
using MobileShooter.Events;

namespace UnityEngine
{
    public struct Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero = new Vector3(0, 0, 0);
        public static Vector3 up = new Vector3(0, 1, 0);
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }

        public static Color magenta = new Color(1, 0, 1, 1);
        public static Color white = new Color(1, 1, 1, 1);
    }

    public class Mathf
    {
        public static float Max(float a, float b) => System.Math.Max(a, b);
    }

    public class Object
    {
        public static void DestroyImmediate(Object obj) { }
    }

    public class Component : Object
    {
        public GameObject gameObject;
        public T GetComponent<T>() where T : Component
        {
            return gameObject.GetComponent<T>();
        }

        public T[] GetComponentsInChildren<T>() where T : Component
        {
            return gameObject.GetComponentsInChildren<T>();
        }
    }

    public class Behaviour : Component
    {
        public bool enabled = true;
    }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine) { return new Coroutine(); }
        public void StopCoroutine(Coroutine routine) { }
    }

    public class GameObject : Object
    {
        private List<Component> components = new List<Component>();

        public void SetActive(bool value) { }

        public T AddComponent<T>() where T : Component, new()
        {
            T comp = new T();
            comp.gameObject = this;
            components.Add(comp);
            return comp;
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var comp in components)
            {
                if (comp is T) return (T)comp;
            }
            return null;
        }

        public T[] GetComponentsInChildren<T>() where T : Component
        {
            // Simple mock for tests
            return new T[0];
        }
    }

    public class Coroutine { }

    public class WaitForSeconds
    {
        public WaitForSeconds(float seconds) { }
    }

    public class Material
    {
        public Color color;
    }

    public class Renderer : Component
    {
        public Material material = new Material();
    }
}

namespace MobileShooter.Boss
{
    public class BossAIController : UnityEngine.MonoBehaviour
    {
        public void ForceState(BossState state) { }
    }
}

namespace UnityEngine
{
    public class HeaderAttribute : System.Attribute
    {
        public HeaderAttribute(string text) { }
    }
}

namespace MobileShooter.Core
{
    public class AttachmentData { }
    public enum AttachmentType { Barrel, Scope, Stock, Magazine }
}
