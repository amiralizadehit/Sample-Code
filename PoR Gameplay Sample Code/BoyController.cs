using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

public class BoyController : EventManager
{
    private SkeletonAnimation skeletonAnimation;
    private Spine.AnimationState animationState;
    private Spine.Skeleton skeleton;
    private Point point;
    private JetPack jetpack;
    private MeshRenderer meshRenderer;
    private BoxCollider2D collider;
    private LayerMask allMask;
    private LayerMask platformMask;
    private Rigidbody2D rigidbody2D;
    private bool isTired;
    private bool pointAbility;
    private Direction direction;
    private Animation currentState;
    private bool parachuteActivated = false;
    private bool jetpackEquipped = false;
    private bool axeEquipped = false;
    private AudioSource audioSource;
    private float ballTime;

    [SpineAnimation] public string jetpackUp;
    [SpineAnimation] public string jetpackDown;
    [SpineAnimation] public string jetpackRight;
    [SpineAnimation] public string jetpackLeft;
    [SpineAnimation] public string fall;
    [SpineAnimation] public string walk;
    [SpineAnimation] public string axeAttack;
    [SpineAnimation] public string run;
    [SpineAnimation] public string fear;
    [SpineAnimation] public string tired;
    [SpineAnimation] public string idle;
    [SpineAnimation] public string balls;

    public bool physicsNeeded;
    public bool jetpackRequiredLevel;
    public float rayCastLength = 0.025f;
    public float tirednessRate = 5;
    public float energyRegainingRate = 1.5f;
    public float cooldownTime = 3;
    public float ballStartTime = 5;
    public Collider2D ignoreCollider;
    public AudioClip parachuteSound;
    public AudioClip landingSound;



    public static State state = State.ON_THE_GROUND;
    public static State previousState = State.ON_THE_GROUND;

    void Start()
    {
        point = GetComponent<Point>();
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        skeleton = skeletonAnimation.skeleton;
        audioSource = GetComponent<AudioSource>();
        animationState = skeletonAnimation.AnimationState;
        skeletonAnimation.state.Event += HandleEvent;
        rigidbody2D = GetComponent<Rigidbody2D>();
        meshRenderer = GetComponent<MeshRenderer>();
        jetpack = GetComponent<JetPack>();
        collider = GetComponent<BoxCollider2D>();
        allMask = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Platform"));
        platformMask = 1 << LayerMask.NameToLayer("Platform");
        if(ignoreCollider!=null)
            Physics2D.IgnoreCollision(ignoreCollider, collider);
        ballTime = ballStartTime;
    }

    void Update()
    {
        if (physicsNeeded)
        {
            RaycastHit2D middleHit = Physics2D.Raycast(new Vector2(collider.bounds.center.x, collider.bounds.min.y),
                Vector2.down, rayCastLength, allMask);

            if (state == State.ON_THE_GROUND || state == State.ON_THE_EDGE)
            {
                RaycastHit2D rightHit = Physics2D.Raycast(
                    new Vector2(collider.bounds.max.x + 0.025f, collider.bounds.min.y),
                    Vector2.down, rayCastLength, platformMask);
                RaycastHit2D leftHit = Physics2D.Raycast(new Vector2(collider.bounds.min.x - 0.025f, collider.bounds.min.y),
                    Vector2.down, rayCastLength, platformMask);

                if (leftHit && rightHit && middleHit)
                    state = State.ON_THE_GROUND;

                if (skeleton.FlipX)
                {
                    if (leftHit && !rightHit)
                    {
                        if (state != State.ON_THE_EDGE)
                            OnEdgeReach();
                    }
                }
                else
                {
                    if (rightHit && !leftHit)
                    {
                        if (state != State.ON_THE_EDGE)
                            OnEdgeReach();
                    }
                }
            }

            if (middleHit)
            {
                if (state == State.IN_THE_AIR)
                    OnReachGround();
            }
            else
            {
                print("kkkk");
                if (state == State.ON_THE_GROUND || state == State.ON_THE_EDGE)
                    OnLeaveGround();
            }

            TirednessCheck();
            BallAnimationCheck();
            GetDirection();
            if (jetpackRequiredLevel)
            {
                EjectCheck();
            }
        }

    }


