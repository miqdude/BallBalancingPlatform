using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Pad3DBallAgent : Agent
{
    public GameObject ball;
    Rigidbody ball_rb;
    EnvironmentParameters resetParams;

    public override void Initialize()
    {
        ball_rb = ball.GetComponent<Rigidbody>();
        resetParams = Academy.Instance.EnvironmentParameters;
        setResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Pad's rotation
        sensor.AddObservation(gameObject.transform.rotation.z / 360.0f);
        sensor.AddObservation(gameObject.transform.rotation.x / 360.0f);

        // Ball's position relative to pad
        sensor.AddObservation(ball.transform.position - gameObject.transform.position);

        // Ball's velocity
        sensor.AddObservation(ball_rb.velocity);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var actionZ = Mathf.Clamp(vectorAction[0], -1f, 1f);
        var actionX = Mathf.Clamp(vectorAction[1], -1f, 1f);

        gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
        gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);

        // If the ball drops reduce the reward
        // and restart
        if ( Mathf.Abs(ball.transform.position.x - gameObject.transform.position.x) > 5f ||
             Mathf.Abs(ball.transform.position.z - gameObject.transform.position.z) > 5f ||
             (ball.transform.position.y - gameObject.transform.position.y) < 0f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else
        {
            SetReward(0.1f);
        }
    }

    public override void OnEpisodeBegin()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        ball_rb.velocity = new Vector3(0f, 0f, 0f);

        gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));

        // Give randomness to the ball relative to Pad's position
        ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(2.0f, 3.0f), Random.Range(-1.5f, 1.5f))
            + gameObject.transform.position;
        // Reset the parameters when the agent is reset
        setResetParameters();
    }

    public void setBall()
    {
        ball_rb.mass = resetParams.GetWithDefault("mass", 5.0f);
        var scale = resetParams.GetWithDefault("scale", 2.0f);
        ball.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void setResetParameters()
    {
        setBall();
    }
}
