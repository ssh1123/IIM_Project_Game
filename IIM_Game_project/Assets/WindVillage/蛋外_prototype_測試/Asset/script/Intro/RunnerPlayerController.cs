using UnityEngine;
public class RunnerPlayerController : MonoBehaviour
{
  

    [Header("Move")]
    [SerializeField] private float forwardSpeed = 4f;
    [SerializeField] private float laneChangeSpeed = 12f;

    [Header("Lanes")]
    [SerializeField] private Transform laneTop;
    [SerializeField] private Transform laneMiddle;
    [SerializeField] private Transform laneBottom;


    private int currentLaneIndex = 1; // 0=Top, 1=Middle, 2=Bottom
    private float targetY;

    // ★ 新增這兩行
    private bool canMove = true;
    public void SetCanMove(bool value) => canMove = value;

    private void Start()
    {
        targetY = laneMiddle.position.y;
        Vector3 p = transform.position;
        p.y = targetY;
        transform.position = p;
    }

    private void Update()
    {
        if (!canMove) return;    // ★ 新增：停用時直接不更新

        transform.position += Vector3.right * forwardSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            ChangeLane(-1);

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            ChangeLane(1);

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetY, laneChangeSpeed * Time.deltaTime);
        transform.position = pos;
    }

    private void ChangeLane(int delta)
    {
        currentLaneIndex = Mathf.Clamp(currentLaneIndex + delta, 0, 2);

        targetY = currentLaneIndex switch
        {
            0 => laneTop.position.y,
            1 => laneMiddle.position.y,
            2 => laneBottom.position.y,
            _ => targetY
        };
    }
}