    void OnEnable()
    {
        StartListening("OnReachGround", OnReachGround);
        StartListening("OnLeaveGround", OnLeaveGround);
        StartListening("OnEnableJetpack", OnEnableJetpack);
        StartListening("OnDisableJetpack", OnDisableJetpack);
        StartListening("OnEnablePoint", OnEnablePoint);
        StartListening("OnDisablePoint", OnDisablePoint);
        StartListening("OnEnableAxe", OnEnableAxe);
        StartListening("OnEnablePointAbility", OnEnablePointAbility);
        StartListening("OnDisablePointAbility", OnDisablePointAbility);
    }

    void OnDisable()
    {
        StopListening("OnReachGround", OnReachGround);
        StopListening("OnLeaveGround", OnLeaveGround);
        StopListening("OnEnableJetpack", OnEnableJetpack);
        StopListening("OnDisableJetpack", OnDisableJetpack);
        StopListening("OnEnablePoint", OnEnablePoint);
        StopListening("OnDisablePoint", OnDisablePoint);
        StopListening("OnEnableAxe", OnEnableAxe);
        StopListening("OnEnablePointAbility", OnEnablePointAbility);
        StopListening("OnDisablePointAbility", OnDisablePointAbility);
    }


    void EjectCheck()
    {
        var forceX = 0f;
        var forceY = 0f;
        if (state == State.IN_THE_AIR)
        {
            if (previousState == State.ON_THE_EDGE)
            {
                if (jetpack != null)
                {
                    if (jetpack.GetFuel() > 0)
                    {
                        RunAnimation(Animation.JETPACK_DOWN, true, 1, 0);
                        forceY = 1; //air drag
                        forceX = ((skeleton.FlipX) ? 1 : -1) * 1f;
                    }
                }
                else
                {
                    RunAnimation(Animation.FALL, true, 1, 0);
                    forceY = 1; //air drag
                    forceX = ((skeleton.FlipX) ? 1 : -1) * 1f;
                    PlayParachuteSound();
                }
                previousState = State.IN_THE_AIR;
            }
            else
            {
                if (parachuteActivated)
                {
                    RunAnimation(Animation.FALL, true, 1, 0);
                    forceY = 0.05f; //air drag
                }
            }
            rigidbody2D.velocity += new Vector2(forceX, forceY);
        }
    }

    void BallAnimationCheck()
    {
        if (point.enabled && !axeEquipped)
        {
            if (currentState == Animation.IDLE)
            {

                ballTime -= Time.deltaTime;
                if (ballTime < 0)
                {
                    RunAnimation(Animation.BALLS, true, 2, 0);
                }
            }
            else
            {
                ballTime = ballStartTime;
            }
        }
    }

    void TirednessCheck()
    {
        if (point.enabled)
        {
            if (currentState == Animation.RUN)
            {
                tirednessRate -= Time.deltaTime;
                if (tirednessRate < 0)
                {
                    RunAnimation(Animation.Tired, true, 1, 0);
                    point.PlayBreathingSound(false);
                    isTired = true;
                }
            }
            else if (currentState != Animation.RUN && tirednessRate < 5)
            {
                tirednessRate += Time.deltaTime * energyRegainingRate;
            }
        }
    }

    void OnReachGround()
    {
        print("here");
        RunAnimation(Animation.IDLE, true, 1, 0);
        state = State.ON_THE_GROUND;
        PlayLandingSound();
        
        parachuteActivated = false;
        if (!point.enabled && pointAbility)
        {
            OnEnablePoint();
            
        }
            
    }

    void OnLeaveGround()
    {
        if (point.enabled)
            OnDisablePoint();
        previousState = state;
        state = State.IN_THE_AIR;
    }

    void OnEdgeReach()
    {
        RunAnimation(Animation.FEAR, true, 1, 0);
        state = State.ON_THE_EDGE;
        FreezeCharacter();
    }

