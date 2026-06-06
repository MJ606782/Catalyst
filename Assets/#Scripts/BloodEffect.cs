using UnityEngine;

public static class BloodEffect
{
    private static Material _brightRed, _red, _darkRed, _blackRed;

    static void Init()
    {
        if (_brightRed != null) return;
        _brightRed = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.7f, 0.03f, 0.03f) };
        _red = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.5f, 0.02f, 0.02f) };
        _darkRed = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.3f, 0.01f, 0.01f) };
        _blackRed = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.15f, 0f, 0f) };
    }

    static Material Bright() { Init(); return _brightRed; }
    static Material Red() { Init(); return _red; }
    static Material Dark() { Init(); return _darkRed; }
    static Material Black() { Init(); return _blackRed; }

    static GameObject Spawn(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject o = GameObject.CreatePrimitive(type);
        o.name = name;
        o.transform.position = pos;
        o.transform.localScale = scale;
        o.transform.rotation = Random.rotation;
        o.GetComponent<Renderer>().material = mat;
        Object.Destroy(o.GetComponent<Collider>());
        return o;
    }

    static void AddPhysics(GameObject o, Vector3 force, float mass = 0.1f, float drag = 0.4f)
    {
        Rigidbody rb = o.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.drag = drag;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * Random.Range(100f, 400f), ForceMode.Impulse);
    }

    public static void SpawnBloodSpray(Vector3 pos, Vector3 dir, int count = 20)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 d = (dir + Random.insideUnitSphere * 0.35f).normalized;
            float spd = Random.Range(4f, 11f);
            float sz = Random.Range(0.04f, 0.15f);
            GameObject o = Spawn("b", PrimitiveType.Sphere, pos, Vector3.one * sz, Random.value < 0.2f ? Bright() : Red());
            AddPhysics(o, d * spd, 0.05f, 0.3f);
            Object.Destroy(o, Random.Range(0.3f, 0.8f));
        }
        for (int i = 0; i < count / 4; i++)
        {
            Vector3 d = (dir + Random.insideUnitSphere * 0.2f).normalized;
            float spd = Random.Range(3f, 7f);
            Vector3 s = new Vector3(Random.Range(0.03f, 0.06f), Random.Range(0.03f, 0.06f), Random.Range(0.08f, 0.15f));
            GameObject o = Spawn("bc", PrimitiveType.Cube, pos, s, Dark());
            AddPhysics(o, d * spd, 0.08f, 0.5f);
            Object.Destroy(o, Random.Range(0.5f, 1.2f));
        }
    }

    public static void SpawnBloodSplatter(Vector3 pos, int count = 40)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 d = Random.insideUnitSphere.normalized;
            d.y = Mathf.Abs(d.y) * 0.6f + 0.2f;
            float spd = Random.Range(2f, 8f);
            float sz = Random.Range(0.06f, 0.22f);
            Material m = Red();
            if (Random.value < 0.15f) m = Dark();
            if (Random.value < 0.1f) m = Black();
            GameObject o = Spawn("s", Random.value < 0.2f ? PrimitiveType.Cube : PrimitiveType.Sphere,
                pos, Vector3.one * sz, m);
            AddPhysics(o, d * spd, 0.06f, 0.3f);
            Object.Destroy(o, Random.Range(0.4f, 1.2f));
        }
    }

    public static void SpawnDeathGeyser(Vector3 pos, int count = 60)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 d = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.6f, 1f), Random.Range(-0.5f, 0.5f)).normalized;
            float spd = Random.Range(5f, 14f);
            float sz = Random.Range(0.08f, 0.35f);
            Material m = Random.value < 0.2f ? Bright() : (Random.value < 0.3f ? Dark() : Red());
            GameObject o = Spawn("g", PrimitiveType.Sphere, pos, Vector3.one * sz, m);
            AddPhysics(o, d * spd, 0.08f, 0.2f);
            Object.Destroy(o, Random.Range(0.8f, 2.5f));
        }
        for (int i = 0; i < count / 3; i++)
        {
            Vector3 d = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(0.3f, 0.8f), Random.Range(-0.6f, 0.6f)).normalized;
            float spd = Random.Range(2f, 5f);
            Vector3 s = new Vector3(Random.Range(0.05f, 0.15f), Random.Range(0.03f, 0.08f), Random.Range(0.05f, 0.12f));
            GameObject o = Spawn("gc", PrimitiveType.Cube, pos, s, Black());
            AddPhysics(o, d * spd + Vector3.down * 2f, 0.15f, 0.5f);
            Object.Destroy(o, Random.Range(1f, 3f));
        }
        for (int i = 0; i < 3; i++)
        {
            Vector3 d = new Vector3(Random.Range(-0.3f, 0.3f), 1f + Random.Range(-0.2f, 0.2f), Random.Range(-0.3f, 0.3f)).normalized;
            float spd = Random.Range(8f, 14f);
            GameObject o = Spawn("bg", PrimitiveType.Sphere, pos, Vector3.one * Random.Range(0.3f, 0.5f), Dark());
            AddPhysics(o, d * spd, 0.3f, 0.1f);
            Object.Destroy(o, Random.Range(1.5f, 3f));
        }
    }

    public static void SpawnGroundBloodPool(Vector3 pos)
    {
        for (int i = 0; i < Random.Range(4, 8); i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.4f, 0.4f), 0.01f, Random.Range(-0.4f, 0.4f));
            float r = Random.Range(0.1f, 0.5f);
            GameObject o = Spawn("pool", PrimitiveType.Sphere, pos + offset,
                new Vector3(r, 0.02f, r * Random.Range(0.5f, 1f)),
                Random.value < 0.4f ? Dark() : Black());
            Object.Destroy(o, 20f);
        }
    }
}
