using MPWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public SegmentType defaultSegment;
    public List<SpecialSegmentIndex> specialSegments;
    public int segmentCount = 25;
    public float gravityScale = 1;
    public float ropeLineWidth = 0.22f;
    public int relaxIterations = 20;

    private LineRenderer ropeLine;
    private readonly List<Point> vertList = new List<Point>();
    private readonly List<Stick> stickList = new List<Stick>();


    [System.Serializable]
    public class SegmentType
    {
        public GameObject gameObject;
        public float length;
    }

    [System.Serializable]
    public class SpecialSegmentIndex
    {
        public int index;
        public SegmentType segment;
    }

    [System.Serializable]
    public class Point
    {
        public Vector3 oldPosition;
        public Vector3 relaxPos;
        public GameObject gameObject;
        public Transform transform;
        public RopeSegment segment;
        public IGravityUser gravityUser;
    }

    [System.Serializable]
    public class Stick
    {
        public Point front, back;
        public float length;
    }

    private void Awake()
    {
        ropeLine = GetComponent<LineRenderer>();
        defaultSegment.gameObject.GetComponent<SphereCollider>().radius = defaultSegment.length / 2;

        GenerateRope();
    }

    void Update()
    {
        // Draw rope line
        if (ropeLine)
        {
            ropeLine.startWidth = ropeLineWidth;
            ropeLine.endWidth = ropeLineWidth;
            ropeLine.positionCount = vertList.Count;
            ropeLine.SetPositions((from seg in vertList select seg.transform.position).ToArray());
        }
    }

    private void FixedUpdate()
    {
        // Initialize position
        foreach (var v in vertList)
            v.oldPosition = v.relaxPos = v.transform.position;

        // Relax Position
        for (int i = 0; i < relaxIterations; i++)
            foreach (Stick s in stickList)
            {
                Vector3 currentOffset = s.front.relaxPos - s.back.relaxPos;
                Vector3 changeAmount = (currentOffset.normalized * s.length - currentOffset) * 0.5f;
                
                if(s.back.segment && !s.back.segment.IsKenematic)
                    s.back.relaxPos -= changeAmount;

                if (s.front.segment && !s.front.segment.IsKenematic)
                    s.front.relaxPos += changeAmount;
            }

        // Apply Relaxed Position
        foreach (var v in vertList)
            v.transform.position = v.relaxPos;

        // RELAX ROTATION
        for(int i = 0; i < vertList.Count; i++)
        {
            Point vert = vertList[i];
            Vector3 lookDir = i == 0 
                ? vertList[1].transform.position - vert.transform.position
                : vert.transform.position - vertList[i - 1].transform.position;

            vert.transform.rotation = Quaternion.LookRotation(lookDir, -Physics.gravity);
        }

        // SET VELOCITY
        foreach (var vert in vertList)
        {
            IGravityUser gravityUser = vert.gameObject.GetComponent<IGravityUser>();
            
            if(gravityUser != null)
                gravityUser.Velocity += (vert.transform.position - vert.oldPosition) / Time.fixedDeltaTime;
        }
    }

    private void GenerateRope()
    {
        // MAKE VERTS
        for (int i = 0; i < segmentCount; i++)
        {
            // get a special or default segment
            SegmentType segment = (from seg in specialSegments
                                   where seg.index == i
                                   select seg.segment).FirstOrDefault() ?? defaultSegment;

            // instantiate new segment
            GameObject go = Instantiate(segment.gameObject,
                transform.position - transform.forward * segment.length * i,
                Quaternion.identity,
                transform);

            // add new segment to vert list
            vertList.Add(new Point()
            {
                gameObject = go,
                transform = go.transform,
                segment = go.GetComponent<RopeSegment>(),
                gravityUser = go.GetComponent<IGravityUser>()
            });
        }

        // MAKE STICKS
        for (int i = 1; i < vertList.Count; i++)
            stickList.Add(new Stick
            {
                front = vertList[i - 1],
                back = vertList[i],
                length = Vector3.Distance(vertList[i - 1].transform.position, vertList[i].transform.position)
            });
    }
}