    void OnDisableJetpack()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        //EjectCheck();
        parachuteActivated = true;
        PlayParachuteSound();
        jetpack = null;
        jetpackEquipped = false;
    }

    void OnEnableJetpack()
    {
        if (jetpackEquipped)
        {
            jetpack.Refuel();
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
            jetpack = transform.GetChild(0).gameObject.GetComponent<JetPack>();
            jetpack.Refuel();
        }
    }

    void OnEnablePointAbility()
    {
        pointAbility = true;
        if (!point.enabled)
            point.enabled = true;
    }

    void OnEnableAxe()
    {
        transform.GetChild(0).transform.gameObject.SetActive(true);
        axeEquipped = true;
        RunAnimation(Animation.IDLE, true, 1, 0);
    }

    void OnDisablePointAbility()
    {
        pointAbility = false;
    }

    void OnEnablePoint()
    {
        point.enabled = true;
    }

    void OnDisablePoint()
    {
        if (point.enabled)
        {
            point.enabled = false;
        }
    }

    void FreezeCharacter()
    {
        point.ResetPoint();
    }

    void HandleEvent(TrackEntry trackentry, Spine.Event e)
    {
        if (e.data.name.Equals("step"))
        {
            point.PlayStepSound();
        }
    }

    void PlayParachuteSound()
    {
        audioSource.clip = parachuteSound;
        audioSource.Play();
    }

    void PlayLandingSound()
    {
        audioSource.clip = landingSound;
        audioSource.Play();
    }

    public void ChangeLayer(string layer)
    {
        meshRenderer.sortingLayerName = layer;
    }

    public Direction GetDirection()
    {
        if (skeleton.FlipX)
        {
            return Direction.RIGHT;
        }
        else
        {
            return Direction.LEFT;
        }
    }

    public void SetDirection(Direction direction)
    {
        if (direction == Direction.RIGHT)
        {
            skeleton.FlipX = true;
        }
        else
        {
            skeleton.FlipX = false;
        }
    }

    public bool IsOnTheGround()
    {
        return (state == State.ON_THE_GROUND);
    }

    public bool IsOnTheEdge()
    {
        return (state == State.ON_THE_EDGE);
    }

    public Animation GetAnimation()
    {
        return currentState;
    }

    public void RunAnimation(Animation state, bool loop, float timeScale, int trackIndex)
    {
        string anim = "";
        
        if (currentState != state)
        {
            switch (state)
            {
                case Animation.FALL:
                    anim = fall;
                    break;
                case Animation.FEAR:
                    anim = fear;
                    break;
                case Animation.IDLE:
                    anim = idle;
                    break;
                case Animation.WALK:
                    anim = walk;
                    break;
                case Animation.Tired:
                    anim = tired;
                    break;
                case Animation.RUN:
                    anim = run;
                    break;
                case Animation.BALLS:
                    anim = balls;
                    break;
                case Animation.AXE_ATTACK:
                    anim = axeAttack;
                    break;
                case Animation.JETPACK_UP:
                    anim = jetpackUp;
                    break;
                case Animation.JETPACK_DOWN:
                    anim = jetpackDown;
                    break;
                case Animation.JETPACK_LEFT:
                    anim = jetpackLeft;
                    break;
                case Animation.JETPACK_RIGHT:
                    anim = jetpackRight;
                    break;
            }
            if(state!=Animation.AXE_ATTACK)
                animationState.AddEmptyAnimation(1,0.25f,0);
            currentState = state;
            var track = animationState.SetAnimation(trackIndex, anim, loop);
            track.timeScale = timeScale;
            if (state == Animation.Tired)
            {
                StartCoroutine(ResetAnimation());
            }
        }
    }

    IEnumerator ResetAnimation()
    {
        yield return new WaitForSeconds(cooldownTime);
        RunAnimation(Animation.IDLE, true, 1, 0);
        tirednessRate = 5;
        point.PlayBreathingSound(true);
        isTired = false;
    }

    public bool IsTired()
    {
        return isTired;
    }

    public bool IsAxeEquipped()
    {
        return axeEquipped;
    }

    public enum State
    {
        ON_THE_GROUND,
        IN_THE_AIR,
        ON_THE_EDGE
    }

    public enum Animation
    {
        FEAR,
        Tired,
        FALL,
        WALK,
        RUN,
        BALLS,
        AXE_ATTACK,
        JETPACK_UP,
        JETPACK_DOWN,
        JETPACK_RIGHT,
        JETPACK_LEFT,
        IDLE,
    }

    public enum Direction
    {
        RIGHT,
        LEFT
    }
}