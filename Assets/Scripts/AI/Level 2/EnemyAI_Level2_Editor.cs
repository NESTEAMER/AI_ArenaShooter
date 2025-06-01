using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyAI_Level2))]
public class EnemyAI_Level2_Editor : Editor
{
    void OnSceneGUI()
    {
        EnemyAI_Level2 fov = (EnemyAI_Level2)target;

        Handles.color = Color.white;
        Handles.DrawWireDisc(fov.transform.position, Vector3.forward, fov.sightRadius);

        Vector2 forwardDirection = fov.transform.up;
        Vector2 viewAngleA_2D = Quaternion.Euler(0, 0, -fov.sightAngle / 2) * forwardDirection;
        Vector2 viewAngleB_2D = Quaternion.Euler(0, 0, fov.sightAngle / 2) * forwardDirection;

        Handles.color = Color.green;
        Handles.DrawLine(fov.transform.position, fov.transform.position + (Vector3)viewAngleA_2D * fov.sightRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + (Vector3)viewAngleB_2D * fov.sightRadius);

        if (fov.canSeePlayer && fov.playerSeenTargetGO != null)
        {
            Handles.color = Color.red;
            Handles.DrawLine(fov.transform.position, fov.playerSeenTargetGO.position);
        }

        //Audio Sensor
        Handles.color = new Color(1f, 0.5f, 0f);//Orange specifically, they really don't got it in the autofill
        Handles.DrawWireDisc(fov.transform.position, Vector3.forward, fov.hearingRadius);

        if (fov.heardSound)
        {
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(fov.lastHeardSoundPosition, Vector3.forward, 0.3f);
        }
    }
}
