using System.Collections;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Transform[] wayPoints;
    [SerializeField]
    private float wayTime;
    [SerializeField]
    private float unitPerSecond = 1;
    [SerializeField]
    private bool isPlayOnAwake = true;
    [SerializeField]
    private bool isLoop = true;

    private int wayPointCount;
    private int currentIndex = 0;

    private void Awake()
    {
        wayPointCount = wayPoints.Length;
        if (target == null) target = transform;
        if (isPlayOnAwake) Play();
    }

    public void Play()
    {
        StartCoroutine(nameof(Process));
    }

    private IEnumerator Process()
    {
        var wait = new WaitForSeconds(wayTime);
        while (true)
        {
            yield return StartCoroutine(MoveAToB(target.position, wayPoints[currentIndex].position));
            if (currentIndex < wayPointCount - 1)
                currentIndex++;
            else
            {
                if (isLoop) currentIndex = 0;
                else break;
            }
            yield return wait;
        }
        Debug.Log("모든 경로의 탐색을 완료했습니다.");
    }

    private IEnumerator MoveAToB(Vector3 start, Vector3 end)
    {
        float percent = 0;
        float moveTime = Vector3.Distance(start, end) / unitPerSecond;

        Debug.Log($"이동거리 : {Vector3.Distance(start, end)}, 이동시간 : {moveTime}");

        while (percent < 1)
        {
            percent += Time.deltaTime / moveTime;
            target.position = Vector3.Lerp(start, end, percent);
            yield return null;
        }
    }
}