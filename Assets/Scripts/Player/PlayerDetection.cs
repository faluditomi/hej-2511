using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerDetection : MonoBehaviour
{
    private GuardStateMachine myStateMachine;

    [Tooltip("The layers that guard can't see through.")]
    [SerializeField] private LayerMask obstacleMask;

    [Tooltip("The object that the FOV mesh can be assigned to.")]
    [SerializeField] private MeshFilter viewMeshFilter;

    private Mesh viewMesh;

    [Tooltip("Increasing this number makes the FOV cone smoother at the price of a some performance.")]
    [SerializeField] private float meshResolution = 1f;
    [Tooltip("The distance from which the guard can spot the player. Represented visaully in the scene.")]
    [SerializeField] private float viewRadius = 10f;
    [Tooltip("The angle at which the guard sees in front of itself. Represented visually in the scene.")]
    [SerializeField] [Range(0,360)] private float viewAngle = 90;

    private void Awake()
    {
        myStateMachine = GetComponent<GuardStateMachine>();
    }

    private void Start()
    {
        viewMesh = new Mesh();

        viewMesh.name = "Field of View";

        viewMeshFilter.mesh = viewMesh;
    }
    private void LateUpdate()
    {
        DrawFieldOfViewCone();
    }

    public bool IsPlayerInSight()
    {
        Vector3 vectorToPlayer = (myStateMachine.GetPlayer().position - transform.position).normalized;
        
        float distanceToPlayer = Vector3.Distance(transform.position, myStateMachine.GetPlayer().position);

        if(Vector3.Angle(transform.forward, vectorToPlayer) < viewAngle / 2f && distanceToPlayer < viewRadius)
        {
            return !Physics.Raycast(transform.position, vectorToPlayer, distanceToPlayer, obstacleMask);
        }

        return false;
    }

    //Returns the vector that is at a certain angle from the guard.
    public Vector3 VectorFromAngle(float angleInDegrees, bool isAngleGlobal)
    {
        if(!isAngleGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void DrawFieldOfViewCone()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);

        //The angle between two vertices.
        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();

        //Casting a number of rays according to the mesh resolution.
        for(int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;

            ViewCastInfo newViewCast = ViewCast(angle);

            viewPoints.Add(newViewCast.point);
        }

        //The number of points we have in our mesh.
        int vertexCount = viewPoints.Count + 1;

        //The number of points we shoot rays to.
        Vector3[] vertices = new Vector3[vertexCount];

        //The number of trianges that make up our mesh.
        int[] triangles = new int[(vertexCount - 2) * 3];

        //The starting point has to be in local space, so it's relative to the guard.
        vertices[0] = Vector3.zero;

        //-1 because we already set the first vertex in the previous line.
        for(int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            //Setting up the array of triangles for the mesh in the form that Unity requires.
            if(i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
            
                triangles[i * 3 + 1] = i + 1;

                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;

        viewMesh.triangles = triangles;

        viewMesh.RecalculateNormals();
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = VectorFromAngle(globalAngle, true);

        RaycastHit hit;

        if(Physics.Raycast(transform.position, direction, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + direction * viewRadius, viewRadius, globalAngle);
        }
    }

    public float GetViewRadius()
    {
        return viewRadius;
    }

    public float GetViewAngle()
    {
        return viewAngle;
    }

    //A struct to hold the necessary info about the vertices of our mesh.
    public struct ViewCastInfo
    {
        public bool hit;
        
        public Vector3 point;

        public float distance;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;

            this.point = point;

            this.distance = distance;

            this.angle = angle;
        }
    }
}
