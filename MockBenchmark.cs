using System;
using System.Diagnostics;
using System.Collections.Generic;

public class Component { }
public class Collider : Component {
    public GameObject gameObject;
}
public class GameObject {
    public GameObject parent;
    public List<Component> components = new List<Component>();

    public T GetComponent<T>() where T : class {
        foreach (var c in components) {
            if (c is T) return c as T;
        }
        return null;
    }

    public T GetComponentInParent<T>() where T : class {
        GameObject current = this;
        while (current != null) {
            T comp = current.GetComponent<T>();
            if (comp != null) return comp;
            current = current.parent;
        }
        return null;
    }

    public bool TryGetComponent<T>(out T component) where T : class {
        component = GetComponent<T>();
        return component != null;
    }
}

public interface IDamageable {}
public class BossHealth : Component, IDamageable {}
public enum HitboxType { Body, Head }
public class BossHitbox : Component {
    public HitboxType hitboxType = HitboxType.Body;
    private IDamageable _damageable;
    public IDamageable Damageable {
        get {
            if (_damageable == null) {
                // In Unity this would be GetComponentInParent, here we simulate
                _damageable = this.gameObject.GetComponentInParent<IDamageable>();
            }
            return _damageable;
        }
    }
}

public class RaycastHit {
    public Collider collider;
}

public class Benchmark {
    public static void Main() {
        // Setup hierarchy
        GameObject root = new GameObject();
        root.components.Add(new BossHealth());

        GameObject child1 = new GameObject() { parent = root };
        GameObject child2 = new GameObject() { parent = child1 };
        GameObject child3 = new GameObject() { parent = child2 };

        BossHitbox hitbox = new BossHitbox();
        hitbox.gameObject = child3;
        child3.components.Add(hitbox);

        Collider collider = new Collider();
        collider.gameObject = child3;
        child3.components.Add(collider);

        RaycastHit hit = new RaycastHit();
        hit.collider = collider;

        int iterations = 1000000;

        // Baseline
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++) {
            IDamageable damageable = hit.collider.gameObject.GetComponentInParent<IDamageable>();
            if (damageable != null) {
                HitboxType type = HitboxType.Body;
                BossHitbox hb = hit.collider.gameObject.GetComponent<BossHitbox>();
                if (hb != null) {
                    type = hb.hitboxType;
                }
            }
        }
        sw.Stop();
        Console.WriteLine($"Baseline: {sw.ElapsedMilliseconds} ms");

        // Optimized
        sw.Restart();
        for (int i = 0; i < iterations; i++) {
            HitboxType type = HitboxType.Body;
            IDamageable damageable = null;

            if (hit.collider.gameObject.TryGetComponent(out BossHitbox hb)) {
                type = hb.hitboxType;
                damageable = hb.Damageable;
            } else {
                damageable = hit.collider.gameObject.GetComponentInParent<IDamageable>();
            }

            if (damageable != null) {
                // do nothing
            }
        }
        sw.Stop();
        Console.WriteLine($"Optimized: {sw.ElapsedMilliseconds} ms");
    }
}
