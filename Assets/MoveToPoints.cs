using UnityEngine;

public class MoveToPoints : MonoBehaviour
{
    [Header("จุดอ้างอิงตำแหน่ง (ใส่ใน Inspector)")]
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;

    [Header("วัตถุที่จะขยับเมื่อถึงจุด C")]
    public GameObject OB1;
    public GameObject OB2;
    public GameObject OB3;

    [Header("ความเร็วการเคลื่อนที่")]
    public float moveSpeed = 3f;
    public float objectMoveSpeed = 2f;   // ความเร็วที่ OB1-3 เคลื่อนลง

    private int hitCount = 0;            // นับจำนวนครั้งที่ชน B1
    private Transform targetPoint;       // จุดเป้าหมายของวัตถุหลัก
    private bool isMoving = false;

    // ตัวแปรสำหรับ OB1-3
    private Vector3 ob1Target;
    private Vector3 ob2Target;
    private Vector3 ob3Target;
    private bool moveOBs = false;

    void Update()
    {
        // เคลื่อนวัตถุหลักไปยังเป้าหมาย
        if (isMoving && targetPoint != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPoint.position) < 0.01f)
            {
                isMoving = false;

                // ถ้าไปถึงจุด C → เริ่มให้ OB1, OB2, OB3 เคลื่อนลง
                if (targetPoint == pointC)
                {
                    SetOBTargets();
                    moveOBs = true;
                }
            }
        }

        // เคลื่อน OB1-3 ลง
        if (moveOBs)
        {
            MoveObjectsStep();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("B1"))
        {
            hitCount++;

            if (hitCount == 1 && pointA != null)
            {
                targetPoint = pointA;
                isMoving = true;
            }
            else if (hitCount == 2 && pointB != null)
            {
                targetPoint = pointB;
                isMoving = true;
            }
            else if (hitCount == 3 && pointC != null)
            {
                targetPoint = pointC;
                isMoving = true;
            }
        }
    }

    // เซ็ตตำแหน่งเป้าหมายของ OB1-3
    private void SetOBTargets()
    {
        if (OB1 != null) ob1Target = OB1.transform.position + new Vector3(0, -200f, 0);
        if (OB2 != null) ob2Target = OB2.transform.position + new Vector3(0, -200f, 0);
        if (OB3 != null) ob3Target = OB3.transform.position + new Vector3(0, -200f, 0);
    }

    // เคลื่อน OB1-3 ลงทีละนิด
    private void MoveObjectsStep()
    {
        if (OB1 != null) OB1.transform.position = Vector3.MoveTowards(OB1.transform.position, ob1Target, objectMoveSpeed * Time.deltaTime);
        if (OB2 != null) OB2.transform.position = Vector3.MoveTowards(OB2.transform.position, ob2Target, objectMoveSpeed * Time.deltaTime);
        if (OB3 != null) OB3.transform.position = Vector3.MoveTowards(OB3.transform.position, ob3Target, objectMoveSpeed * Time.deltaTime);

        // หยุดเมื่อถึงเป้าหมายทั้งหมด
        bool done1 = (OB1 == null) || Vector3.Distance(OB1.transform.position, ob1Target) < 0.01f;
        bool done2 = (OB2 == null) || Vector3.Distance(OB2.transform.position, ob2Target) < 0.01f;
        bool done3 = (OB3 == null) || Vector3.Distance(OB3.transform.position, ob3Target) < 0.01f;

        if (done1 && done2 && done3)
        {
            moveOBs = false;
        }
    }
}
