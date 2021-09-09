using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public enum TransitionParameter
    {
        Move,
        Jump,
        ForceTransition,
        Grounded,
    }

    public class EnemyController : MonoBehaviour
    {
        public Animator animator;
        public bool MoveRight;
        public bool MoveLeft;
        public bool Jump;
        public GameObject ColliderEdgePrefab;
        public List<GameObject> BottomSpheres = new List<GameObject>();
        public List<GameObject> FrontSpheres = new List<GameObject>();
        public List<Collider> RagdollParts = new List<Collider>();

        public float GravityMultiplier;
        public float PullMultiplier;

        //DS
        public Transform bloodFXTransform;
        public Transform headTransform;

        private Rigidbody rigid;
        public Rigidbody RIGID_BODY
        {
            get
            {
                if (rigid == null)
                {
                    rigid = GetComponent<Rigidbody>();
                }
                return rigid;
            }
        }

        private void Awake()
        {
            SetRagdollParts();
            //SetColliderSpheres();
        }

    //private IEnumerator Start()
    //{
    //    yield return new WaitForSeconds(3f);
    //    RIGID_BODY.AddForce(10f * Vector3.up);
    //    yield return new WaitForSeconds(1f);
    //    TurnOnRagdoll();
    //}

    private void SetRagdollParts()
        {
            Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();

            foreach (Collider c in colliders)
            {
                if (c.gameObject != this.gameObject)
                {
                    c.isTrigger = true;
                    RagdollParts.Add(c);
                }
            }
        }

        public void TurnOnRagdoll()
        {
            Debug.Log("RAGDOLL");
            RIGID_BODY.useGravity = false;
            //DS
            RIGID_BODY.velocity = Vector3.zero;
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
            animator.enabled = false;
            animator.avatar = null;

            foreach (Collider c in RagdollParts)
            {
                c.isTrigger = false;
                //DS
                c.attachedRigidbody.velocity = Vector3.zero;
            }

            RIGID_BODY.AddForce(200f * Vector3.up);
    }

        private void SetColliderSpheres()
        {
            BoxCollider box = GetComponent<BoxCollider>();

            float bottom = box.bounds.center.y - box.bounds.extents.y;
            float top = box.bounds.center.y + box.bounds.extents.y;
            float front = box.bounds.center.z + box.bounds.extents.z;
            float back = box.bounds.center.z - box.bounds.extents.z;

            GameObject bottomFront = CreateEdgeSphere(new Vector3(0f, bottom, front));
            GameObject bottomBack = CreateEdgeSphere(new Vector3(0f, bottom, back));
            GameObject topFront = CreateEdgeSphere(new Vector3(0f, top, front));

            bottomFront.transform.parent = this.transform;
            bottomBack.transform.parent = this.transform;
            topFront.transform.parent = this.transform;

            BottomSpheres.Add(bottomFront);
            BottomSpheres.Add(bottomBack);

            FrontSpheres.Add(bottomFront);
            FrontSpheres.Add(topFront);

            float horSec = (bottomFront.transform.position - bottomBack.transform.position).magnitude / 5f;
            CreateMiddleSpheres(bottomFront, -this.transform.forward, horSec, 4, BottomSpheres);

            float verSec = (bottomFront.transform.position - topFront.transform.position).magnitude / 10f;
            CreateMiddleSpheres(bottomFront, this.transform.up, verSec, 9, FrontSpheres);
        }

        private void FixedUpdate()
        {
            if (RIGID_BODY.velocity.y < 0f)
            {
                RIGID_BODY.velocity += (-Vector3.up * GravityMultiplier);
            }

            if (RIGID_BODY.velocity.y > 0f && !Jump)
            {
                RIGID_BODY.velocity += (-Vector3.up * PullMultiplier);
            }
        }

        public void CreateMiddleSpheres(GameObject start, Vector3 dir, float sec, int interations, List<GameObject> spheresList)
        {
            for (int i = 0; i < interations; i++)
            {
                Vector3 pos = start.transform.position + (dir * sec * (i + 1));

                GameObject newObj = CreateEdgeSphere(pos);
                newObj.transform.parent = this.transform;
                spheresList.Add(newObj);
            }
        }

        public GameObject CreateEdgeSphere(Vector3 pos)
        {
            GameObject obj = Instantiate(ColliderEdgePrefab, pos, Quaternion.identity);
            return obj;
        }

    //DS
    public void ProcessRaycastCollision(Vector3 direction, Vector3 hitPoint)//Collision collision)
    {
        //ProcessImpact(direction);
        //Debug.Log("RAYCAST COLLISION");
        ////Debug.Log(collision.collider.gameObject.name);

        //Vector3 relativeHitPoint = hitPoint - transform.position;
        //Vector3 relativeRotation = direction - transform.rotation.eulerAngles;
        //bloodFXTransform.localPosition = relativeHitPoint;
        //bloodFXTransform.localEulerAngles = relativeRotation;
        bloodFXTransform.gameObject.SetActive(true);

        Debug.Log(direction);
        RIGID_BODY.AddForce(5f * Vector3.up);
        TurnOnRagdoll();
        GameObject.Find("AudioEnemyHit").GetComponent<AudioSource>().Play();
    }

    //private IEnumerator ProcessImpact(Vector3 direction)
    //{
        //yield return new WaitForSeconds(3f);
        //RIGID_BODY.AddForce(200f * direction);
        //yield return new WaitForSeconds(2f);
        //TurnOnRagdoll();
    //}
}
