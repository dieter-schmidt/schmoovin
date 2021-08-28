using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
	public interface ILadder
    {
        Transform localTransform { get; }
        Collider boxCollider { get; }

        Vector3 top { get; }
        Vector3 worldTop { get; }
		float spacing { get; }
        float length { get; }
        float width { get; }

        Vector3 up { get; }
        Vector3 forward { get; }
        Vector3 across { get; }
    }
}