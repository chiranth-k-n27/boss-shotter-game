using System;
using System.Diagnostics;
using UnityEngine;
using MobileShooter.Core;

public class WeaponAssemblerBenchmark : MonoBehaviour
{
    public void RunBenchmark()
    {
        WeaponAssembler assembler = gameObject.AddComponent<WeaponAssembler>();

        Stopwatch sw = new Stopwatch();
        sw.Start();

        for(int i = 0; i < 1000; i++)
        {
            AttachmentData data = ScriptableObject.CreateInstance<AttachmentData>();
            data.type = AttachmentType.Barrel;
            data.shape = PrimitiveShape.Cube;
            data.meshColor = Color.red;

            assembler.EquipAttachment(data);
        }

        sw.Stop();
        UnityEngine.Debug.Log($"Benchmark time: {sw.ElapsedMilliseconds} ms");
    }
}
